// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.Binary;

[AttributeUsage(AttributeTargets.Property)]
public sealed class BitFieldAttribute : Attribute {
	public BitFieldAttribute(int length) => Length = length;

	public int Length { get; }
}
