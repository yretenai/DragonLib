// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.Binary;

[AttributeUsage(AttributeTargets.Property)]
public sealed class BitFieldAttribute : Attribute {
	public BitFieldAttribute(int length) => Length = length;

	public int Length { get; }
}
