// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

// ReSharper disable once CheckNamespace

namespace DragonLib.SourceGen.MagicGenerator;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateMagicAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MagicAttribute(string magic) : Attribute {
	public string Magic { get; } = magic;
}

/*

[GenerateMagic]
public sealed partial class FileMagic {
	[Magic("IDX ")]
	public partial uint Index { get; }

	[Magic("DATA")]
	public partial uint Data { get; }
}

*/
