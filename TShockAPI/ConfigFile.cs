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
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TShockAPI
{
	/// <summary>ConfigFile - The config file class, which contains the configuration for a server that is serialized into JSON and deserialized on load.</summary>
	public class ConfigFile
	{
		/// <summary>InvasionMultiplier - The equation for calculating invasion size = 100 + (multiplier * (number of active players > 200 hp)).</summary>
		[Description("The equation for calculating invasion size is 100 + (multiplier * (number of active players with greater than 200 health)).")]
		public int InvasionMultiplier = 1;

		/// <summary>DefaultMaximumSpawns - The default max spawns per wave.</summary>
		[Description("The default maximum mobs that will spawn per wave. Higher means more mobs in that wave.")]
		public int DefaultMaximumSpawns = 5;

		/// <summary>DefaultSpawnRate - The default spawn rate.</summary>
		[Description("The delay between waves. Lower values lead to more mobs.")]
		public int DefaultSpawnRate = 600;

		/// <summary>ServerPort - The configured server port.</summary>
		[Description("The port the server runs on.")]
		public int ServerPort = 7777;

		/// <summary>EnableWhitelist - boolean if the whitelist functionality should be turned on.</summary>
		[Description("Enable or disable the whitelist based on IP addresses in whitelist.txt.")]
		public bool EnableWhitelist;

		/// <summary>InfiniteInvasion - Whether or not infinite invasion mode should be on.</summary>
		[Description("Enable the ability for invasion size to never decrease. Make sure to run /invade, and note that this adds 2 million+ goblins to the spawn queue for the map.")]
		public bool InfiniteInvasion;

		/// <summary>PvPMode - The server PvP mode (normal, always, or disabled).</summary>
		[Description("Set the server pvp mode. Valid types are: \"normal\", \"always\" and \"disabled\".")]
		public string PvPMode = "normal";

		/// <summary>SpawnProtection - Enables the spawn protection system.</summary>
		[Description("Prevents tiles from being placed within SpawnProtectionRadius of the default spawn.")]
		public bool SpawnProtection = true;

		/// <summary>SpawnProtectionRadius - The spawn protection tile radius.</summary>
		[Description("Radius from spawn tile for SpawnProtection.")]
		public int SpawnProtectionRadius = 10;

		/// <summary>MaxSlots - The server's max slots.</summary>
		[Description("Max slots for the server. If you want people to be kicked with \"Server is full\" set this to how many players you want max and then set Terraria max players to 2 higher.")]
		public int MaxSlots = 8;

		/// <summary>RangeChecks - Whether or not the anti-grief system based on range should be enabled.</summary>
		[Description("Global protection agent for any block distance based anti-grief check.")]
		public bool RangeChecks = true;

		/// <summary>DisableBuild - Whether or not building should be enabled.</summary>
		[Description("Disables any building / placing of blocks.")]
		public bool DisableBuild;

		/// <summary>SuperAdminChatRGB - The chat color for the superadmin group.</summary>
		[Description("#.#.# = Red/Blue/Green - RGB Colors for the Admin Chat Color. Max value: 255.")]
		public int[] SuperAdminChatRGB = { 255, 255, 255 };

		/// <summary>SuperAdminChatPrefix - The superadmin chat prefix.</summary>
		[Description("Super admin group chat prefix.")]
		public string SuperAdminChatPrefix = "(Super Admin) ";

		/// <summary>SuperAdminChatSuffix - The superadmin chat suffix.</summary>
		[Description("Super admin group chat suffix.")]
		public string SuperAdminChatSuffix = "";

		/// <summary>BackupInterval - The backup frequency in minutes.</summary>
		[Description("Backup frequency in minutes. So, a value of 60 = 60 minutes. Backups are stored in the \\tshock\\backups folder.")]
		public int BackupInterval;

		/// <summary>BackupKeepFor - Backup max age in minutes.</summary>
		[Description("How long backups are kept in minutes. 2880 = 2 days.")]
		public int BackupKeepFor = 60;

		/// <summary>RememberLeavePos - Whether or not to remember where an IP player was when they left.</summary>
		[Description("Remembers where a player left off. It works by remembering the IP, NOT the character.\neg. When you try to disconnect, and reconnect to be automatically placed at spawn, you'll be at your last location. Note: Won't save after server restarts.")]
		public bool RememberLeavePos;

		/// <summary>HardcoreOnly - Whether or not HardcoreOnly should be enabled.</summary>
		[Description("Hardcore players ONLY. This means softcore players cannot join.")]
		public bool HardcoreOnly;

		/// <summary>MediumcoreOnly - Whether or not MediumCore only players should be enabled.</summary>
		[Description("Mediumcore players ONLY. This means softcore players cannot join.")]
		public bool MediumcoreOnly;

		/// <summary>KickOnMediumcoreDeath - Whether or not to kick mediumcore players on death.</summary>
		[Description("Kicks a mediumcore player on death.")]
		public bool KickOnMediumcoreDeath;

		/// <summary>BanOnMediumcoreDeath - Whether or not to ban mediumcore players on death.</summary>
		[Description("Bans a mediumcore player on death.")]
		public bool BanOnMediumcoreDeath;

		/// <summary>AutoSave - Whether or not to use Terraria's built-in world auto save.</summary>
		[Description("Enable/disable Terraria's built in auto save.")]
		public bool AutoSave = true;
		/// <summary>AnnounceSave - Whether or not to broadcast world saves.</summary>
		[Description("Enable/disable save announcements.")]
		public bool AnnounceSave = true;

		/// <summary>MaximumLoginAttempts - Number of failed login attempts before kicking a player.</summary>
		[Description("Number of failed login attempts before kicking the player.")]
		public int MaximumLoginAttempts = 3;

		/// <summary>ServerName - Sent to the client.</summary>
		[Description("Replaces the world name during a session if UseServerName is true.")]
		public string ServerName = "";
		/// <summary>UseServerName - Whether or not to use ServerName in place of the world name.</summary>
		[Description("Sends ServerName in place of the world name to clients.")]
		public bool UseServerName = false;

		/// <summary>StorageType - The type of SQL database to use when storing data (either "sqlite" or "mysql").</summary>
		[Description("Valid types are \"sqlite\" and \"mysql\".")]
		public string StorageType = "sqlite";

		/// <summary>MySqlHost - The hostname and port to to use when connecting to a MySQL database.</summary>
		[Description("The MySQL hostname and port to direct connections to.")]
		public string MySqlHost = "localhost:3306";
		/// <summary>MySqlDbName - The database name to use when connecting to a MySQL database.</summary>
		[Description("Database name to connect to.")]
		public string MySqlDbName = "";
		/// <summary>MySqlUsername - The username for the login credentials used when connecting to a MySQL database.</summary>
		[Description("Database username to connect with.")]
		public string MySqlUsername = "";
		/// <summary>MySqlPassword - The password for the login credentials used when connecting to a MySQL database.</summary>
		[Description("Database password to connect with.")]
		public string MySqlPassword = "";

		/// <summary>MediumcoreBanReason - The reason given if banning mediumcore players on death.</summary>
		[Description("The reason given when banning a mediumcore player on death if BanOnMediumcoreDeath is set to true.")]
		public string MediumcoreBanReason = "Death results in a ban";
		/// <summary>MediumcoreKickReason - The reason given if kicking mediumcore players on death.</summary>
		[Description("The reason given when kicking a mediumcore player on death if KickOnMediumcoreDeath is set to true.")]
		public string MediumcoreKickReason = "Death results in a kick";

		/// <summary>EnableIPBans - Whether or not to kick players on join that match a banned IP address.</summary>
		[Description("Enables kicking of banned users by matching their IP Address.")]
		public bool EnableIPBans = true;

		/// <summary>EnableUUIDBans - Whether or not to kick players on join that match a banned UUID.</summary>
		[Description("Enables kicking of banned users by matching their client UUID.")]
		public bool EnableUUIDBans = true;

		/// <summary>EnableBanOnUsernames - Whether or not to kick players on join that match a banned character name.</summary>
		[Description("Enables kicking of banned users by matching their Character Name.")]
		public bool EnableBanOnUsernames;

		/// <summary>DefaultRegistrationGroupName - The default group name to place newly registered users under.</summary>
		[Description("Selects the default group name to place new registrants under.")]
		public string DefaultRegistrationGroupName = "default";

		/// <summary>DefaultGuestGroupName - The default group name to place unregistered players under.</summary>
		[Description("Selects the default group name to place unregistered players under.")]
		public string DefaultGuestGroupName = "guest";

		/// <summary>DisableSpewLogs - Whether or not to send logs as messages to players with the log permission.</summary>
		[Description("Force-disable printing logs to players with the log permission.")]
		public bool DisableSpewLogs = true;

		/// <summary>DisableSecondUpdateLogs - Prevents OnSecondUpdate() checks from writing to the log file.</summary>
		[Description("Prevents OnSecondUpdate checks from writing to the log file.")]
		public bool DisableSecondUpdateLogs = false;

		/// <summary>HashAlgorithm - The hash algorithm used to encrypt user passwords.
		/// Valid types: "sha512", "sha256" and "md5". Append with "-xp" for the xp supported algorithms.</summary>
		[Description("The hash algorithm used to encrypt user passwords. Valid types: \"sha512\", \"sha256\" and \"md5\". Append with \"-xp\" for the xp supported algorithms.")]
		public string HashAlgorithm = "sha512";

		/// <summary>ServerFullReason - The reason given when kicking players when the server is full.</summary>
		[Description("String that is used when kicking people when the server is full.")]
		public string ServerFullReason = "Server is full";

		/// <summary>WhitelistKickReason - The reason given when kicking players for not being on the whitelist.</summary>
		[Description("String that is used when a user is kicked due to not being on the whitelist.")]
		public string WhitelistKickReason = "You are not on the whitelist.";

		/// <summary>ServerFullNoReservedReason - The reason given when kicking players when the server is full and there are no reserved slots open.</summary>
		[Description("String that is used when kicking people when the server is full with no reserved slots.")]
		public string ServerFullNoReservedReason = "Server is full. No reserved slots open.";

		/// <summary>SaveWorldOnCrash - Attempts to save world in the server crashes due to an unhandled exception.</summary>
		[Description("This will save the world if Terraria crashes from an unhandled exception.")]
		public bool SaveWorldOnCrash = true;

		/// <summary>EnableGeoIP - Whether or not to announce a player's location on join.</summary>
		[Description("This will announce a player's location on join.")]
		public bool EnableGeoIP;

		/// <summary>DisableTombstones - Disable tombstone dropping during death for all players.</summary>
		[Description("Disable tombstone dropping during death for all players.")]
		public bool DisableTombstones = true;

		/// <summary>DisplayIPToAdmins - Displays a player's IP on join to everyone with the log permission.</summary>
		[Description("Displays a player's IP on join to everyone who has the log permission.")]
		public bool DisplayIPToAdmins;

		/// <summary>KickProxyUsers - If the GeoIP service is running, this will kick users under a proxy.</summary>
		[Description("Kicks users using a proxy as identified with the GeoIP database.")]
		public bool KickProxyUsers = true;

		/// <summary>DisableHardmode - If set to true, hardmode will not be activated by the Wall of Flesh or the /starthardmode command.</summary>
		[Description("Disables hardmode, can't never be activated. Overrides /starthardmode.")]
		public bool DisableHardmode;

		/// <summary>DisableDungeonGuardian - Disables the dungeon guardian from being spawned while sending players to their spawn point instead.</summary>
		[Description("Disables the dungeon guardian from being spawned while sending players to their spawn point instead.")]
		public bool DisableDungeonGuardian;

		/// <summary>DisableClownBombs - Disables clown bomb projectiles from spawning.</summary>
		[Description("Disables clown bomb projectiles from spawning.")]
		public bool DisableClownBombs;

		/// <summary>DisableSnowBalls - Disables snow ball projectiles from spawning.</summary>
		[Description("Disables snow ball projectiles from spawning.")]
		public bool DisableSnowBalls;

		/// <summary>ChatFormat - Controls the in-game chat format. {0} = Group Name, {1} = Group Prefix, {2} = Player Name, {3} = Group Suffix, {4} = Chat Message.</summary>
		[Description("Changes in-game chat format: {0} = Group Name, {1} = Group Prefix, {2} = Player Name, {3} = Group Suffix, {4} = Chat Message.")]
		public string ChatFormat = "{1}{2}{3}: {4}";

		/// <summary>ChatAboveHeadsFormat - Modifies the player name when using chat above heads. Same formatting options as ChatFormat.</summary>
		[Description("Change the player name when using chat above heads. This begins with a player name wrapped in brackets, as per Terraria's formatting. Same formatting as ChatFormat(minus the text aka {4}).")]
		public string ChatAboveHeadsFormat = "{2}";

		/// <summary>ForceTime - Can be either "normal", "day" or "night". When set to one of the latter two, the blocks other.</summary>
		[Description("Force the world time to be normal, day, or night.")]
		public string ForceTime = "normal";

		/// <summary>TileKillThreshold - Disables/reverts a player if this number of tile kills is exceeded within 1 second.</summary>
		[Description("Disables/reverts a player if this number of tile kills is exceeded within 1 second.")]
		public int TileKillThreshold = 60;

		/// <summary>TilePlaceThreshold - Disables/reverts a player if this number of tile placements is exceeded within 1 second.</summary>
		[Description("Disables/reverts a player if this number of tile places is exceeded within 1 second.")]
		public int TilePlaceThreshold = 20;

		/// <summary>TileLiquidThreshold - Disables a player if this number of liquid sets is exceeded within 1 second.</summary>
		[Description("Disables a player if this number of liquid sets is exceeded within 1 second.")]
		public int TileLiquidThreshold = 15;

		/// <summary>ProjectileThreshold - Disables a player if this number of projectiles is created within 1 second.</summary>
		[Description("Disable a player if this number of projectiles is created within 1 second.")]
		public int ProjectileThreshold = 50;

		/// <summary>HealOtherThreshold - Disables a player if this number of HealOtherPlayer packets is sent within 1 second.</summary>
		[Description("Disables a player if this number of HealOtherPlayer packets is sent within 1 second.")]
		public int HealOtherThreshold = 50;

		/// <summary>ProjIgnoreShrapnel - Whether or not to ignore shrapnel from crystal bullets for the projectile threshold count.</summary>
		[Description("Ignore shrapnel from crystal bullets for projectile threshold.")]
		public bool ProjIgnoreShrapnel = true;

		/// <summary>RequireLogin - Requires all players to register or login before being allowed to play.</summary>
		[Description("Requires all players to register or login before being allowed to play.")]
		public bool RequireLogin;

		/// <summary>DisableInvisPvP - Whether or not to turn a player invisible if using invisibility potions during PvP.</summary>
		[Description("Disables invisibility potions from being used in PvP (Note, can be used in the client, but the effect isn't sent to the rest of the server).")]
		public bool DisableInvisPvP;

		/// <summary>MaxRangeForDisabled - The maximum distance, in tiles, that disabled players can move from.</summary>
		[Description("The maximum distance players disabled for various reasons can move from.")]
		public int MaxRangeForDisabled = 10;

		/// <summary>ServerPassword - The server password required to join the server.</summary>
		[Description("Server password required to join the server.")]
		public string ServerPassword = "";

		/// <summary>RegionProtectChests - Whether or not region protection should apply to chests.</summary>
		[Description("Protect chests with region and build permissions.")]
		public bool RegionProtectChests;

		/// <summary>RegionProtectGemLocks - Whether or not region protection should apply to gem locks.</summary>
		[Description("Protect gem locks with region and build permissions.")]
		public bool RegionProtectGemLocks = true;

		/// <summary>DisableLoginBeforeJoin - This will prevent users from being able to login before connecting.</summary>
		[Description("Disable users from being able to login with account password when joining.")]
		public bool DisableLoginBeforeJoin;

		/// <summary>DisableUUIDLogin - This will disable automatic login through a saved client UUID.</summary>
		[Description("Disable users from being able to login with their client UUID.")]
		public bool DisableUUIDLogin;

		/// <summary>KickEmptyUUID - Kick clients that don't send a UUID to the server.</summary>
		[Description("Kick clients that don't send a UUID to the server.")]
		public bool KickEmptyUUID;

		/// <summary>AllowRegisterAnyUsername - Allows users to register a username that doesn't necessarily match their character name.</summary>
		[Description("Allows users to register any username with /register.")]
		public bool AllowRegisterAnyUsername;

		/// <summary>AllowLoginAnyUsername - Allows users to login to any account even if the username doesn't match their character name.</summary>
		[Description("Allows users to login with any username with /login.")]
		public bool AllowLoginAnyUsername = true;

		/// <summary>The maximum damage a player/NPC can inflict.</summary>
		[Description("The maximum damage a player/npc can inflict.")]
		public int MaxDamage = 1175;

		/// <summary>The maximum damage a projectile can inflict.</summary>
		[Description("The maximum damage a projectile can inflict.")]
		public int MaxProjDamage = 1175;

		/// <summary>KickOnDamageThresholdBroken - Whether or not to kick users when they surpass the MaxDamage threshold.</summary>
		[Description("Kicks a user if set to true, if they inflict more damage then the max damage.")]
		public bool KickOnDamageThresholdBroken = false;

		/// <summary>IgnoreProjUpdate - Ignores checking to see if player 'can' update a projectile.</summary>
		[Description("Ignores checking to see if player 'can' update a projectile.")]
		public bool IgnoreProjUpdate = false;

		/// <summary>IgnoreProjKill - Ignores checking to see if player 'can' kill a projectile.</summary>
		[Description("Ignores checking to see if player 'can' kill a projectile.")]
		public bool IgnoreProjKill = false;

		/// <summary>AlllowIce - Allows ice placement even where a user cannot usually build.</summary>
		[Description("Allow ice placement even when user does not have canbuild.")]
		public bool AllowIce = false;

		/// <summary>AllowCrimsonCreep - Enables or disables crimson to spread when a world is in hardmode.</summary>
		[Description("Allows crimson to spread when a world is hardmode.")]
		public bool AllowCrimsonCreep = true;

		/// <summary>AllowCorruptionCreep - Enables or disables corruption to spread when a world is in hardmode.</summary>
		[Description("Allows corruption to spread when a world is hardmode.")]
		public bool AllowCorruptionCreep = true;

		/// <summary>AllowHallowCreep - Enables or disables hallow to spread when a world is in hardmode.</summary>
		[Description("Allows hallow to spread when a world is hardmode.")]
		public bool AllowHallowCreep = true;

		/// <summary>StatueSpawn200 - How many NPCs a statue can spawn within 200 pixels(?) before it stops spawning.</summary>
		[Description("How many things a statue can spawn within 200 pixels(?) before it stops spawning. Default = 3.")]
		public int StatueSpawn200 = 3;

		/// <summary>StatueSpawn600 - How many NPCs a statue can spawn within 600 pixels(?) before it stops spawning.</summary>
		[Description("How many things a statue can spawn within 600 pixels(?) before it stops spawning. Default = 6.")]
		public int StatueSpawn600 = 6;

		/// <summary>StatueSpawnWorld - How many NPCs a statue can spawn before it stops spawning.</summary>
		[Description("How many things a statue can spawn before it stops spawning. Default = 10.")]
		public int StatueSpawnWorld = 10;

		/// <summary>PreventBannedItemSpawn - Prevents banned items from being spawned with commands.</summary>
		[Description("Prevent banned items from being /i or /give.")]
		public bool PreventBannedItemSpawn = false;

		/// <summary>PreventDeadModification - Prevent players from interacting with the world if dead.</summary>
		[Description("Prevent players from interacting with the world if dead.")]
		public bool PreventDeadModification = true;

		/// <summary>EnableChatAboveHeads - Whether or not to display chat messages above players' heads.</summary>
		[Description("Displays chat messages above players' heads, but will disable chat prefixes to compensate.")]
		public bool EnableChatAboveHeads = false;

		/// <summary>ForceXmas - Force Christmas-only events to occur all year.</summary>
		[Description("Force Christmas-only events to occur all year.")]
		public bool ForceXmas = false;

		/// <summary>AllowAllowedGroupsToSpawnBannedItems - Allows groups on the banned item allowed list to spawn banned items even if <see cref="PreventBannedItemSpawn"/> is set to true.</summary>
		[Description("Allows groups on the banned item allowed list to spawn banned items.")]
		public bool AllowAllowedGroupsToSpawnBannedItems = false;

		/// <summary>IgnoreChestStacksOnLoad - Allows stacks in chests to be beyond the stack limit during world load.</summary>
		[Description("Allows stacks in chests to be beyond the stack limit.")]
		public bool IgnoreChestStacksOnLoad = false;

		/// <summary>LogPath - The path of the directory where logs should be written to.</summary>
		[Description("The path of the directory where logs should be written into.")]
		public string LogPath = "tshock";

		/// <summary>UseSqlLogs - Whether or not to save logs to a SQL database instead of a text file.</summary>
		[Description("Save logs to an SQL database instead of a text file. Default = false.")]
		public bool UseSqlLogs = false;

		/// <summary>RevertToTextLogsOnSqlFailures - Number of times the SQL log must fail to insert logs before falling back to the text log.</summary>
		[Description("Number of times the SQL log must fail to insert logs before falling back to the text log.")]
		public int RevertToTextLogsOnSqlFailures = 10;

		/// <summary>PreventInvalidPlaceStyle - Prevents players from placing tiles with an invalid style.</summary>
		[Description("Prevents players from placing tiles with an invalid style.")]
		public bool PreventInvalidPlaceStyle = true;

		/// <summary>BroadCastRGB - The RGB values used for the color of broadcast messages.</summary>
		[Description("#.#.# = Red/Blue/Green - RGB Colors for broadcasts. Max value: 255.")]
		public int[] BroadcastRGB = { 127, 255, 212 };

		/// <summary>ReservedSlots - The number of reserved slots past your max server slot that can be joined by reserved players.</summary>
		[Description("The number of reserved slots past your max server slot that can be joined by reserved players.")]
		public int ReservedSlots = 20;

		/// <summary>RespawnSeconds - The number of seconds a player must wait before being respawned.</summary>
		[Description("The number of seconds a player must wait before being respawned.")]
		public int RespawnSeconds = 5;

		/// <summary>RespawnBossSeconds - "The number of seconds a player must wait before being respawned if there is a boss nearby.</summary>
		[Description("The number of seconds a player must wait before being respawned if there is a boss nearby.")]
		public int RespawnBossSeconds = 10;

		/// <summary>TilePaintThreshold - Disables a player if this number of tiles is painted within 1 second.</summary>
		[Description("Disables a player if this number of tiles is painted within 1 second.")]
		public int TilePaintThreshold = 15;

		/// <summary>ForceHalloween - Forces Halloween-only events to occur all year.</summary>
		[Description("Forces your world to be in Halloween mode regardless of the data.")]
		public bool ForceHalloween = false;

		/// <summary>AllowCutTilesAndBreakables - Allows players to break temporary tiles (grass, pots, etc) even if they cannot typically build in a region.</summary>
		[Description("Allows anyone to break grass, pots, etc.")]
		public bool AllowCutTilesAndBreakables = false;

		/// <summary>CommandSpecifier - Specifies which string starts a command.
		/// Note: Will not function properly if the string length is bigger than 1.</summary>
		[Description("Specifies which string starts a command.")]
		public string CommandSpecifier = "/";

		/// <summary>CommandSilentSpecifier - Specifies which string starts a command silently.
		/// Note: Will not function properly if the string length is bigger than 1.</summary>
		[Description("Specifies which string starts a command silently.")]
		public string CommandSilentSpecifier = ".";

		/// <summary>KickOnHardcoreDeath - Whether or not to kick a hardcore player on death.</summary>
		[Description("Kicks a hardcore player on death.")]
		public bool KickOnHardcoreDeath;

		/// <summary>BanOnHardcoreDeath - Whether or not to ban a hardcore player on death.</summary>
		[Description("Bans a hardcore player on death.")]
		public bool BanOnHardcoreDeath;

		/// <summary>HardcoreBanReason - The reason given when banning a hardcore player on death.</summary>
		[Description("Bans a hardcore player on death.")]
		public string HardcoreBanReason = "Death results in a ban";

		/// <summary>HardcoreKickReason - The reason given when kicking a hardcore player on death.</summary>
		[Description("Kicks a hardcore player on death.")]
		public string HardcoreKickReason = "Death results in a kick";

		/// <summary>AnonymousBossInvasions - Whether or not to announce boss spawning or invasion starts.</summary>
		[Description("Whether bosses or invasions should be anonymously spawned.")]
		public bool AnonymousBossInvasions = true;

		/// <summary>MaxHP - The maximum allowable HP, before equipment buffs.</summary>
		[Description("The maximum allowable HP, before equipment buffs.")]
		public int MaxHP = 500;

		/// <summary>MaxMP - The maximum allowable MP, before equipment buffs.</summary>
		[Description("The maximum allowable MP, before equipment buffs.")]
		public int MaxMP = 200;

		/// <summary>SaveWorldOnLastPlayerExit - Whether or not to save the world when the last player disconnects.</summary>
		[Description("Determines if the server should save the world if the last player exits.")]
		public bool SaveWorldOnLastPlayerExit = true;

		/// <summary>BCryptWorkFactor - Determines the BCrypt work factor to use. If increased, all passwords will be upgraded to new work-factor on verify.
		/// The number of computational rounds is 2^n. Increase with caution. Range: 5-31.</summary>
		[Description("Determines the BCrypt work factor to use. If increased, all passwords will be upgraded to new work-factor on verify. The number of computational rounds is 2^n. Increase with caution. Range: 5-31.")]
		public int BCryptWorkFactor = 7;

		/// <summary>MinimumPasswordLength - The minimum password length for new user accounts.</summary>
		[Description("The minimum password length for new user accounts. Minimum value is 4.")]
		public int MinimumPasswordLength = 4;

		/// <summary>ShowBackupAutosaveMessages - Whether or not to show backup auto save messages.</summary>
		[Description("Show backup autosave messages.")]
		public bool ShowBackupAutosaveMessages = true;

		/// <summary>
		/// Reads a configuration file from a given path
		/// </summary>
		/// <param name="path">string path</param>
		/// <returns>ConfigFile object</returns>
		public static ConfigFile Read(string path)
		{
			if (!File.Exists(path))
				return new ConfigFile();
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return Read(fs);
			}
		}

		/// <summary>
		/// Reads the configuration file from a stream
		/// </summary>
		/// <param name="stream">stream</param>
		/// <returns>ConfigFile object</returns>
		public static ConfigFile Read(Stream stream)
		{
			using (var sr = new StreamReader(stream))
			{
				var cf = JsonConvert.DeserializeObject<ConfigFile>(sr.ReadToEnd());
				if (ConfigRead != null)
					ConfigRead(cf);
				return cf;
			}
		}

		/// <summary>
		/// Writes the configuration to a given path
		/// </summary>
		/// <param name="path">string path - Location to put the config file</param>
		public void Write(string path)
		{
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				Write(fs);
			}
		}

		/// <summary>
		/// Writes the configuration to a stream
		/// </summary>
		/// <param name="stream">stream</param>
		public void Write(Stream stream)
		{
			var str = JsonConvert.SerializeObject(this, Formatting.Indented);
			using (var sw = new StreamWriter(stream))
			{
				sw.Write(str);
			}
		}

		/// <summary>
		/// On config read hook
		/// </summary>
		public static Action<ConfigFile> ConfigRead;

		/// <summary>
		/// Dumps all configuration options to a text file in Markdown format
		/// </summary>
		public static void DumpDescriptions()
		{
			var sb = new StringBuilder();
			var defaults = new ConfigFile();

			foreach (var field in defaults.GetType().GetFields().OrderBy(f => f.Name))
			{
				if (field.IsStatic)
					continue;

				var name = field.Name;
				var type = field.FieldType.Name;

				var descattr =
					field.GetCustomAttributes(false).FirstOrDefault(o => o is DescriptionAttribute) as DescriptionAttribute;
				var desc = descattr != null && !string.IsNullOrWhiteSpace(descattr.Description) ? descattr.Description : "None";

				var def = field.GetValue(defaults);

				sb.AppendLine("{0}  ".SFormat(name));
				sb.AppendLine("Type: {0}  ".SFormat(type));
				sb.AppendLine("Description: {0}  ".SFormat(desc));
				sb.AppendLine("Default: \"{0}\"  ".SFormat(def));
				sb.AppendLine();
			}

			File.WriteAllText("ConfigDescriptions.txt", sb.ToString());
		}
	}
}
