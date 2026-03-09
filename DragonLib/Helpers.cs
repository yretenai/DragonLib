// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Threading;

namespace DragonLib;

public static class Helpers {
	public static void ResetCulture() {
		Thread.CurrentThread.CurrentCulture =
			Thread.CurrentThread.CurrentUICulture =
				CultureInfo.CurrentCulture =
					CultureInfo.CurrentUICulture =
						CultureInfo.DefaultThreadCurrentCulture =
							CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
	}
}
