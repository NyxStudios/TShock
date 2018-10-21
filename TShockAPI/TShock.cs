/*
TShock, a server mod for Terraria
Copyright (C) 2011-2018 Pryaxis & TShock Contributors

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using MaxMind;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI.DB;
using TShockAPI.Hooks;
using TShockAPI.ServerSideCharacters;
using Terraria.Utilities;
using Microsoft.Xna.Framework;
using TShockAPI.Sockets;
using TShockAPI.CLI;
using TShockAPI.Localization;

namespace TShockAPI
{
	/// <summary>
	/// This is the TShock main class. TShock is a plugin on the TerrariaServerAPI, so it extends the base TerrariaPlugin.
	/// TShock also complies with the API versioning system, and defines its required API version here.
	/// </summary>
	[ApiVersion(2, 1)]
	public class TShock : TerrariaPlugin
	{
		/// <summary>VersionNum - The version number the TerrariaAPI will return back to the API. We just use the Assembly info.</summary>
		public static readonly Version VersionNum = Assembly.GetExecutingAssembly().GetName().Version;
		/// <summary>VersionCodename - The version codename is displayed when the server starts. Inspired by software codenames conventions.</summary>
		public static readonly string VersionCodename = "Alnitak";

		/// <summary>SavePath - This is the path TShock saves its data in. This path is relative to the TerrariaServer.exe (not in ServerPlugins).</summary>
		public static string SavePath = "tshock";
		/// <summary>LogFormatDefault - This is the default log file naming format. Actually, this is the only log format, because it never gets set again.</summary>
		private const string LogFormatDefault = "yyyy-MM-dd_HH-mm-ss";
		//TODO: Set the log path in the config file.
		/// <summary>LogFormat - This is the log format, which is never set again.</summary>
		private static string LogFormat = LogFormatDefault;
		/// <summary>LogPathDefault - The default log path.</summary>
		private const string LogPathDefault = "tshock";
		/// <summary>This is the log path, which is initially set to the default log path, and then to the config file log path later.</summary>
		private static string LogPath = LogPathDefault;
		/// <summary>LogClear - Determines whether or not the log file should be cleared on initialization.</summary>
		private static bool LogClear;

		/// <summary>Will be set to true once Utils.StopServer() is called.</summary>
		public static bool ShuttingDown;

		/// <summary>Players - Contains all TSPlayer objects for accessing TSPlayers currently on the server</summary>
		public static TSPlayer[] Players = new TSPlayer[Main.maxPlayers];
		/// <summary>Bans - Static reference to the ban manager for accessing bans & related functions.</summary>
		public static BanManager Bans;
		/// <summary>Warps - Static reference to the warp manager for accessing the warp system.</summary>
		public static WarpManager Warps;
		/// <summary>Regions - Static reference to the region manager for accessing the region system.</summary>
		public static RegionManager Regions;
		/// <summary>Backups - Static reference to the backup manager for accessing the backup system.</summary>
		public static BackupManager Backups;
		/// <summary>Groups - Static reference to the group manager for accessing the group system.</summary>
		public static GroupManager Groups;
		/// <summary>Users - Static reference to the user manager for accessing the user database system.</summary>
		public static UserAccountManager UserAccounts;
		/// <summary>Itembans - Static reference to the item ban system.</summary>
		public static ItemManager Itembans;
		/// <summary>ProjectileBans - Static reference to the projectile ban system.</summary>
		public static ProjectileManagager ProjectileBans;
		/// <summary>TileBans - Static reference to the tile ban system.</summary>
		public static TileManager TileBans;
		/// <summary>RememberedPos - Static reference to the remembered position manager.</summary>
		public static RememberedPosManager RememberedPos;
		/// <summary>CharacterDB - Static reference to the SSC character manager.</summary>
		public static CharacterManager CharacterDB;
		/// <summary>Config - Static reference to the config system, for accessing values set in users' config files.</summary>
		public static ConfigFile Config { get; set; }
		/// <summary>ServerSideCharacterConfig - Static reference to the server side character config, for accessing values set by users to modify SSC.</summary>
		public static ServerSideConfig ServerSideCharacterConfig;
		/// <summary>DB - Static reference to the database.</summary>
		public static IDbConnection DB;
		/// <summary>OverridePort - Determines if TShock should override the server port.</summary>
		public static bool OverridePort;
		/// <summary>Geo - Static reference to the GeoIP system which determines the location of an IP address.</summary>
		public static GeoIPCountry Geo;
		/// <summary>Utils - Static reference to the utilities class, which contains a variety of utility functions.</summary>
		public static Utils Utils = Utils.Instance;
		/// <summary>UpdateManager - Static reference to the update checker, which checks for updates and notifies server admins of updates.</summary>
		public static UpdateManager UpdateManager;
		/// <summary>Log - Static reference to the log system, which outputs to either SQL or a text file, depending on user config.</summary>
		public static ILog Log;
		/// <summary>instance - Static reference to the TerrariaPlugin instance.</summary>
		public static TerrariaPlugin instance;
		/// <summary>
		/// Static reference to a <see cref="CommandLineParser"/> used for simple command-line parsing
		/// </summary>
		public static CommandLineParser CliParser { get; } = new CommandLineParser();

		/// <summary>The TShock anti-cheat/anti-exploit system.</summary>
		internal Bouncer Bouncer;

		/// <summary>
		/// TShock's Region subsystem.
		/// </summary>
		internal RegionHandler RegionSystem;

		/// <summary>
		/// Called after TShock is initialized. Useful for plugins that needs hooks before tshock but also depend on tshock being loaded.
		/// </summary>
		public static event Action Initialized;

		/// <summary>Version - The version required by the TerrariaAPI to be passed back for checking & loading the plugin.</summary>
		/// <value>value - The version number specified in the Assembly, based on the VersionNum variable set in this class.</value>
		public override Version Version
		{
			get { return VersionNum; }
		}

		/// <summary>Name - The plugin name.</summary>
		/// <value>value - "TShock"</value>
		public override string Name
		{
			get { return "TShock"; }
		}

		/// <summary>Author - The author of the plugin.</summary>
		/// <value>value - "The TShock Team"</value>
		public override string Author
		{
			get { return "The TShock Team"; }
		}

		/// <summary>Description - The plugin description.</summary>
		/// <value>value - "The administration modification of the future."</value>
		public override string Description
		{
			get { return "The administration modification of the future."; }
		}

		/// <summary>TShock - The constructor for the TShock plugin.</summary>
		/// <param name="game">game - The Terraria main game.</param>
		public TShock(Main game)
			: base(game)
		{
			Config = new ConfigFile();
			ServerSideCharacterConfig = new ServerSideConfig();
			ServerSideCharacterConfig.StartingInventory.Add(new NetItem(-15, 1, 0));
			ServerSideCharacterConfig.StartingInventory.Add(new NetItem(-13, 1, 0));
			ServerSideCharacterConfig.StartingInventory.Add(new NetItem(-16, 1, 0));
			Order = 0;
			instance = this;
		}

		/// <summary>Initialize - Called by the TerrariaServerAPI during initialization.</summary>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public override void Initialize()
		{
			string logFilename;
			string logPathSetupWarning;

			OTAPI.Hooks.Net.Socket.Create = () =>
			{
				//Console.WriteLine($"Creating socket {nameof(LinuxTcpSocket)}");
				return new LinuxTcpSocket();
				//return new OTAPI.Sockets.PoolSocket();
				//return new Terraria.Net.Sockets.TcpSocket();
			};
			OTAPI.Hooks.Player.Announce = (int playerId) =>
			{
				//TShock handles this
				return OTAPI.HookResult.Cancel;
			};

			Main.SettingsUnlock_WorldEvil = true;

			TerrariaApi.Reporting.CrashReporter.HeapshotRequesting += CrashReporter_HeapshotRequesting;

			try
			{
				CliParser.Reset();
				HandleCommandLine(Environment.GetCommandLineArgs());

				if (!Directory.Exists(SavePath))
					Directory.CreateDirectory(SavePath);

				ConfigFile.ConfigRead += OnConfigRead;
				FileTools.SetupConfig();

				Main.ServerSideCharacter = ServerSideCharacterConfig.Enabled;

				//TSAPI previously would do this automatically, but the vanilla server wont
				if (Netplay.ServerIP == null)
					Netplay.ServerIP = IPAddress.Any;

				DateTime now = DateTime.Now;
				// Log path was not already set by the command line parameter?
				if (LogPath == LogPathDefault)
					LogPath = Config.LogPath;
				try
				{
					logFilename = Path.Combine(LogPath, now.ToString(LogFormat) + ".log");
					if (!Directory.Exists(LogPath))
						Directory.CreateDirectory(LogPath);
				}
				catch (Exception ex)
				{
					logPathSetupWarning =
						"Could not apply the given log path / log format, defaults will be used. Exception details:\n" + ex;

					ServerApi.LogWriter.PluginWriteLine(this, logPathSetupWarning, TraceLevel.Error);

					// Problem with the log path or format use the default
					logFilename = Path.Combine(LogPathDefault, now.ToString(LogFormatDefault) + ".log");
				}

				AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			}
			catch (Exception ex)
			{
				// Will be handled by the server api and written to its crashlog.txt.
				throw new Exception("Fatal TShock initialization exception. See inner exception for details.", ex);
			}

			// Further exceptions are written to TShock's log from now on.
			try
			{
				if (Config.StorageType.ToLower() == "sqlite")
				{
					string sql = Path.Combine(SavePath, "tshock.sqlite");
					DB = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
				}
				else if (Config.StorageType.ToLower() == "mysql")
				{
					try
					{
						var hostport = Config.MySqlHost.Split(':');
						DB = new MySqlConnection();
						DB.ConnectionString =
							String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
								hostport[0],
								hostport.Length > 1 ? hostport[1] : "3306",
								Config.MySqlDbName,
								Config.MySqlUsername,
								Config.MySqlPassword
								);
					}
					catch (MySqlException ex)
					{
						ServerApi.LogWriter.PluginWriteLine(this, ex.ToString(), TraceLevel.Error);
						throw new Exception("MySql not setup correctly");
					}
				}
				else
				{
					throw new Exception("Invalid storage type");
				}

				if (Config.UseSqlLogs)
					Log = new SqlLog(DB, logFilename, LogClear);
				else
					Log = new TextLog(logFilename, LogClear);

				if (File.Exists(Path.Combine(SavePath, "tshock.pid")))
				{
					Log.ConsoleInfo(
						"TShock was improperly shut down. Please use the exit command in the future to prevent this.");
					File.Delete(Path.Combine(SavePath, "tshock.pid"));
				}
				File.WriteAllText(Path.Combine(SavePath, "tshock.pid"),
					Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture));

				CliParser.Reset();
				HandleCommandLinePostConfigLoad(Environment.GetCommandLineArgs());

				Backups = new BackupManager(Path.Combine(SavePath, "backups"));
				Backups.KeepFor = Config.BackupKeepFor;
				Backups.Interval = Config.BackupInterval;
				Bans = new BanManager(DB);
				Warps = new WarpManager(DB);
				Regions = new RegionManager(DB);
				UserAccounts = new UserAccountManager(DB);
				Groups = new GroupManager(DB);
				Itembans = new ItemManager(DB);
				ProjectileBans = new ProjectileManagager(DB);
				TileBans = new TileManager(DB);
				RememberedPos = new RememberedPosManager(DB);
				CharacterDB = new CharacterManager(DB);
				Bouncer = new Bouncer();
				RegionSystem = new RegionHandler(Regions);

				var geoippath = "GeoIP.dat";
				if (Config.EnableGeoIP && File.Exists(geoippath))
					Geo = new GeoIPCountry(geoippath);

				Log.ConsoleInfo("TShock {0} ({1}) now running.", Version, VersionCodename);

				ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInit);
				ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
				ServerApi.Hooks.GameHardmodeTileUpdate.Register(this, OnHardUpdate);
				ServerApi.Hooks.GameStatueSpawn.Register(this, OnStatueSpawn);
				ServerApi.Hooks.ServerConnect.Register(this, OnConnect);
				ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
				ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
				ServerApi.Hooks.ServerChat.Register(this, OnChat);
				ServerApi.Hooks.ServerCommand.Register(this, ServerHooks_OnCommand);
				ServerApi.Hooks.NetGetData.Register(this, OnGetData);
				ServerApi.Hooks.NetSendData.Register(this, NetHooks_SendData);
				ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
				ServerApi.Hooks.NpcStrike.Register(this, NpcHooks_OnStrikeNpc);
				ServerApi.Hooks.ProjectileSetDefaults.Register(this, OnProjectileSetDefaults);
				ServerApi.Hooks.WorldStartHardMode.Register(this, OnStartHardMode);
				ServerApi.Hooks.WorldSave.Register(this, SaveManager.Instance.OnSaveWorld);
				ServerApi.Hooks.WorldChristmasCheck.Register(this, OnXmasCheck);
				ServerApi.Hooks.WorldHalloweenCheck.Register(this, OnHalloweenCheck);
				ServerApi.Hooks.NetNameCollision.Register(this, NetHooks_NameCollision);
				ServerApi.Hooks.ItemForceIntoChest.Register(this, OnItemForceIntoChest);
				ServerApi.Hooks.WorldGrassSpread.Register(this, OnWorldGrassSpread);
				Hooks.PlayerHooks.PlayerPreLogin += OnPlayerPreLogin;
				Hooks.PlayerHooks.PlayerPostLogin += OnPlayerLogin;
				Hooks.AccountHooks.AccountDelete += OnAccountDelete;
				Hooks.AccountHooks.AccountCreate += OnAccountCreate;

				GetDataHandlers.InitGetDataHandler();
				Commands.InitCommands();

				EnglishLanguage.Initialize();

				Log.ConsoleInfo("AutoSave " + (Config.AutoSave ? "Enabled" : "Disabled"));
				Log.ConsoleInfo("Backups " + (Backups.Interval > 0 ? "Enabled" : "Disabled"));

				if (Initialized != null)
					Initialized();

				Log.ConsoleInfo("Welcome to TShock for Terraria!");
				Log.ConsoleInfo("TShock comes with no warranty & is free software.");
				Log.ConsoleInfo("You can modify & distribute it under the terms of the GNU GPLv3.");

			}
			catch (Exception ex)
			{
				Log.Error("Fatal Startup Exception");
				Log.Error(ex.ToString());
				Environment.Exit(1);
			}
		}

		protected void CrashReporter_HeapshotRequesting(object sender, EventArgs e)
		{
			foreach (TSPlayer player in TShock.Players)
			{
				player.Account = null;
			}
		}

		/// <summary>Dispose - Called when disposing.</summary>
		/// <param name="disposing">disposing - If set, disposes of all hooks and other systems.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// NOTE: order is important here
				if (Geo != null)
				{
					Geo.Dispose();
				}
				SaveManager.Instance.Dispose();

				ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInit);
				ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
				ServerApi.Hooks.GameHardmodeTileUpdate.Deregister(this, OnHardUpdate);
				ServerApi.Hooks.GameStatueSpawn.Deregister(this, OnStatueSpawn);
				ServerApi.Hooks.ServerConnect.Deregister(this, OnConnect);
				ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
				ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
				ServerApi.Hooks.ServerCommand.Deregister(this, ServerHooks_OnCommand);
				ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
				ServerApi.Hooks.NetSendData.Deregister(this, NetHooks_SendData);
				ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
				ServerApi.Hooks.NpcStrike.Deregister(this, NpcHooks_OnStrikeNpc);
				ServerApi.Hooks.ProjectileSetDefaults.Deregister(this, OnProjectileSetDefaults);
				ServerApi.Hooks.WorldStartHardMode.Deregister(this, OnStartHardMode);
				ServerApi.Hooks.WorldSave.Deregister(this, SaveManager.Instance.OnSaveWorld);
				ServerApi.Hooks.WorldChristmasCheck.Deregister(this, OnXmasCheck);
				ServerApi.Hooks.WorldHalloweenCheck.Deregister(this, OnHalloweenCheck);
				ServerApi.Hooks.NetNameCollision.Deregister(this, NetHooks_NameCollision);
				ServerApi.Hooks.ItemForceIntoChest.Deregister(this, OnItemForceIntoChest);
				ServerApi.Hooks.WorldGrassSpread.Deregister(this, OnWorldGrassSpread);
				TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= OnPlayerLogin;

				if (File.Exists(Path.Combine(SavePath, "tshock.pid")))
				{
					File.Delete(Path.Combine(SavePath, "tshock.pid"));
				}

				Log.Dispose();

				RegionSystem.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <summary>OnPlayerLogin - Fires the PlayerLogin hook to listening plugins.</summary>
		/// <param name="args">args - The PlayerPostLoginEventArgs object.</param>
		private void OnPlayerLogin(PlayerPostLoginEventArgs args)
		{
			List<String> KnownIps = new List<string>();
			if (!string.IsNullOrWhiteSpace(args.Player.Account.KnownIps))
			{
				KnownIps = JsonConvert.DeserializeObject<List<String>>(args.Player.Account.KnownIps);
			}

			if (KnownIps.Count == 0)
			{
				KnownIps.Add(args.Player.IP);
			}
			else
			{
				bool last = KnownIps.Last() == args.Player.IP;
				if (!last)
				{
					if (KnownIps.Count == 100)
					{
						KnownIps.RemoveAt(0);
					}

					KnownIps.Add(args.Player.IP);
				}
			}

			args.Player.Account.KnownIps = JsonConvert.SerializeObject(KnownIps, Formatting.Indented);
			UserAccounts.UpdateLogin(args.Player.Account);

			Ban potentialBan = Bans.GetBanByAccountName(args.Player.Account.Name);

			if (potentialBan != null)
			{
				// A user just signed in successfully despite being banned by account name.
				// We should fix the ban database so that all of their ban info is up to date.
				Bans.AddBan(args.Player.IP, args.Player.Name, args.Player.UUID, args.Player.Account.Name,
					potentialBan.Reason, false, potentialBan.BanningUser, potentialBan.Expiration);

				// And then get rid of them.
				if (potentialBan.Expiration == "")
				{
					args.Player.Kick(String.Format("Permanently banned by {0} for {1}", potentialBan.BanningUser
						,potentialBan.Reason), true, true);
				}
				else
				{
					args.Player.Kick(String.Format("Still banned by {0} for {1}", potentialBan.BanningUser,
						potentialBan.Reason), true, true);
				}
			}
		}

		/// <summary>OnAccountDelete - Internal hook fired on account delete.</summary>
		/// <param name="args">args - The AccountDeleteEventArgs object.</param>
		private void OnAccountDelete(Hooks.AccountDeleteEventArgs args)
		{
			CharacterDB.RemovePlayer(args.Account.ID);
		}

		/// <summary>OnAccountCreate - Internal hook fired on account creation.</summary>
		/// <param name="args">args - The AccountCreateEventArgs object.</param>
		private void OnAccountCreate(Hooks.AccountCreateEventArgs args)
		{
			CharacterDB.SeedInitialData(UserAccounts.GetUserAccount(args.Account));
		}

		/// <summary>OnPlayerPreLogin - Internal hook fired when on player pre login.</summary>
		/// <param name="args">args - The PlayerPreLoginEventArgs object.</param>
		private void OnPlayerPreLogin(Hooks.PlayerPreLoginEventArgs args)
		{
			if (args.Player.IsLoggedIn)
				args.Player.SaveServerCharacter();
		}

		/// <summary>NetHooks_NameCollision - Internal hook fired when a name collision happens.</summary>
		/// <param name="args">args - The NameCollisionEventArgs object.</param>
		private void NetHooks_NameCollision(NameCollisionEventArgs args)
		{
			if (args.Handled)
			{
				return;
			}

			string ip = Utils.GetRealIP(Netplay.Clients[args.Who].Socket.GetRemoteAddress().ToString());

			var player = Players.First(p => p != null && p.Name == args.Name && p.Index != args.Who);
			if (player != null)
			{
				if (player.IP == ip)
				{
					Netplay.Clients[player.Index].PendingTermination = true;
					args.Handled = true;
					return;
				}
				if (player.IsLoggedIn)
				{
					var ips = JsonConvert.DeserializeObject<List<string>>(player.Account.KnownIps);
					if (ips.Contains(ip))
					{
						Netplay.Clients[player.Index].PendingTermination = true;
						args.Handled = true;
					}
				}
			}
		}

		/// <summary>OnItemForceIntoChest - Internal hook fired when a player quick stacks items into a chest.</summary>
		/// <param name="args">The <see cref="ForceItemIntoChestEventArgs"/> object.</param>
		private void OnItemForceIntoChest(ForceItemIntoChestEventArgs args)
		{
			if (args.Handled)
			{
				return;
			}

			if (args.Player == null)
			{
				args.Handled = true;
				return;
			}

			TSPlayer tsplr = Players[args.Player.whoAmI];
			if (tsplr == null)
			{
				args.Handled = true;
				return;
			}

			if (args.Chest != null)
			{
				if (Config.RegionProtectChests && !Regions.CanBuild((int)args.WorldPosition.X, (int)args.WorldPosition.Y, tsplr))
				{
					args.Handled = true;
					return;
				}

				if (!tsplr.IsInRange(args.Chest.x, args.Chest.y))
				{
					args.Handled = true;
					return;
				}
			}
		}

		/// <summary>OnXmasCheck - Internal hook fired when the XMasCheck happens.</summary>
		/// <param name="args">args - The ChristmasCheckEventArgs object.</param>
		private void OnXmasCheck(ChristmasCheckEventArgs args)
		{
			if (args.Handled)
				return;

			if (Config.ForceXmas)
			{
				args.Xmas = true;
				args.Handled = true;
			}
		}

		/// <summary>OnHalloweenCheck - Internal hook fired when the HalloweenCheck happens.</summary>
		/// <param name="args">args - The HalloweenCheckEventArgs object.</param>
		private void OnHalloweenCheck(HalloweenCheckEventArgs args)
		{
			if (args.Handled)
				return;

			if (Config.ForceHalloween)
			{
				args.Halloween = true;
				args.Handled = true;
			}
		}

		/// <summary>
		/// Handles exceptions that we didn't catch earlier in the code, or in Terraria.
		/// </summary>
		/// <param name="sender">sender - The object that sent the exception.</param>
		/// <param name="e">e - The UnhandledExceptionEventArgs object.</param>
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Error(e.ExceptionObject.ToString());

			if (e.ExceptionObject.ToString().Contains("Terraria.Netplay.ListenForClients") ||
				e.ExceptionObject.ToString().Contains("Terraria.Netplay.ServerLoop"))
			{
				var sb = new List<string>();
				for (int i = 0; i < Netplay.Clients.Length; i++)
				{
					if (Netplay.Clients[i] == null)
					{
						sb.Add("Client[" + i + "]");
					}
					else if (Netplay.Clients[i].Socket == null)
					{
						sb.Add("Tcp[" + i + "]");
					}
				}
				Log.Error(string.Join(", ", sb));
			}

			if (e.IsTerminating)
			{
				if (Main.worldPathName != null && Config.SaveWorldOnCrash)
				{
					Main.ActiveWorldFileData._path += ".crash";
					SaveManager.Instance.SaveWorld();
				}
			}
		}

		/// <summary>HandleCommandLine - Handles the command line parameters passed to the server.</summary>
		/// <param name="parms">parms - The array of arguments passed in through the command line.</param>
		private void HandleCommandLine(string[] parms)
		{
			string path = null;

			//Generic method for doing a path sanity check
			Action<string> pathChecker = (p) =>
			{
				if (!string.IsNullOrWhiteSpace(p) && p.IndexOfAny(Path.GetInvalidPathChars()) == -1)
				{
					path = p;
				}
			};

			//Prepare the parser with all the flags available
			CliParser
				.AddFlag("-configpath", pathChecker)
					//The .After Action is run after the pathChecker Action
					.After(() =>
					{
						SavePath = path ?? "tshock";
						if (path != null)
						{
							ServerApi.LogWriter.PluginWriteLine(this, "Config path has been set to " + path, TraceLevel.Info);
						}
					})

				.AddFlag("-worldpath", pathChecker)
					.After(() =>
					{
						if (path != null)
						{
							Main.WorldPath = path;
							ServerApi.LogWriter.PluginWriteLine(this, "World path has been set to " + path, TraceLevel.Info);
						}
					})

				.AddFlag("-logpath", pathChecker)
					.After(() =>
					{
						if (path != null)
						{
							LogPath = path;
							ServerApi.LogWriter.PluginWriteLine(this, "Log path has been set to " + path, TraceLevel.Info);
						}
					})

				.AddFlag("-logformat", (format) =>
					{
						if (!string.IsNullOrWhiteSpace(format)) { LogFormat = format; }
					})

				.AddFlag("-config", (cfg) =>
					{
						if (!string.IsNullOrWhiteSpace(cfg))
						{
							ServerApi.LogWriter.PluginWriteLine(this, string.Format("Loading dedicated config file: {0}", cfg), TraceLevel.Verbose);
							Main.instance.LoadDedConfig(cfg);
						}
					})

				.AddFlag("-port", (p) =>
					{
						int port;
						if (int.TryParse(p, out port))
						{
							Netplay.ListenPort = port;
							ServerApi.LogWriter.PluginWriteLine(this, string.Format("Listening on port {0}.", port), TraceLevel.Verbose);
						}
					})

				.AddFlag("-worldname", (world) =>
					{
						if (!string.IsNullOrWhiteSpace(world))
						{
							Main.instance.SetWorldName(world);
							ServerApi.LogWriter.PluginWriteLine(this, string.Format("World name will be overridden by: {0}", world), TraceLevel.Verbose);
						}
					})

				.AddFlag("-ip", (ip) =>
					{
						IPAddress addr;
						if (IPAddress.TryParse(ip, out addr))
						{
							Netplay.ServerIP = addr;
							ServerApi.LogWriter.PluginWriteLine(this, string.Format("Listening on IP {0}.", addr), TraceLevel.Verbose);
						}
						else
						{
							// The server should not start up if this argument is invalid.
							throw new InvalidOperationException("Invalid value given for command line argument \"-ip\".");
						}
					})

				.AddFlag("-autocreate", (size) =>
					{
						if (!string.IsNullOrWhiteSpace(size))
						{
							Main.instance.autoCreate(size);
						}
					})


				//Flags without arguments
				.AddFlag("-logclear", () => LogClear = true)
				.AddFlag("-autoshutdown", () => Main.instance.EnableAutoShutdown())
				.AddFlag("-dump", () => Utils.Dump());

			CliParser.ParseFromSource(parms);
		}

		/// <summary>HandleCommandLinePostConfigLoad - Handles additional command line options after the config file is read.</summary>
		/// <param name="parms">parms - The array of arguments passed in through the command line.</param>
		public static void HandleCommandLinePostConfigLoad(string[] parms)
		{
			FlagSet portSet = new FlagSet("-port");
			FlagSet playerSet = new FlagSet("-maxplayers", "-players");

			CliParser
				.AddFlags(portSet, (p) =>
					{
						int port;
						if (int.TryParse(p, out port))
						{
							Netplay.ListenPort = port;
							Config.ServerPort = port;
							OverridePort = true;
							Log.ConsoleInfo("Port overridden by startup argument. Set to " + port);
						}
					})
				.AddFlags(playerSet, (p)=>
					{
						int slots;
						if (int.TryParse(p, out slots))
						{
							Config.MaxSlots = slots;
							Console.WriteLine("Startup parameter overrode maximum player slot configuration value.");
						}
					});

			CliParser.ParseFromSource(parms);
		}

		/// <summary>SetupToken - The auth token used by the setup system to grant temporary superadmin access to new admins.</summary>
		public static int SetupToken = -1;
		private string _cliPassword = null;

		/// <summary>OnPostInit - Fired when the server loads a map, to perform world specific operations.</summary>
		/// <param name="args">args - The EventArgs object.</param>
		private void OnPostInit(EventArgs args)
		{
			Utils.SetConsoleTitle(false);

			//This is to prevent a bug where a CLI-defined password causes packets to be
			//sent in an unexpected order, resulting in clients being unable to connect
			if (!string.IsNullOrEmpty(Netplay.ServerPassword))
			{
				//CLI defined password overrides a config password
				_cliPassword = Netplay.ServerPassword;
				Netplay.ServerPassword = "";
				Config.ServerPassword = _cliPassword;
			}

			// Disable the auth system if "setup.lock" is present or a user account already exists
			if (File.Exists(Path.Combine(SavePath, "setup.lock")) || (UserAccounts.GetUserAccounts().Count() > 0))
			{
				SetupToken = 0;

				if (File.Exists(Path.Combine(SavePath, "setup-code.txt")))
				{
					Log.ConsoleInfo("A superadmin account has been detected in the user database, but setup-code.txt is still present.");
					Log.ConsoleInfo("TShock will now disable the initial setup system and remove setup-code.txt as it is no longer needed.");
					File.Delete(Path.Combine(SavePath, "setup-code.txt"));
				}

				if (!File.Exists(Path.Combine(SavePath, "setup.lock")))
				{
					// This avoids unnecessary database work, which can get ridiculously high on old servers as all users need to be fetched
					File.Create(Path.Combine(SavePath, "setup.lock"));
				}
			}
			else if (!File.Exists(Path.Combine(SavePath, "setup-code.txt")))
			{
				var r = new Random((int)DateTime.Now.ToBinary());
				SetupToken = r.Next(100000, 10000000);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("To setup the server, join the game and type {0}setup {1}", Commands.Specifier, SetupToken);
				Console.WriteLine("This token will display until disabled by verification. ({0}setup)", Commands.Specifier);
				Console.ResetColor();
				File.WriteAllText(Path.Combine(SavePath, "setup-code.txt"), SetupToken.ToString());
			}
			else
			{
				SetupToken = Convert.ToInt32(File.ReadAllText(Path.Combine(SavePath, "setup-code.txt")));
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("TShock Notice: setup-code.txt is still present, and the code located in that file will be used.");
				Console.WriteLine("To setup the server, join the game and type {0}setup {1}", Commands.Specifier, SetupToken);
				Console.WriteLine("This token will display until disabled by verification. ({0}setup)", Commands.Specifier);
				Console.ResetColor();
			}

			Regions.Reload();
			Warps.ReloadWarps();

			Utils.ComputeMaxStyles();
			Utils.FixChestStacks();

			if (Config.UseServerName)
			{
				Main.worldName = Config.ServerName;
			}

			UpdateManager = new UpdateManager();
		}

		/// <summary>LastCheck - Used to keep track of the last check for basically all time based checks.</summary>
		private DateTime LastCheck = DateTime.UtcNow;

		/// <summary>LastSave - Used to keep track of SSC save intervals.</summary>
		private DateTime LastSave = DateTime.UtcNow;

		/// <summary>OnUpdate - Called when ever the server ticks.</summary>
		/// <param name="args">args - EventArgs args</param>
		private void OnUpdate(EventArgs args)
		{
			if (Backups.IsBackupTime)
				Backups.Backup();
			//call these every second, not every update
			if ((DateTime.UtcNow - LastCheck).TotalSeconds >= 1)
			{
				OnSecondUpdate();
				LastCheck = DateTime.UtcNow;
			}

			if (Main.ServerSideCharacter && (DateTime.UtcNow - LastSave).TotalMinutes >= ServerSideCharacterConfig.ServerSideCharacterSave)
			{
				foreach (TSPlayer player in Players)
				{
					// prevent null point exceptions
					if (player != null && player.IsLoggedIn && !player.IsDisabledPendingTrashRemoval)
					{

						CharacterDB.InsertPlayerData(player);
					}
				}
				LastSave = DateTime.UtcNow;
			}
		}

		/// <summary>OnSecondUpdate - Called effectively every second for all time based checks.</summary>
		private void OnSecondUpdate()
		{
			DisableFlags flags = Config.DisableSecondUpdateLogs ? DisableFlags.WriteToConsole : DisableFlags.WriteToLogAndConsole;

			if (Config.ForceTime != "normal")
			{
				switch (Config.ForceTime)
				{
					case "day":
						TSPlayer.Server.SetTime(true, 27000.0);
						break;
					case "night":
						TSPlayer.Server.SetTime(false, 16200.0);
						break;
				}
			}

			foreach (TSPlayer player in Players)
			{
				if (player != null && player.Active)
				{
					if (player.TilesDestroyed != null)
					{
						if (player.TileKillThreshold >= Config.TileKillThreshold)
						{
							player.Disable("Reached TileKill threshold.", flags);
							TSPlayer.Server.RevertTiles(player.TilesDestroyed);
							player.TilesDestroyed.Clear();
						}
					}
					if (player.TileKillThreshold > 0)
					{
						player.TileKillThreshold = 0;
						//We don't want to revert the entire map in case of a disable.
						lock (player.TilesDestroyed)
							player.TilesDestroyed.Clear();
					}

					if (player.TilesCreated != null)
					{
						if (player.TilePlaceThreshold >= Config.TilePlaceThreshold)
						{
							player.Disable("Reached TilePlace threshold", flags);
							lock (player.TilesCreated)
							{
								TSPlayer.Server.RevertTiles(player.TilesCreated);
								player.TilesCreated.Clear();
							}
						}
					}
					if (player.TilePlaceThreshold > 0)
					{
						player.TilePlaceThreshold = 0;
					}

					if (player.RecentFuse > 0)
						player.RecentFuse--;

					if ((Main.ServerSideCharacter) && (player.TPlayer.SpawnX > 0) && (player.sX != player.TPlayer.SpawnX))
					{
						player.sX = player.TPlayer.SpawnX;
						player.sY = player.TPlayer.SpawnY;
					}

					if ((Main.ServerSideCharacter) && (player.sX > 0) && (player.sY > 0) && (player.TPlayer.SpawnX < 0))
					{
						player.TPlayer.SpawnX = player.sX;
						player.TPlayer.SpawnY = player.sY;
					}

					if (player.RPPending > 0)
					{
						if (player.RPPending == 1)
						{
							var pos = RememberedPos.GetLeavePos(player.Name, player.IP);
							player.Teleport(pos.X * 16, pos.Y * 16);
							player.RPPending = 0;
						}
						else
						{
							player.RPPending--;
						}
					}

					if (player.TileLiquidThreshold >= Config.TileLiquidThreshold)
					{
						player.Disable("Reached TileLiquid threshold", flags);
					}
					if (player.TileLiquidThreshold > 0)
					{
						player.TileLiquidThreshold = 0;
					}

					if (player.ProjectileThreshold >= Config.ProjectileThreshold)
					{
						player.Disable("Reached projectile threshold", flags);
					}
					if (player.ProjectileThreshold > 0)
					{
						player.ProjectileThreshold = 0;
					}

					if (player.PaintThreshold >= Config.TilePaintThreshold)
					{
						player.Disable("Reached paint threshold", flags);
					}
					if (player.PaintThreshold > 0)
					{
						player.PaintThreshold = 0;
					}

					if (player.HealOtherThreshold >= TShock.Config.HealOtherThreshold)
					{
						player.Disable("Reached HealOtherPlayer threshold", flags);
					}
					if (player.HealOtherThreshold > 0)
					{
						player.HealOtherThreshold = 0;
					}

					if (player.RespawnTimer > 0 && --player.RespawnTimer == 0 && player.Difficulty != 2)
					{
						player.Spawn();
					}

					if (Main.ServerSideCharacter && !player.IsLoggedIn)
					{
						if (player.IsBeingDisabled())
						{
							player.Disable(flags: flags);
						}
						else if (Itembans.ItemIsBanned(EnglishLanguage.GetItemNameById(player.TPlayer.inventory[player.TPlayer.selectedItem].netID), player))
						{
							player.Disable($"holding banned item: {player.TPlayer.inventory[player.TPlayer.selectedItem].Name}", flags);
							player.SendErrorMessage($"You are holding a banned item: {player.TPlayer.inventory[player.TPlayer.selectedItem].Name}");
						}
					}
					else if (!Main.ServerSideCharacter || (Main.ServerSideCharacter && player.IsLoggedIn))
					{
						if (!player.HasPermission(Permissions.ignorestackhackdetection))
						{
							player.IsDisabledForStackDetection = player.HasHackedItemStacks(shouldWarnPlayer: true);
						}
						string check = "none";
						// Please don't remove this for the time being; without it, players wearing banned equipment will only get debuffed once
						foreach (Item item in player.TPlayer.armor)
						{
							if (Itembans.ItemIsBanned(EnglishLanguage.GetItemNameById(item.type), player))
							{
								player.SetBuff(BuffID.Frozen, 330, true);
								player.SetBuff(BuffID.Stoned, 330, true);
								player.SetBuff(BuffID.Webbed, 330, true);
								check = "Remove armor/accessory " + item.Name;

								player.SendErrorMessage("You are wearing banned equipment. {0}", check);
								break;
							}
						}
						foreach (Item item in player.TPlayer.dye)
						{
							if (Itembans.ItemIsBanned(EnglishLanguage.GetItemNameById(item.type), player))
							{
								player.SetBuff(BuffID.Frozen, 330, true);
								player.SetBuff(BuffID.Stoned, 330, true);
								player.SetBuff(BuffID.Webbed, 330, true);
								check = "Remove dye " + item.Name;

								player.SendErrorMessage("You are wearing banned equipment. {0}", check);
								break;
							}
						}
						foreach (Item item in player.TPlayer.miscEquips)
						{
							if (Itembans.ItemIsBanned(EnglishLanguage.GetItemNameById(item.type), player))
							{
								player.SetBuff(BuffID.Frozen, 330, true);
								player.SetBuff(BuffID.Stoned, 330, true);
								player.SetBuff(BuffID.Webbed, 330, true);
								check = "Remove misc equip " + item.Name;

								player.SendErrorMessage("You are wearing banned equipment. {0}", check);
								break;
							}
						}
						foreach (Item item in player.TPlayer.miscDyes)
						{
							if (Itembans.ItemIsBanned(EnglishLanguage.GetItemNameById(item.type), player))
							{
								player.SetBuff(BuffID.Frozen, 330, true);
								player.SetBuff(BuffID.Stoned, 330, true);
								player.SetBuff(BuffID.Webbed, 330, true);
								check = "Remove misc dye " + item.Name;

								player.SendErrorMessage("You are wearing banned equipment. {0}", check);
								break;
							}
						}
						if (check != "none")
							player.IsDisabledForBannedWearable = true;

						if (player.IsBeingDisabled())
						{
							player.Disable(flags: flags);
						}
						else if (Itembans.ItemIsBanned(EnglishLanguage.GetItemNameById(player.TPlayer.inventory[player.TPlayer.selectedItem].netID), player))
						{
							player.Disable($"holding banned item: {player.TPlayer.inventory[player.TPlayer.selectedItem].Name}", flags);
							player.SendErrorMessage($"You are holding a banned item: {player.TPlayer.inventory[player.TPlayer.selectedItem].Name}");
						}
					}
				}
			}
			Utils.SetConsoleTitle(false);
		}

		/// <summary>OnHardUpdate - Fired when a hardmode tile update event happens.</summary>
		/// <param name="args">args - The HardmodeTileUpdateEventArgs object.</param>
		private void OnHardUpdate(HardmodeTileUpdateEventArgs args)
		{
			if (args.Handled)
				return;

			if (!OnCreep(args.Type))
			{
				args.Handled = true;
			}
		}

		/// <summary>OnWorldGrassSpread - Fired when grass is attempting to spread.</summary>
		/// <param name="args">args - The GrassSpreadEventArgs object.</param>
		private void OnWorldGrassSpread(GrassSpreadEventArgs args)
		{
			if (args.Handled)
				return;

			if(!OnCreep(args.Grass))
			{
				args.Handled = true;
			}
		}

		/// <summary>
		/// Checks if the tile type is allowed to creep
		/// </summary>
		/// <param name="tileType">Tile id</param>
		/// <returns>True if allowed, otherwise false</returns>
		private bool OnCreep(int tileType)
		{
			if (!Config.AllowCrimsonCreep && (tileType == TileID.Dirt || tileType == TileID.FleshWeeds
				|| TileID.Sets.Crimson[tileType]))
			{
				return false;
			}

			if (!Config.AllowCorruptionCreep && (tileType == TileID.Dirt || tileType == TileID.CorruptThorns
				|| TileID.Sets.Corrupt[tileType]))
			{
				return false;
			}

			if (!Config.AllowHallowCreep && (TileID.Sets.Hallow[tileType]))
			{
				return false;
			}

			return true;
		}

		/// <summary>OnStatueSpawn - Fired when a statue spawns.</summary>
		/// <param name="args">args - The StatueSpawnEventArgs object.</param>
		private void OnStatueSpawn(StatueSpawnEventArgs args)
		{
			if (args.Within200 < Config.StatueSpawn200 && args.Within600 < Config.StatueSpawn600 && args.WorldWide < Config.StatueSpawnWorld)
			{
				args.Handled = true;
			}
			else
			{
				args.Handled = false;
			}
		}

		/// <summary>OnConnect - Fired when a player connects to the server.</summary>
		/// <param name="args">args - The ConnectEventArgs object.</param>
		private void OnConnect(ConnectEventArgs args)
		{
			if (ShuttingDown)
			{
				NetMessage.SendData((int)PacketTypes.Disconnect, args.Who, -1, NetworkText.FromLiteral("Server is shutting down..."));
				args.Handled = true;
				return;
			}

			var player = new TSPlayer(args.Who);

			if (Utils.GetActivePlayerCount() + 1 > Config.MaxSlots + Config.ReservedSlots)
			{
				player.Kick(Config.ServerFullNoReservedReason, true, true, null, false);
				args.Handled = true;
				return;
			}

			if (!FileTools.OnWhitelist(player.IP))
			{
				player.Kick(Config.WhitelistKickReason, true, true, null, false);
				args.Handled = true;
				return;
			}

			if (Geo != null)
			{
				var code = Geo.TryGetCountryCode(IPAddress.Parse(player.IP));
				player.Country = code == null ? "N/A" : GeoIPCountry.GetCountryNameByCode(code);
				if (code == "A1")
				{
					if (Config.KickProxyUsers)
					{
						player.Kick("Connecting via a proxy is not allowed.", true, true, null, false);
						args.Handled = true;
						return;
					}
				}
			}
			Players[args.Who] = player;
		}

		/// <summary>OnJoin - Internal hook called when a player joins. This is called after OnConnect.</summary>
		/// <param name="args">args - The JoinEventArgs object.</param>
		private void OnJoin(JoinEventArgs args)
		{
			var player = Players[args.Who];
			if (player == null)
			{
				args.Handled = true;
				return;
			}

			if (Config.KickEmptyUUID && String.IsNullOrWhiteSpace(player.UUID))
			{
				player.Kick("Your client sent a blank UUID. Configure it to send one or use a different client.", true, true, null, false);
				args.Handled = true;
				return;
			}

			Ban ban = null;
			if (Config.EnableBanOnUsernames)
			{
				var newban = Bans.GetBanByName(player.Name);
				if (null != newban)
					ban = newban;
			}

			if (Config.EnableIPBans && null == ban)
			{
				ban = Bans.GetBanByIp(player.IP);
			}

			if (Config.EnableUUIDBans && null == ban && !String.IsNullOrWhiteSpace(player.UUID))
			{
				ban = Bans.GetBanByUUID(player.UUID);
			}

			if (ban != null)
			{
				if (!Bans.RemoveBanIfExpired(ban))
				{
					DateTime exp;
					if (!DateTime.TryParse(ban.Expiration, out exp))
					{
						player.Disconnect("Permanently banned for: " + ban.Reason);
					}
					else
					{
						TimeSpan ts = exp - DateTime.UtcNow;
						int months = ts.Days / 30;
						if (months > 0)
						{
							player.Disconnect(String.Format("You are banned for {0} month{1} and {2} day{3}: {4}",
								months, months == 1 ? "" : "s", ts.Days, ts.Days == 1 ? "" : "s", ban.Reason));
						}
						else if (ts.Days > 0)
						{
							player.Disconnect(String.Format("You are banned for {0} day{1} and {2} hour{3}: {4}",
								ts.Days, ts.Days == 1 ? "" : "s", ts.Hours, ts.Hours == 1 ? "" : "s", ban.Reason));
						}
						else if (ts.Hours > 0)
						{
							player.Disconnect(String.Format("You are banned for {0} hour{1} and {2} minute{3}: {4}",
								ts.Hours, ts.Hours == 1 ? "" : "s", ts.Minutes, ts.Minutes == 1 ? "" : "s", ban.Reason));
						}
						else if (ts.Minutes > 0)
						{
							player.Disconnect(String.Format("You are banned for {0} minute{1} and {2} second{3}: {4}",
								ts.Minutes, ts.Minutes == 1 ? "" : "s", ts.Seconds, ts.Seconds == 1 ? "" : "s", ban.Reason));
						}
						else
						{
							player.Disconnect(String.Format("You are banned for {0} second{1}: {2}",
								ts.Seconds, ts.Seconds == 1 ? "" : "s", ban.Reason));
						}
					}
					args.Handled = true;
				}
			}
		}

		/// <summary>OnLeave - Called when a player leaves the server.</summary>
		/// <param name="args">args - The LeaveEventArgs object.</param>
		private void OnLeave(LeaveEventArgs args)
		{
			if (args.Who >= Players.Length || args.Who < 0)
			{
				//Something not right has happened
				return;
			}

			var tsplr = Players[args.Who];
			if (tsplr == null)
			{
				return;
			}

			Players[args.Who] = null;

			if (tsplr.ReceivedInfo)
			{
				if (!tsplr.SilentKickInProgress && tsplr.State >= 3)
					Utils.Broadcast(tsplr.Name + " has left.", Color.Yellow);
				Log.Info("{0} disconnected.", tsplr.Name);

				if (tsplr.IsLoggedIn && !tsplr.IsDisabledPendingTrashRemoval && Main.ServerSideCharacter && (!tsplr.Dead || tsplr.TPlayer.difficulty != 2))
				{
					tsplr.PlayerData.CopyCharacter(tsplr);
					CharacterDB.InsertPlayerData(tsplr);
				}

				if (Config.RememberLeavePos && !tsplr.LoginHarassed)
				{
					RememberedPos.InsertLeavePos(tsplr.Name, tsplr.IP, (int)(tsplr.X / 16), (int)(tsplr.Y / 16));
				}

				if (tsplr.tempGroupTimer != null)
				{
					tsplr.tempGroupTimer.Stop();
				}
			}

			// Fire the OnPlayerLogout hook too, if the player was logged in and they have a TSPlayer object.
			if (tsplr.IsLoggedIn)
			{
				Hooks.PlayerHooks.OnPlayerLogout(tsplr);
			}

			// The last player will leave after this hook is executed.
			if (Utils.GetActivePlayerCount() == 1)
			{
				if (Config.SaveWorldOnLastPlayerExit)
					SaveManager.Instance.SaveWorld();
				Utils.SetConsoleTitle(true);
			}
		}

		/// <summary>OnChat - Fired when a player chats. Used for handling chat and commands.</summary>
		/// <param name="args">args - The ServerChatEventArgs object.</param>
		private void OnChat(ServerChatEventArgs args)
		{
			if (args.Handled)
				return;

			var tsplr = Players[args.Who];
			if (tsplr == null)
			{
				args.Handled = true;
				return;
			}

			if (args.Text.Length > 500)
			{
				tsplr.Kick("Crash attempt via long chat packet.", true);
				args.Handled = true;
				return;
			}

			string text = args.Text;

			// Terraria now has chat commands on the client side.
			// These commands remove the commands prefix (e.g. /me /playing) and send the command id instead
			// In order for us to keep legacy code we must reverse this and get the prefix using the command id
			foreach (var item in Terraria.UI.Chat.ChatManager.Commands._localizedCommands)
			{
				if (item.Value._name == args.CommandId._name)
				{
					if (!String.IsNullOrEmpty(text))
					{
						text = item.Key.Value + ' ' + text;
					}
					else
					{
						text = item.Key.Value;
					}
					break;
				}
			}

			if ((text.StartsWith(Config.CommandSpecifier) || text.StartsWith(Config.CommandSilentSpecifier))
				&& !string.IsNullOrWhiteSpace(text.Substring(1)))
			{
				try
				{
					args.Handled = true;
					if (!Commands.HandleCommand(tsplr, text))
					{
						// This is required in case anyone makes HandleCommand return false again
						tsplr.SendErrorMessage("Unable to parse command. Please contact an administrator for assistance.");
						Log.ConsoleError("Unable to parse command '{0}' from player {1}.", text, tsplr.Name);
					}
				}
				catch (Exception ex)
				{
					Log.ConsoleError("An exception occurred executing a command.");
					Log.Error(ex.ToString());
				}
			}
			else
			{
				if (!tsplr.HasPermission(Permissions.canchat))
				{
					args.Handled = true;
				}
				else if (tsplr.mute)
				{
					tsplr.SendErrorMessage("You are muted!");
					args.Handled = true;
				}
				else if (!TShock.Config.EnableChatAboveHeads)
				{
					text = String.Format(Config.ChatFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix,
											 args.Text);
					Hooks.PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);
					Utils.Broadcast(text, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
					args.Handled = true;
				}
				else
				{
					Player ply = Main.player[args.Who];
					string name = ply.name;
					ply.name = String.Format(Config.ChatAboveHeadsFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix);
					//Update the player's name to format text nicely. This needs to be done because Terraria automatically formats messages against our will
					NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, NetworkText.FromLiteral(ply.name), args.Who, 0, 0, 0, 0);

					//Give that poor player their name back :'c
					ply.name = name;
					PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);

					//This netpacket is used to send chat text from the server to clients, in this case on behalf of a client
					Terraria.Net.NetPacket packet = Terraria.GameContent.NetModules.NetTextModule.SerializeServerMessage(
						NetworkText.FromLiteral(text), new Color(tsplr.Group.R, tsplr.Group.G, tsplr.Group.B), (byte)args.Who
					);
					//Broadcast to everyone except the player who sent the message.
					//This is so that we can send them the same nicely formatted message that everyone else gets
					Terraria.Net.NetManager.Instance.Broadcast(packet, args.Who);

					//Reset their name
					NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, NetworkText.FromLiteral(name), args.Who, 0, 0, 0, 0);

					string msg = String.Format("<{0}> {1}",
						String.Format(Config.ChatAboveHeadsFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix),
						text
					);

					//Send the original sender their nicely formatted message, and do all the loggy things
					tsplr.SendMessage(msg, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
					TSPlayer.Server.SendMessage(msg, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
					Log.Info("Broadcast: {0}", msg);
					args.Handled = true;
				}
			}
		}

		/// <summary>
		/// Called when a command is issued from the server console.
		/// </summary>
		/// <param name="args">The CommandEventArgs object</param>
		private void ServerHooks_OnCommand(CommandEventArgs args)
		{
			if (args.Handled)
				return;

			if (string.IsNullOrWhiteSpace(args.Command))
			{
				args.Handled = true;
				return;
			}

			// Damn you ThreadStatic and Redigit
			if (Main.rand == null)
			{
				Main.rand = new UnifiedRandom();
			}

			if (args.Command == "autosave")
			{
				Main.autoSave = Config.AutoSave = !Config.AutoSave;
				Log.ConsoleInfo("AutoSave " + (Config.AutoSave ? "Enabled" : "Disabled"));
			}
			else if (args.Command.StartsWith(Commands.Specifier) || args.Command.StartsWith(Commands.SilentSpecifier))
			{
				Commands.HandleCommand(TSPlayer.Server, args.Command);
			}
			else
			{
				Commands.HandleCommand(TSPlayer.Server, "/" + args.Command);
			}
			args.Handled = true;
		}

		/// <summary>OnGetData - Called when the server gets raw data packets.</summary>
		/// <param name="e">e - The GetDataEventArgs object.</param>
		private void OnGetData(GetDataEventArgs e)
		{
			if (e.Handled)
				return;

			PacketTypes type = e.MsgID;

			Debug.WriteLine("Recv: {0:X}: {2} ({1:XX})", e.Msg.whoAmI, (byte)type, type);

			var player = Players[e.Msg.whoAmI];
			if (player == null || !player.ConnectionAlive)
			{
				e.Handled = true;
				return;
			}

			if (player.RequiresPassword && type != PacketTypes.PasswordSend)
			{
				e.Handled = true;
				return;
			}

			if ((player.State < 10 || player.Dead) && (int)type > 12 && (int)type != 16 && (int)type != 42 && (int)type != 50 &&
				(int)type != 38 && (int)type != 21 && (int)type != 22)
			{
				e.Handled = true;
				return;
			}

			int length = e.Length - 1;
			if (length < 0)
			{
				length = 0;
			}
			using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length - 1))
			{
				// Exceptions are already handled
				e.Handled = GetDataHandlers.HandlerGetData(type, player, data);
			}
		}

		/// <summary>OnGreetPlayer - Fired when a player is greeted by the server. Handles things like the MOTD, join messages, etc.</summary>
		/// <param name="args">args - The GreetPlayerEventArgs object.</param>
		private void OnGreetPlayer(GreetPlayerEventArgs args)
		{
			var player = Players[args.Who];
			if (player == null)
			{
				args.Handled = true;
				return;
			}

			player.LoginMS = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

			if (Config.EnableGeoIP && TShock.Geo != null)
			{
				Log.Info("{0} ({1}) from '{2}' group from '{3}' joined. ({4}/{5})", player.Name, player.IP,
									   player.Group.Name, player.Country, TShock.Utils.GetActivePlayerCount(),
									   TShock.Config.MaxSlots);
				if (!player.SilentJoinInProgress)
					Utils.Broadcast(string.Format("{0} ({1}) has joined.", player.Name, player.Country), Color.Yellow);
			}
			else
			{
				Log.Info("{0} ({1}) from '{2}' group joined. ({3}/{4})", player.Name, player.IP,
									   player.Group.Name, TShock.Utils.GetActivePlayerCount(), TShock.Config.MaxSlots);
				if (!player.SilentJoinInProgress)
					Utils.Broadcast(player.Name + " has joined.", Color.Yellow);
			}

			if (Config.DisplayIPToAdmins)
				Utils.SendLogs(string.Format("{0} has joined. IP: {1}", player.Name, player.IP), Color.Blue);

			player.SendFileTextAsMessage(FileTools.MotdPath);

			string pvpMode = Config.PvPMode.ToLowerInvariant();
			if (pvpMode == "always")
			{
				player.TPlayer.hostile = true;
				player.SendData(PacketTypes.TogglePvp, "", player.Index);
				TSPlayer.All.SendData(PacketTypes.TogglePvp, "", player.Index);
			}

			if (!player.IsLoggedIn)
			{
				if (Main.ServerSideCharacter)
				{
					player.IsDisabledForSSC = true;
					player.SendErrorMessage(String.Format("Server side characters is enabled! Please {0}register or {0}login to play!", Commands.Specifier));
					player.LoginHarassed = true;
				}
				else if (Config.RequireLogin)
				{
					player.SendErrorMessage("Please {0}register or {0}login to play!", Commands.Specifier);
					player.LoginHarassed = true;
				}
			}

			player.LastNetPosition = new Vector2(Main.spawnTileX * 16f, Main.spawnTileY * 16f);

			if (Config.RememberLeavePos && (RememberedPos.GetLeavePos(player.Name, player.IP) != Vector2.Zero) && !player.LoginHarassed)
			{
				player.RPPending = 3;
				player.SendInfoMessage("You will be teleported to your last known location...");
			}

			args.Handled = true;
		}

		/// <summary>NpcHooks_OnStrikeNpc - Fired when an NPC strike packet happens.</summary>
		/// <param name="e">e - The NpcStrikeEventArgs object.</param>
		private void NpcHooks_OnStrikeNpc(NpcStrikeEventArgs e)
		{
			if (Config.InfiniteInvasion)
			{
				if (Main.invasionSize < 10)
				{
					Main.invasionSize = 20000000;
				}
			}
		}

		/// <summary>OnProjectileSetDefaults - Called when a projectile sets the default attributes for itself.</summary>
		/// <param name="e">e - The SetDefaultsEventArgs object praameterized with Projectile and int.</param>
		private void OnProjectileSetDefaults(SetDefaultsEventArgs<Projectile, int> e)
		{
			//tombstone fix.
			if (e.Info == 43 || (e.Info >= 201 && e.Info <= 205) || (e.Info >= 527 && e.Info <= 531))
				if (Config.DisableTombstones)
					e.Object.SetDefaults(0);
			if (e.Info == 75)
				if (Config.DisableClownBombs)
					e.Object.SetDefaults(0);
			if (e.Info == 109)
				if (Config.DisableSnowBalls)
					e.Object.SetDefaults(0);
		}

		/// <summary>NetHooks_SendData - Fired when the server sends data.</summary>
		/// <param name="e">e - The SendDataEventArgs object.</param>
		private void NetHooks_SendData(SendDataEventArgs e)
		{
			if (e.MsgId == PacketTypes.PlayerHp)
			{
				if (Main.player[(byte)e.number].statLife <= 0)
				{
					e.Handled = true;
					return;
				}
			}
		}

		/// <summary>OnStartHardMode - Fired when hard mode is started.</summary>
		/// <param name="e">e - The HandledEventArgs object.</param>
		private void OnStartHardMode(HandledEventArgs e)
		{
			if (Config.DisableHardmode)
				e.Handled = true;
		}

		/// <summary>OnConfigRead - Fired when the config file has been read.</summary>
		/// <param name="file">file - The config file object.</param>
		public void OnConfigRead(ConfigFile file)
		{
			NPC.defaultMaxSpawns = file.DefaultMaximumSpawns;
			NPC.defaultSpawnRate = file.DefaultSpawnRate;

			Main.autoSave = file.AutoSave;
			if (Backups != null)
			{
				Backups.KeepFor = file.BackupKeepFor;
				Backups.Interval = file.BackupInterval;
			}
			if (!OverridePort)
			{
				Netplay.ListenPort = file.ServerPort;
			}

			if (file.MaxSlots > Main.maxPlayers - file.ReservedSlots)
				file.MaxSlots = Main.maxPlayers - file.ReservedSlots;
			Main.maxNetPlayers = file.MaxSlots + file.ReservedSlots;

			Netplay.ServerPassword = "";
			if (!string.IsNullOrEmpty(_cliPassword))
			{
				//This prevents a config reload from removing/updating a CLI-defined password
				file.ServerPassword = _cliPassword;
			}

			Netplay.spamCheck = false;
		}
	}
}
