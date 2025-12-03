// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DragonLib.CommandLine;

public record CommandLineFlags {
	[Flag("positionals", Positional = 0, Hidden = true)]
	public Collection<string> Positionals { get; set; } = [];

	[Flag("h", Help = "Print this help text and exit", Aliases = ["help", "?"])]
	public bool Help { get; set; }

	[Flag("v", Help = "Print the program version and exit", Aliases = ["version"])]
	public virtual bool Version { get; set; }

	public record Singleton<T> : CommandLineFlags where T : CommandLineFlags {
		[field: AllowNull] [field: MaybeNull]
		public static T Instance {
			get => field ??= CommandLineFlagsParser.ParseFlags<T>();
			set;
		}
	}
}
