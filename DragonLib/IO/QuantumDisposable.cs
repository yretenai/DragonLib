// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO;

public sealed class QuantumDisposable : IDisposable {
	public QuantumDisposable(IDisposable? disposable = null) => Disposable = disposable;

	public IDisposable? Disposable { get; set; }
	public void Dispose() => Disposable?.Dispose();
}
