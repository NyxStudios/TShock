using System;
using System.Linq;
using System.Reflection;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using OTAPI;
using OTAPI.Shims.TShock;
using TerrariaApi.Server;

namespace TShock.Launcher
{
	class Program
	{
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
			{
				if (eventArgs.Name.StartsWith("TerrariaServer,"))
					return typeof(ServerApi).Assembly;
				return null;
			};

			ServerApi.PluginManager.RegisterPluginLoader(
				new TShockPluginLoader("ServerPlugins", args.Any(a => a == "-ignoreversion")));

			Launch.Start(args);
		}
	}
}
