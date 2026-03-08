// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

// ReSharper disable once CheckNamespace

namespace DragonLib.SourceGen.BitStructGenerator;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class BitStructAttribute(int bytes) : Attribute {
	public int Bytes { get; } = bytes;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class BitFieldAttribute(int bits) : Attribute {
	public int Bits { get; } = bits;
}

/*

[BitStruct(4)]
public partial struct PackedStruct {
	[BitField(4)]
	public partial byte Nya { get; set; }

	[BitField(28)]
	public partial int Meow { get; set; }
}

*/
