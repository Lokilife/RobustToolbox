using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Robust.UnitTesting")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Robust.Benchmarks")]
[assembly: InternalsVisibleTo("Content.DiscordBot")]

#if NET5_0_OR_GREATER
[module: SkipLocalsInit]
#endif
