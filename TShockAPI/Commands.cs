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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI.DB;
using TerrariaApi.Server;
using TShockAPI.Hooks;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using TShockAPI.Localization;
using System.Text.RegularExpressions;

namespace TShockAPI
{
	public delegate void CommandDelegate(CommandArgs args);

	public class CommandArgs : EventArgs
	{
		public string Message { get; private set; }
		public TSPlayer Player { get; private set; }
		public bool Silent { get; private set; }

		/// <summary>
		/// Parameters passed to the arguement. Does not include the command name.
		/// IE '/kick "jerk face"' will only have 1 argument
		/// </summary>
		public List<string> Parameters { get; private set; }

		public Player TPlayer
		{
			get { return Player.TPlayer; }
		}

		public CommandArgs(string message, TSPlayer ply, List<string> args)
		{
			Message = message;
			Player = ply;
			Parameters = args;
			Silent = false;
		}

		public CommandArgs(string message, bool silent, TSPlayer ply, List<string> args)
		{
			Message = message;
			Player = ply;
			Parameters = args;
			Silent = silent;
		}
	}

	public class Command
	{
		/// <summary>
		/// Gets or sets whether to allow non-players to use this command.
		/// </summary>
		public bool AllowServer { get; set; }
		/// <summary>
		/// Gets or sets whether to do logging of this command.
		/// </summary>
		public bool DoLog { get; set; }
		/// <summary>
		/// Gets or sets the help text of this command.
		/// </summary>
		public string HelpText { get; set; }
		/// <summary>
		/// Gets or sets an extended description of this command.
		/// </summary>
		public string[] HelpDesc { get; set; }
		/// <summary>
		/// Gets the name of the command.
		/// </summary>
		public string Name { get { return Names[0]; } }
		/// <summary>
		/// Gets the names of the command.
		/// </summary>
		public List<string> Names { get; protected set; }
		/// <summary>
		/// Gets the permissions of the command.
		/// </summary>
		public List<string> Permissions { get; protected set; }

		private CommandDelegate commandDelegate;
		public CommandDelegate CommandDelegate
		{
			get { return commandDelegate; }
			set
			{
				if (value == null)
					throw new ArgumentNullException();

				commandDelegate = value;
			}
		}

		public Command(List<string> permissions, CommandDelegate cmd, params string[] names)
			: this(cmd, names)
		{
			Permissions = permissions;
		}

		public Command(string permissions, CommandDelegate cmd, params string[] names)
			: this(cmd, names)
		{
			Permissions = new List<string> { permissions };
		}

		public Command(CommandDelegate cmd, params string[] names)
		{
			if (cmd == null)
				throw new ArgumentNullException("cmd");
			if (names == null || names.Length < 1)
				throw new ArgumentException("names");

			AllowServer = true;
			CommandDelegate = cmd;
			DoLog = true;
			HelpText = "No help available.";
			HelpDesc = null;
			Names = new List<string>(names);
			Permissions = new List<string>();
		}

		public bool Run(string msg, bool silent, TSPlayer ply, List<string> parms)
		{
			if (!CanRun(ply))
				return false;

			try
			{
				CommandDelegate(new CommandArgs(msg, silent, ply, parms));
			}
			catch (Exception e)
			{
				ply.SendErrorMessage("Command failed, check logs for more details.");
				TShock.Log.Error(e.ToString());
			}

			return true;
		}

		public bool Run(string msg, TSPlayer ply, List<string> parms)
		{
			return Run(msg, false, ply, parms);
		}

		public bool HasAlias(string name)
		{
			return Names.Contains(name);
		}

		public bool CanRun(TSPlayer ply)
		{
			if (Permissions == null || Permissions.Count < 1)
				return true;
			foreach (var Permission in Permissions)
			{
				if (ply.HasPermission(Permission))
					return true;
			}
			return false;
		}
	}

	public static class Commands
	{
		public static List<Command> ChatCommands = new List<Command>();
		public static ReadOnlyCollection<Command> TShockCommands = new ReadOnlyCollection<Command>(new List<Command>());

		/// <summary>
		/// The command specifier, defaults to "/"
		/// </summary>
		public static string Specifier
		{
			get { return string.IsNullOrWhiteSpace(TShock.Config.CommandSpecifier) ? "/" : TShock.Config.CommandSpecifier; }
		}

		/// <summary>
		/// The silent command specifier, defaults to "."
		/// </summary>
		public static string SilentSpecifier
		{
			get { return string.IsNullOrWhiteSpace(TShock.Config.CommandSilentSpecifier) ? "." : TShock.Config.CommandSilentSpecifier; }
		}

		private delegate void AddChatCommand(string permission, CommandDelegate command, params string[] names);

		public static void InitCommands()
		{
			List<Command> tshockCommands = new List<Command>(100);
			Action<Command> add = (cmd) =>
			{
				tshockCommands.Add(cmd);
				ChatCommands.Add(cmd);
			};

			add(new Command(SetupToken, "setup")
			{
				AllowServer = false,
				HelpText = "Used to authenticate as superadmin when first setting up TShock."
			});
			add(new Command(Permissions.user, ManageUsers, "user")
			{
				DoLog = false,
				HelpText = "Manages user accounts."
			});

			#region Account Commands
			add(new Command(Permissions.canlogin, AttemptLogin, "login")
			{
				AllowServer = false,
				DoLog = false,
				HelpText = "Logs you into an account."
			});
			add(new Command(Permissions.canlogout, Logout, "logout")
			{
				AllowServer = false,
				DoLog = false,
				HelpText = "Logs you out of your current account."
			});
			add(new Command(Permissions.canchangepassword, PasswordUser, "password")
			{
				AllowServer = false,
				DoLog = false,
				HelpText = "Changes your account's password."
			});
			add(new Command(Permissions.canregister, RegisterUser, "register")
			{
				AllowServer = false,
				DoLog = false,
				HelpText = "Registers you an account."
			});
			add(new Command(Permissions.checkaccountinfo, ViewAccountInfo, "accountinfo", "ai")
			{
				HelpText = "Shows information about a user."
			});
			#endregion
			#region Admin Commands
			add(new Command(Permissions.ban, Ban, "ban")
			{
				HelpText = "Manages player bans."
			});
			add(new Command(Permissions.broadcast, Broadcast, "broadcast", "bc", "say")
			{
				HelpText = "Broadcasts a message to everyone on the server."
			});
			add(new Command(Permissions.logs, DisplayLogs, "displaylogs")
			{
				HelpText = "Toggles whether you receive server logs."
			});
			add(new Command(Permissions.managegroup, Group, "group")
			{
				HelpText = "Manages groups."
			});
			add(new Command(Permissions.manageitem, ItemBan, "itemban")
			{
				HelpText = "Manages item bans."
			});
			add(new Command(Permissions.manageprojectile, ProjectileBan, "projban")
			{
				HelpText = "Manages projectile bans."
			});
			add(new Command(Permissions.managetile, TileBan, "tileban")
			{
				HelpText = "Manages tile bans."
			});
			add(new Command(Permissions.manageregion, Region, "region")
			{
				HelpText = "Manages regions."
			});
			add(new Command(Permissions.kick, Kick, "kick")
			{
				HelpText = "Removes a player from the server."
			});
			add(new Command(Permissions.mute, Mute, "mute", "unmute")
			{
				HelpText = "Prevents a player from talking."
			});
			add(new Command(Permissions.savessc, OverrideSSC, "overridessc", "ossc")
			{
				HelpText = "Overrides serverside characters for a player, temporarily."
			});
			add(new Command(Permissions.savessc, SaveSSC, "savessc")
			{
				HelpText = "Saves all serverside characters."
			});
			add(new Command(Permissions.uploaddata, UploadJoinData, "uploadssc")
			{
				HelpText = "Upload the account information when you joined the server as your Server Side Character data."
			});
			add(new Command(Permissions.settempgroup, TempGroup, "tempgroup")
			{
				HelpText = "Temporarily sets another player's group."
			});
			add(new Command(Permissions.su, SubstituteUser, "su")
			{
				HelpText = "Temporarily elevates you to Super Admin."
			});
			add(new Command(Permissions.su, SubstituteUserDo, "sudo")
			{
				HelpText = "Executes a command as the super admin."
			});
			add(new Command(Permissions.userinfo, GrabUserUserInfo, "userinfo", "ui")
			{
				HelpText = "Shows information about a player."
			});
			#endregion
			#region Annoy Commands
			add(new Command(Permissions.annoy, Annoy, "annoy")
			{
				HelpText = "Annoys a player for an amount of time."
			});
			add(new Command(Permissions.annoy, Confuse, "confuse")
			{
				HelpText = "Confuses a player for an amount of time."
			});
			add(new Command(Permissions.annoy, Rocket, "rocket")
			{
				HelpText = "Rockets a player upwards. Requires SSC."
			});
			add(new Command(Permissions.annoy, FireWork, "firework")
			{
				HelpText = "Spawns fireworks at a player."
			});
			#endregion
			#region Configuration Commands
			add(new Command(Permissions.maintenance, CheckUpdates, "checkupdates")
			{
				HelpText = "Checks for TShock updates."
			});
			add(new Command(Permissions.maintenance, Off, "off", "exit", "stop")
			{
				HelpText = "Shuts down the server while saving."
			});
			add(new Command(Permissions.maintenance, OffNoSave, "off-nosave", "exit-nosave", "stop-nosave")
			{
				HelpText = "Shuts down the server without saving."
			});
			add(new Command(Permissions.cfgreload, Reload, "reload")
			{
				HelpText = "Reloads the server configuration file."
			});
			add(new Command(Permissions.cfgpassword, ServerPassword, "serverpassword")
			{
				HelpText = "Changes the server password."
			});
			add(new Command(Permissions.maintenance, GetVersion, "version")
			{
				HelpText = "Shows the TShock version."
			});
			add(new Command(Permissions.whitelist, Whitelist, "whitelist")
			{
				HelpText = "Manages the server whitelist."
			});
			#endregion
			#region Item Commands
			add(new Command(Permissions.give, Give, "give", "g")
			{
				HelpText = "Gives another player an item."
			});
			add(new Command(Permissions.item, Item, "item", "i")
			{
				AllowServer = false,
				HelpText = "Gives yourself an item."
			});
			#endregion
			#region NPC Commands
			add(new Command(Permissions.butcher, Butcher, "butcher")
			{
				HelpText = "Kills hostile NPCs or NPCs of a certain type."
			});
			add(new Command(Permissions.renamenpc, RenameNPC, "renamenpc")
			{
				HelpText = "Renames an NPC."
			});
			add(new Command(Permissions.invade, Invade, "invade")
			{
				HelpText = "Starts an NPC invasion."
			});
			add(new Command(Permissions.maxspawns, MaxSpawns, "maxspawns")
			{
				HelpText = "Sets the maximum number of NPCs."
			});
			add(new Command(Permissions.spawnboss, SpawnBoss, "spawnboss", "sb")
			{
				AllowServer = false,
				HelpText = "Spawns a number of bosses around you."
			});
			add(new Command(Permissions.spawnmob, SpawnMob, "spawnmob", "sm")
			{
				AllowServer = false,
				HelpText = "Spawns a number of mobs around you."
			});
			add(new Command(Permissions.spawnrate, SpawnRate, "spawnrate")
			{
				HelpText = "Sets the spawn rate of NPCs."
			});
			add(new Command(Permissions.clearangler, ClearAnglerQuests, "clearangler")
			{
				HelpText = "Resets the list of users who have completed an angler quest that day."
			});
			#endregion
			#region TP Commands
			add(new Command(Permissions.home, Home, "home")
			{
				AllowServer = false,
				HelpText = "Sends you to your spawn point."
			});
			add(new Command(Permissions.spawn, Spawn, "spawn")
			{
				AllowServer = false,
				HelpText = "Sends you to the world's spawn point."
			});
			add(new Command(Permissions.tp, TP, "tp")
			{
				AllowServer = false,
				HelpText = "Teleports a player to another player."
			});
			add(new Command(Permissions.tpothers, TPHere, "tphere")
			{
				AllowServer = false,
				HelpText = "Teleports a player to yourself."
			});
			add(new Command(Permissions.tpnpc, TPNpc, "tpnpc")
			{
				AllowServer = false,
				HelpText = "Teleports you to an npc."
			});
			add(new Command(Permissions.tppos, TPPos, "tppos")
			{
				AllowServer = false,
				HelpText = "Teleports you to tile coordinates."
			});
			add(new Command(Permissions.getpos, GetPos, "pos")
			{
				AllowServer = false,
				HelpText = "Returns the user's or specified user's current position."
			});
			add(new Command(Permissions.tpallow, TPAllow, "tpallow")
			{
				AllowServer = false,
				HelpText = "Toggles whether other people can teleport you."
			});
			#endregion
			#region World Commands
			add(new Command(Permissions.toggleexpert, ToggleExpert, "expert", "expertmode")
			{
				HelpText = "Toggles expert mode."
			});
			add(new Command(Permissions.antibuild, ToggleAntiBuild, "antibuild")
			{
				HelpText = "Toggles build protection."
			});
			add(new Command(Permissions.bloodmoon, Bloodmoon, "bloodmoon")
			{
				HelpText = "Sets a blood moon."
			});
			add(new Command(Permissions.grow, Grow, "grow")
			{
				AllowServer = false,
				HelpText = "Grows plants at your location."
			});
			add(new Command(Permissions.dropmeteor, DropMeteor, "dropmeteor")
			{
				HelpText = "Drops a meteor somewhere in the world."
			});
			add(new Command(Permissions.eclipse, Eclipse, "eclipse")
			{
				HelpText = "Sets an eclipse."
			});
			add(new Command(Permissions.halloween, ForceHalloween, "forcehalloween")
			{
				HelpText = "Toggles halloween mode (goodie bags, pumpkins, etc)."
			});
			add(new Command(Permissions.xmas, ForceXmas, "forcexmas")
			{
				HelpText = "Toggles christmas mode (present spawning, santa, etc)."
			});
			add(new Command(Permissions.fullmoon, Fullmoon, "fullmoon")
			{
				HelpText = "Sets a full moon."
			});
			add(new Command(Permissions.hardmode, Hardmode, "hardmode")
			{
				HelpText = "Toggles the world's hardmode status."
			});
			add(new Command(Permissions.editspawn, ProtectSpawn, "protectspawn")
			{
				HelpText = "Toggles spawn protection."
			});
			add(new Command(Permissions.sandstorm, Sandstorm, "sandstorm")
			{
				HelpText = "Toggles sandstorms."
			});
			add(new Command(Permissions.rain, Rain, "rain")
			{
				HelpText = "Toggles the rain."
			});
			add(new Command(Permissions.worldsave, Save, "save")
			{
				HelpText = "Saves the world file."
			});
			add(new Command(Permissions.worldspawn, SetSpawn, "setspawn")
			{
				AllowServer = false,
				HelpText = "Sets the world's spawn point to your location."
			});
			add(new Command(Permissions.dungeonposition, SetDungeon, "setdungeon")
			{
				AllowServer = false,
				HelpText = "Sets the dungeon's position to your location."
			});
			add(new Command(Permissions.worldsettle, Settle, "settle")
			{
				HelpText = "Forces all liquids to update immediately."
			});
			add(new Command(Permissions.time, Time, "time")
			{
				HelpText = "Sets the world time."
			});
			add(new Command(Permissions.wind, Wind, "wind")
			{
				HelpText = "Changes the wind speed."
			});
			add(new Command(Permissions.worldinfo, WorldInfo, "world")
			{
				HelpText = "Shows information about the current world."
			});
			#endregion
			#region Other Commands
			add(new Command(Permissions.buff, Buff, "buff")
			{
				AllowServer = false,
				HelpText = "Gives yourself a buff for an amount of time."
			});
			add(new Command(Permissions.clear, Clear, "clear")
			{
				HelpText = "Clears item drops or projectiles."
			});
			add(new Command(Permissions.buffplayer, GBuff, "gbuff", "buffplayer")
			{
				HelpText = "Gives another player a buff for an amount of time."
			});
			add(new Command(Permissions.godmode, ToggleGodMode, "godmode")
			{
				HelpText = "Toggles godmode on a player."
			});
			add(new Command(Permissions.heal, Heal, "heal")
			{
				HelpText = "Heals a player in HP and MP."
			});
			add(new Command(Permissions.kill, Kill, "kill")
			{
				HelpText = "Kills another player."
			});
			add(new Command(Permissions.cantalkinthird, ThirdPerson, "me")
			{
				HelpText = "Sends an action message to everyone."
			});
			add(new Command(Permissions.canpartychat, PartyChat, "party", "p")
			{
				AllowServer = false,
				HelpText = "Sends a message to everyone on your team."
			});
			add(new Command(Permissions.whisper, Reply, "reply", "r")
			{
				HelpText = "Replies to a PM sent to you."
			});
			add(new Command(Permissions.slap, Slap, "slap")
			{
				HelpText = "Slaps a player, dealing damage."
			});
			add(new Command(Permissions.serverinfo, ServerInfo, "serverinfo")
			{
				HelpText = "Shows the server information."
			});
			add(new Command(Permissions.warp, Warp, "warp")
			{
				HelpText = "Teleports you to a warp point or manages warps."
			});
			add(new Command(Permissions.whisper, Whisper, "whisper", "w", "tell")
			{
				HelpText = "Sends a PM to a player."
			});
			add(new Command(Permissions.createdumps, CreateDumps, "dump-reference-data")
			{
				HelpText = "Creates a reference tables for Terraria data types and the TShock permission system in the server folder."
			});
			#endregion

			add(new Command(Aliases, "aliases")
			{
				HelpText = "Shows a command's aliases."
			});
			add(new Command(Help, "help")
			{
				HelpText = "Lists commands or gives help on them."
			});
			add(new Command(Motd, "motd")
			{
				HelpText = "Shows the message of the day."
			});
			add(new Command(ListConnectedPlayers, "playing", "online", "who")
			{
				HelpText = "Shows the currently connected players."
			});
			add(new Command(Rules, "rules")
			{
				HelpText = "Shows the server's rules."
			});

			TShockCommands = new ReadOnlyCollection<Command>(tshockCommands);
		}

		public static bool HandleCommand(TSPlayer player, string text)
		{
			string cmdText = text.Remove(0, 1);
			string cmdPrefix = text[0].ToString();
			bool silent = false;

			if (cmdPrefix == SilentSpecifier)
				silent = true;

			int index = -1;
			for (int i = 0; i < cmdText.Length; i++)
			{
				if (IsWhiteSpace(cmdText[i]))
				{
					index = i;
					break;
				}
			}
			string cmdName;
			if (index == 0) // Space after the command specifier should not be supported
			{
				player.SendErrorMessage("Invalid command entered. Type {0}help for a list of valid commands.", Specifier);
				return true;
			}
			else if (index < 0)
				cmdName = cmdText.ToLower();
			else
				cmdName = cmdText.Substring(0, index).ToLower();

			List<string> args;
			if (index < 0)
				args = new List<string>();
			else
				args = ParseParameters(cmdText.Substring(index));

			IEnumerable<Command> cmds = ChatCommands.FindAll(c => c.HasAlias(cmdName));

			if (Hooks.PlayerHooks.OnPlayerCommand(player, cmdName, cmdText, args, ref cmds, cmdPrefix))
				return true;

			if (cmds.Count() == 0)
			{
				if (player.AwaitingResponse.ContainsKey(cmdName))
				{
					Action<CommandArgs> call = player.AwaitingResponse[cmdName];
					player.AwaitingResponse.Remove(cmdName);
					call(new CommandArgs(cmdText, player, args));
					return true;
				}
				player.SendErrorMessage("Invalid command entered. Type {0}help for a list of valid commands.", Specifier);
				return true;
			}
			foreach (Command cmd in cmds)
			{
				if (!cmd.CanRun(player))
				{
					TShock.Utils.SendLogs(string.Format("{0} tried to execute {1}{2}.", player.Name, Specifier, cmdText), Color.PaleVioletRed, player);
					player.SendErrorMessage("You do not have access to this command.");
					if (player.HasPermission(Permissions.su))
					{
						player.SendInfoMessage("You can use '{0}sudo {0}{1}' to override this check.", Specifier, cmdText);
					}
				}
				else if (!cmd.AllowServer && !player.RealPlayer)
				{
					player.SendErrorMessage("You must use this command in-game.");
				}
				else
				{
					if (cmd.DoLog)
						TShock.Utils.SendLogs(string.Format("{0} executed: {1}{2}.", player.Name, silent ? SilentSpecifier : Specifier, cmdText), Color.PaleVioletRed, player);
					cmd.Run(cmdText, silent, player, args);
				}
			}
			return true;
		}

		/// <summary>
		/// Parses a string of parameters into a list. Handles quotes.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static List<String> ParseParameters(string str)
		{
			var ret = new List<string>();
			var sb = new StringBuilder();
			bool instr = false;
			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];

				if (c == '\\' && ++i < str.Length)
				{
					if (str[i] != '"' && str[i] != ' ' && str[i] != '\\')
						sb.Append('\\');
					sb.Append(str[i]);
				}
				else if (c == '"')
				{
					instr = !instr;
					if (!instr)
					{
						ret.Add(sb.ToString());
						sb.Clear();
					}
					else if (sb.Length > 0)
					{
						ret.Add(sb.ToString());
						sb.Clear();
					}
				}
				else if (IsWhiteSpace(c) && !instr)
				{
					if (sb.Length > 0)
					{
						ret.Add(sb.ToString());
						sb.Clear();
					}
				}
				else
					sb.Append(c);
			}
			if (sb.Length > 0)
				ret.Add(sb.ToString());

			return ret;
		}

		private static bool IsWhiteSpace(char c)
		{
			return c == ' ' || c == '\t' || c == '\n';
		}

		#region Account commands

		private static void AttemptLogin(CommandArgs args)
		{
			if (args.Player.LoginAttempts > TShock.Config.MaximumLoginAttempts && (TShock.Config.MaximumLoginAttempts != -1))
			{
				TShock.Log.Warn(String.Format("{0} ({1}) had {2} or more invalid login attempts and was kicked automatically.",
					args.Player.IP, args.Player.Name, TShock.Config.MaximumLoginAttempts));
				args.Player.Kick("Too many invalid login attempts.");
				return;
			}

			if (args.Player.IsLoggedIn)
			{
				args.Player.SendErrorMessage("You are already logged in, and cannot login again.");
				return;
			}

			UserAccount account = TShock.UserAccounts.GetUserAccountByName(args.Player.Name);
			string password = "";
			bool usingUUID = false;
			if (args.Parameters.Count == 0 && !TShock.Config.DisableUUIDLogin)
			{
				if (PlayerHooks.OnPlayerPreLogin(args.Player, args.Player.Name, ""))
					return;
				usingUUID = true;
			}
			else if (args.Parameters.Count == 1)
			{
				if (PlayerHooks.OnPlayerPreLogin(args.Player, args.Player.Name, args.Parameters[0]))
					return;
				password = args.Parameters[0];
			}
			else if (args.Parameters.Count == 2 && TShock.Config.AllowLoginAnyUsername)
			{
				if (String.IsNullOrEmpty(args.Parameters[0]))
				{
					args.Player.SendErrorMessage("Bad login attempt.");
					return;
				}

				if (PlayerHooks.OnPlayerPreLogin(args.Player, args.Parameters[0], args.Parameters[1]))
					return;

				account = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
				password = args.Parameters[1];
			}
			else
			{
				args.Player.SendErrorMessage("Syntax: {0}login - Logs in using your UUID and character name", Specifier);
				args.Player.SendErrorMessage("        {0}login <password> - Logs in using your password and character name", Specifier);
				args.Player.SendErrorMessage("        {0}login <username> <password> - Logs in using your username and password", Specifier);
				args.Player.SendErrorMessage("If you forgot your password, there is no way to recover it.");
				return;
			}
			try
			{
				if (account == null)
				{
					args.Player.SendErrorMessage("A user account by that name does not exist.");
				}
				else if (account.VerifyPassword(password) ||
						(usingUUID && account.UUID == args.Player.UUID && !TShock.Config.DisableUUIDLogin &&
						!String.IsNullOrWhiteSpace(args.Player.UUID)))
				{
					args.Player.PlayerData = TShock.CharacterDB.GetPlayerData(args.Player, account.ID);

					var group = TShock.Groups.GetGroupByName(account.Group);

					args.Player.Group = group;
					args.Player.tempGroup = null;
					args.Player.Account = account;
					args.Player.IsLoggedIn = true;
					args.Player.IsDisabledForSSC = false;

					if (Main.ServerSideCharacter)
					{
						if (args.Player.HasPermission(Permissions.bypassssc))
						{
							args.Player.PlayerData.CopyCharacter(args.Player);
							TShock.CharacterDB.InsertPlayerData(args.Player);
						}
						args.Player.PlayerData.RestoreCharacter(args.Player);
					}
					args.Player.LoginFailsBySsi = false;

					if (args.Player.HasPermission(Permissions.ignorestackhackdetection))
						args.Player.IsDisabledForStackDetection = false;

					if (args.Player.HasPermission(Permissions.usebanneditem))
						args.Player.IsDisabledForBannedWearable = false;

					args.Player.SendSuccessMessage("Authenticated as " + account.Name + " successfully.");

					TShock.Log.ConsoleInfo(args.Player.Name + " authenticated successfully as user: " + account.Name + ".");
					if ((args.Player.LoginHarassed) && (TShock.Config.RememberLeavePos))
					{
						if (TShock.RememberedPos.GetLeavePos(args.Player.Name, args.Player.IP) != Vector2.Zero)
						{
							Vector2 pos = TShock.RememberedPos.GetLeavePos(args.Player.Name, args.Player.IP);
							args.Player.Teleport((int)pos.X * 16, (int)pos.Y * 16);
						}
						args.Player.LoginHarassed = false;

					}
					TShock.UserAccounts.SetUserAccountUUID(account, args.Player.UUID);

					Hooks.PlayerHooks.OnPlayerPostLogin(args.Player);
				}
				else
				{
					if (usingUUID && !TShock.Config.DisableUUIDLogin)
					{
						args.Player.SendErrorMessage("UUID does not match this character!");
					}
					else
					{
						args.Player.SendErrorMessage("Invalid password!");
					}
					TShock.Log.Warn(args.Player.IP + " failed to authenticate as user: " + account.Name + ".");
					args.Player.LoginAttempts++;
				}
			}
			catch (Exception ex)
			{
				args.Player.SendErrorMessage("There was an error processing your request.");
				TShock.Log.Error(ex.ToString());
			}
		}

		private static void Logout(CommandArgs args)
		{
			if (!args.Player.IsLoggedIn)
			{
				args.Player.SendErrorMessage("You are not logged in.");
				return;
			}

			args.Player.Logout();
			args.Player.SendSuccessMessage("You have been successfully logged out of your account.");
			if (Main.ServerSideCharacter)
			{
				args.Player.SendWarningMessage("Server side characters are enabled. You need to be logged in to play.");
			}
		}

		private static void PasswordUser(CommandArgs args)
		{
			try
			{
				if (args.Player.IsLoggedIn && args.Parameters.Count == 2)
				{
					string password = args.Parameters[0];
					if (args.Player.Account.VerifyPassword(password))
					{
						try
						{
							args.Player.SendSuccessMessage("You changed your password!");
							TShock.UserAccounts.SetUserAccountPassword(args.Player.Account, args.Parameters[1]); // SetUserPassword will hash it for you.
							TShock.Log.ConsoleInfo(args.Player.IP + " named " + args.Player.Name + " changed the password of account " +
												   args.Player.Account.Name + ".");
						}
						catch (ArgumentOutOfRangeException)
						{
							args.Player.SendErrorMessage("Password must be greater than or equal to " + TShock.Config.MinimumPasswordLength + " characters.");
						}
					}
					else
					{
						args.Player.SendErrorMessage("You failed to change your password!");
						TShock.Log.ConsoleError(args.Player.IP + " named " + args.Player.Name + " failed to change password for account: " +
												args.Player.Account.Name + ".");
					}
				}
				else
				{
					args.Player.SendErrorMessage("Not logged in or invalid syntax! Proper syntax: {0}password <oldpassword> <newpassword>", Specifier);
				}
			}
			catch (UserAccountManagerException ex)
			{
				args.Player.SendErrorMessage("Sorry, an error occured: " + ex.Message + ".");
				TShock.Log.ConsoleError("PasswordUser returned an error: " + ex);
			}
		}

		private static void RegisterUser(CommandArgs args)
		{
			try
			{
				var account = new UserAccount();
				string echoPassword = "";
				if (args.Parameters.Count == 1)
				{
					account.Name = args.Player.Name;
					echoPassword = args.Parameters[0];
					try
					{
						account.CreateBCryptHash(args.Parameters[0]);
					}
					catch (ArgumentOutOfRangeException)
					{
						args.Player.SendErrorMessage("Password must be greater than or equal to " + TShock.Config.MinimumPasswordLength + " characters.");
						return;
					}
				}
				else if (args.Parameters.Count == 2 && TShock.Config.AllowRegisterAnyUsername)
				{
					account.Name = args.Parameters[0];
					echoPassword = args.Parameters[1];
					try
					{
						account.CreateBCryptHash(args.Parameters[1]);
					}
					catch (ArgumentOutOfRangeException)
					{
						args.Player.SendErrorMessage("Password must be greater than or equal to " + TShock.Config.MinimumPasswordLength + " characters.");
						return;
					}
				}
				else
				{
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}register <password>", Specifier);
					return;
				}

				account.Group = TShock.Config.DefaultRegistrationGroupName; // FIXME -- we should get this from the DB. --Why?
				account.UUID = args.Player.UUID;

				if (TShock.UserAccounts.GetUserAccountByName(account.Name) == null && account.Name != TSServerPlayer.AccountName) // Cheap way of checking for existance of a user
				{
					args.Player.SendSuccessMessage("Account \"{0}\" has been registered.", account.Name);
					args.Player.SendSuccessMessage("Your password is {0}.", echoPassword);
					TShock.UserAccounts.AddUserAccount(account);
					TShock.Log.ConsoleInfo("{0} registered an account: \"{1}\".", args.Player.Name, account.Name);
				}
				else
				{
					args.Player.SendErrorMessage("Sorry, " + account.Name + " was already taken by another person.");
					args.Player.SendErrorMessage("Please try a different username.");
					TShock.Log.ConsoleInfo(args.Player.Name + " failed to register an existing account: " + account.Name);
				}
			}
			catch (UserAccountManagerException ex)
			{
				args.Player.SendErrorMessage("Sorry, an error occured: " + ex.Message + ".");
				TShock.Log.ConsoleError("RegisterUser returned an error: " + ex);
			}
		}

		private static void ManageUsers(CommandArgs args)
		{
			// This guy needs to be here so that people don't get exceptions when they type /user
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid user syntax. Try {0}user help.", Specifier);
				return;
			}

			string subcmd = args.Parameters[0];

			// Add requires a username, password, and a group specified.
			if (subcmd == "add" && args.Parameters.Count == 4)
			{
				var account = new UserAccount();

				account.Name = args.Parameters[1];
				try
				{
					account.CreateBCryptHash(args.Parameters[2]);
				}
				catch (ArgumentOutOfRangeException)
				{
					args.Player.SendErrorMessage("Password must be greater than or equal to " + TShock.Config.MinimumPasswordLength + " characters.");
					return;
				}
				account.Group = args.Parameters[3];

				try
				{
					TShock.UserAccounts.AddUserAccount(account);
					args.Player.SendSuccessMessage("Account " + account.Name + " has been added to group " + account.Group + "!");
					TShock.Log.ConsoleInfo(args.Player.Name + " added Account " + account.Name + " to group " + account.Group);
				}
				catch (GroupNotExistsException)
				{
					args.Player.SendErrorMessage("Group " + account.Group + " does not exist!");
				}
				catch (UserAccountExistsException)
				{
					args.Player.SendErrorMessage("User " + account.Name + " already exists!");
				}
				catch (UserAccountManagerException e)
				{
					args.Player.SendErrorMessage("User " + account.Name + " could not be added, check console for details.");
					TShock.Log.ConsoleError(e.ToString());
				}
			}
			// User deletion requires a username
			else if (subcmd == "del" && args.Parameters.Count == 2)
			{
				var account = new UserAccount();
				account.Name = args.Parameters[1];

				try
				{
					TShock.UserAccounts.RemoveUserAccount(account);
					args.Player.SendSuccessMessage("Account removed successfully.");
					TShock.Log.ConsoleInfo(args.Player.Name + " successfully deleted account: " + args.Parameters[1] + ".");
				}
				catch (UserAccountNotExistException)
				{
					args.Player.SendErrorMessage("The user " + account.Name + " does not exist! Deleted nobody!");
				}
				catch (UserAccountManagerException ex)
				{
					args.Player.SendErrorMessage(ex.Message);
					TShock.Log.ConsoleError(ex.ToString());
				}
			}

			// Password changing requires a username, and a new password to set
			else if (subcmd == "password" && args.Parameters.Count == 3)
			{
				var account = new UserAccount();
				account.Name = args.Parameters[1];

				try
				{
					TShock.UserAccounts.SetUserAccountPassword(account, args.Parameters[2]);
					TShock.Log.ConsoleInfo(args.Player.Name + " changed the password of account " + account.Name);
					args.Player.SendSuccessMessage("Password change succeeded for " + account.Name + ".");
				}
				catch (UserAccountNotExistException)
				{
					args.Player.SendErrorMessage("User " + account.Name + " does not exist!");
				}
				catch (UserAccountManagerException e)
				{
					args.Player.SendErrorMessage("Password change for " + account.Name + " failed! Check console!");
					TShock.Log.ConsoleError(e.ToString());
				}
				catch (ArgumentOutOfRangeException)
				{
					args.Player.SendErrorMessage("Password must be greater than or equal to " + TShock.Config.MinimumPasswordLength + " characters.");
				}
			}
			// Group changing requires a username or IP address, and a new group to set
			else if (subcmd == "group" && args.Parameters.Count == 3)
			{
				var account = new UserAccount();
				account.Name = args.Parameters[1];

				try
				{
					TShock.UserAccounts.SetUserGroup(account, args.Parameters[2]);
					TShock.Log.ConsoleInfo(args.Player.Name + " changed account " + account.Name + " to group " + args.Parameters[2] + ".");
					args.Player.SendSuccessMessage("Account " + account.Name + " has been changed to group " + args.Parameters[2] + "!");
				}
				catch (GroupNotExistsException)
				{
					args.Player.SendErrorMessage("That group does not exist!");
				}
				catch (UserAccountNotExistException)
				{
					args.Player.SendErrorMessage("User " + account.Name + " does not exist!");
				}
				catch (UserAccountManagerException e)
				{
					args.Player.SendErrorMessage("User " + account.Name + " could not be added. Check console for details.");
					TShock.Log.ConsoleError(e.ToString());
				}
			}
			else if (subcmd == "help")
			{
				args.Player.SendInfoMessage("Use command help:");
				args.Player.SendInfoMessage("{0}user add username password group   -- Adds a specified user", Specifier);
				args.Player.SendInfoMessage("{0}user del username                  -- Removes a specified user", Specifier);
				args.Player.SendInfoMessage("{0}user password username newpassword -- Changes a user's password", Specifier);
				args.Player.SendInfoMessage("{0}user group username newgroup       -- Changes a user's group", Specifier);
			}
			else
			{
				args.Player.SendErrorMessage("Invalid user syntax. Try {0}user help.", Specifier);
			}
		}

		#endregion

		#region Stupid commands

		private static void ServerInfo(CommandArgs args)
		{
			args.Player.SendInfoMessage("Memory usage: " + Process.GetCurrentProcess().WorkingSet64);
			args.Player.SendInfoMessage("Allocated memory: " + Process.GetCurrentProcess().VirtualMemorySize64);
			args.Player.SendInfoMessage("Total processor time: " + Process.GetCurrentProcess().TotalProcessorTime);
			args.Player.SendInfoMessage("WinVer: " + Environment.OSVersion);
			args.Player.SendInfoMessage("Proc count: " + Environment.ProcessorCount);
			args.Player.SendInfoMessage("Machine name: " + Environment.MachineName);
		}

		private static void WorldInfo(CommandArgs args)
		{
			args.Player.SendInfoMessage("World name: " + (TShock.Config.UseServerName ? TShock.Config.ServerName : Main.worldName));
			args.Player.SendInfoMessage("World size: {0}x{1}", Main.maxTilesX, Main.maxTilesY);
			args.Player.SendInfoMessage("World ID: " + Main.worldID);
		}

		#endregion

		#region Player Management Commands

		private static void GrabUserUserInfo(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}userinfo <player>", Specifier);
				return;
			}

			var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (players.Count < 1)
				args.Player.SendErrorMessage("Invalid player.");
			else if (players.Count > 1)
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			else
			{
				var message = new StringBuilder();
				message.Append("IP Address: ").Append(players[0].IP);
				if (players[0].Account != null && players[0].IsLoggedIn)
					message.Append(" | Logged in as: ").Append(players[0].Account.Name).Append(" | Group: ").Append(players[0].Group.Name);
				args.Player.SendSuccessMessage(message.ToString());
			}
		}

		private static void ViewAccountInfo(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}accountinfo <username>", Specifier);
				return;
			}

			string username = String.Join(" ", args.Parameters);
			if (!string.IsNullOrWhiteSpace(username))
			{
				var account = TShock.UserAccounts.GetUserAccountByName(username);
				if (account != null)
				{
					DateTime LastSeen;
					string Timezone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString("+#;-#");

					if (DateTime.TryParse(account.LastAccessed, out LastSeen))
					{
						LastSeen = DateTime.Parse(account.LastAccessed).ToLocalTime();
						args.Player.SendSuccessMessage("{0}'s last login occured {1} {2} UTC{3}.", account.Name, LastSeen.ToShortDateString(),
							LastSeen.ToShortTimeString(), Timezone);
					}

					if (args.Player.Group.HasPermission(Permissions.advaccountinfo))
					{
						List<string> KnownIps = JsonConvert.DeserializeObject<List<string>>(account.KnownIps?.ToString() ?? string.Empty);
						string ip = KnownIps?[KnownIps.Count - 1] ?? "N/A";
						DateTime Registered = DateTime.Parse(account.Registered).ToLocalTime();

						args.Player.SendSuccessMessage("{0}'s group is {1}.", account.Name, account.Group);
						args.Player.SendSuccessMessage("{0}'s last known IP is {1}.", account.Name, ip);
						args.Player.SendSuccessMessage("{0}'s register date is {1} {2} UTC{3}.", account.Name, Registered.ToShortDateString(), Registered.ToShortTimeString(), Timezone);
					}
				}
				else
					args.Player.SendErrorMessage("User {0} does not exist.", username);
			}
			else args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}accountinfo <username>", Specifier);
		}

		private static void Kick(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}kick <player> [reason]", Specifier);
				return;
			}
			if (args.Parameters[0].Length == 0)
			{
				args.Player.SendErrorMessage("Missing player name.");
				return;
			}

			string plStr = args.Parameters[0];
			var players = TSPlayer.FindByNameOrID(plStr);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			}
			else
			{
				string reason = args.Parameters.Count > 1
									? String.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1))
									: "Misbehaviour.";
				if (!players[0].Kick(reason, !args.Player.RealPlayer, false, args.Player.Name))
				{
					args.Player.SendErrorMessage("You can't kick another admin!");
				}
			}
		}

		private static void Ban(CommandArgs args)
		{
			string subcmd = args.Parameters.Count == 0 ? "help" : args.Parameters[0].ToLower();
			switch (subcmd)
			{
				case "add":
					#region Add Ban
					{
						if (args.Parameters.Count < 2)
						{
							args.Player.SendErrorMessage("Invalid command. Format: {0}ban add <player> [time] [reason]", Specifier);
							args.Player.SendErrorMessage("Example: {0}ban add Shank 10d Hacking and cheating", Specifier);
							args.Player.SendErrorMessage("Example: {0}ban add Ash", Specifier);
							args.Player.SendErrorMessage("Use the time 0 (zero) for a permanent ban.");
							return;
						}

						// Used only to notify if a ban was successful and who the ban was about
						bool success = false;
						string targetGeneralizedName = "";

						// Effective ban target assignment
						List<TSPlayer> players = TSPlayer.FindByNameOrID(args.Parameters[1]);

						// Bad case: Players contains more than 1 person so we can't ban them
						if (players.Count > 1)
						{
							//Fail fast
							args.Player.SendMultipleMatchError(players.Select(p => p.Name));
							return;
						}
						
						UserAccount offlineUserAccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);

						// Storage variable to determine if the command executor is the server console
						// If it is, we assume they have full control and let them override permission checks
						bool callerIsServerConsole = args.Player is TSServerPlayer;

						// The ban reason the ban is going to have
						string banReason = "Unknown.";

						// The default ban length
						// 0 is permanent ban, otherwise temp ban
						int banLengthInSeconds = 0;

						// Figure out if param 2 is a time or 0 or garbage
						if (args.Parameters.Count >= 3)
						{
							bool parsedOkay = false;
							if (args.Parameters[2] != "0")
							{
								parsedOkay = TShock.Utils.TryParseTime(args.Parameters[2], out banLengthInSeconds);
							} 
							else 
							{
								parsedOkay = true;
							}

							if (!parsedOkay)
							{
								args.Player.SendErrorMessage("Invalid time format. Example: 10d 5h 3m 2s.");
								args.Player.SendErrorMessage("Use 0 (zero) for a permanent ban.");
								return;
							}
						}

						// If a reason exists, use the given reason.
						if (args.Parameters.Count > 3)
						{
							banReason = String.Join(" ", args.Parameters.Skip(3));
						}

						// Good case: Online ban for matching character.
						if (players.Count == 1)
						{
							TSPlayer target = players[0];

							if (target.HasPermission(Permissions.immunetoban) && !callerIsServerConsole)
							{
								args.Player.SendErrorMessage("Permission denied. Target {0} is immune to ban.", target.Name);
								return;
							}

							targetGeneralizedName = target.Name;
							success = TShock.Bans.AddBan(target.IP, target.Name, target.UUID, target.Account?.Name ?? "", banReason, false, args.Player.Account.Name,
								banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

							// Since this is an online ban, we need to dc the player and tell them now.
							if (success)
							{
								if (banLengthInSeconds == 0)
								{
									target.Disconnect(String.Format("Permanently banned for {0}", banReason));
								}
								else
								{
									target.Disconnect(String.Format("Banned for {0} seconds for {1}", banLengthInSeconds, banReason));
								}
							}
						}

						// Case: Players & user are invalid, could be IP?
						// Note: Order matters. If this method is above the online player check,
						// This enables you to ban an IP even if the player exists in the database as a player.
						// You'll get two bans for the price of one, in theory, because both IP and user named IP will be banned.
						// ??? edge cases are weird, but this is going to happen
						// The only way around this is to either segregate off the IP code or do something else.
						if (players.Count == 0)
						{
							// If the target is a valid IP...
							string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
							Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
							if (r.IsMatch(args.Parameters[1]))
							{
								targetGeneralizedName = "IP: " + args.Parameters[1];
								success = TShock.Bans.AddBan(args.Parameters[1], "", "", "", banReason,
									false, args.Player.Account.Name, banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));
								if (success && offlineUserAccount != null)
								{
									args.Player.SendSuccessMessage("Target IP {0} was banned successfully.", targetGeneralizedName);
									args.Player.SendErrorMessage("Note: An account named with this IP address also exists.");
									args.Player.SendErrorMessage("Note: It will also be banned.");
								}
							} 
							else 
							{
								// Apparently there is no way to not IP ban someone
								// This means that where we would normally just ban a "character name" here
								// We can't because it requires some IP as a primary key.
								if (offlineUserAccount == null)
								{
									args.Player.SendErrorMessage("Unable to ban target {0}.", args.Parameters[1]);
									args.Player.SendErrorMessage("Target is not a valid IP address, a valid online player, or a known offline user.");
									return;
								}
							}

						}

						// Case: Offline ban
						if (players.Count == 0 && offlineUserAccount != null)
						{
							// Catch: we don't know an offline player's last login character name
							// This means that we're banning their *user name* on the assumption that
							// user name == character name
							// (which may not be true)
							// This needs to be fixed in a future implementation.
							targetGeneralizedName = offlineUserAccount.Name;

							if (TShock.Groups.GetGroupByName(offlineUserAccount.Group).HasPermission(Permissions.immunetoban) && 
								!callerIsServerConsole)
							{
								args.Player.SendErrorMessage("Permission denied. Target {0} is immune to ban.", targetGeneralizedName);
								return;
							}

							if (offlineUserAccount.KnownIps == null)
							{
								args.Player.SendErrorMessage("Unable to ban target {0} because they have no valid IP to ban.", targetGeneralizedName);
								return;
							}

							string lastIP = JsonConvert.DeserializeObject<List<string>>(offlineUserAccount.KnownIps).Last();

							success = 
								TShock.Bans.AddBan(lastIP,
									"", offlineUserAccount.UUID, offlineUserAccount.Name, banReason, false, args.Player.Account.Name,
									banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));
						}

						if (success)
						{
							args.Player.SendSuccessMessage("{0} was successfully banned.", targetGeneralizedName);
							args.Player.SendInfoMessage("Length: {0}", banLengthInSeconds == 0 ? "Permanent." : banLengthInSeconds + " seconds.");
							args.Player.SendInfoMessage("Reason: {0}", banReason);
							if (!args.Silent)
							{
								if (banLengthInSeconds == 0)
								{
									TSPlayer.All.SendErrorMessage("{0} was permanently banned by {1} for: {2}",
										targetGeneralizedName, args.Player.Account.Name, banReason);
								}
								else
								{
									TSPlayer.All.SendErrorMessage("{0} was temp banned for {1} seconds by {2} for: {3}",
										targetGeneralizedName, banLengthInSeconds, args.Player.Account.Name, banReason);
								}
							}
						}
						else
						{
							args.Player.SendErrorMessage("{0} was NOT banned due to a database error or other system problem.", targetGeneralizedName);
							args.Player.SendErrorMessage("If this player is online, they have NOT been kicked.");
							args.Player.SendErrorMessage("Check the system logs for details.");
						}

						return;
					}
					#endregion
				case "del":
					#region Delete ban
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}ban del <player>", Specifier);
							return;
						}

						string plStr = args.Parameters[1];
						Ban ban = TShock.Bans.GetBanByName(plStr, false);
						if (ban != null)
						{
							if (TShock.Bans.RemoveBan(ban.Name, true))
								args.Player.SendSuccessMessage("Unbanned {0} ({1}).", ban.Name, ban.IP);
							else
								args.Player.SendErrorMessage("Failed to unban {0} ({1}), check logs.", ban.Name, ban.IP);
						}
						else
							args.Player.SendErrorMessage("No bans for {0} exist.", plStr);
					}
					#endregion
					return;
				case "delip":
					#region Delete IP ban
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}ban delip <ip>", Specifier);
							return;
						}

						string ip = args.Parameters[1];
						Ban ban = TShock.Bans.GetBanByIp(ip);
						if (ban != null)
						{
							if (TShock.Bans.RemoveBan(ban.IP, false))
								args.Player.SendSuccessMessage("Unbanned IP {0} ({1}).", ban.IP, ban.Name);
							else
								args.Player.SendErrorMessage("Failed to unban IP {0} ({1}), check logs.", ban.IP, ban.Name);
						}
						else
							args.Player.SendErrorMessage("IP {0} is not banned.", ip);
					}
					#endregion
					return;
				case "help":
					#region Help
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;

						var lines = new List<string>
						{
							"add <target> <time> [reason] - Bans a player or user account if the player is not online.",
							"del <player> - Unbans a player.",
							"delip <ip> - Unbans an IP.",
							"list [page] - Lists all player bans.",
							"listip [page] - Lists all IP bans."
						};

						PaginationTools.SendPage(args.Player, pageNumber, lines,
							new PaginationTools.Settings
							{
								HeaderFormat = "Ban Sub-Commands ({0}/{1}):",
								FooterFormat = "Type {0}ban help {{0}} for more sub-commands.".SFormat(Specifier)
							}
						);
					}
					#endregion
					return;
				case "list":
					#region List bans
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
						{
							return;
						}

						List<Ban> bans = TShock.Bans.GetBans();

						var nameBans = from ban in bans
									   where !String.IsNullOrEmpty(ban.Name)
									   select ban.Name;

						PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(nameBans),
							new PaginationTools.Settings
							{
								HeaderFormat = "Bans ({0}/{1}):",
								FooterFormat = "Type {0}ban list {{0}} for more.".SFormat(Specifier),
								NothingToDisplayString = "There are currently no bans."
							});
					}
					#endregion
					return;
				case "listip":
					#region List IP bans
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
						{
							return;
						}

						List<Ban> bans = TShock.Bans.GetBans();

						var ipBans = from ban in bans
									 where String.IsNullOrEmpty(ban.Name)
									 select ban.IP;

						PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(ipBans),
							new PaginationTools.Settings
							{
								HeaderFormat = "IP Bans ({0}/{1}):",
								FooterFormat = "Type {0}ban listip {{0}} for more.".SFormat(Specifier),
								NothingToDisplayString = "There are currently no IP bans."
							});
					}
					#endregion
					return;
				default:
					args.Player.SendErrorMessage("Invalid subcommand! Type {0}ban help for more information.", Specifier);
					return;
			}
		}

		private static void Whitelist(CommandArgs args)
		{
			if (args.Parameters.Count == 1)
			{
				using (var tw = new StreamWriter(FileTools.WhitelistPath, true))
				{
					tw.WriteLine(args.Parameters[0]);
				}
				args.Player.SendSuccessMessage("Added " + args.Parameters[0] + " to the whitelist.");
			}
		}

		private static void DisplayLogs(CommandArgs args)
		{
			args.Player.DisplayLogs = (!args.Player.DisplayLogs);
			args.Player.SendSuccessMessage("You will " + (args.Player.DisplayLogs ? "now" : "no longer") + " receive logs.");
		}

		private static void SaveSSC(CommandArgs args)
		{
			if (Main.ServerSideCharacter)
			{
				args.Player.SendSuccessMessage("SSC has been saved.");
				foreach (TSPlayer player in TShock.Players)
				{
					if (player != null && player.IsLoggedIn && !player.IsDisabledPendingTrashRemoval)
					{
						TShock.CharacterDB.InsertPlayerData(player, true);
					}
				}
			}
		}

		private static void OverrideSSC(CommandArgs args)
		{
			if (!Main.ServerSideCharacter)
			{
				args.Player.SendErrorMessage("Server Side Characters is disabled.");
				return;
			}
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Correct usage: {0}overridessc|{0}ossc <player name>", Specifier);
				return;
			}

			string playerNameToMatch = string.Join(" ", args.Parameters);
			var matchedPlayers = TSPlayer.FindByNameOrID(playerNameToMatch);
			if (matchedPlayers.Count < 1)
			{
				args.Player.SendErrorMessage("No players matched \"{0}\".", playerNameToMatch);
				return;
			}
			else if (matchedPlayers.Count > 1)
			{
				args.Player.SendMultipleMatchError(matchedPlayers.Select(p => p.Name));
				return;
			}

			TSPlayer matchedPlayer = matchedPlayers[0];
			if (matchedPlayer.IsLoggedIn)
			{
				args.Player.SendErrorMessage("Player \"{0}\" is already logged in.", matchedPlayer.Name);
				return;
			}
			if (!matchedPlayer.LoginFailsBySsi)
			{
				args.Player.SendErrorMessage("Player \"{0}\" has to perform a /login attempt first.", matchedPlayer.Name);
				return;
			}
			if (matchedPlayer.IsDisabledPendingTrashRemoval)
			{
				args.Player.SendErrorMessage("Player \"{0}\" has to reconnect first.", matchedPlayer.Name);
				return;
			}

			TShock.CharacterDB.InsertPlayerData(matchedPlayer);
			args.Player.SendSuccessMessage("SSC of player \"{0}\" has been overriden.", matchedPlayer.Name);
		}

		private static void UploadJoinData(CommandArgs args)
		{
			TSPlayer targetPlayer = args.Player;
			if (args.Parameters.Count == 1 && args.Player.HasPermission(Permissions.uploadothersdata))
			{
				List<TSPlayer> players = TSPlayer.FindByNameOrID(args.Parameters[0]);
				if (players.Count > 1)
				{
					args.Player.SendMultipleMatchError(players.Select(p => p.Name));
					return;
				}
				else if (players.Count == 0)
				{
					args.Player.SendErrorMessage("No player was found matching'{0}'", args.Parameters[0]);
					return;
				}
				else
				{
					targetPlayer = players[0];
				}
			}
			else if (args.Parameters.Count == 1)
			{
				args.Player.SendErrorMessage("You do not have permission to upload another player's character data.");
				return;
			}
			else if (args.Parameters.Count > 0)
			{
				args.Player.SendErrorMessage("Usage: /uploadssc [playername]");
				return;
			}
			else if (args.Parameters.Count == 0 && args.Player is TSServerPlayer)
			{
				args.Player.SendErrorMessage("A console can not upload their player data.");
				args.Player.SendErrorMessage("Usage: /uploadssc [playername]");
				return;
			}

			if (targetPlayer.IsLoggedIn)
			{
				if (TShock.CharacterDB.InsertSpecificPlayerData(targetPlayer, targetPlayer.DataWhenJoined))
				{
					targetPlayer.DataWhenJoined.RestoreCharacter(targetPlayer);
					targetPlayer.SendSuccessMessage("Your local character data has been uploaded to the server.");
					args.Player.SendSuccessMessage("The player's character data was successfully uploaded.");
				}
				else
				{
					args.Player.SendErrorMessage("Failed to upload your character data, are you logged in to an account?");
				}
			}
			else
			{
				args.Player.SendErrorMessage("The target player has not logged in yet.");
			}
		}

		private static void ForceHalloween(CommandArgs args)
		{
			TShock.Config.ForceHalloween = !TShock.Config.ForceHalloween;
			Main.checkHalloween();
			if (args.Silent)
				args.Player.SendInfoMessage("{0}abled halloween mode!", (TShock.Config.ForceHalloween ? "en" : "dis"));
			else
				TSPlayer.All.SendInfoMessage("{0} {1}abled halloween mode!", args.Player.Name, (TShock.Config.ForceHalloween ? "en" : "dis"));
		}

		private static void ForceXmas(CommandArgs args)
		{
			TShock.Config.ForceXmas = !TShock.Config.ForceXmas;
			Main.checkXMas();
			if (args.Silent)
				args.Player.SendInfoMessage("{0}abled Christmas mode!", (TShock.Config.ForceXmas ? "en" : "dis"));
			else
				TSPlayer.All.SendInfoMessage("{0} {1}abled Christmas mode!", args.Player.Name, (TShock.Config.ForceXmas ? "en" : "dis"));
		}

		private static void TempGroup(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendInfoMessage("Invalid usage");
				args.Player.SendInfoMessage("Usage: {0}tempgroup <username> <new group> [time]", Specifier);
				return;
			}

			List<TSPlayer> ply = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (ply.Count < 1)
			{
				args.Player.SendErrorMessage("Could not find player {0}.", args.Parameters[0]);
				return;
			}

			if (ply.Count > 1)
			{
				args.Player.SendMultipleMatchError(ply.Select(p => p.Account.Name));
			}

			if (!TShock.Groups.GroupExists(args.Parameters[1]))
			{
				args.Player.SendErrorMessage("Could not find group {0}", args.Parameters[1]);
				return;
			}

			if (args.Parameters.Count > 2)
			{
				int time;
				if (!TShock.Utils.TryParseTime(args.Parameters[2], out time))
				{
					args.Player.SendErrorMessage("Invalid time string! Proper format: _d_h_m_s, with at least one time specifier.");
					args.Player.SendErrorMessage("For example, 1d and 10h-30m+2m are both valid time strings, but 2 is not.");
					return;
				}

				ply[0].tempGroupTimer = new System.Timers.Timer(time * 1000);
				ply[0].tempGroupTimer.Elapsed += ply[0].TempGroupTimerElapsed;
				ply[0].tempGroupTimer.Start();
			}

			Group g = TShock.Groups.GetGroupByName(args.Parameters[1]);

			ply[0].tempGroup = g;

			if (args.Parameters.Count < 3)
			{
				args.Player.SendSuccessMessage(String.Format("You have changed {0}'s group to {1}", ply[0].Name, g.Name));
				ply[0].SendSuccessMessage(String.Format("Your group has temporarily been changed to {0}", g.Name));
			}
			else
			{
				args.Player.SendSuccessMessage(String.Format("You have changed {0}'s group to {1} for {2}",
					ply[0].Name, g.Name, args.Parameters[2]));
				ply[0].SendSuccessMessage(String.Format("Your group has been changed to {0} for {1}",
					g.Name, args.Parameters[2]));
			}
		}

		private static void SubstituteUser(CommandArgs args)
		{

			if (args.Player.tempGroup != null)
			{
				args.Player.tempGroup = null;
				args.Player.tempGroupTimer.Stop();
				args.Player.SendSuccessMessage("Your previous permission set has been restored.");
				return;
			}
			else
			{
				args.Player.tempGroup = new SuperAdminGroup();
				args.Player.tempGroupTimer = new System.Timers.Timer(600 * 1000);
				args.Player.tempGroupTimer.Elapsed += args.Player.TempGroupTimerElapsed;
				args.Player.tempGroupTimer.Start();
				args.Player.SendSuccessMessage("Your account has been elevated to Super Admin for 10 minutes.");
				return;
			}
		}

		#endregion Player Management Commands

		#region Server Maintenence Commands

		// Executes a command as a superuser if you have sudo rights.
		private static void SubstituteUserDo(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Usage: /sudo [command].");
				args.Player.SendErrorMessage("Example: /sudo /ban add Shank 2d Hacking.");
				return;
			}

			string replacementCommand = String.Join(" ", args.Parameters);
			args.Player.tempGroup = new SuperAdminGroup();
			HandleCommand(args.Player, replacementCommand);
			args.Player.tempGroup = null;
			return;
		}

		private static void Broadcast(CommandArgs args)
		{
			string message = string.Join(" ", args.Parameters);

			TShock.Utils.Broadcast(
				"(Server Broadcast) " + message,
				Convert.ToByte(TShock.Config.BroadcastRGB[0]), Convert.ToByte(TShock.Config.BroadcastRGB[1]),
				Convert.ToByte(TShock.Config.BroadcastRGB[2]));
		}

		private static void Off(CommandArgs args)
		{

			if (Main.ServerSideCharacter)
			{
				foreach (TSPlayer player in TShock.Players)
				{
					if (player != null && player.IsLoggedIn && !player.IsDisabledPendingTrashRemoval)
					{
						player.SaveServerCharacter();
					}
				}
			}

			string reason = ((args.Parameters.Count > 0) ? "Server shutting down: " + String.Join(" ", args.Parameters) : "Server shutting down!");
			TShock.Utils.StopServer(true, reason);
		}

		private static void OffNoSave(CommandArgs args)
		{
			string reason = ((args.Parameters.Count > 0) ? "Server shutting down: " + String.Join(" ", args.Parameters) : "Server shutting down!");
			TShock.Utils.StopServer(false, reason);
		}

		private static void CheckUpdates(CommandArgs args)
		{
			args.Player.SendInfoMessage("An update check has been queued.");
			try
			{
				TShock.UpdateManager.UpdateCheckAsync(null);
			}
			catch (Exception)
			{
				//swallow the exception
				return;
			}
		}

		#endregion Server Maintenence Commands

		#region Cause Events and Spawn Monsters Commands

		private static void DropMeteor(CommandArgs args)
		{
			WorldGen.spawnMeteor = false;
			WorldGen.dropMeteor();
			if (args.Silent)
			{
				args.Player.SendInfoMessage("A meteor has been triggered.");
			}
			else
			{
				TSPlayer.All.SendInfoMessage("{0} triggered a meteor.", args.Player.Name);
			}
		}

		private static void Fullmoon(CommandArgs args)
		{
			TSPlayer.Server.SetFullMoon();
			if (args.Silent)
			{
				args.Player.SendInfoMessage("Started a full moon.");
			}
			else
			{
				TSPlayer.All.SendInfoMessage("{0} started a full moon.", args.Player.Name);
			}
		}

		private static void Bloodmoon(CommandArgs args)
		{
			TSPlayer.Server.SetBloodMoon(!Main.bloodMoon);
			if (args.Silent)
			{
				args.Player.SendInfoMessage("{0}ed a blood moon.", Main.bloodMoon ? "start" : "stopp");
			}
			else
			{
				TSPlayer.All.SendInfoMessage("{0} {1}ed a blood moon.", args.Player.Name, Main.bloodMoon ? "start" : "stopp");
			}
		}

		private static void Eclipse(CommandArgs args)
		{
			TSPlayer.Server.SetEclipse(!Main.eclipse);
			if (args.Silent)
			{
				args.Player.SendInfoMessage("{0}ed an eclipse.", Main.eclipse ? "start" : "stopp");
			}
			else
			{
				TSPlayer.All.SendInfoMessage("{0} {1}ed an eclipse.", args.Player.Name, Main.eclipse ? "start" : "stopp");
			}
		}

		private static void Invade(CommandArgs args)
		{
			if (Main.invasionSize <= 0)
			{
				if (args.Parameters.Count < 1)
				{
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}invade <invasion type> [wave]", Specifier);
					return;
				}

				int wave = 1;
				switch (args.Parameters[0].ToLower())
				{
					case "goblin":
					case "goblins":
						TSPlayer.All.SendInfoMessage("{0} has started a goblin army invasion.", args.Player.Name);
						TShock.Utils.StartInvasion(1);
						break;

					case "snowman":
					case "snowmen":
						TSPlayer.All.SendInfoMessage("{0} has started a snow legion invasion.", args.Player.Name);
						TShock.Utils.StartInvasion(2);
						break;

					case "pirate":
					case "pirates":
						TSPlayer.All.SendInfoMessage("{0} has started a pirate invasion.", args.Player.Name);
						TShock.Utils.StartInvasion(3);
						break;

					case "pumpkin":
					case "pumpkinmoon":
						if (args.Parameters.Count > 1)
						{
							if (!int.TryParse(args.Parameters[1], out wave) || wave <= 0)
							{
								args.Player.SendErrorMessage("Invalid wave!");
								break;
							}
						}

						TSPlayer.Server.SetPumpkinMoon(true);
						Main.bloodMoon = false;
						NPC.waveKills = 0f;
						NPC.waveNumber = wave;
						TSPlayer.All.SendInfoMessage("{0} started the pumpkin moon at wave {1}!", args.Player.Name, wave);
						break;

					case "frost":
					case "frostmoon":
						if (args.Parameters.Count > 1)
						{
							if (!int.TryParse(args.Parameters[1], out wave) || wave <= 0)
							{
								args.Player.SendErrorMessage("Invalid wave!");
								return;
							}
						}

						TSPlayer.Server.SetFrostMoon(true);
						Main.bloodMoon = false;
						NPC.waveKills = 0f;
						NPC.waveNumber = wave;
						TSPlayer.All.SendInfoMessage("{0} started the frost moon at wave {1}!", args.Player.Name, wave);
						break;

					case "martian":
					case "martians":
						TSPlayer.All.SendInfoMessage("{0} has started a martian invasion.", args.Player.Name);
						TShock.Utils.StartInvasion(4);
						break;
				}
			}
			else if (DD2Event.Ongoing)
			{
				DD2Event.StopInvasion();
				TSPlayer.All.SendInfoMessage("{0} has ended the Old One's Army event.", args.Player.Name);
			}
			else
			{
				TSPlayer.All.SendInfoMessage("{0} has ended the invasion.", args.Player.Name);
				Main.invasionSize = 0;
			}
		}

		private static void ClearAnglerQuests(CommandArgs args)
		{
			if (args.Parameters.Count > 0)
			{
				var result = Main.anglerWhoFinishedToday.RemoveAll(s => s.ToLower().Equals(args.Parameters[0].ToLower()));
				if (result > 0)
				{
					args.Player.SendSuccessMessage("Removed {0} players from the angler quest completion list for today.", result);
					foreach (TSPlayer ply in TShock.Players.Where(p => p != null && p.Active && p.TPlayer.name.ToLower().Equals(args.Parameters[0].ToLower())))
					{
						//this will always tell the client that they have not done the quest today.
						ply.SendData((PacketTypes)74, "");
					}
				}
				else
					args.Player.SendErrorMessage("Failed to find any users by that name on the list.");

			}
			else
			{
				Main.anglerWhoFinishedToday.Clear();
				NetMessage.SendAnglerQuest(-1);
				args.Player.SendSuccessMessage("Cleared all users from the angler quest completion list for today.");
			}
		}

		private static void ToggleExpert(CommandArgs args)
		{
			Main.expertMode = !Main.expertMode;
			TSPlayer.All.SendData(PacketTypes.WorldInfo);
			args.Player.SendSuccessMessage("Expert mode is now {0}.", Main.expertMode ? "on" : "off");
		}

		private static void Hardmode(CommandArgs args)
		{
			if (Main.hardMode)
			{
				Main.hardMode = false;
				TSPlayer.All.SendData(PacketTypes.WorldInfo);
				args.Player.SendSuccessMessage("Hardmode is now off.");
			}
			else if (!TShock.Config.DisableHardmode)
			{
				WorldGen.StartHardmode();
				args.Player.SendSuccessMessage("Hardmode is now on.");
			}
			else
			{
				args.Player.SendErrorMessage("Hardmode is disabled via config.");
			}
		}

		private static void SpawnBoss(CommandArgs args)
		{
			if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}spawnboss <boss type> [amount]", Specifier);
				return;
			}

			int amount = 1;
			if (args.Parameters.Count == 2 && (!int.TryParse(args.Parameters[1], out amount) || amount <= 0))
			{
				args.Player.SendErrorMessage("Invalid boss amount!");
				return;
			}

			NPC npc = new NPC();
			switch (args.Parameters[0].ToLower())
			{
				case "*":
				case "all":
					int[] npcIds = { 4, 13, 35, 50, 125, 126, 127, 134, 222, 245, 262, 266, 370, 398 };
					TSPlayer.Server.SetTime(false, 0.0);
					foreach (int i in npcIds)
					{
						npc.SetDefaults(i);
						TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					}
					TSPlayer.All.SendSuccessMessage("{0} has spawned all bosses {1} time(s).", args.Player.Name, amount);
					return;
				case "brain":
				case "brain of cthulhu":
					npc.SetDefaults(266);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned the Brain of Cthulhu {1} time(s).", args.Player.Name, amount);
					return;
				case "destroyer":
					npc.SetDefaults(134);
					TSPlayer.Server.SetTime(false, 0.0);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned the Destroyer {1} time(s).", args.Player.Name, amount);
					return;
				case "duke":
				case "duke fishron":
				case "fishron":
					npc.SetDefaults(370);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned Duke Fishron {1} time(s).", args.Player.Name, amount);
					return;
				case "eater":
				case "eater of worlds":
					npc.SetDefaults(13);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned the Eater of Worlds {1} time(s).", args.Player.Name, amount);
					return;
				case "eye":
				case "eye of cthulhu":
					npc.SetDefaults(4);
					TSPlayer.Server.SetTime(false, 0.0);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned the Eye of Cthulhu {1} time(s).", args.Player.Name, amount);
					return;
				case "golem":
					npc.SetDefaults(245);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned Golem {1} time(s).", args.Player.Name, amount);
					return;
				case "king":
				case "king slime":
					npc.SetDefaults(50);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned King Slime {1} time(s).", args.Player.Name, amount);
					return;
				case "plantera":
					npc.SetDefaults(262);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned Plantera {1} time(s).", args.Player.Name, amount);
					return;
				case "prime":
				case "skeletron prime":
					npc.SetDefaults(127);
					TSPlayer.Server.SetTime(false, 0.0);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned Skeletron Prime {1} time(s).", args.Player.Name, amount);
					return;
				case "queen":
				case "queen bee":
					npc.SetDefaults(222);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned Queen Bee {1} time(s).", args.Player.Name, amount);
					return;
				case "skeletron":
					npc.SetDefaults(35);
					TSPlayer.Server.SetTime(false, 0.0);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned Skeletron {1} time(s).", args.Player.Name, amount);
					return;
				case "twins":
					TSPlayer.Server.SetTime(false, 0.0);
					npc.SetDefaults(125);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					npc.SetDefaults(126);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned the Twins {1} time(s).", args.Player.Name, amount);
					return;
				case "wof":
				case "wall of flesh":
					if (Main.wof >= 0)
					{
						args.Player.SendErrorMessage("There is already a Wall of Flesh!");
						return;
					}
					if (args.Player.Y / 16f < Main.maxTilesY - 205)
					{
						args.Player.SendErrorMessage("You must spawn the Wall of Flesh in hell!");
						return;
					}
					NPC.SpawnWOF(new Vector2(args.Player.X, args.Player.Y));
					TSPlayer.All.SendSuccessMessage("{0} has spawned the Wall of Flesh.", args.Player.Name);
					return;
				case "moon":
				case "moon lord":
					npc.SetDefaults(398);
					TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX, args.Player.TileY);
					TSPlayer.All.SendSuccessMessage("{0} has spawned the Moon Lord {1} time(s).", args.Player.Name, amount);
					return;
				default:
					args.Player.SendErrorMessage("Invalid boss type!");
					return;
			}
		}

		private static void SpawnMob(CommandArgs args)
		{
			if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}spawnmob <mob type> [amount]", Specifier);
				return;
			}
			if (args.Parameters[0].Length == 0)
			{
				args.Player.SendErrorMessage("Invalid mob type!");
				return;
			}

			int amount = 1;
			if (args.Parameters.Count == 2 && !int.TryParse(args.Parameters[1], out amount))
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}spawnmob <mob type> [amount]", Specifier);
				return;
			}

			amount = Math.Min(amount, Main.maxNPCs);

			var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
			if (npcs.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid mob type!");
			}
			else if (npcs.Count > 1)
			{
				args.Player.SendMultipleMatchError(npcs.Select(n => $"{n.FullName}({n.type})"));
			}
			else
			{
				var npc = npcs[0];
				if (npc.type >= 1 && npc.type < Main.maxNPCTypes && npc.type != 113)
				{
					TSPlayer.Server.SpawnNPC(npc.netID, npc.FullName, amount, args.Player.TileX, args.Player.TileY, 50, 20);
					if (args.Silent)
					{
						args.Player.SendSuccessMessage("Spawned {0} {1} time(s).", npc.FullName, amount);
					}
					else
					{
						TSPlayer.All.SendSuccessMessage("{0} has spawned {1} {2} time(s).", args.Player.Name, npc.FullName, amount);
					}
				}
				else if (npc.type == 113)
				{
					if (Main.wof >= 0 || (args.Player.Y / 16f < (Main.maxTilesY - 205)))
					{
						args.Player.SendErrorMessage("Can't spawn Wall of Flesh!");
						return;
					}
					NPC.SpawnWOF(new Vector2(args.Player.X, args.Player.Y));
					if (args.Silent)
					{
						args.Player.SendSuccessMessage("Spawned Wall of Flesh!");
					}
					else
					{
						TSPlayer.All.SendSuccessMessage("{0} has spawned a Wall of Flesh!", args.Player.Name);
					}
				}
				else
				{
					args.Player.SendErrorMessage("Invalid mob type!");
				}
			}
		}

		#endregion Cause Events and Spawn Monsters Commands

		#region Teleport Commands

		private static void Home(CommandArgs args)
		{
			args.Player.Spawn();
			args.Player.SendSuccessMessage("Teleported to your spawnpoint.");
		}

		private static void Spawn(CommandArgs args)
		{
			if (args.Player.Teleport(Main.spawnTileX * 16, (Main.spawnTileY * 16) - 48))
				args.Player.SendSuccessMessage("Teleported to the map's spawnpoint.");
		}

		private static void TP(CommandArgs args)
		{
			if (args.Parameters.Count != 1 && args.Parameters.Count != 2)
			{
				if (args.Player.HasPermission(Permissions.tpothers))
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tp <player> [player 2]", Specifier);
				else
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tp <player>", Specifier);
				return;
			}

			if (args.Parameters.Count == 1)
			{
				var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
				if (players.Count == 0)
					args.Player.SendErrorMessage("Invalid player!");
				else if (players.Count > 1)
					args.Player.SendMultipleMatchError(players.Select(p => p.Name));
				else
				{
					var target = players[0];
					if (!target.TPAllow && !args.Player.HasPermission(Permissions.tpoverride))
					{
						args.Player.SendErrorMessage("{0} has disabled players from teleporting.", target.Name);
						return;
					}
					if (args.Player.Teleport(target.TPlayer.position.X, target.TPlayer.position.Y))
					{
						args.Player.SendSuccessMessage("Teleported to {0}.", target.Name);
						if (!args.Player.HasPermission(Permissions.tpsilent))
							target.SendInfoMessage("{0} teleported to you.", args.Player.Name);
					}
				}
			}
			else
			{
				if (!args.Player.HasPermission(Permissions.tpothers))
				{
					args.Player.SendErrorMessage("You do not have access to this command.");
					return;
				}

				var players1 = TSPlayer.FindByNameOrID(args.Parameters[0]);
				var players2 = TSPlayer.FindByNameOrID(args.Parameters[1]);

				if (players2.Count == 0)
					args.Player.SendErrorMessage("Invalid player!");
				else if (players2.Count > 1)
					args.Player.SendMultipleMatchError(players2.Select(p => p.Name));
				else if (players1.Count == 0)
				{
					if (args.Parameters[0] == "*")
					{
						if (!args.Player.HasPermission(Permissions.tpallothers))
						{
							args.Player.SendErrorMessage("You do not have access to this command.");
							return;
						}

						var target = players2[0];
						foreach (var source in TShock.Players.Where(p => p != null && p != args.Player))
						{
							if (!target.TPAllow && !args.Player.HasPermission(Permissions.tpoverride))
								continue;
							if (source.Teleport(target.TPlayer.position.X, target.TPlayer.position.Y))
							{
								if (args.Player != source)
								{
									if (args.Player.HasPermission(Permissions.tpsilent))
										source.SendSuccessMessage("You were teleported to {0}.", target.Name);
									else
										source.SendSuccessMessage("{0} teleported you to {1}.", args.Player.Name, target.Name);
								}
								if (args.Player != target)
								{
									if (args.Player.HasPermission(Permissions.tpsilent))
										target.SendInfoMessage("{0} was teleported to you.", source.Name);
									if (!args.Player.HasPermission(Permissions.tpsilent))
										target.SendInfoMessage("{0} teleported {1} to you.", args.Player.Name, source.Name);
								}
							}
						}
						args.Player.SendSuccessMessage("Teleported everyone to {0}.", target.Name);
					}
					else
						args.Player.SendErrorMessage("Invalid player!");
				}
				else if (players1.Count > 1)
					args.Player.SendMultipleMatchError(players1.Select(p => p.Name));
				else
				{
					var source = players1[0];
					if (!source.TPAllow && !args.Player.HasPermission(Permissions.tpoverride))
					{
						args.Player.SendErrorMessage("{0} has disabled players from teleporting.", source.Name);
						return;
					}
					var target = players2[0];
					if (!target.TPAllow && !args.Player.HasPermission(Permissions.tpoverride))
					{
						args.Player.SendErrorMessage("{0} has disabled players from teleporting.", target.Name);
						return;
					}
					args.Player.SendSuccessMessage("Teleported {0} to {1}.", source.Name, target.Name);
					if (source.Teleport(target.TPlayer.position.X, target.TPlayer.position.Y))
					{
						if (args.Player != source)
						{
							if (args.Player.HasPermission(Permissions.tpsilent))
								source.SendSuccessMessage("You were teleported to {0}.", target.Name);
							else
								source.SendSuccessMessage("{0} teleported you to {1}.", args.Player.Name, target.Name);
						}
						if (args.Player != target)
						{
							if (args.Player.HasPermission(Permissions.tpsilent))
								target.SendInfoMessage("{0} was teleported to you.", source.Name);
							if (!args.Player.HasPermission(Permissions.tpsilent))
								target.SendInfoMessage("{0} teleported {1} to you.", args.Player.Name, source.Name);
						}
					}
				}
			}
		}

		private static void TPHere(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				if (args.Player.HasPermission(Permissions.tpallothers))
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tphere <player|*>", Specifier);
				else
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tphere <player>", Specifier);
				return;
			}

			string playerName = String.Join(" ", args.Parameters);
			var players = TSPlayer.FindByNameOrID(playerName);
			if (players.Count == 0)
			{
				if (playerName == "*")
				{
					if (!args.Player.HasPermission(Permissions.tpallothers))
					{
						args.Player.SendErrorMessage("You do not have permission to use this command.");
						return;
					}
					for (int i = 0; i < Main.maxPlayers; i++)
					{
						if (Main.player[i].active && (Main.player[i] != args.TPlayer))
						{
							if (TShock.Players[i].Teleport(args.TPlayer.position.X, args.TPlayer.position.Y))
								TShock.Players[i].SendSuccessMessage(String.Format("You were teleported to {0}.", args.Player.Name));
						}
					}
					args.Player.SendSuccessMessage("Teleported everyone to yourself.");
				}
				else
					args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			else
			{
				var plr = players[0];
				if (plr.Teleport(args.TPlayer.position.X, args.TPlayer.position.Y))
				{
					plr.SendInfoMessage("You were teleported to {0}.", args.Player.Name);
					args.Player.SendSuccessMessage("Teleported {0} to yourself.", plr.Name);
				}
			}
		}

		private static void TPNpc(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tpnpc <NPC>", Specifier);
				return;
			}

			var npcStr = string.Join(" ", args.Parameters);
			var matches = new List<NPC>();
			foreach (var npc in Main.npc.Where(npc => npc.active))
			{
				var englishName = EnglishLanguage.GetNpcNameById(npc.netID);

				if (string.Equals(npc.FullName, npcStr, StringComparison.InvariantCultureIgnoreCase) ||
				    string.Equals(englishName, npcStr, StringComparison.InvariantCultureIgnoreCase))
				{
					matches = new List<NPC> { npc };
					break;
				}
				if (npc.FullName.ToLowerInvariant().StartsWith(npcStr.ToLowerInvariant()) ||
				    englishName?.StartsWith(npcStr, StringComparison.InvariantCultureIgnoreCase) == true)
					matches.Add(npc);
			}

			if (matches.Count > 1)
			{
				args.Player.SendMultipleMatchError(matches.Select(n => $"{n.FullName}({n.whoAmI})"));
				return;
			}
			if (matches.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid NPC!");
				return;
			}

			var target = matches[0];
			args.Player.Teleport(target.position.X, target.position.Y);
			args.Player.SendSuccessMessage("Teleported to the '{0}'.", target.FullName);
		}

		private static void GetPos(CommandArgs args)
		{
			var player = args.Player.Name;
			if (args.Parameters.Count > 0)
			{
				player = String.Join(" ", args.Parameters);
			}

			var players = TSPlayer.FindByNameOrID(player);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			}
			else
			{
				args.Player.SendSuccessMessage("Location of {0} is ({1}, {2}).", players[0].Name, players[0].TileX, players[0].TileY);
			}
		}

		private static void TPPos(CommandArgs args)
		{
			if (args.Parameters.Count != 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tppos <tile x> <tile y>", Specifier);
				return;
			}

			int x, y;
			if (!int.TryParse(args.Parameters[0], out x) || !int.TryParse(args.Parameters[1], out y))
			{
				args.Player.SendErrorMessage("Invalid tile positions!");
				return;
			}
			x = Math.Max(0, x);
			y = Math.Max(0, y);
			x = Math.Min(x, Main.maxTilesX - 1);
			y = Math.Min(y, Main.maxTilesY - 1);

			args.Player.Teleport(16 * x, 16 * y);
			args.Player.SendSuccessMessage("Teleported to {0}, {1}!", x, y);
		}

		private static void TPAllow(CommandArgs args)
		{
			if (!args.Player.TPAllow)
				args.Player.SendSuccessMessage("You have removed your teleportation protection.");
			if (args.Player.TPAllow)
				args.Player.SendSuccessMessage("You have enabled teleportation protection.");
			args.Player.TPAllow = !args.Player.TPAllow;
		}

		private static void Warp(CommandArgs args)
		{
			bool hasManageWarpPermission = args.Player.HasPermission(Permissions.managewarp);
			if (args.Parameters.Count < 1)
			{
				if (hasManageWarpPermission)
				{
					args.Player.SendInfoMessage("Invalid syntax! Proper syntax: {0}warp [command] [arguments]", Specifier);
					args.Player.SendInfoMessage("Commands: add, del, hide, list, send, [warpname]");
					args.Player.SendInfoMessage("Arguments: add [warp name], del [warp name], list [page]");
					args.Player.SendInfoMessage("Arguments: send [player] [warp name], hide [warp name] [Enable(true/false)]");
					args.Player.SendInfoMessage("Examples: {0}warp add foobar, {0}warp hide foobar true, {0}warp foobar", Specifier);
					return;
				}
				else
				{
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}warp [name] or {0}warp list <page>", Specifier);
					return;
				}
			}

			if (args.Parameters[0].Equals("list"))
			{
				#region List warps
				int pageNumber;
				if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
					return;
				IEnumerable<string> warpNames = from warp in TShock.Warps.Warps
												where !warp.IsPrivate
												select warp.Name;
				PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(warpNames),
					new PaginationTools.Settings
					{
						HeaderFormat = "Warps ({0}/{1}):",
						FooterFormat = "Type {0}warp list {{0}} for more.".SFormat(Specifier),
						NothingToDisplayString = "There are currently no warps defined."
					});
				#endregion
			}
			else if (args.Parameters[0].ToLower() == "add" && hasManageWarpPermission)
			{
				#region Add warp
				if (args.Parameters.Count == 2)
				{
					string warpName = args.Parameters[1];
					if (warpName == "list" || warpName == "hide" || warpName == "del" || warpName == "add")
					{
						args.Player.SendErrorMessage("Name reserved, use a different name.");
					}
					else if (TShock.Warps.Add(args.Player.TileX, args.Player.TileY, warpName))
					{
						args.Player.SendSuccessMessage("Warp added: " + warpName);
					}
					else
					{
						args.Player.SendErrorMessage("Warp " + warpName + " already exists.");
					}
				}
				else
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}warp add [name]", Specifier);
				#endregion
			}
			else if (args.Parameters[0].ToLower() == "del" && hasManageWarpPermission)
			{
				#region Del warp
				if (args.Parameters.Count == 2)
				{
					string warpName = args.Parameters[1];
					if (TShock.Warps.Remove(warpName))
					{
						args.Player.SendSuccessMessage("Warp deleted: " + warpName);
					}
					else
						args.Player.SendErrorMessage("Could not find the specified warp.");
				}
				else
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}warp del [name]", Specifier);
				#endregion
			}
			else if (args.Parameters[0].ToLower() == "hide" && hasManageWarpPermission)
			{
				#region Hide warp
				if (args.Parameters.Count == 3)
				{
					string warpName = args.Parameters[1];
					bool state = false;
					if (Boolean.TryParse(args.Parameters[2], out state))
					{
						if (TShock.Warps.Hide(args.Parameters[1], state))
						{
							if (state)
								args.Player.SendSuccessMessage("Warp " + warpName + " is now private.");
							else
								args.Player.SendSuccessMessage("Warp " + warpName + " is now public.");
						}
						else
							args.Player.SendErrorMessage("Could not find specified warp.");
					}
					else
						args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}warp hide [name] <true/false>", Specifier);
				}
				else
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}warp hide [name] <true/false>", Specifier);
				#endregion
			}
			else if (args.Parameters[0].ToLower() == "send" && args.Player.HasPermission(Permissions.tpothers))
			{
				#region Warp send
				if (args.Parameters.Count < 3)
				{
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}warp send [player] [warpname]", Specifier);
					return;
				}

				var foundplr = TSPlayer.FindByNameOrID(args.Parameters[1]);
				if (foundplr.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid player!");
					return;
				}
				else if (foundplr.Count > 1)
				{
					args.Player.SendMultipleMatchError(foundplr.Select(p => p.Name));
					return;
				}

				string warpName = args.Parameters[2];
				var warp = TShock.Warps.Find(warpName);
				var plr = foundplr[0];
				if (warp.Position != Point.Zero)
				{
					if (plr.Teleport(warp.Position.X * 16, warp.Position.Y * 16))
					{
						plr.SendSuccessMessage(String.Format("{0} warped you to {1}.", args.Player.Name, warpName));
						args.Player.SendSuccessMessage(String.Format("You warped {0} to {1}.", plr.Name, warpName));
					}
				}
				else
				{
					args.Player.SendErrorMessage("Specified warp not found.");
				}
				#endregion
			}
			else
			{
				string warpName = String.Join(" ", args.Parameters);
				var warp = TShock.Warps.Find(warpName);
				if (warp != null)
				{
					if (args.Player.Teleport(warp.Position.X * 16, warp.Position.Y * 16))
						args.Player.SendSuccessMessage("Warped to " + warpName + ".");
				}
				else
				{
					args.Player.SendErrorMessage("The specified warp was not found.");
				}
			}
		}

		#endregion Teleport Commands

		#region Group Management

		private static void Group(CommandArgs args)
		{
			string subCmd = args.Parameters.Count == 0 ? "help" : args.Parameters[0].ToLower();

			switch (subCmd)
			{
				case "add":
					#region Add group
					{
						if (args.Parameters.Count < 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group add <group name> [permissions]", Specifier);
							return;
						}

						string groupName = args.Parameters[1];
						args.Parameters.RemoveRange(0, 2);
						string permissions = String.Join(",", args.Parameters);

						try
						{
							TShock.Groups.AddGroup(groupName, null, permissions, TShockAPI.Group.defaultChatColor);
							args.Player.SendSuccessMessage("The group was added successfully!");
						}
						catch (GroupExistsException)
						{
							args.Player.SendErrorMessage("That group already exists!");
						}
						catch (GroupManagerException ex)
						{
							args.Player.SendErrorMessage(ex.ToString());
						}
					}
					#endregion
					return;
				case "addperm":
					#region Add permissions
					{
						if (args.Parameters.Count < 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group addperm <group name> <permissions...>", Specifier);
							return;
						}

						string groupName = args.Parameters[1];
						args.Parameters.RemoveRange(0, 2);
						if (groupName == "*")
						{
							foreach (Group g in TShock.Groups)
							{
								TShock.Groups.AddPermissions(g.Name, args.Parameters);
							}
							args.Player.SendSuccessMessage("Modified all groups.");
							return;
						}
						try
						{
							string response = TShock.Groups.AddPermissions(groupName, args.Parameters);
							if (response.Length > 0)
							{
								args.Player.SendSuccessMessage(response);
							}
							return;
						}
						catch (GroupManagerException ex)
						{
							args.Player.SendErrorMessage(ex.ToString());
						}
					}
					#endregion
					return;
				case "help":
					#region Help
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;

						var lines = new List<string>
						{
							"add <name> <permissions...> - Adds a new group.",
							"addperm <group> <permissions...> - Adds permissions to a group.",
							"color <group> <rrr,ggg,bbb> - Changes a group's chat color.",
							"rename <group> <new name> - Changes a group's name.",
							"del <group> - Deletes a group.",
							"delperm <group> <permissions...> - Removes permissions from a group.",
							"list [page] - Lists groups.",
							"listperm <group> [page] - Lists a group's permissions.",
							"parent <group> <parent group> - Changes a group's parent group.",
							"prefix <group> <prefix> - Changes a group's prefix.",
							"suffix <group> <suffix> - Changes a group's suffix."
						};

						PaginationTools.SendPage(args.Player, pageNumber, lines,
							new PaginationTools.Settings
							{
								HeaderFormat = "Group Sub-Commands ({0}/{1}):",
								FooterFormat = "Type {0}group help {{0}} for more sub-commands.".SFormat(Specifier)
							}
						);
					}
					#endregion
					return;
				case "parent":
					#region Parent
					{
						if (args.Parameters.Count < 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group parent <group name> [new parent group name]", Specifier);
							return;
						}

						string groupName = args.Parameters[1];
						Group group = TShock.Groups.GetGroupByName(groupName);
						if (group == null)
						{
							args.Player.SendErrorMessage("No such group \"{0}\".", groupName);
							return;
						}

						if (args.Parameters.Count > 2)
						{
							string newParentGroupName = string.Join(" ", args.Parameters.Skip(2));
							if (!string.IsNullOrWhiteSpace(newParentGroupName) && !TShock.Groups.GroupExists(newParentGroupName))
							{
								args.Player.SendErrorMessage("No such group \"{0}\".", newParentGroupName);
								return;
							}

							try
							{
								TShock.Groups.UpdateGroup(groupName, newParentGroupName, group.Permissions, group.ChatColor, group.Suffix, group.Prefix);

								if (!string.IsNullOrWhiteSpace(newParentGroupName))
									args.Player.SendSuccessMessage("Parent of group \"{0}\" set to \"{1}\".", groupName, newParentGroupName);
								else
									args.Player.SendSuccessMessage("Removed parent of group \"{0}\".", groupName);
							}
							catch (GroupManagerException ex)
							{
								args.Player.SendErrorMessage(ex.Message);
							}
						}
						else
						{
							if (group.Parent != null)
								args.Player.SendSuccessMessage("Parent of \"{0}\" is \"{1}\".", group.Name, group.Parent.Name);
							else
								args.Player.SendSuccessMessage("Group \"{0}\" has no parent.", group.Name);
						}
					}
					#endregion
					return;
				case "suffix":
					#region Suffix
					{
						if (args.Parameters.Count < 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group suffix <group name> [new suffix]", Specifier);
							return;
						}

						string groupName = args.Parameters[1];
						Group group = TShock.Groups.GetGroupByName(groupName);
						if (group == null)
						{
							args.Player.SendErrorMessage("No such group \"{0}\".", groupName);
							return;
						}

						if (args.Parameters.Count > 2)
						{
							string newSuffix = string.Join(" ", args.Parameters.Skip(2));

							try
							{
								TShock.Groups.UpdateGroup(groupName, group.Parent.Name, group.Permissions, group.ChatColor, newSuffix, group.Prefix);

								if (!string.IsNullOrWhiteSpace(newSuffix))
									args.Player.SendSuccessMessage("Suffix of group \"{0}\" set to \"{1}\".", groupName, newSuffix);
								else
									args.Player.SendSuccessMessage("Removed suffix of group \"{0}\".", groupName);
							}
							catch (GroupManagerException ex)
							{
								args.Player.SendErrorMessage(ex.Message);
							}
						}
						else
						{
							if (!string.IsNullOrWhiteSpace(group.Suffix))
								args.Player.SendSuccessMessage("Suffix of \"{0}\" is \"{1}\".", group.Name, group.Suffix);
							else
								args.Player.SendSuccessMessage("Group \"{0}\" has no suffix.", group.Name);
						}
					}
					#endregion
					return;
				case "prefix":
					#region Prefix
					{
						if (args.Parameters.Count < 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group prefix <group name> [new prefix]", Specifier);
							return;
						}

						string groupName = args.Parameters[1];
						Group group = TShock.Groups.GetGroupByName(groupName);
						if (group == null)
						{
							args.Player.SendErrorMessage("No such group \"{0}\".", groupName);
							return;
						}

						if (args.Parameters.Count > 2)
						{
							string newPrefix = string.Join(" ", args.Parameters.Skip(2));

							try
							{
								TShock.Groups.UpdateGroup(groupName, group.Parent.Name, group.Permissions, group.ChatColor, group.Suffix, newPrefix);

								if (!string.IsNullOrWhiteSpace(newPrefix))
									args.Player.SendSuccessMessage("Prefix of group \"{0}\" set to \"{1}\".", groupName, newPrefix);
								else
									args.Player.SendSuccessMessage("Removed prefix of group \"{0}\".", groupName);
							}
							catch (GroupManagerException ex)
							{
								args.Player.SendErrorMessage(ex.Message);
							}
						}
						else
						{
							if (!string.IsNullOrWhiteSpace(group.Prefix))
								args.Player.SendSuccessMessage("Prefix of \"{0}\" is \"{1}\".", group.Name, group.Prefix);
							else
								args.Player.SendSuccessMessage("Group \"{0}\" has no prefix.", group.Name);
						}
					}
					#endregion
					return;
				case "color":
					#region Color
					{
						if (args.Parameters.Count < 2 || args.Parameters.Count > 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group color <group name> [new color(000,000,000)]", Specifier);
							return;
						}

						string groupName = args.Parameters[1];
						Group group = TShock.Groups.GetGroupByName(groupName);
						if (group == null)
						{
							args.Player.SendErrorMessage("No such group \"{0}\".", groupName);
							return;
						}

						if (args.Parameters.Count == 3)
						{
							string newColor = args.Parameters[2];

							String[] parts = newColor.Split(',');
							byte r;
							byte g;
							byte b;
							if (parts.Length == 3 && byte.TryParse(parts[0], out r) && byte.TryParse(parts[1], out g) && byte.TryParse(parts[2], out b))
							{
								try
								{
									TShock.Groups.UpdateGroup(groupName, group.Parent.Name, group.Permissions, newColor, group.Suffix, group.Prefix);

									args.Player.SendSuccessMessage("Color of group \"{0}\" set to \"{1}\".", groupName, newColor);
								}
								catch (GroupManagerException ex)
								{
									args.Player.SendErrorMessage(ex.Message);
								}
							}
							else
							{
								args.Player.SendErrorMessage("Invalid syntax for color, expected \"rrr,ggg,bbb\"");
							}
						}
						else
						{
							args.Player.SendSuccessMessage("Color of \"{0}\" is \"{1}\".", group.Name, group.ChatColor);
						}
					}
					#endregion
					return;
				case "rename":
					#region Rename group
					{
						if (args.Parameters.Count != 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group rename <group> <new name>", Specifier);
							return;
						}

						string group = args.Parameters[1];
						string newName = args.Parameters[2];
						try
						{
							string response = TShock.Groups.RenameGroup(group, newName);
							args.Player.SendSuccessMessage(response);
						}
						catch (GroupManagerException ex)
						{
							args.Player.SendErrorMessage(ex.Message);
						}
					}
					#endregion
					return;
				case "del":
					#region Delete group
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group del <group name>", Specifier);
							return;
						}

						try
						{
							string response = TShock.Groups.DeleteGroup(args.Parameters[1]);
							if (response.Length > 0)
							{
								args.Player.SendSuccessMessage(response);
							}
						}
						catch (GroupManagerException ex)
						{
							args.Player.SendErrorMessage(ex.ToString());
						}
					}
					#endregion
					return;
				case "delperm":
					#region Delete permissions
					{
						if (args.Parameters.Count < 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group delperm <group name> <permissions...>", Specifier);
							return;
						}

						string groupName = args.Parameters[1];
						args.Parameters.RemoveRange(0, 2);
						if (groupName == "*")
						{
							foreach (Group g in TShock.Groups)
							{
								TShock.Groups.DeletePermissions(g.Name, args.Parameters);
							}
							args.Player.SendSuccessMessage("Modified all groups.");
							return;
						}
						try
						{
							string response = TShock.Groups.DeletePermissions(groupName, args.Parameters);
							if (response.Length > 0)
							{
								args.Player.SendSuccessMessage(response);
							}
							return;
						}
						catch (GroupManagerException ex)
						{
							args.Player.SendErrorMessage(ex.ToString());
						}
					}
					#endregion
					return;
				case "list":
					#region List groups
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;
						var groupNames = from grp in TShock.Groups.groups
										 select grp.Name;
						PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(groupNames),
							new PaginationTools.Settings
							{
								HeaderFormat = "Groups ({0}/{1}):",
								FooterFormat = "Type {0}group list {{0}} for more.".SFormat(Specifier)
							});
					}
					#endregion
					return;
				case "listperm":
					#region List permissions
					{
						if (args.Parameters.Count == 1)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}group listperm <group name> [page]", Specifier);
							return;
						}
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 2, args.Player, out pageNumber))
							return;

						if (!TShock.Groups.GroupExists(args.Parameters[1]))
						{
							args.Player.SendErrorMessage("Invalid group.");
							return;
						}
						Group grp = TShock.Groups.GetGroupByName(args.Parameters[1]);
						List<string> permissions = grp.TotalPermissions;

						PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(permissions),
							new PaginationTools.Settings
							{
								HeaderFormat = "Permissions for " + grp.Name + " ({0}/{1}):",
								FooterFormat = "Type {0}group listperm {1} {{0}} for more.".SFormat(Specifier, grp.Name),
								NothingToDisplayString = "There are currently no permissions for " + grp.Name + "."
							});
					}
					#endregion
					return;
			}
		}
		#endregion Group Management

		#region Item Management

		private static void ItemBan(CommandArgs args)
		{
			string subCmd = args.Parameters.Count == 0 ? "help" : args.Parameters[0].ToLower();
			switch (subCmd)
			{
				case "add":
					#region Add item
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}itemban add <item name>", Specifier);
							return;
						}

						List<Item> items = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
						if (items.Count == 0)
						{
							args.Player.SendErrorMessage("Invalid item.");
						}
						else if (items.Count > 1)
						{
							args.Player.SendMultipleMatchError(items.Select(i => $"{i.Name}({i.netID})"));
						}
						else
						{
							TShock.Itembans.AddNewBan(EnglishLanguage.GetItemNameById(items[0].type));
							args.Player.SendSuccessMessage("Banned " + items[0].Name + ".");
						}
					}
					#endregion
					return;
				case "allow":
					#region Allow group to item
					{
						if (args.Parameters.Count != 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}itemban allow <item name> <group name>", Specifier);
							return;
						}

						List<Item> items = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
						if (items.Count == 0)
						{
							args.Player.SendErrorMessage("Invalid item.");
						}
						else if (items.Count > 1)
						{
							args.Player.SendMultipleMatchError(items.Select(i => $"{i.Name}({i.netID})"));
						}
						else
						{
							if (!TShock.Groups.GroupExists(args.Parameters[2]))
							{
								args.Player.SendErrorMessage("Invalid group.");
								return;
							}

							ItemBan ban = TShock.Itembans.GetItemBanByName(EnglishLanguage.GetItemNameById(items[0].type));
							if (ban == null)
							{
								args.Player.SendErrorMessage("{0} is not banned.", items[0].Name);
								return;
							}
							if (!ban.AllowedGroups.Contains(args.Parameters[2]))
							{
								TShock.Itembans.AllowGroup(EnglishLanguage.GetItemNameById(items[0].type), args.Parameters[2]);
								args.Player.SendSuccessMessage("{0} has been allowed to use {1}.", args.Parameters[2], items[0].Name);
							}
							else
							{
								args.Player.SendWarningMessage("{0} is already allowed to use {1}.", args.Parameters[2], items[0].Name);
							}
						}
					}
					#endregion
					return;
				case "del":
					#region Delete item
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}itemban del <item name>", Specifier);
							return;
						}

						List<Item> items = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
						if (items.Count == 0)
						{
							args.Player.SendErrorMessage("Invalid item.");
						}
						else if (items.Count > 1)
						{
							args.Player.SendMultipleMatchError(items.Select(i => $"{i.Name}({i.netID})"));
						}
						else
						{
							TShock.Itembans.RemoveBan(EnglishLanguage.GetItemNameById(items[0].type));
							args.Player.SendSuccessMessage("Unbanned " + items[0].Name + ".");
						}
					}
					#endregion
					return;
				case "disallow":
					#region Disllow group from item
					{
						if (args.Parameters.Count != 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}itemban disallow <item name> <group name>", Specifier);
							return;
						}

						List<Item> items = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
						if (items.Count == 0)
						{
							args.Player.SendErrorMessage("Invalid item.");
						}
						else if (items.Count > 1)
						{
							args.Player.SendMultipleMatchError(items.Select(i => $"{i.Name}({i.netID})"));
						}
						else
						{
							if (!TShock.Groups.GroupExists(args.Parameters[2]))
							{
								args.Player.SendErrorMessage("Invalid group.");
								return;
							}

							ItemBan ban = TShock.Itembans.GetItemBanByName(EnglishLanguage.GetItemNameById(items[0].type));
							if (ban == null)
							{
								args.Player.SendErrorMessage("{0} is not banned.", items[0].Name);
								return;
							}
							if (ban.AllowedGroups.Contains(args.Parameters[2]))
							{
								TShock.Itembans.RemoveGroup(EnglishLanguage.GetItemNameById(items[0].type), args.Parameters[2]);
								args.Player.SendSuccessMessage("{0} has been disallowed to use {1}.", args.Parameters[2], items[0].Name);
							}
							else
							{
								args.Player.SendWarningMessage("{0} is already disallowed to use {1}.", args.Parameters[2], items[0].Name);
							}
						}
					}
					#endregion
					return;
				case "help":
					#region Help
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;

						var lines = new List<string>
						{
							"add <item> - Adds an item ban.",
							"allow <item> <group> - Allows a group to use an item.",
							"del <item> - Deletes an item ban.",
							"disallow <item> <group> - Disallows a group from using an item.",
							"list [page] - Lists all item bans."
						};

						PaginationTools.SendPage(args.Player, pageNumber, lines,
							new PaginationTools.Settings
							{
								HeaderFormat = "Item Ban Sub-Commands ({0}/{1}):",
								FooterFormat = "Type {0}itemban help {{0}} for more sub-commands.".SFormat(Specifier)
							}
						);
					}
					#endregion
					return;
				case "list":
					#region List items
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;
						IEnumerable<string> itemNames = from itemBan in TShock.Itembans.ItemBans
														select itemBan.Name;
						PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(itemNames),
							new PaginationTools.Settings
							{
								HeaderFormat = "Item bans ({0}/{1}):",
								FooterFormat = "Type {0}itemban list {{0}} for more.".SFormat(Specifier),
								NothingToDisplayString = "There are currently no banned items."
							});
					}
					#endregion
					return;
			}
		}
		#endregion Item Management

		#region Projectile Management

		private static void ProjectileBan(CommandArgs args)
		{
			string subCmd = args.Parameters.Count == 0 ? "help" : args.Parameters[0].ToLower();
			switch (subCmd)
			{
				case "add":
					#region Add projectile
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}projban add <proj id>", Specifier);
							return;
						}
						short id;
						if (Int16.TryParse(args.Parameters[1], out id) && id > 0 && id < Main.maxProjectileTypes)
						{
							TShock.ProjectileBans.AddNewBan(id);
							args.Player.SendSuccessMessage("Banned projectile {0}.", id);
						}
						else
							args.Player.SendErrorMessage("Invalid projectile ID!");
					}
					#endregion
					return;
				case "allow":
					#region Allow group to projectile
					{
						if (args.Parameters.Count != 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}projban allow <id> <group>", Specifier);
							return;
						}

						short id;
						if (Int16.TryParse(args.Parameters[1], out id) && id > 0 && id < Main.maxProjectileTypes)
						{
							if (!TShock.Groups.GroupExists(args.Parameters[2]))
							{
								args.Player.SendErrorMessage("Invalid group.");
								return;
							}

							ProjectileBan ban = TShock.ProjectileBans.GetBanById(id);
							if (ban == null)
							{
								args.Player.SendErrorMessage("Projectile {0} is not banned.", id);
								return;
							}
							if (!ban.AllowedGroups.Contains(args.Parameters[2]))
							{
								TShock.ProjectileBans.AllowGroup(id, args.Parameters[2]);
								args.Player.SendSuccessMessage("{0} has been allowed to use projectile {1}.", args.Parameters[2], id);
							}
							else
								args.Player.SendWarningMessage("{0} is already allowed to use projectile {1}.", args.Parameters[2], id);
						}
						else
							args.Player.SendErrorMessage("Invalid projectile ID!");
					}
					#endregion
					return;
				case "del":
					#region Delete projectile
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}projban del <id>", Specifier);
							return;
						}

						short id;
						if (Int16.TryParse(args.Parameters[1], out id) && id > 0 && id < Main.maxProjectileTypes)
						{
							TShock.ProjectileBans.RemoveBan(id);
							args.Player.SendSuccessMessage("Unbanned projectile {0}.", id);
							return;
						}
						else
							args.Player.SendErrorMessage("Invalid projectile ID!");
					}
					#endregion
					return;
				case "disallow":
					#region Disallow group from projectile
					{
						if (args.Parameters.Count != 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}projban disallow <id> <group name>", Specifier);
							return;
						}

						short id;
						if (Int16.TryParse(args.Parameters[1], out id) && id > 0 && id < Main.maxProjectileTypes)
						{
							if (!TShock.Groups.GroupExists(args.Parameters[2]))
							{
								args.Player.SendErrorMessage("Invalid group.");
								return;
							}

							ProjectileBan ban = TShock.ProjectileBans.GetBanById(id);
							if (ban == null)
							{
								args.Player.SendErrorMessage("Projectile {0} is not banned.", id);
								return;
							}
							if (ban.AllowedGroups.Contains(args.Parameters[2]))
							{
								TShock.ProjectileBans.RemoveGroup(id, args.Parameters[2]);
								args.Player.SendSuccessMessage("{0} has been disallowed from using projectile {1}.", args.Parameters[2], id);
								return;
							}
							else
								args.Player.SendWarningMessage("{0} is already prevented from using projectile {1}.", args.Parameters[2], id);
						}
						else
							args.Player.SendErrorMessage("Invalid projectile ID!");
					}
					#endregion
					return;
				case "help":
					#region Help
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;

						var lines = new List<string>
						{
							"add <projectile ID> - Adds a projectile ban.",
							"allow <projectile ID> <group> - Allows a group to use a projectile.",
							"del <projectile ID> - Deletes an projectile ban.",
							"disallow <projectile ID> <group> - Disallows a group from using a projectile.",
							"list [page] - Lists all projectile bans."
						};

						PaginationTools.SendPage(args.Player, pageNumber, lines,
							new PaginationTools.Settings
							{
								HeaderFormat = "Projectile Ban Sub-Commands ({0}/{1}):",
								FooterFormat = "Type {0}projban help {{0}} for more sub-commands.".SFormat(Specifier)
							}
						);
					}
					#endregion
					return;
				case "list":
					#region List projectiles
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;
						IEnumerable<Int16> projectileIds = from projectileBan in TShock.ProjectileBans.ProjectileBans
														   select projectileBan.ID;
						PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(projectileIds),
							new PaginationTools.Settings
							{
								HeaderFormat = "Projectile bans ({0}/{1}):",
								FooterFormat = "Type {0}projban list {{0}} for more.".SFormat(Specifier),
								NothingToDisplayString = "There are currently no banned projectiles."
							});
					}
					#endregion
					return;
			}
		}
		#endregion Projectile Management

		#region Tile Management
		private static void TileBan(CommandArgs args)
		{
			string subCmd = args.Parameters.Count == 0 ? "help" : args.Parameters[0].ToLower();
			switch (subCmd)
			{
				case "add":
					#region Add tile
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tileban add <tile id>", Specifier);
							return;
						}
						short id;
						if (Int16.TryParse(args.Parameters[1], out id) && id >= 0 && id < Main.maxTileSets)
						{
							TShock.TileBans.AddNewBan(id);
							args.Player.SendSuccessMessage("Banned tile {0}.", id);
						}
						else
							args.Player.SendErrorMessage("Invalid tile ID!");
					}
					#endregion
					return;
				case "allow":
					#region Allow group to place tile
					{
						if (args.Parameters.Count != 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tileban allow <id> <group>", Specifier);
							return;
						}

						short id;
						if (Int16.TryParse(args.Parameters[1], out id) && id >= 0 && id < Main.maxTileSets)
						{
							if (!TShock.Groups.GroupExists(args.Parameters[2]))
							{
								args.Player.SendErrorMessage("Invalid group.");
								return;
							}

							TileBan ban = TShock.TileBans.GetBanById(id);
							if (ban == null)
							{
								args.Player.SendErrorMessage("Tile {0} is not banned.", id);
								return;
							}
							if (!ban.AllowedGroups.Contains(args.Parameters[2]))
							{
								TShock.TileBans.AllowGroup(id, args.Parameters[2]);
								args.Player.SendSuccessMessage("{0} has been allowed to place tile {1}.", args.Parameters[2], id);
							}
							else
								args.Player.SendWarningMessage("{0} is already allowed to place tile {1}.", args.Parameters[2], id);
						}
						else
							args.Player.SendErrorMessage("Invalid tile ID!");
					}
					#endregion
					return;
				case "del":
					#region Delete tile ban
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tileban del <id>", Specifier);
							return;
						}

						short id;
						if (Int16.TryParse(args.Parameters[1], out id) && id >= 0 && id < Main.maxTileSets)
						{
							TShock.TileBans.RemoveBan(id);
							args.Player.SendSuccessMessage("Unbanned tile {0}.", id);
							return;
						}
						else
							args.Player.SendErrorMessage("Invalid tile ID!");
					}
					#endregion
					return;
				case "disallow":
					#region Disallow group from placing tile
					{
						if (args.Parameters.Count != 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}tileban disallow <id> <group name>", Specifier);
							return;
						}

						short id;
						if (Int16.TryParse(args.Parameters[1], out id) && id >= 0 && id < Main.maxTileSets)
						{
							if (!TShock.Groups.GroupExists(args.Parameters[2]))
							{
								args.Player.SendErrorMessage("Invalid group.");
								return;
							}

							TileBan ban = TShock.TileBans.GetBanById(id);
							if (ban == null)
							{
								args.Player.SendErrorMessage("Tile {0} is not banned.", id);
								return;
							}
							if (ban.AllowedGroups.Contains(args.Parameters[2]))
							{
								TShock.TileBans.RemoveGroup(id, args.Parameters[2]);
								args.Player.SendSuccessMessage("{0} has been disallowed from placing tile {1}.", args.Parameters[2], id);
								return;
							}
							else
								args.Player.SendWarningMessage("{0} is already prevented from placing tile {1}.", args.Parameters[2], id);
						}
						else
							args.Player.SendErrorMessage("Invalid tile ID!");
					}
					#endregion
					return;
				case "help":
					#region Help
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;

						var lines = new List<string>
						{
							"add <tile ID> - Adds a tile ban.",
							"allow <tile ID> <group> - Allows a group to place a tile.",
							"del <tile ID> - Deletes a tile ban.",
							"disallow <tile ID> <group> - Disallows a group from place a tile.",
							"list [page] - Lists all tile bans."
						};

						PaginationTools.SendPage(args.Player, pageNumber, lines,
							new PaginationTools.Settings
							{
								HeaderFormat = "Tile Ban Sub-Commands ({0}/{1}):",
								FooterFormat = "Type {0}tileban help {{0}} for more sub-commands.".SFormat(Specifier)
							}
						);
					}
					#endregion
					return;
				case "list":
					#region List tile bans
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;
						IEnumerable<Int16> tileIds = from tileBan in TShock.TileBans.TileBans
													 select tileBan.ID;
						PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(tileIds),
							new PaginationTools.Settings
							{
								HeaderFormat = "Tile bans ({0}/{1}):",
								FooterFormat = "Type {0}tileban list {{0}} for more.".SFormat(Specifier),
								NothingToDisplayString = "There are currently no banned tiles."
							});
					}
					#endregion
					return;
			}
		}
		#endregion Tile Management

		#region Server Config Commands

		private static void SetSpawn(CommandArgs args)
		{
			Main.spawnTileX = args.Player.TileX + 1;
			Main.spawnTileY = args.Player.TileY + 3;
			SaveManager.Instance.SaveWorld(false);
			args.Player.SendSuccessMessage("Spawn has now been set at your location.");
		}

		private static void SetDungeon(CommandArgs args)
		{
			Main.dungeonX = args.Player.TileX + 1;
			Main.dungeonY = args.Player.TileY + 3;
			SaveManager.Instance.SaveWorld(false);
			args.Player.SendSuccessMessage("The dungeon's position has now been set at your location.");
		}

		private static void Reload(CommandArgs args)
		{
			TShock.Utils.Reload();
			Hooks.GeneralHooks.OnReloadEvent(args.Player);

			args.Player.SendSuccessMessage(
				"Configuration, permissions, and regions reload complete. Some changes may require a server restart.");
		}

		private static void ServerPassword(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}serverpassword \"<new password>\"", Specifier);
				return;
			}
			string passwd = args.Parameters[0];
			TShock.Config.ServerPassword = passwd;
			args.Player.SendSuccessMessage(string.Format("Server password has been changed to: {0}.", passwd));
		}

		private static void Save(CommandArgs args)
		{
			SaveManager.Instance.SaveWorld(false);
			foreach (TSPlayer tsply in TShock.Players.Where(tsply => tsply != null))
			{
				tsply.SaveServerCharacter();
			}
			args.Player.SendSuccessMessage("Save succeeded.");
		}

		private static void Settle(CommandArgs args)
		{
			if (Liquid.panicMode)
			{
				args.Player.SendWarningMessage("Liquids are already settling!");
				return;
			}
			Liquid.StartPanic();
			args.Player.SendInfoMessage("Settling liquids.");
		}

		private static void MaxSpawns(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendInfoMessage("Current maximum spawns: {0}", TShock.Config.DefaultMaximumSpawns);
				return;
			}

			if (String.Equals(args.Parameters[0], "default", StringComparison.CurrentCultureIgnoreCase))
			{
				TShock.Config.DefaultMaximumSpawns = NPC.defaultMaxSpawns = 5;
				if (args.Silent)
				{
					args.Player.SendInfoMessage("Changed the maximum spawns to 5.");
				}
				else
				{
					TSPlayer.All.SendInfoMessage("{0} changed the maximum spawns to 5.", args.Player.Name);
				}
				return;
			}

			int maxSpawns = -1;
			if (!int.TryParse(args.Parameters[0], out maxSpawns) || maxSpawns < 0 || maxSpawns > Main.maxNPCs)
			{
				args.Player.SendWarningMessage("Invalid maximum spawns!  Acceptable range is {0} to {1}", 0, Main.maxNPCs);
				return;
			}

			TShock.Config.DefaultMaximumSpawns = NPC.defaultMaxSpawns = maxSpawns;
			if (args.Silent)
			{
				args.Player.SendInfoMessage("Changed the maximum spawns to {0}.", maxSpawns);
			}
			else
			{
				TSPlayer.All.SendInfoMessage("{0} changed the maximum spawns to {1}.", args.Player.Name, maxSpawns);
			}
		}

		private static void SpawnRate(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendInfoMessage("Current spawn rate: {0}", TShock.Config.DefaultSpawnRate);
				return;
			}

			if (String.Equals(args.Parameters[0], "default", StringComparison.CurrentCultureIgnoreCase))
			{
				TShock.Config.DefaultSpawnRate = NPC.defaultSpawnRate = 600;
				if (args.Silent)
				{
					args.Player.SendInfoMessage("Changed the spawn rate to 600.");
				}
				else
				{
					TSPlayer.All.SendInfoMessage("{0} changed the spawn rate to 600.", args.Player.Name);
				}
				return;
			}

			int spawnRate = -1;
			if (!int.TryParse(args.Parameters[0], out spawnRate) || spawnRate < 0)
			{
				args.Player.SendWarningMessage("Invalid spawn rate!");
				return;
			}
			TShock.Config.DefaultSpawnRate = NPC.defaultSpawnRate = spawnRate;
			if (args.Silent)
			{
				args.Player.SendInfoMessage("Changed the spawn rate to {0}.", spawnRate);
			}
			else
			{
				TSPlayer.All.SendInfoMessage("{0} changed the spawn rate to {1}.", args.Player.Name, spawnRate);
			}
		}

		#endregion Server Config Commands

		#region Time/PvpFun Commands

		private static void Time(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				double time = Main.time / 3600.0;
				time += 4.5;
				if (!Main.dayTime)
					time += 15.0;
				time = time % 24.0;
				args.Player.SendInfoMessage("The current time is {0}:{1:D2}.", (int)Math.Floor(time), (int)Math.Floor((time % 1.0) * 60.0));
				return;
			}

			switch (args.Parameters[0].ToLower())
			{
				case "day":
					TSPlayer.Server.SetTime(true, 0.0);
					TSPlayer.All.SendInfoMessage("{0} set the time to 4:30.", args.Player.Name);
					break;
				case "night":
					TSPlayer.Server.SetTime(false, 0.0);
					TSPlayer.All.SendInfoMessage("{0} set the time to 19:30.", args.Player.Name);
					break;
				case "noon":
					TSPlayer.Server.SetTime(true, 27000.0);
					TSPlayer.All.SendInfoMessage("{0} set the time to 12:00.", args.Player.Name);
					break;
				case "midnight":
					TSPlayer.Server.SetTime(false, 16200.0);
					TSPlayer.All.SendInfoMessage("{0} set the time to 0:00.", args.Player.Name);
					break;
				default:
					string[] array = args.Parameters[0].Split(':');
					if (array.Length != 2)
					{
						args.Player.SendErrorMessage("Invalid time string! Proper format: hh:mm, in 24-hour time.");
						return;
					}

					int hours;
					int minutes;
					if (!int.TryParse(array[0], out hours) || hours < 0 || hours > 23
						|| !int.TryParse(array[1], out minutes) || minutes < 0 || minutes > 59)
					{
						args.Player.SendErrorMessage("Invalid time string! Proper format: hh:mm, in 24-hour time.");
						return;
					}

					decimal time = hours + (minutes / 60.0m);
					time -= 4.50m;
					if (time < 0.00m)
						time += 24.00m;

					if (time >= 15.00m)
					{
						TSPlayer.Server.SetTime(false, (double)((time - 15.00m) * 3600.0m));
					}
					else
					{
						TSPlayer.Server.SetTime(true, (double)(time * 3600.0m));
					}
					TSPlayer.All.SendInfoMessage("{0} set the time to {1}:{2:D2}.", args.Player.Name, hours, minutes);
					break;
			}
		}

		private static void Sandstorm(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}sandstorm <stop/start>", Specifier);
				return;
			}

			switch (args.Parameters[0].ToLowerInvariant())
			{
				case "start":
					Terraria.GameContent.Events.Sandstorm.StartSandstorm();
					TSPlayer.All.SendInfoMessage("{0} started a sandstorm.", args.Player.Name);
					break;
				case "stop":
					Terraria.GameContent.Events.Sandstorm.StopSandstorm();
					TSPlayer.All.SendInfoMessage("{0} stopped the sandstorm.", args.Player.Name);
					break;
				default:
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}sandstorm <stop/start>", Specifier);
					break;
			}
		}

		private static void Rain(CommandArgs args)
		{
			if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}rain [slime] <stop/start>", Specifier);
				return;
			}

			int switchIndex = 0;
			if (args.Parameters.Count == 2 && args.Parameters[0].ToLowerInvariant() == "slime")
			{
				switchIndex = 1;
			}

			switch (args.Parameters[switchIndex].ToLower())
			{
				case "start":
					if (switchIndex == 1)
					{
						Main.StartSlimeRain(false);
						TSPlayer.All.SendData(PacketTypes.WorldInfo);
						TSPlayer.All.SendInfoMessage("{0} caused it to rain slime.", args.Player.Name);
					}
					else
					{
						Main.StartRain();
						TSPlayer.All.SendData(PacketTypes.WorldInfo);
						TSPlayer.All.SendInfoMessage("{0} caused it to rain.", args.Player.Name);
					}
					break;
				case "stop":
					if (switchIndex == 1)
					{
						Main.StopSlimeRain(false);
						TSPlayer.All.SendData(PacketTypes.WorldInfo);
						TSPlayer.All.SendInfoMessage("{0} ended the slimey downpour.", args.Player.Name);
					}
					else
					{
						Main.StopRain();
						TSPlayer.All.SendData(PacketTypes.WorldInfo);
						TSPlayer.All.SendInfoMessage("{0} ended the downpour.", args.Player.Name);
					}
					break;
				default:
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}rain [slime] <stop/start>", Specifier);
					break;

			}
		}

		private static void Slap(CommandArgs args)
		{
			if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}slap <player> [damage]", Specifier);
				return;
			}
			if (args.Parameters[0].Length == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
				return;
			}

			string plStr = args.Parameters[0];
			var players = TSPlayer.FindByNameOrID(plStr);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			}
			else
			{
				var plr = players[0];
				int damage = 5;
				if (args.Parameters.Count == 2)
				{
					int.TryParse(args.Parameters[1], out damage);
				}
				if (!args.Player.HasPermission(Permissions.kill))
				{
					damage = TShock.Utils.Clamp(damage, 15, 0);
				}
				plr.DamagePlayer(damage);
				TSPlayer.All.SendInfoMessage("{0} slapped {1} for {2} damage.", args.Player.Name, plr.Name, damage);
				TShock.Log.Info("{0} slapped {1} for {2} damage.", args.Player.Name, plr.Name, damage);
			}
		}

		private static void Wind(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}wind <speed>", Specifier);
				return;
			}

			int speed;
			if (!int.TryParse(args.Parameters[0], out speed) || speed * 100 < 0)
			{
				args.Player.SendErrorMessage("Invalid wind speed!");
				return;
			}

			Main.windSpeed = speed;
			Main.windSpeedSet = speed;
			Main.windSpeedSpeed = 0f;
			TSPlayer.All.SendData(PacketTypes.WorldInfo);
			TSPlayer.All.SendInfoMessage("{0} changed the wind speed to {1}.", args.Player.Name, speed);
		}

		#endregion Time/PvpFun Commands

		#region Region Commands

		private static void Region(CommandArgs args)
		{
			string cmd = "help";
			if (args.Parameters.Count > 0)
			{
				cmd = args.Parameters[0].ToLower();
			}
			switch (cmd)
			{
				case "name":
					{
						{
							args.Player.SendInfoMessage("Hit a block to get the name of the region");
							args.Player.AwaitingName = true;
							args.Player.AwaitingNameParameters = args.Parameters.Skip(1).ToArray();
						}
						break;
					}
				case "set":
					{
						int choice = 0;
						if (args.Parameters.Count == 2 &&
							int.TryParse(args.Parameters[1], out choice) &&
							choice >= 1 && choice <= 2)
						{
							args.Player.SendInfoMessage("Hit a block to Set Point " + choice);
							args.Player.AwaitingTempPoint = choice;
						}
						else
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /region set <1/2>");
						}
						break;
					}
				case "define":
					{
						if (args.Parameters.Count > 1)
						{
							if (!args.Player.TempPoints.Any(p => p == Point.Zero))
							{
								string regionName = String.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
								var x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
								var y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
								var width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
								var height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);

								if (TShock.Regions.AddRegion(x, y, width, height, regionName, args.Player.Account.Name,
															 Main.worldID.ToString()))
								{
									args.Player.TempPoints[0] = Point.Zero;
									args.Player.TempPoints[1] = Point.Zero;
									args.Player.SendInfoMessage("Set region " + regionName);
								}
								else
								{
									args.Player.SendErrorMessage("Region " + regionName + " already exists");
								}
							}
							else
							{
								args.Player.SendErrorMessage("Points not set up yet");
							}
						}
						else
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region define <name>", Specifier);
						break;
					}
				case "protect":
					{
						if (args.Parameters.Count == 3)
						{
							string regionName = args.Parameters[1];
							if (args.Parameters[2].ToLower() == "true")
							{
								if (TShock.Regions.SetRegionState(regionName, true))
									args.Player.SendInfoMessage("Protected region " + regionName);
								else
									args.Player.SendErrorMessage("Could not find specified region");
							}
							else if (args.Parameters[2].ToLower() == "false")
							{
								if (TShock.Regions.SetRegionState(regionName, false))
									args.Player.SendInfoMessage("Unprotected region " + regionName);
								else
									args.Player.SendErrorMessage("Could not find specified region");
							}
							else
								args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region protect <name> <true/false>", Specifier);
						}
						else
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /region protect <name> <true/false>", Specifier);
						break;
					}
				case "delete":
					{
						if (args.Parameters.Count > 1)
						{
							string regionName = String.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
							if (TShock.Regions.DeleteRegion(regionName))
							{
								args.Player.SendInfoMessage("Deleted region \"{0}\".", regionName);
							}
							else
								args.Player.SendErrorMessage("Could not find the specified region!");
						}
						else
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region delete <name>", Specifier);
						break;
					}
				case "clear":
					{
						args.Player.TempPoints[0] = Point.Zero;
						args.Player.TempPoints[1] = Point.Zero;
						args.Player.SendInfoMessage("Cleared temporary points.");
						args.Player.AwaitingTempPoint = 0;
						break;
					}
				case "allow":
					{
						if (args.Parameters.Count > 2)
						{
							string playerName = args.Parameters[1];
							string regionName = "";

							for (int i = 2; i < args.Parameters.Count; i++)
							{
								if (regionName == "")
								{
									regionName = args.Parameters[2];
								}
								else
								{
									regionName = regionName + " " + args.Parameters[i];
								}
							}
							if (TShock.UserAccounts.GetUserAccountByName(playerName) != null)
							{
								if (TShock.Regions.AddNewUser(regionName, playerName))
								{
									args.Player.SendInfoMessage("Added user " + playerName + " to " + regionName);
								}
								else
									args.Player.SendErrorMessage("Region " + regionName + " not found");
							}
							else
							{
								args.Player.SendErrorMessage("Player " + playerName + " not found");
							}
						}
						else
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region allow <name> <region>", Specifier);
						break;
					}
				case "remove":
					if (args.Parameters.Count > 2)
					{
						string playerName = args.Parameters[1];
						string regionName = "";

						for (int i = 2; i < args.Parameters.Count; i++)
						{
							if (regionName == "")
							{
								regionName = args.Parameters[2];
							}
							else
							{
								regionName = regionName + " " + args.Parameters[i];
							}
						}
						if (TShock.UserAccounts.GetUserAccountByName(playerName) != null)
						{
							if (TShock.Regions.RemoveUser(regionName, playerName))
							{
								args.Player.SendInfoMessage("Removed user " + playerName + " from " + regionName);
							}
							else
								args.Player.SendErrorMessage("Region " + regionName + " not found");
						}
						else
						{
							args.Player.SendErrorMessage("Player " + playerName + " not found");
						}
					}
					else
						args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region remove <name> <region>", Specifier);
					break;
				case "allowg":
					{
						if (args.Parameters.Count > 2)
						{
							string group = args.Parameters[1];
							string regionName = "";

							for (int i = 2; i < args.Parameters.Count; i++)
							{
								if (regionName == "")
								{
									regionName = args.Parameters[2];
								}
								else
								{
									regionName = regionName + " " + args.Parameters[i];
								}
							}
							if (TShock.Groups.GroupExists(group))
							{
								if (TShock.Regions.AllowGroup(regionName, group))
								{
									args.Player.SendInfoMessage("Added group " + group + " to " + regionName);
								}
								else
									args.Player.SendErrorMessage("Region " + regionName + " not found");
							}
							else
							{
								args.Player.SendErrorMessage("Group " + group + " not found");
							}
						}
						else
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region allowg <group> <region>", Specifier);
						break;
					}
				case "removeg":
					if (args.Parameters.Count > 2)
					{
						string group = args.Parameters[1];
						string regionName = "";

						for (int i = 2; i < args.Parameters.Count; i++)
						{
							if (regionName == "")
							{
								regionName = args.Parameters[2];
							}
							else
							{
								regionName = regionName + " " + args.Parameters[i];
							}
						}
						if (TShock.Groups.GroupExists(group))
						{
							if (TShock.Regions.RemoveGroup(regionName, group))
							{
								args.Player.SendInfoMessage("Removed group " + group + " from " + regionName);
							}
							else
								args.Player.SendErrorMessage("Region " + regionName + " not found");
						}
						else
						{
							args.Player.SendErrorMessage("Group " + group + " not found");
						}
					}
					else
						args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region removeg <group> <region>", Specifier);
					break;
				case "list":
					{
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
							return;

						IEnumerable<string> regionNames = from region in TShock.Regions.Regions
														  where region.WorldID == Main.worldID.ToString()
														  select region.Name;
						PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(regionNames),
							new PaginationTools.Settings
							{
								HeaderFormat = "Regions ({0}/{1}):",
								FooterFormat = "Type {0}region list {{0}} for more.".SFormat(Specifier),
								NothingToDisplayString = "There are currently no regions defined."
							});
						break;
					}
				case "info":
					{
						if (args.Parameters.Count == 1 || args.Parameters.Count > 4)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region info <region> [-d] [page]", Specifier);
							break;
						}

						string regionName = args.Parameters[1];
						bool displayBoundaries = args.Parameters.Skip(2).Any(
							p => p.Equals("-d", StringComparison.InvariantCultureIgnoreCase)
						);

						Region region = TShock.Regions.GetRegionByName(regionName);
						if (region == null)
						{
							args.Player.SendErrorMessage("Region \"{0}\" does not exist.", regionName);
							break;
						}

						int pageNumberIndex = displayBoundaries ? 3 : 2;
						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, pageNumberIndex, args.Player, out pageNumber))
							break;

						List<string> lines = new List<string>
						{
							string.Format("X: {0}; Y: {1}; W: {2}; H: {3}, Z: {4}", region.Area.X, region.Area.Y, region.Area.Width, region.Area.Height, region.Z),
							string.Concat("Owner: ", region.Owner),
							string.Concat("Protected: ", region.DisableBuild.ToString()),
						};

						if (region.AllowedIDs.Count > 0)
						{
							IEnumerable<string> sharedUsersSelector = region.AllowedIDs.Select(userId =>
							{
								UserAccount account = TShock.UserAccounts.GetUserAccountByID(userId);
								if (account != null)
									return account.Name;

								return string.Concat("{ID: ", userId, "}");
							});
							List<string> extraLines = PaginationTools.BuildLinesFromTerms(sharedUsersSelector.Distinct());
							extraLines[0] = "Shared with: " + extraLines[0];
							lines.AddRange(extraLines);
						}
						else
						{
							lines.Add("Region is not shared with any users.");
						}

						if (region.AllowedGroups.Count > 0)
						{
							List<string> extraLines = PaginationTools.BuildLinesFromTerms(region.AllowedGroups.Distinct());
							extraLines[0] = "Shared with groups: " + extraLines[0];
							lines.AddRange(extraLines);
						}
						else
						{
							lines.Add("Region is not shared with any groups.");
						}

						PaginationTools.SendPage(
							args.Player, pageNumber, lines, new PaginationTools.Settings
							{
								HeaderFormat = string.Format("Information About Region \"{0}\" ({{0}}/{{1}}):", region.Name),
								FooterFormat = string.Format("Type {0}region info {1} {{0}} for more information.", Specifier, regionName)
							}
						);

						if (displayBoundaries)
						{
							Rectangle regionArea = region.Area;
							foreach (Point boundaryPoint in Utils.Instance.EnumerateRegionBoundaries(regionArea))
							{
								// Preferring dotted lines as those should easily be distinguishable from actual wires.
								if ((boundaryPoint.X + boundaryPoint.Y & 1) == 0)
								{
									// Could be improved by sending raw tile data to the client instead but not really
									// worth the effort as chances are very low that overwriting the wire for a few
									// nanoseconds will cause much trouble.
									ITile tile = Main.tile[boundaryPoint.X, boundaryPoint.Y];
									bool oldWireState = tile.wire();
									tile.wire(true);

									try
									{
										args.Player.SendTileSquare(boundaryPoint.X, boundaryPoint.Y, 1);
									}
									finally
									{
										tile.wire(oldWireState);
									}
								}
							}

							Timer boundaryHideTimer = null;
							boundaryHideTimer = new Timer((state) =>
							{
								foreach (Point boundaryPoint in Utils.Instance.EnumerateRegionBoundaries(regionArea))
									if ((boundaryPoint.X + boundaryPoint.Y & 1) == 0)
										args.Player.SendTileSquare(boundaryPoint.X, boundaryPoint.Y, 1);

								Debug.Assert(boundaryHideTimer != null);
								boundaryHideTimer.Dispose();
							},
								null, 5000, Timeout.Infinite
							);
						}

						break;
					}
				case "z":
					{
						if (args.Parameters.Count == 3)
						{
							string regionName = args.Parameters[1];
							int z = 0;
							if (int.TryParse(args.Parameters[2], out z))
							{
								if (TShock.Regions.SetZ(regionName, z))
									args.Player.SendInfoMessage("Region's z is now " + z);
								else
									args.Player.SendErrorMessage("Could not find specified region");
							}
							else
								args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region z <name> <#>", Specifier);
						}
						else
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region z <name> <#>", Specifier);
						break;
					}
				case "resize":
				case "expand":
					{
						if (args.Parameters.Count == 4)
						{
							int direction;
							switch (args.Parameters[2])
							{
								case "u":
								case "up":
									{
										direction = 0;
										break;
									}
								case "r":
								case "right":
									{
										direction = 1;
										break;
									}
								case "d":
								case "down":
									{
										direction = 2;
										break;
									}
								case "l":
								case "left":
									{
										direction = 3;
										break;
									}
								default:
									{
										direction = -1;
										break;
									}
							}
							int addAmount;
							int.TryParse(args.Parameters[3], out addAmount);
							if (TShock.Regions.ResizeRegion(args.Parameters[1], addAmount, direction))
							{
								args.Player.SendInfoMessage("Region Resized Successfully!");
								TShock.Regions.Reload();
							}
							else
								args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region resize <region> <u/d/l/r> <amount>", Specifier);
						}
						else
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region resize <region> <u/d/l/r> <amount>", Specifier);
						break;
					}
				case "rename":
					{
						if (args.Parameters.Count != 3)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region rename <region> <new name>", Specifier);
							break;
						}
						else
						{
							string oldName = args.Parameters[1];
							string newName = args.Parameters[2];

							if (oldName == newName)
							{
								args.Player.SendErrorMessage("Error: both names are the same.");
								break;
							}

							Region oldRegion = TShock.Regions.GetRegionByName(oldName);

							if (oldRegion == null)
							{
								args.Player.SendErrorMessage("Invalid region \"{0}\".", oldName);
								break;
							}

							Region newRegion = TShock.Regions.GetRegionByName(newName);

							if (newRegion != null)
							{
								args.Player.SendErrorMessage("Region \"{0}\" already exists.", newName);
								break;
							}
							
							if(TShock.Regions.RenameRegion(oldName, newName))
							{
								args.Player.SendInfoMessage("Region renamed successfully!");
							}
							else
							{
								args.Player.SendErrorMessage("Failed to rename the region.");
							}
						}
						break;
					}
				case "tp":
					{
						if (!args.Player.HasPermission(Permissions.tp))
						{
							args.Player.SendErrorMessage("You don't have the necessary permission to do that.");
							break;
						}
						if (args.Parameters.Count <= 1)
						{
							args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}region tp <region>.", Specifier);
							break;
						}

						string regionName = string.Join(" ", args.Parameters.Skip(1));
						Region region = TShock.Regions.GetRegionByName(regionName);
						if (region == null)
						{
							args.Player.SendErrorMessage("Region \"{0}\" does not exist.", regionName);
							break;
						}

						args.Player.Teleport(region.Area.Center.X * 16, region.Area.Center.Y * 16);
						break;
					}
				case "help":
				default:
					{
						int pageNumber;
						int pageParamIndex = 0;
						if (args.Parameters.Count > 1)
							pageParamIndex = 1;
						if (!PaginationTools.TryParsePageNumber(args.Parameters, pageParamIndex, args.Player, out pageNumber))
							return;

						List<string> lines = new List<string> {
						  "set <1/2> - Sets the temporary region points.",
						  "clear - Clears the temporary region points.",
						  "define <name> - Defines the region with the given name.",
						  "delete <name> - Deletes the given region.",
						  "name [-u][-z][-p] - Shows the name of the region at the given point.",
						  "rename <region> <new name> - Renames the given region.",
						  "list - Lists all regions.",
						  "resize <region> <u/d/l/r> <amount> - Resizes a region.",
						  "allow <user> <region> - Allows a user to a region.",
						  "remove <user> <region> - Removes a user from a region.",
						  "allowg <group> <region> - Allows a user group to a region.",
						  "removeg <group> <region> - Removes a user group from a region.",
						  "info <region> [-d] - Displays several information about the given region.",
						  "protect <name> <true/false> - Sets whether the tiles inside the region are protected or not.",
						  "z <name> <#> - Sets the z-order of the region.",
						};
						if (args.Player.HasPermission(Permissions.tp))
							lines.Add("tp <region> - Teleports you to the given region's center.");

						PaginationTools.SendPage(
						  args.Player, pageNumber, lines,
						  new PaginationTools.Settings
						  {
							  HeaderFormat = "Available Region Sub-Commands ({0}/{1}):",
							  FooterFormat = "Type {0}region {{0}} for more sub-commands.".SFormat(Specifier)
						  }
						);
						break;
					}
			}
		}

		#endregion Region Commands

		#region World Protection Commands

		private static void ToggleAntiBuild(CommandArgs args)
		{
			TShock.Config.DisableBuild = !TShock.Config.DisableBuild;
			TSPlayer.All.SendSuccessMessage(string.Format("Anti-build is now {0}.", (TShock.Config.DisableBuild ? "on" : "off")));
		}

		private static void ProtectSpawn(CommandArgs args)
		{
			TShock.Config.SpawnProtection = !TShock.Config.SpawnProtection;
			TSPlayer.All.SendSuccessMessage(string.Format("Spawn is now {0}.", (TShock.Config.SpawnProtection ? "protected" : "open")));
		}

		#endregion World Protection Commands

		#region General Commands

		private static void Help(CommandArgs args)
		{
			if (args.Parameters.Count > 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}help <command/page>", Specifier);
				return;
			}

			int pageNumber;
			if (args.Parameters.Count == 0 || int.TryParse(args.Parameters[0], out pageNumber))
			{
				if (!PaginationTools.TryParsePageNumber(args.Parameters, 0, args.Player, out pageNumber))
				{
					return;
				}

				IEnumerable<string> cmdNames = from cmd in ChatCommands
											   where cmd.CanRun(args.Player) && (cmd.Name != "auth" || TShock.SetupToken != 0)
											   select Specifier + cmd.Name;

				PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(cmdNames),
					new PaginationTools.Settings
					{
						HeaderFormat = "Commands ({0}/{1}):",
						FooterFormat = "Type {0}help {{0}} for more.".SFormat(Specifier)
					});
			}
			else
			{
				string commandName = args.Parameters[0].ToLower();
				if (commandName.StartsWith(Specifier))
				{
					commandName = commandName.Substring(1);
				}

				Command command = ChatCommands.Find(c => c.Names.Contains(commandName));
				if (command == null)
				{
					args.Player.SendErrorMessage("Invalid command.");
					return;
				}
				if (!command.CanRun(args.Player))
				{
					args.Player.SendErrorMessage("You do not have access to this command.");
					return;
				}

				args.Player.SendSuccessMessage("{0}{1} help: ", Specifier, command.Name);
				if (command.HelpDesc == null)
				{
					args.Player.SendInfoMessage(command.HelpText);
					return;
				}
				foreach (string line in command.HelpDesc)
				{
					args.Player.SendInfoMessage(line);
				}
			}
		}

		private static void GetVersion(CommandArgs args)
		{
			args.Player.SendInfoMessage("TShock: {0} ({1}).", TShock.VersionNum, TShock.VersionCodename);
		}

		private static void ListConnectedPlayers(CommandArgs args)
		{
			bool invalidUsage = (args.Parameters.Count > 2);

			bool displayIdsRequested = false;
			int pageNumber = 1;
			if (!invalidUsage)
			{
				foreach (string parameter in args.Parameters)
				{
					if (parameter.Equals("-i", StringComparison.InvariantCultureIgnoreCase))
					{
						displayIdsRequested = true;
						continue;
					}

					if (!int.TryParse(parameter, out pageNumber))
					{
						invalidUsage = true;
						break;
					}
				}
			}
			if (invalidUsage)
			{
				args.Player.SendErrorMessage("Invalid usage, proper usage: {0}who [-i] [pagenumber]", Specifier);
				return;
			}
			if (displayIdsRequested && !args.Player.HasPermission(Permissions.seeids))
			{
				args.Player.SendErrorMessage("You don't have the required permission to list player ids.");
				return;
			}

			args.Player.SendSuccessMessage("Online Players ({0}/{1})", TShock.Utils.GetActivePlayerCount(), TShock.Config.MaxSlots);

			var players = new List<string>();

			foreach (TSPlayer ply in TShock.Players)
			{
				if (ply != null && ply.Active)
				{
					if (displayIdsRequested)
					{
						players.Add(String.Format("{0} (ID: {1}{2})", ply.Name, ply.Index, ply.Account != null ? ", ID: " + ply.Account.ID : ""));
					}
					else
					{
						players.Add(ply.Name);
					}
				}
			}

			PaginationTools.SendPage(
				args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(players),
				new PaginationTools.Settings
				{
					IncludeHeader = false,
					FooterFormat = string.Format("Type {0}who {1}{{0}} for more.", Specifier, displayIdsRequested ? "-i " : string.Empty)
				}
			);
		}

		private static void SetupToken(CommandArgs args)
		{
			if (TShock.SetupToken == 0)
			{
				if (args.Player.Group.Name == new SuperAdminGroup().Name)
					args.Player.SendInfoMessage("The initial setup system is already disabled.");
				else
				{
					args.Player.SendWarningMessage("The initial setup system is disabled. This incident has been logged.");
					TShock.Log.Warn("{0} attempted to use the initial setup system even though it's disabled.", args.Player.IP);
					return;
				}
			}

			// If the user account is already a superadmin (permanent), disable the system
			if (args.Player.IsLoggedIn && args.Player.tempGroup == null)
			{
				args.Player.SendSuccessMessage("Your new account has been verified, and the {0}setup system has been turned off.", Specifier);
				args.Player.SendSuccessMessage("You can always use the {0}user command to manage players.", Specifier);
				args.Player.SendSuccessMessage("The setup system will remain disabled as long as a superadmin exists (even if you delete setup.lock).");
				args.Player.SendSuccessMessage("Share your server, talk with other admins, and more on our forums -- https://tshock.co/");
				args.Player.SendSuccessMessage("Thank you for using TShock for Terraria!");
				FileTools.CreateFile(Path.Combine(TShock.SavePath, "setup.lock"));
				File.Delete(Path.Combine(TShock.SavePath, "setup-code.txt"));
				TShock.SetupToken = 0;
				return;
			}

			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("You must provide a setup code!");
				return;
			}

			int givenCode;
			if (!Int32.TryParse(args.Parameters[0], out givenCode) || givenCode != TShock.SetupToken)
			{
				args.Player.SendErrorMessage("Incorrect setup code. This incident has been logged.");
				TShock.Log.Warn(args.Player.IP + " attempted to use an incorrect setup code.");
				return;
			}

			if (args.Player.Group.Name != "superadmin")
				args.Player.tempGroup = new SuperAdminGroup();

			args.Player.SendInfoMessage("Temporary system access has been given to you, so you can run one command.");
			args.Player.SendInfoMessage("Please use the following to create a permanent account for you.");
			args.Player.SendInfoMessage("{0}user add <username> <password> owner", Specifier);
			args.Player.SendInfoMessage("Creates: <username> with the password <password> as part of the owner group.");
			args.Player.SendInfoMessage("Please use {0}login <username> <password> after this process.", Specifier);
			args.Player.SendInfoMessage("If you understand, please {0}login <username> <password> now, and then type {0}setup.", Specifier);
			return;
		}

		private static void ThirdPerson(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}me <text>", Specifier);
				return;
			}
			if (args.Player.mute)
				args.Player.SendErrorMessage("You are muted.");
			else
				TSPlayer.All.SendMessage(string.Format("*{0} {1}", args.Player.Name, String.Join(" ", args.Parameters)), 205, 133, 63);
		}

		private static void PartyChat(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}p <team chat text>", Specifier);
				return;
			}
			int playerTeam = args.Player.Team;

			if (args.Player.mute)
				args.Player.SendErrorMessage("You are muted.");
			else if (playerTeam != 0)
			{
				string msg = string.Format("<{0}> {1}", args.Player.Name, String.Join(" ", args.Parameters));
				foreach (TSPlayer player in TShock.Players)
				{
					if (player != null && player.Active && player.Team == playerTeam)
						player.SendMessage(msg, Main.teamColor[playerTeam].R, Main.teamColor[playerTeam].G, Main.teamColor[playerTeam].B);
				}
			}
			else
				args.Player.SendErrorMessage("You are not in a party!");
		}

		private static void Mute(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}mute <player> [reason]", Specifier);
				return;
			}

			var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			}
			else if (players[0].HasPermission(Permissions.mute))
			{
				args.Player.SendErrorMessage("You cannot mute this player.");
			}
			else if (players[0].mute)
			{
				var plr = players[0];
				plr.mute = false;
				TSPlayer.All.SendInfoMessage("{0} has been unmuted by {1}.", plr.Name, args.Player.Name);
			}
			else
			{
				string reason = "No reason specified.";
				if (args.Parameters.Count > 1)
					reason = String.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);
				var plr = players[0];
				plr.mute = true;
				TSPlayer.All.SendInfoMessage("{0} has been muted by {1} for {2}.", plr.Name, args.Player.Name, reason);
			}
		}

		private static void Motd(CommandArgs args)
		{
			args.Player.SendFileTextAsMessage(FileTools.MotdPath);
		}

		private static void Rules(CommandArgs args)
		{
			args.Player.SendFileTextAsMessage(FileTools.RulesPath);
		}

		private static void Whisper(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}whisper <player> <text>", Specifier);
				return;
			}

			var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			}
			else if (args.Player.mute)
			{
				args.Player.SendErrorMessage("You are muted.");
			}
			else
			{
				var plr = players[0];
				var msg = string.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);
				plr.SendMessage(String.Format("<From {0}> {1}", args.Player.Name, msg), Color.MediumPurple);
				args.Player.SendMessage(String.Format("<To {0}> {1}", plr.Name, msg), Color.MediumPurple);
				plr.LastWhisper = args.Player;
				args.Player.LastWhisper = plr;
			}
		}

		private static void Reply(CommandArgs args)
		{
			if (args.Player.mute)
			{
				args.Player.SendErrorMessage("You are muted.");
			}
			else if (args.Player.LastWhisper != null)
			{
				var msg = string.Join(" ", args.Parameters);
				args.Player.LastWhisper.SendMessage(String.Format("<From {0}> {1}", args.Player.Name, msg), Color.MediumPurple);
				args.Player.SendMessage(String.Format("<To {0}> {1}", args.Player.LastWhisper.Name, msg), Color.MediumPurple);
			}
			else
			{
				args.Player.SendErrorMessage("You haven't previously received any whispers. Please use {0}whisper to whisper to other people.", Specifier);
			}
		}

		private static void Annoy(CommandArgs args)
		{
			if (args.Parameters.Count != 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}annoy <player> <seconds to annoy>", Specifier);
				return;
			}
			int annoy = 5;
			int.TryParse(args.Parameters[1], out annoy);

			var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (players.Count == 0)
				args.Player.SendErrorMessage("Invalid player!");
			else if (players.Count > 1)
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			else
			{
				var ply = players[0];
				args.Player.SendSuccessMessage("Annoying " + ply.Name + " for " + annoy + " seconds.");
				(new Thread(ply.Whoopie)).Start(annoy);
			}
		}

		private static void Confuse(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}confuse <player>", Specifier);
				return;
			}
			var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (players.Count == 0)
				args.Player.SendErrorMessage("Invalid player!");
			else if (players.Count > 1)
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			else
			{
				var ply = players[0];
				ply.Confused = !ply.Confused;
				args.Player.SendSuccessMessage("{0} is {1} confused.", ply.Name, ply.Confused ? "now" : "no longer");
			}
		}

		private static void Rocket(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}rocket <player>", Specifier);
				return;
			}
			var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (players.Count == 0)
				args.Player.SendErrorMessage("Invalid player!");
			else if (players.Count > 1)
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			else
			{
				var ply = players[0];

				if (ply.IsLoggedIn && Main.ServerSideCharacter)
				{
					ply.TPlayer.velocity.Y = -50;
					TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", ply.Index);
					args.Player.SendSuccessMessage("Rocketed {0}.", ply.Name);
				}
				else
				{
					args.Player.SendErrorMessage("Failed to rocket player: Not logged in or not SSC mode.");
				}
			}
		}

		private static void FireWork(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}firework <player> [red|green|blue|yellow]", Specifier);
				return;
			}
			var players = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (players.Count == 0)
				args.Player.SendErrorMessage("Invalid player!");
			else if (players.Count > 1)
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			else
			{
				int type = 167;
				if (args.Parameters.Count > 1)
				{
					if (args.Parameters[1].ToLower() == "green")
						type = 168;
					else if (args.Parameters[1].ToLower() == "blue")
						type = 169;
					else if (args.Parameters[1].ToLower() == "yellow")
						type = 170;
				}
				var ply = players[0];
				int p = Projectile.NewProjectile(ply.TPlayer.position.X, ply.TPlayer.position.Y - 64f, 0f, -8f, type, 0, (float)0);
				Main.projectile[p].Kill();
				args.Player.SendSuccessMessage("Launched Firework on {0}.", ply.Name);
			}
		}

		private static void Aliases(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}aliases <command or alias>", Specifier);
				return;
			}

			string givenCommandName = string.Join(" ", args.Parameters);
			if (string.IsNullOrWhiteSpace(givenCommandName))
			{
				args.Player.SendErrorMessage("Please enter a proper command name or alias.");
				return;
			}

			string commandName;
			if (givenCommandName[0] == Specifier[0])
				commandName = givenCommandName.Substring(1);
			else
				commandName = givenCommandName;

			bool didMatch = false;
			foreach (Command matchingCommand in ChatCommands.Where(cmd => cmd.Names.IndexOf(commandName) != -1))
			{
				if (matchingCommand.Names.Count > 1)
					args.Player.SendInfoMessage(
						"Aliases of {0}{1}: {0}{2}", Specifier, matchingCommand.Name, string.Join(", {0}".SFormat(Specifier), matchingCommand.Names.Skip(1)));
				else
					args.Player.SendInfoMessage("{0}{1} defines no aliases.", Specifier, matchingCommand.Name);

				didMatch = true;
			}

			if (!didMatch)
				args.Player.SendErrorMessage("No command or command alias matching \"{0}\" found.", givenCommandName);
		}

		private static void CreateDumps(CommandArgs args)
		{
			TShock.Utils.DumpPermissionMatrix("PermissionMatrix.txt");
			TShock.Utils.Dump(false);
			args.Player.SendSuccessMessage("Your reference dumps have been created in the server folder.");
			return;
		}

		#endregion General Commands

		#region Cheat Commands

		private static void Clear(CommandArgs args)
		{
			if (args.Parameters.Count != 1 && args.Parameters.Count != 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clear <item/npc/projectile> [radius]", Specifier);
				return;
			}

			int radius = 50;
			if (args.Parameters.Count == 2)
			{
				if (!int.TryParse(args.Parameters[1], out radius) || radius <= 0)
				{
					args.Player.SendErrorMessage("Invalid radius.");
					return;
				}
			}

			switch (args.Parameters[0].ToLower())
			{
				case "item":
				case "items":
					{
						int cleared = 0;
						for (int i = 0; i < Main.maxItems; i++)
						{
							float dX = Main.item[i].position.X - args.Player.X;
							float dY = Main.item[i].position.Y - args.Player.Y;

							if (Main.item[i].active && dX * dX + dY * dY <= radius * radius * 256f)
							{
								Main.item[i].active = false;
								TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
								cleared++;
							}
						}
						args.Player.SendSuccessMessage("Deleted {0} items within a radius of {1}.", cleared, radius);
					}
					break;
				case "npc":
				case "npcs":
					{
						int cleared = 0;
						for (int i = 0; i < Main.maxNPCs; i++)
						{
							float dX = Main.npc[i].position.X - args.Player.X;
							float dY = Main.npc[i].position.Y - args.Player.Y;

							if (Main.npc[i].active && dX * dX + dY * dY <= radius * radius * 256f)
							{
								Main.npc[i].active = false;
								Main.npc[i].type = 0;
								TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
								cleared++;
							}
						}
						args.Player.SendSuccessMessage("Deleted {0} NPCs within a radius of {1}.", cleared, radius);
					}
					break;
				case "proj":
				case "projectile":
				case "projectiles":
					{
						int cleared = 0;
						for (int i = 0; i < Main.maxProjectiles; i++)
						{
							float dX = Main.projectile[i].position.X - args.Player.X;
							float dY = Main.projectile[i].position.Y - args.Player.Y;

							if (Main.projectile[i].active && dX * dX + dY * dY <= radius * radius * 256f)
							{
								Main.projectile[i].active = false;
								Main.projectile[i].type = 0;
								TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", i);
								cleared++;
							}
						}
						args.Player.SendSuccessMessage("Deleted {0} projectiles within a radius of {1}.", cleared, radius);
					}
					break;
				default:
					args.Player.SendErrorMessage("Invalid clear option!");
					break;
			}
		}

		private static void Kill(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}kill <player>", Specifier);
				return;
			}

			string plStr = String.Join(" ", args.Parameters);
			var players = TSPlayer.FindByNameOrID(plStr);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendMultipleMatchError(players.Select(p => p.Name));
			}
			else
			{
				var plr = players[0];
				plr.KillPlayer();
				args.Player.SendSuccessMessage(string.Format("You just killed {0}!", plr.Name));
				plr.SendErrorMessage("{0} just killed you!", args.Player.Name);
			}
		}

		private static void Butcher(CommandArgs args)
		{
			if (args.Parameters.Count > 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}butcher [mob type]", Specifier);
				return;
			}

			int npcId = 0;

			if (args.Parameters.Count == 1)
			{
				var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
				if (npcs.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid mob type!");
					return;
				}
				else if (npcs.Count > 1)
				{
					args.Player.SendMultipleMatchError(npcs.Select(n => $"{n.FullName}({n.type})"));
					return;
				}
				else
				{
					npcId = npcs[0].netID;
				}
			}

			int kills = 0;
			for (int i = 0; i < Main.npc.Length; i++)
			{
				if (Main.npc[i].active && ((npcId == 0 && !Main.npc[i].townNPC && Main.npc[i].netID != NPCID.TargetDummy) || Main.npc[i].netID == npcId))
				{
					TSPlayer.Server.StrikeNPC(i, (int)(Main.npc[i].life + (Main.npc[i].defense * 0.6)), 0, 0);
					kills++;
				}
			}
			TSPlayer.All.SendInfoMessage("{0} butchered {1} NPCs.", args.Player.Name, kills);
		}

		private static void Item(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}item <item name/id> [item amount] [prefix id/name]", Specifier);
				return;
			}

			int amountParamIndex = -1;
			int itemAmount = 0;
			for (int i = 1; i < args.Parameters.Count; i++)
			{
				if (int.TryParse(args.Parameters[i], out itemAmount))
				{
					amountParamIndex = i;
					break;
				}
			}

			string itemNameOrId;
			if (amountParamIndex == -1)
				itemNameOrId = string.Join(" ", args.Parameters);
			else
				itemNameOrId = string.Join(" ", args.Parameters.Take(amountParamIndex));

			Item item;
			List<Item> matchedItems = TShock.Utils.GetItemByIdOrName(itemNameOrId);
			if (matchedItems.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid item type!");
				return;
			}
			else if (matchedItems.Count > 1)
			{
				args.Player.SendMultipleMatchError(matchedItems.Select(i => $"{i.Name}({i.netID})"));
				return;
			}
			else
			{
				item = matchedItems[0];
			}
			if (item.type < 1 && item.type >= Main.maxItemTypes)
			{
				args.Player.SendErrorMessage("The item type {0} is invalid.", itemNameOrId);
				return;
			}

			int prefixId = 0;
			if (amountParamIndex != -1 && args.Parameters.Count > amountParamIndex + 1)
			{
				string prefixidOrName = args.Parameters[amountParamIndex + 1];
				var prefixIds = TShock.Utils.GetPrefixByIdOrName(prefixidOrName);

				if (item.accessory && prefixIds.Contains(PrefixID.Quick))
				{
					prefixIds.Remove(PrefixID.Quick);
					prefixIds.Remove(PrefixID.Quick2);
					prefixIds.Add(PrefixID.Quick2);
				}
				else if (!item.accessory && prefixIds.Contains(PrefixID.Quick))
					prefixIds.Remove(PrefixID.Quick2);

				if (prefixIds.Count > 1)
				{
					args.Player.SendMultipleMatchError(prefixIds.Select(p => p.ToString()));
					return;
				}
				else if (prefixIds.Count == 0)
				{
					args.Player.SendErrorMessage("No prefix matched \"{0}\".", prefixidOrName);
					return;
				}
				else
				{
					prefixId = prefixIds[0];
				}
			}

			if (args.Player.InventorySlotAvailable || (item.type > 70 && item.type < 75) || item.ammo > 0 || item.type == 58 || item.type == 184)
			{
				if (itemAmount == 0 || itemAmount > item.maxStack)
					itemAmount = item.maxStack;

				if (args.Player.GiveItemCheck(item.type, EnglishLanguage.GetItemNameById(item.type), itemAmount, prefixId))
				{
					item.prefix = (byte)prefixId;
					args.Player.SendSuccessMessage("Gave {0} {1}(s).", itemAmount, item.AffixName());
				}
				else
				{
					args.Player.SendErrorMessage("You cannot spawn banned items.");
				}
			}
			else
			{
				args.Player.SendErrorMessage("Your inventory seems full.");
			}
		}

		private static void RenameNPC(CommandArgs args)
		{
			if (args.Parameters.Count != 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}renameNPC <guide, nurse, etc.> <newname>", Specifier);
				return;
			}
			int npcId = 0;
			if (args.Parameters.Count == 2)
			{
				List<NPC> npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
				if (npcs.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid mob type!");
					return;
				}
				else if (npcs.Count > 1)
				{
					args.Player.SendMultipleMatchError(npcs.Select(n => $"{n.FullName}({n.type})"));
					return;
				}
				else if (args.Parameters[1].Length > 200)
				{
					args.Player.SendErrorMessage("New name is too large!");
					return;
				}
				else
				{
					npcId = npcs[0].netID;
				}
			}
			int done = 0;
			for (int i = 0; i < Main.npc.Length; i++)
			{
				if (Main.npc[i].active && ((npcId == 0 && !Main.npc[i].townNPC) || (Main.npc[i].netID == npcId && Main.npc[i].townNPC)))
				{
					Main.npc[i].GivenName = args.Parameters[1];
					NetMessage.SendData(56, -1, -1, NetworkText.FromLiteral(args.Parameters[1]), i, 0f, 0f, 0f, 0);
					done++;
				}
			}
			if (done > 0)
			{
				TSPlayer.All.SendInfoMessage("{0} renamed the {1}.", args.Player.Name, args.Parameters[0]);
			}
			else
			{
				args.Player.SendErrorMessage("Could not rename {0}!", args.Parameters[0]);
			}
		}

		private static void Give(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage(
					"Invalid syntax! Proper syntax: {0}give <item type/id> <player> [item amount] [prefix id/name]", Specifier);
				return;
			}
			if (args.Parameters[0].Length == 0)
			{
				args.Player.SendErrorMessage("Missing item name/id.");
				return;
			}
			if (args.Parameters[1].Length == 0)
			{
				args.Player.SendErrorMessage("Missing player name.");
				return;
			}
			int itemAmount = 0;
			int prefix = 0;
			var items = TShock.Utils.GetItemByIdOrName(args.Parameters[0]);
			args.Parameters.RemoveAt(0);
			string plStr = args.Parameters[0];
			args.Parameters.RemoveAt(0);
			if (args.Parameters.Count == 1)
				int.TryParse(args.Parameters[0], out itemAmount);
			if (items.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid item type!");
			}
			else if (items.Count > 1)
			{
				args.Player.SendMultipleMatchError(items.Select(i => $"{i.Name}({i.netID})"));
			}
			else
			{
				var item = items[0];

				if (args.Parameters.Count == 2)
				{
					int.TryParse(args.Parameters[0], out itemAmount);
					var prefixIds = TShock.Utils.GetPrefixByIdOrName(args.Parameters[1]);
					if (item.accessory && prefixIds.Contains(PrefixID.Quick))
					{
						prefixIds.Remove(PrefixID.Quick);
						prefixIds.Remove(PrefixID.Quick2);
						prefixIds.Add(PrefixID.Quick2);
					}
					else if (!item.accessory && prefixIds.Contains(PrefixID.Quick))
						prefixIds.Remove(PrefixID.Quick2);
					if (prefixIds.Count == 1)
						prefix = prefixIds[0];
				}

				if (item.type >= 1 && item.type < Main.maxItemTypes)
				{
					var players = TSPlayer.FindByNameOrID(plStr);
					if (players.Count == 0)
					{
						args.Player.SendErrorMessage("Invalid player!");
					}
					else if (players.Count > 1)
					{
						args.Player.SendMultipleMatchError(players.Select(p => p.Name));
					}
					else
					{
						var plr = players[0];
						if (plr.InventorySlotAvailable || (item.type > 70 && item.type < 75) || item.ammo > 0 || item.type == 58 || item.type == 184)
						{
							if (itemAmount == 0 || itemAmount > item.maxStack)
								itemAmount = item.maxStack;
							if (plr.GiveItemCheck(item.type, EnglishLanguage.GetItemNameById(item.type), itemAmount, prefix))
							{
								args.Player.SendSuccessMessage(string.Format("Gave {0} {1} {2}(s).", plr.Name, itemAmount, item.Name));
								plr.SendSuccessMessage(string.Format("{0} gave you {1} {2}(s).", args.Player.Name, itemAmount, item.Name));
							}
							else
							{
								args.Player.SendErrorMessage("You cannot spawn banned items.");
							}

						}
						else
						{
							args.Player.SendErrorMessage("Player does not have free slots!");
						}
					}
				}
				else
				{
					args.Player.SendErrorMessage("Invalid item type!");
				}
			}
		}

		private static void Heal(CommandArgs args)
		{
			TSPlayer playerToHeal;
			if (args.Parameters.Count > 0)
			{
				string plStr = String.Join(" ", args.Parameters);
				var players = TSPlayer.FindByNameOrID(plStr);
				if (players.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid player!");
					return;
				}
				else if (players.Count > 1)
				{
					args.Player.SendMultipleMatchError(players.Select(p => p.Name));
					return;
				}
				else
				{
					playerToHeal = players[0];
				}
			}
			else if (!args.Player.RealPlayer)
			{
				args.Player.SendErrorMessage("You can't heal yourself!");
				return;
			}
			else
			{
				playerToHeal = args.Player;
			}

			playerToHeal.Heal();
			if (playerToHeal == args.Player)
			{
				args.Player.SendSuccessMessage("You just got healed!");
			}
			else
			{
				args.Player.SendSuccessMessage(string.Format("You just healed {0}", playerToHeal.Name));
				playerToHeal.SendSuccessMessage(string.Format("{0} just healed you!", args.Player.Name));
			}
		}

		private static void Buff(CommandArgs args)
		{
			if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}buff <buff id/name> [time(seconds)]", Specifier);
				return;
			}
			int id = 0;
			int time = 60;
			if (!int.TryParse(args.Parameters[0], out id))
			{
				var found = TShock.Utils.GetBuffByName(args.Parameters[0]);
				if (found.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid buff name!");
					return;
				}
				else if (found.Count > 1)
				{
					args.Player.SendMultipleMatchError(found.Select(f => Lang.GetBuffName(f)));
					return;
				}
				id = found[0];
			}
			if (args.Parameters.Count == 2)
				int.TryParse(args.Parameters[1], out time);
			if (id > 0 && id < Main.maxBuffTypes)
			{
				if (time < 0 || time > short.MaxValue)
					time = 60;
				args.Player.SetBuff(id, time * 60);
				args.Player.SendSuccessMessage(string.Format("You have buffed yourself with {0}({1}) for {2} seconds!",
													  TShock.Utils.GetBuffName(id), TShock.Utils.GetBuffDescription(id), (time)));
			}
			else
				args.Player.SendErrorMessage("Invalid buff ID!");
		}

		private static void GBuff(CommandArgs args)
		{
			if (args.Parameters.Count < 2 || args.Parameters.Count > 3)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}gbuff <player> <buff id/name> [time(seconds)]", Specifier);
				return;
			}
			int id = 0;
			int time = 60;
			var foundplr = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (foundplr.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
				return;
			}
			else if (foundplr.Count > 1)
			{
				args.Player.SendMultipleMatchError(foundplr.Select(p => p.Name));
				return;
			}
			else
			{
				if (!int.TryParse(args.Parameters[1], out id))
				{
					var found = TShock.Utils.GetBuffByName(args.Parameters[1]);
					if (found.Count == 0)
					{
						args.Player.SendErrorMessage("Invalid buff name!");
						return;
					}
					else if (found.Count > 1)
					{
						args.Player.SendMultipleMatchError(found.Select(b => Lang.GetBuffName(b)));
						return;
					}
					id = found[0];
				}
				if (args.Parameters.Count == 3)
					int.TryParse(args.Parameters[2], out time);
				if (id > 0 && id < Main.maxBuffTypes)
				{
					if (time < 0 || time > short.MaxValue)
						time = 60;
					foundplr[0].SetBuff(id, time * 60);
					args.Player.SendSuccessMessage(string.Format("You have buffed {0} with {1}({2}) for {3} seconds!",
														  foundplr[0].Name, TShock.Utils.GetBuffName(id),
														  TShock.Utils.GetBuffDescription(id), (time)));
					foundplr[0].SendSuccessMessage(string.Format("{0} has buffed you with {1}({2}) for {3} seconds!",
														  args.Player.Name, TShock.Utils.GetBuffName(id),
														  TShock.Utils.GetBuffDescription(id), (time)));
				}
				else
					args.Player.SendErrorMessage("Invalid buff ID!");
			}
		}

		private static void Grow(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}grow <tree/epictree/mushroom/cactus/herb>", Specifier);
				return;
			}
			var name = "Fail";
			var x = args.Player.TileX;
			var y = args.Player.TileY + 3;

			if (!TShock.Regions.CanBuild(x, y, args.Player))
			{
				args.Player.SendErrorMessage("You're not allowed to change tiles here!");
				return;
			}

			switch (args.Parameters[0].ToLower())
			{
				case "tree":
					for (int i = x - 1; i < x + 2; i++)
					{
						Main.tile[i, y].active(true);
						Main.tile[i, y].type = 2;
						Main.tile[i, y].wall = 0;
					}
					Main.tile[x, y - 1].wall = 0;
					WorldGen.GrowTree(x, y);
					name = "Tree";
					break;
				case "epictree":
					for (int i = x - 1; i < x + 2; i++)
					{
						Main.tile[i, y].active(true);
						Main.tile[i, y].type = 2;
						Main.tile[i, y].wall = 0;
					}
					Main.tile[x, y - 1].wall = 0;
					Main.tile[x, y - 1].liquid = 0;
					Main.tile[x, y - 1].active(true);
					WorldGen.GrowEpicTree(x, y);
					name = "Epic Tree";
					break;
				case "mushroom":
					for (int i = x - 1; i < x + 2; i++)
					{
						Main.tile[i, y].active(true);
						Main.tile[i, y].type = 70;
						Main.tile[i, y].wall = 0;
					}
					Main.tile[x, y - 1].wall = 0;
					WorldGen.GrowShroom(x, y);
					name = "Mushroom";
					break;
				case "cactus":
					Main.tile[x, y].type = 53;
					WorldGen.GrowCactus(x, y);
					name = "Cactus";
					break;
				case "herb":
					Main.tile[x, y].active(true);
					Main.tile[x, y].frameX = 36;
					Main.tile[x, y].type = 83;
					WorldGen.GrowAlch(x, y);
					name = "Herb";
					break;
				default:
					args.Player.SendErrorMessage("Unknown plant!");
					return;
			}
			args.Player.SendTileSquare(x, y);
			args.Player.SendSuccessMessage("Tried to grow a " + name + ".");
		}

		private static void ToggleGodMode(CommandArgs args)
		{
			TSPlayer playerToGod;
			if (args.Parameters.Count > 0)
			{
				if (!args.Player.HasPermission(Permissions.godmodeother))
				{
					args.Player.SendErrorMessage("You do not have permission to god mode another player!");
					return;
				}
				string plStr = String.Join(" ", args.Parameters);
				var players = TSPlayer.FindByNameOrID(plStr);
				if (players.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid player!");
					return;
				}
				else if (players.Count > 1)
				{
					args.Player.SendMultipleMatchError(players.Select(p => p.Name));
					return;
				}
				else
				{
					playerToGod = players[0];
				}
			}
			else if (!args.Player.RealPlayer)
			{
				args.Player.SendErrorMessage("You can't god mode a non player!");
				return;
			}
			else
			{
				playerToGod = args.Player;
			}

			playerToGod.GodMode = !playerToGod.GodMode;

			if (playerToGod == args.Player)
			{
				args.Player.SendSuccessMessage(string.Format("You are {0} in god mode.", args.Player.GodMode ? "now" : "no longer"));
			}
			else
			{
				args.Player.SendSuccessMessage(string.Format("{0} is {1} in god mode.", playerToGod.Name, playerToGod.GodMode ? "now" : "no longer"));
				playerToGod.SendSuccessMessage(string.Format("You are {0} in god mode.", playerToGod.GodMode ? "now" : "no longer"));
			}
		}

		#endregion Cheat Comamnds
	}
}
