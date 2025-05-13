// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO;

public sealed class QuantumDisposable : IDisposable {
	public QuantumDisposable(IDisposable? disposable = null) => Disposable = disposable;

	public IDisposable? Disposable { get; set; }
	public void Dispose() => Disposable?.Dispose();
}
