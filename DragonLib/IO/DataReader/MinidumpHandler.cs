// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DragonLib.IO.DataReader.Minidump;

namespace DragonLib.IO.DataReader;

public sealed class MinidumpHandler : IMemoryHandler {
	public MinidumpHandler(string path) {
		Stream = new FileStream(path, FileMode.Open, FileAccess.Read);

		var header = new MinidumpHeader();
		Stream.ReadExactly(MemoryMarshal.AsBytes(new Span<MinidumpHeader>(ref header)));
		Header = header;
		if (header.Signature != 0x504D444D) {
			throw new InvalidOperationException("Not a MDMP file");
		}

		if ((header.Flags & MinidumpFlags.WithFullMemory) == 0) {
			throw new InvalidOperationException("Only full memory dumps are supported");
		}

		Stream.Position = header.StreamDirectoryRVA;

		Streams = ArrayPool<MinidumpDirectory>.Shared.Rent(header.NumberOfStreams);
		var streamSpan = Streams.AsSpan(0, header.NumberOfStreams);
		Stream.ReadExactly(MemoryMarshal.AsBytes(streamSpan));

		// ReSharper disable once ArrangeRedundantParentheses
		var slopBuffer = (stackalloc byte[0x400]);

		foreach (var stream in streamSpan) {
			Stream.Position = stream.Location.RVA;
			switch (stream.Type) {
				case MinidumpStreamType.ModuleList when Modules is not null: throw new InvalidOperationException("Duplicate Modules Stream");
				case MinidumpStreamType.MemoryInfoList when MemoryInfo is not null: throw new InvalidOperationException("Duplicate MemoryInfo Stream");
				case MinidumpStreamType.MemoryList when MemoryRanges is not null: throw new InvalidOperationException("Duplicate MemoryList Stream");
				case MinidumpStreamType.Memory64List when MemoryRanges64 is not null: throw new InvalidOperationException("Duplicate Memory64List Stream");
				case MinidumpStreamType.ModuleList: {
					Stream.ReadExactly(slopBuffer[..4]);
					var count = MemoryMarshal.Read<int>(slopBuffer);
					Modules = ArrayPool<MinidumpModule>.Shared.Rent(count);
					ModuleNames = ArrayPool<string>.Shared.Rent(count);
					Modules.AsSpan().Clear();
					Stream.ReadExactly(MemoryMarshal.AsBytes(Modules.AsSpan(0, count)));
					Array.Fill(ModuleNames, string.Empty);

					for (var i = 0; i < count; ++i) {
						if (Modules[i].ModuleNameRVA == 0) {
							ModuleNames[i] = string.Empty;
							continue;
						}

						Stream.Position = Modules[i].ModuleNameRVA;
						Stream.ReadExactly(slopBuffer[..4]);
						var length = MemoryMarshal.Read<int>(slopBuffer);
						Stream.ReadExactly(slopBuffer[..length]);
						ModuleNames[i] = new string(MemoryMarshal.Cast<byte, char>(slopBuffer[..length]));
					}

					break;
				}
				case MinidumpStreamType.MemoryInfoList: {
					Stream.ReadExactly(slopBuffer[..16]);
					var headerSize = MemoryMarshal.Read<int>(slopBuffer);
					var entrySize = MemoryMarshal.Read<int>(slopBuffer[4..]);
					var count64 = MemoryMarshal.Read<long>(slopBuffer[8..]);

					if (count64 > int.MaxValue) {
						throw new InvalidOperationException("Too many memory pages");
					}

					if (headerSize != 16) {
						throw new InvalidOperationException("Memory Info List header is an unexpected size");
					}

					if (entrySize != Unsafe.SizeOf<MinidumpMemoryInfo>()) {
						throw new InvalidOperationException("Memory Info List entries are an unexpected size");
					}

					var count = (int) count64;

					MemoryInfo = ArrayPool<MinidumpMemoryInfo>.Shared.Rent(count);
					MemoryInfo.AsSpan().Clear();
					Stream.ReadExactly(MemoryMarshal.AsBytes(MemoryInfo.AsSpan(0, count)));
					break;
				}
				case MinidumpStreamType.MemoryList: {
					Stream.ReadExactly(slopBuffer[..4]);
					var count = MemoryMarshal.Read<int>(slopBuffer);
					MemoryRanges = ArrayPool<MinidumpMemoryDescriptor>.Shared.Rent(count);
					MemoryRanges.AsSpan().Clear();
					Stream.ReadExactly(MemoryMarshal.AsBytes(MemoryRanges.AsSpan(0, count)));
					break;
				}
				case MinidumpStreamType.Memory64List: {
					Stream.ReadExactly(slopBuffer[..16]);
					var count64 = MemoryMarshal.Read<long>(slopBuffer);
					BaseRVA = MemoryMarshal.Read<long>(slopBuffer[8..]);

					if (count64 > int.MaxValue) {
						throw new InvalidOperationException("Too many memory pages");
					}

					var count = (int) count64;
					MemoryRanges64 = ArrayPool<MinidumpMemoryDescriptor64>.Shared.Rent(count);
					MemoryRanges64.AsSpan().Clear();
					Stream.ReadExactly(MemoryMarshal.AsBytes(MemoryRanges64.AsSpan(0, count)));
					break;
				}
			}
		}

		Modules ??= [];
		ModuleNames ??= [];
		MemoryRanges ??= [];
		MemoryRanges64 ??= [];
		MemoryInfo ??= [];
	}

	public MinidumpHeader Header { get; }
	public Stream Stream { get; }
	private object Lock { get; } = new();
	public MinidumpDirectory[] Streams { get; }
	public MinidumpModule[] Modules { get; }
	public MinidumpMemoryDescriptor[] MemoryRanges { get; }
	public MinidumpMemoryDescriptor64[] MemoryRanges64 { get; }
	public MinidumpMemoryInfo[] MemoryInfo { get; }
	public string[] ModuleNames { get; }
	public long BaseRVA { get; }

	public bool ReadBytes(nint address, Span<byte> buffer, out int bytesRead) => ReadBytesFromRanges(address, buffer, out bytesRead) || ReadBytesFromRanges64(address, buffer, out bytesRead);

	public void Dispose() {
		Stream.Dispose();
		ArrayPool<MinidumpDirectory>.Shared.Return(Streams);
		if (Modules.Length > 0) {
			ArrayPool<MinidumpModule>.Shared.Return(Modules);
			ArrayPool<string>.Shared.Return(ModuleNames);
		}

		if (MemoryRanges.Length > 0) {
			ArrayPool<MinidumpMemoryDescriptor>.Shared.Return(MemoryRanges);
		}

		if (MemoryRanges64.Length > 0) {
			ArrayPool<MinidumpMemoryDescriptor64>.Shared.Return(MemoryRanges64);
		}

		if (MemoryInfo.Length > 0) {
			ArrayPool<MinidumpMemoryInfo>.Shared.Return(MemoryInfo);
		}
	}

	public IEnumerable<(nint ModuleStart, int ModuleSize, string ModuleName)> EnumerateModules() {
		for (var index = 0; index < Modules.Length; index++) {
			var module = Modules[index];
			if (module.BaseOfImage > 0) {
				yield return (module.BaseOfImage, module.SizeOfImage, ModuleNames[index]);
			}
		}
	}

	public Dictionary<string, List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>> MapSections() {
		var result = new Dictionary<string, List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>>();
		foreach (var (moduleStart, moduleSize, moduleName) in EnumerateModules()) {
			var sections = new List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>();
			result[moduleName] = sections;
			var currentAddress = moduleStart;
			while (currentAddress < moduleStart + moduleSize) {
				var region = GetInfo(currentAddress);

				var flags = SectionFlags.NoAccess;
				if ((region.Protect & MinidumpMemoryProtect.Execute) != 0) {
					flags |= SectionFlags.Execute;
				}

				if ((region.Protect & MinidumpMemoryProtect.ExecuteRead) != 0) {
					flags |= SectionFlags.Execute;
					flags |= SectionFlags.Read;
				}

				if ((region.Protect & MinidumpMemoryProtect.ExecuteRead) != 0) {
					flags |= SectionFlags.Execute;
					flags |= SectionFlags.Read;
				}

				if ((region.Protect & MinidumpMemoryProtect.ExecuteReadWrite) != 0) {
					flags |= SectionFlags.Execute;
					flags |= SectionFlags.Read;
					flags |= SectionFlags.Write;
				}

				if ((region.Protect & MinidumpMemoryProtect.ExecuteWrite) != 0) {
					flags |= SectionFlags.Execute;
					flags |= SectionFlags.Write;
				}

				if ((region.Protect & MinidumpMemoryProtect.Write) != 0) {
					flags |= SectionFlags.Write;
				}

				if ((region.Protect & MinidumpMemoryProtect.ReadOnly) != 0) {
					flags |= SectionFlags.Read;
				}

				if ((region.Protect & MinidumpMemoryProtect.ReadWrite) != 0) {
					flags |= SectionFlags.Read;
					flags |= SectionFlags.Write;
				}

				sections.Add((region.BaseAddress, (int) region.RegionSize, flags));
				currentAddress = region.BaseAddress + region.RegionSize;
			}
		}

		return result;
	}

	private bool ReadBytesFromRanges(nint address, Span<byte> buffer, out int bytesRead) {
		var read = 0;
		foreach (var memoryRange in MemoryRanges) {
			if (memoryRange.StartOfMemoryRange > address || memoryRange.StartOfMemoryRange + memoryRange.Memory.Size < address) {
				// not contained
				continue;
			}

			var readFromPage = buffer.Length;
			var memoryEnd = memoryRange.StartOfMemoryRange + memoryRange.Memory.Size;
			if (address + readFromPage > memoryEnd) {
				readFromPage = (int) (memoryEnd - address);
			}

			var shift = address - memoryRange.StartOfMemoryRange;
			Stream.Position = memoryRange.Memory.RVA + shift;
			Stream.ReadExactly(buffer[..readFromPage]);
			read += readFromPage;
			buffer = buffer[readFromPage..];
			if (buffer.IsEmpty) {
				break;
			}
		}

		bytesRead = read;
		return read > 0;
	}

	private bool ReadBytesFromRanges64(nint address, Span<byte> buffer, out int bytesRead) {
		var read = 0;
		var rva = BaseRVA;
		foreach (var memoryRange in MemoryRanges64) {
			if (memoryRange.StartOfMemoryRange > address || memoryRange.StartOfMemoryRange + memoryRange.Size < address) {
				rva += memoryRange.Size;
				// not contained
				continue;
			}

			var readFromPage = buffer.Length;
			var memoryEnd = memoryRange.StartOfMemoryRange + memoryRange.Size;
			if (address + readFromPage > memoryEnd) {
				readFromPage = (int) (memoryEnd - address);
			}

			var shift = address - memoryRange.StartOfMemoryRange;

			lock (Lock) {
				Stream.Position = rva + shift;
				Stream.ReadExactly(buffer[..readFromPage]);
			}

			read += readFromPage;
			buffer = buffer[readFromPage..];
			if (buffer.IsEmpty) {
				break;
			}

			address += readFromPage;
			rva += memoryRange.Size;
		}

		bytesRead = read;
		return read > 0;
	}

	public long GetOffset(nint address) {
		var rva = BaseRVA;
		foreach (var memoryRange in MemoryRanges64) {
			if (memoryRange.StartOfMemoryRange > address || memoryRange.StartOfMemoryRange + memoryRange.Size < address) {
				rva += memoryRange.Size;
				// not contained
				continue;
			}

			var shift = address - memoryRange.StartOfMemoryRange;
			return rva + shift;
		}

		return 0;
	}

	public MinidumpMemoryInfo GetInfo(nint address) {
		foreach (var memoryInfo in MemoryInfo) {
			if (memoryInfo.BaseAddress > address || memoryInfo.BaseAddress + memoryInfo.RegionSize <= address) {
				continue;
			}

			return memoryInfo;
		}

		return default;
	}
}
