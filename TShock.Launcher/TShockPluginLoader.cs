using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NLog;
using Terraria;
using TerrariaApi.Server;
#if NETCOREAPP
using System.Runtime.Loader;
#endif

namespace TShock.Launcher
{
	public class TShockPluginLoader : DefaultPluginLoader
	{
		protected override Logger Log => classLogger;

		private static Logger classLogger = LogManager.GetLogger(nameof(TShockPluginLoader));

		public TShockPluginLoader([NotNull] string pluginDirPath, bool ignoreVersion = false) : base(pluginDirPath, ignoreVersion)
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
		}

		public override List<IPlugin> LoadPlugins(bool reload = false)
		{
			var plugins = base.LoadPlugins(reload);
			plugins.Add(new TShockAPI.TShock(Main.instance)
			{
				Loader = this,
			});
			loadedPlugins = (from x in plugins
				orderby x.Order, x.Name
				select x).ToList();
			return loadedPlugins;
		}

		public override void UnloadPlugin(IPlugin plugin)
		{
			if (plugin.GetType() == typeof(TShockAPI.TShock))
				return;
			base.UnloadPlugin(plugin);
		}

		protected override List<IPlugin> LoadPluginFromFile(FileInfo fileInfo)
		{
			Assembly assembly;
			try
			{
#if NETCOREAPP
				using (var ms = new MemoryStream())
				{
					ms.Write(File.ReadAllBytes(fileInfo.FullName));
					ms.Seek(0, SeekOrigin.Begin);
					assembly = new PluginLoadContext(fileInfo.FullName).LoadFromStream(ms);
				}
#else
					assembly = Assembly.Load(File.ReadAllBytes(fileInfo.FullName));
#endif
				return LoadPluginInternal(assembly);
			}
			catch(BadImageFormatException) { }
			catch (Exception e)
			{
				Log.Error($"Error when loading plugin file '{fileInfo.FullName}'\nError: {e}");
			}
			return null;
		}

		private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (args.Name.StartsWith("TShockAPI,"))
				return typeof(TShockAPI.TShock).Assembly;
			return null;
		}

		protected override void DisposeUnmanaged()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
			base.DisposeUnmanaged();
		}
	}
}
