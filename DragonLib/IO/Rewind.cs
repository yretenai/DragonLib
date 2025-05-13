// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO;

public sealed class Rewind : IDisposable {
	public Rewind(Stream stream) {
		Stream = stream;
		Position = stream.Position;
	}

	private Stream Stream { get; }
	public long Position { get; }

	public void Dispose() {
		if (Stream.CanSeek) {
			Stream.Position = Position;
		}
	}
}
