// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

// ReSharper disable once CheckNamespace

namespace DragonLib.SourceGen.MagicGenerator;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateMagicAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MagicAttribute(string magic, bool littleEndian = true) : Attribute {
	public string Magic { get; } = magic;
	public bool LittleEndian { get; } = littleEndian;
}

/*

[GenerateMagic]
public static partial class FileMagic {
	[Magic("IDX ")]
	public static partial uint Index { get; }

	[Magic("DATA", false)]
	public static partial uint Data { get; }
}

*/
