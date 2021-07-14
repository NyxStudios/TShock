using Rests;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace TShockAPI.Configuration
{
	/// <summary>
	/// Settings used in the TShock configuration file
	/// </summary>
	public class TShockSettings
	{

		#region Server Settings

		/// <summary>The server password required to join the server.</summary>
		[Description("The server password required to join the server.")]
		public string ServerPassword = "";

		/// <summary>The port the server runs on.</summary>
		[Description("The port the server runs on.")]
		public int ServerPort = 7777;

		/// <summary>Maximum number of clients connected at once. If lower than Terraria's setting, the server will kick excess connections.</summary>
		[Description("Maximum number of clients connected at once.\nIf you want people to be kicked with \"Server is full\" set this to how many players you want max and then set Terraria max players to 2 higher.")]
		public int MaxSlots = 8;

		/// <summary>The number of reserved slots past your max server slots that can be joined by reserved players.</summary>
		[Description("The number of reserved slots past your max server slots that can be joined by reserved players.")]
		public int ReservedSlots = 20;

		/// <summary>Replaces the world name during a session if UseServerName is true.</summary>
		[Description("Replaces the world name during a session if UseServerName is true.")]
		public string ServerName = "";

		/// <summary>Whether or not to use ServerName in place of the world name.</summary>
		[Description("Whether or not to use ServerName in place of the world name.")]
		public bool UseServerName = false;

		/// <summary>The path to the directory where logs should be written to.</summary>
		[Description("The path to the directory where logs should be written to.")]
		public string LogPath = "tshock/logs";

		/// <summary>Whether or not the server should output debug level messages related to system operation.</summary>
		[Description("Whether or not the server should output debug level messages related to system operation.")]
		public bool DebugLogs = false;

		/// <summary>Prevents users from being able to login before they finish connecting.</summary>
		[Description("Prevents users from being able to login before they finish connecting.")]
		public bool DisableLoginBeforeJoin;

		/// <summary>Allows stacks in chests to go beyond the stack limit during world loading.</summary>
		[Description("Allows stacks in chests to go beyond the stack limit during world loading.")]
		public bool IgnoreChestStacksOnLoad = false;

		#endregion


		#region Backup and Save Settings

		/// <summary>Enable or disable Terraria's built-in world auto save.</summary>
		[Description("Enable or disable Terraria's built-in world auto save.")]
		public bool AutoSave = true;

		/// <summary>Enable or disable world save announcements.</summary>
		[Description("Enable or disable world save announcements.")]
		public bool AnnounceSave = true;

		/// <summary>Whether or not to show backup auto save messages.</summary>
		[Description("Whether or not to show backup auto save messages.")]
		public bool ShowBackupAutosaveMessages = true;

		/// <summary>The interval between backups, in minutes. Backups are stored in the tshock/backups folder.</summary>
		[Description("The interval between backups, in minutes. Backups are stored in the tshock/backups folder.")]
		public int BackupInterval = 10;

		/// <summary>For how long backups are kept in minutes.</summary>
		[Description("For how long backups are kept in minutes.\neg. 2880 = 2 days.")]
		public int BackupKeepFor = 240;

		/// <summary>Whether or not to save the world if the server crashes from an unhandled exception.</summary>
		[Description("Whether or not to save the world if the server crashes from an unhandled exception.")]
		public bool SaveWorldOnCrash = true;

		/// <summary>Whether or not to save the world when the last player disconnects.</summary>
		[Description("Whether or not to save the world when the last player disconnects.")]
		public bool SaveWorldOnLastPlayerExit = true;

		#endregion


		#region World Settings

		/// <summary>Determines the size of invasion events. The equation for calculating invasion size = 100 + (multiplier * (number of active players > 200 hp)).</summary>
		[Description("Determines the size of invasion events.\nThe equation for calculating invasion size is 100 + (multiplier * (number of active players with greater than 200 health)).")]
		public int InvasionMultiplier = 1;

		/// <summary>The default maximum number of mobs that will spawn per wave. Higher means more mobs in that wave.</summary>
		[Description("The default maximum number of mobs that will spawn per wave. Higher means more mobs in that wave.")]
		public int DefaultMaximumSpawns = 5;

		/// <summary>The delay between waves. Lower values lead to more mobs.</summary>
		[Description("The delay between waves. Lower values lead to more mobs.")]
		public int DefaultSpawnRate = 600;

		/// <summary>Enables never-ending invasion events. You still need to start the event.</summary>
		[Description("Enables never ending invasion events. You still need to start the event, such as with the /invade command.")]
		public bool InfiniteInvasion;

		/// <summary>Sets the PvP mode. Valid types are: "normal", "always", "disabled".</summary>
		[Description("Sets the PvP mode. Valid types are: \"normal\", \"always\" and \"disabled\".")]
		public string PvPMode = "normal";

		/// <summary>Prevents tiles from being placed within SpawnProtectionRadius of the default spawn.</summary>
		[Description("Prevents tiles from being placed within SpawnProtectionRadius of the default spawn.")]
		public bool SpawnProtection = true;

		/// <summary>The tile radius around the spawn tile that is protected by the SpawnProtection setting.</summary>
		[Description("The tile radius around the spawn tile that is protected by the SpawnProtection setting.")]
		public int SpawnProtectionRadius = 10;

		/// <summary>Enable or disable anti-cheat range checks based on distance between the player and their block placements.</summary>
		[Description("Enable or disable anti-cheat range checks based on distance between the player and their block placements.")]
		public bool RangeChecks = true;

		/// <summary>Prevents non-hardcore players from connecting.</summary>
		[Description("Prevents non-hardcore players from connecting.")]
		public bool HardcoreOnly;

		/// <summary>Prevents softcore players from connecting.</summary>
		[Description("Prevents softcore players from connecting.")]
		public bool MediumcoreOnly;

		/// <summary>Prevents non-softcore players from connecting.</summary>
		[Description("Prevents non-softcore players from connecting.")]
		public bool SoftcoreOnly;

		/// <summary>Disables any placing, or removal of blocks.</summary>
		[Description("Disables any placing, or removal of blocks.")]
		public bool DisableBuild;

		/// <summary>If enabled, hardmode will not be activated by the Wall of Flesh or the /starthardmode command.</summary>
		[Description("If enabled, hardmode will not be activated by the Wall of Flesh or the /starthardmode command.")]
		public bool DisableHardmode;

		/// <summary>Prevents the dungeon guardian from being spawned while sending players to their spawn point instead.</summary>
		[Description("Prevents the dungeon guardian from being spawned while sending players to their spawn point instead.")]
		public bool DisableDungeonGuardian;

		/// <summary>Disables clown bomb projectiles from spawning.</summary>
		[Description("Disables clown bomb projectiles from spawning.")]
		public bool DisableClownBombs;

		/// <summary>Disables snow ball projectiles from spawning.</summary>
		[Description("Disables snow ball projectiles from spawning.")]
		public bool DisableSnowBalls;

		/// <summary>Disables tombstone dropping during death for all players.</summary>
		[Description("Disables tombstone dropping during death for all players.")]
		public bool DisableTombstones = true;

		/// <summary>Forces the world time to be normal, day, or night.</summary>
		[Description("Forces the world time to be normal, day, or night.")]
		public string ForceTime = "normal";

		/// <summary>Disables the effect of invisibility potions while PvP is enabled by turning the player visible to the other clients.</summary>
		[Description("Disables the effect of invisibility potions while PvP is enabled by turning the player visible to the other clients.")]
		public bool DisableInvisPvP;

		/// <summary>The maximum distance, in tiles, that disabled players can move from.</summary>
		[Description("The maximum distance, in tiles, that disabled players can move from.")]
		public int MaxRangeForDisabled = 10;

		/// <summary>Whether or not region protection should apply to chests.</summary>
		[Description("Whether or not region protection should apply to chests.")]
		public bool RegionProtectChests;

		/// <summary>Whether or not region protection should apply to gem locks.</summary>
		[Description("Whether or not region protection should apply to gem locks.")]
		public bool RegionProtectGemLocks = true;

		/// <summary>Ignores checks to see if a player 'can' update a projectile.</summary>
		[Description("Ignores checks to see if a player 'can' update a projectile.")]
		public bool IgnoreProjUpdate = false;

		/// <summary>Ignores checks to see if a player 'can' kill a projectile.</summary>
		[Description("Ignores checks to see if a player 'can' kill a projectile.")]
		public bool IgnoreProjKill = false;

		/// <summary>Allows players to break temporary tiles (grass, pots, etc) where they cannot usually build.</summary>
		[Description("Allows players to break temporary tiles (grass, pots, etc) where they cannot usually build.")]
		public bool AllowCutTilesAndBreakables = false;

		/// <summary>Allows ice placement even where a user cannot usually build.</summary>
		[Description("Allows ice placement even where a user cannot usually build.")]
		public bool AllowIce = false;

		/// <summary>Allows the crimson to spread when a world is in hardmode.</summary>
		[Description("Allows the crimson to spread when a world is in hardmode.")]
		public bool AllowCrimsonCreep = true;

		/// <summary>Allows the corruption to spread when a world is in hardmode.</summary>
		[Description("Allows the corruption to spread when a world is in hardmode.")]
		public bool AllowCorruptionCreep = true;

		/// <summary>Allows the hallow to spread when a world is in hardmode.</summary>
		[Description("Allows the hallow to spread when a world is in hardmode.")]
		public bool AllowHallowCreep = true;

		/// <summary>How many NPCs a statue can spawn within 200 pixels(?) before it stops spawning.</summary>
		[Description("How many NPCs a statue can spawn within 200 pixels(?) before it stops spawning.\nDefault = 3.")]
		public int StatueSpawn200 = 3;

		/// <summary>How many NPCs a statue can spawn within 600 pixels(?) before it stops spawning.</summary>
		[Description("How many NPCs a statue can spawn within 600 pixels(?) before it stops spawning.\nDefault = 6.")]
		public int StatueSpawn600 = 6;

		/// <summary>How many NPCs a statue can spawn before it stops spawning.</summary>
		[Description("How many NPCs a statue can spawn before it stops spawning.\nDefault = 10.")]
		public int StatueSpawnWorld = 10;

		/// <summary>Prevent banned items from being spawned or given with commands.</summary>
		[Description("Prevent banned items from being spawned or given with commands.")]
		public bool PreventBannedItemSpawn = false;

		/// <summary>Prevent players from interacting with the world while they are dead.</summary>
		[Description("Prevent players from interacting with the world while they are dead.")]
		public bool PreventDeadModification = true;

		/// <summary>Prevents players from placing tiles with an invalid style.</summary>
		[Description("Prevents players from placing tiles with an invalid style.")]
		public bool PreventInvalidPlaceStyle = true;

		/// <summary>Forces Christmas-only events to occur all year.</summary>
		[Description("Forces Christmas-only events to occur all year.")]
		public bool ForceXmas = false;

		/// <summary>Forces Halloween-only events to occur all year.</summary>
		[Description("Forces Halloween-only events to occur all year.")]
		public bool ForceHalloween = false;

		/// <summary>Allows groups on the banned item allowed list to spawn banned items even if PreventBannedItemSpawn is set to true.</summary>
		[Description("Allows groups on the banned item allowed list to spawn banned items even if PreventBannedItemSpawn is set to true.")]
		public bool AllowAllowedGroupsToSpawnBannedItems = false;

		/// <summary>The number of seconds a player must wait before being respawned. Cannot be longer than normal value now. Use at your own risk.</summary>
		[Description("The number of seconds a player must wait before being respawned. Cannot be longer than normal value now. Use at your own risk.")]
		public int RespawnSeconds = 10;

		/// <summary>The number of seconds a player must wait before being respawned if there is a boss nearby. Cannot be longer than normal value now. Use at your own risk.</summary>
		[Description("The number of seconds a player must wait before being respawned if there is a boss nearby. Cannot be longer than normal value now. Use at your own risk.")]
		public int RespawnBossSeconds = 10;

		/// <summary>Whether or not to announce boss spawning or invasion starts.</summary>
		[Description("Whether or not to announce boss spawning or invasion starts.")]
		public bool AnonymousBossInvasions = true;

		/// <summary>The maximum HP a player can have, before equipment buffs.</summary>
		[Description("The maximum HP a player can have, before equipment buffs.")]
		public int MaxHP = 500;

		/// <summary>The maximum MP a player can have, before equipment buffs.</summary>
		[Description("The maximum MP a player can have, before equipment buffs.")]
		public int MaxMP = 200;

		/// <summary>Determines the range in tiles that a bomb can affect tiles from detonation point.</summary>
		[Description("Determines the range in tiles that a bomb can affect tiles from detonation point.")]
		public int BombExplosionRadius = 5;

		#endregion


		#region Login and Ban Settings

		/// <summary>The default group name to place newly registered users under.</summary>
		[Description("The default group name to place newly registered users under.")]
		public string DefaultRegistrationGroupName = "default";

		/// <summary>The default group name to place unregistered players under.</summary>
		[Description("The default group name to place unregistered players under.")]
		public string DefaultGuestGroupName = "guest";

		/// <summary>Remembers where a player left off, based on their IP. Does not persist through server restarts.</summary>
		[Description("Remembers where a player left off, based on their IP. Does not persist through server restarts.\neg. When you try to disconnect, and reconnect to be automatically placed at spawn, you'll be at your last location.")]
		public bool RememberLeavePos;

		/// <summary>Number of failed login attempts before kicking the player.</summary>
		[Description("Number of failed login attempts before kicking the player.")]
		public int MaximumLoginAttempts = 3;

		/// <summary>Whether or not to kick mediumcore players on death.</summary>
		[Description("Whether or not to kick mediumcore players on death.")]
		public bool KickOnMediumcoreDeath;

		/// <summary>The reason given if kicking a mediumcore players on death.</summary>
		[Description("The reason given if kicking a mediumcore players on death.")]
		public string MediumcoreKickReason = "Death results in a kick";

		/// <summary>Whether or not to ban mediumcore players on death.</summary>
		[Description("Whether or not to ban mediumcore players on death.")]
		public bool BanOnMediumcoreDeath;

		/// <summary>The reason given if banning a mediumcore player on death.</summary>
		[Description("The reason given if banning a mediumcore player on death.")]
		public string MediumcoreBanReason = "Death results in a ban";

		/// <summary>Enable or disable the whitelist based on IP addresses in the whitelist.txt file.</summary>
		[Description("Enable or disable the whitelist based on IP addresses in the whitelist.txt file.")]
		public bool EnableWhitelist;

		/// <summary>The reason given when kicking players for not being on the whitelist.</summary>
		[Description("The reason given when kicking players for not being on the whitelist.")]
		public string WhitelistKickReason = "You are not on the whitelist.";

		/// <summary>The reason given when kicking players that attempt to join while the server is full.</summary>
		[Description("The reason given when kicking players that attempt to join while the server is full.")]
		public string ServerFullReason = "Server is full";

		/// <summary>The reason given when kicking players that attempt to join while the server is full with no reserved slots available.</summary>
		[Description("The reason given when kicking players that attempt to join while the server is full with no reserved slots available.")]
		public string ServerFullNoReservedReason = "Server is full. No reserved slots open.";

		/// <summary>Whether or not to kick hardcore players on death.</summary>
		[Description("Whether or not to kick hardcore players on death.")]
		public bool KickOnHardcoreDeath;

		/// <summary>The reason given when kicking hardcore players on death.</summary>
		[Description("The reason given when kicking hardcore players on death.")]
		public string HardcoreKickReason = "Death results in a kick";

		/// <summary>Whether or not to ban hardcore players on death.</summary>
		[Description("Whether or not to ban hardcore players on death.")]
		public bool BanOnHardcoreDeath;

		/// <summary>The reason given when banning hardcore players on death.</summary>
		[Description("The reason given when banning hardcore players on death.")]
		public string HardcoreBanReason = "Death results in a ban";

		/// <summary>If GeoIP is enabled, this will kick users identified as being under a proxy.</summary>
		[Description("If GeoIP is enabled, this will kick users identified as being under a proxy.")]
		public bool KickProxyUsers = true;

		/// <summary>Require all players to register or login before being allowed to play.</summary>
		[Description("Require all players to register or login before being allowed to play.")]
		public bool RequireLogin;

		/// <summary>Allows users to login to any account even if the username doesn't match their character name.</summary>
		[Description("Allows users to login to any account even if the username doesn't match their character name.")]
		public bool AllowLoginAnyUsername = true;

		/// <summary>Allows users to register a username that doesn't necessarily match their character name.</summary>
		[Description("Allows users to register a username that doesn't necessarily match their character name.")]
		public bool AllowRegisterAnyUsername;

		/// <summary>The minimum password length for new user accounts. Can never be lower than 4.</summary>
		[Description("The minimum password length for new user accounts. Can never be lower than 4.")]
		public int MinimumPasswordLength = 4;

		/// <summary>The hash algorithm used to encrypt user passwords.
		/// Valid types: "sha512", "sha256" and "md5". Append with "-xp" for the xp supported algorithms.</summary>
		[Description("The hash algorithm used to encrypt user passwords. Valid types: \"sha512\", \"sha256\" and \"md5\". Append with \"-xp\" for the xp supported algorithms.")]
		public string HashAlgorithm = "sha512";

		/// <summary>Determines the BCrypt work factor to use. If increased, all passwords will be upgraded to new work-factor on verify.
		/// The number of computational rounds is 2^n. Increase with caution. Range: 5-31.</summary>
		[Description("Determines the BCrypt work factor to use. If increased, all passwords will be upgraded to new work-factor on verify. The number of computational rounds is 2^n. Increase with caution. Range: 5-31.")]
		public int BCryptWorkFactor = 7;

		/// <summary>Prevents users from being able to login with their client UUID.</summary>
		[Description("Prevents users from being able to login with their client UUID.")]
		public bool DisableUUIDLogin;

		/// <summary>Kick clients that don't send their UUID to the server.</summary>
		[Description("Kick clients that don't send their UUID to the server.")]
		public bool KickEmptyUUID;

		/// <summary>Disables a player if this number of tiles is painted within 1 second.</summary>
		[Description("Disables a player if this number of tiles is painted within 1 second.")]
		public int TilePaintThreshold = 15;

		/// <summary>Whether or not to kick users when they surpass the TilePaint threshold.</summary>
		[Description("Whether or not to kick users when they surpass the TilePaint threshold.")]
		public bool KickOnTilePaintThresholdBroken = false;

		/// <summary>The maximum damage a player/NPC can inflict.</summary>
		[Description("The maximum damage a player/NPC can inflict.")]
		public int MaxDamage = 1175;

		/// <summary>The maximum damage a projectile can inflict.</summary>
		[Description("The maximum damage a projectile can inflict.")]
		public int MaxProjDamage = 1175;

		/// <summary>Whether or not to kick users when they surpass the MaxDamage threshold.</summary>
		[Description("Whether or not to kick users when they surpass the MaxDamage threshold.")]
		public bool KickOnDamageThresholdBroken = false;

		/// <summary>Disables a player and reverts their actions if this number of tile kills is exceeded within 1 second.</summary>
		[Description("Disables a player and reverts their actions if this number of tile kills is exceeded within 1 second.")]
		public int TileKillThreshold = 60;

		/// <summary>Whether or not to kick users when they surpass the TileKill threshold.</summary>
		[Description("Whether or not to kick users when they surpass the TileKill threshold.")]
		public bool KickOnTileKillThresholdBroken = false;

		/// <summary>Disables a player and reverts their actions if this number of tile places is exceeded within 1 second.</summary>
		[Description("Disables a player and reverts their actions if this number of tile places is exceeded within 1 second.")]
		public int TilePlaceThreshold = 32;

		/// <summary>Whether or not to kick users when they surpass the TilePlace threshold.</summary>
		[Description("Whether or not to kick users when they surpass the TilePlace threshold.")]
		public bool KickOnTilePlaceThresholdBroken = false;

		/// <summary>Disables a player if this number of liquid sets is exceeded within 1 second.</summary>
		[Description("Disables a player if this number of liquid sets is exceeded within 1 second.")]
		public int TileLiquidThreshold = 50;

		/// <summary>Whether or not to kick users when they surpass the TileLiquid threshold.</summary>
		[Description("Whether or not to kick users when they surpass the TileLiquid threshold.")]
		public bool KickOnTileLiquidThresholdBroken = false;

		/// <summary>Whether or not to ignore shrapnel from crystal bullets for the projectile threshold count.</summary>
		[Description("Whether or not to ignore shrapnel from crystal bullets for the projectile threshold count.")]
		public bool ProjIgnoreShrapnel = true;

		/// <summary>Disable a player if this number of projectiles is created within 1 second.</summary>
		[Description("Disable a player if this number of projectiles is created within 1 second.")]
		public int ProjectileThreshold = 50;

		/// <summary>Whether or not to kick users when they surpass the Projectile threshold.</summary>
		[Description("Whether or not to kick users when they surpass the Projectile threshold.")]
		public bool KickOnProjectileThresholdBroken = false;

		/// <summary>Disables a player if this number of HealOtherPlayer packets is sent within 1 second.</summary>
		[Description("Disables a player if this number of HealOtherPlayer packets is sent within 1 second.")]
		public int HealOtherThreshold = 50;

		/// <summary>Whether or not to kick users when they surpass the HealOther threshold.</summary>
		[Description("Whether or not to kick users when they surpass the HealOther threshold.")]
		public bool KickOnHealOtherThresholdBroken = false;

		/// <summary>Disables a player if this number of tiles is present in a Tile Rectangle packet</summary>
		[Description("Disables a player if this number of tiles is present in a Tile Rectangle packet")]
		public int TileRectangleSizeThreshold = 50;

		/// <summary>Whether or not to kick users when they surpass the TileRectangleSize threshold.</summary>
		[Description("Whether or not to kick users when they surpass the TileRectangleSize threshold.")]
		public bool KickOnTileRectangleSizeThresholdBroken = false;

		/// <summary>Whether or not the server should suppress build permission failure warnings from regions, spawn point, or server edit failure.</summary>
		[Description("Whether or not the server should suppress build permission failure warnings from regions, spawn point, or server edit failure.")]
		public bool SuppressPermissionFailureNotices = false;
		#endregion


		#region Chat Settings

		/// <summary>Specifies which string starts a command.
		/// Note: Will not function properly if the string length is bigger than 1.</summary>
		[Description("Specifies which string starts a command.\nNote: Will not function properly if the string length is bigger than 1.")]
		public string CommandSpecifier = "/";

		/// <summary>Specifies which string starts a command silently.
		/// Note: Will not function properly if the string length is bigger than 1.</summary>
		[Description("Specifies which string starts a command silently.\nNote: Will not function properly if the string length is bigger than 1.")]
		public string CommandSilentSpecifier = ".";

		/// <summary>Disables sending logs as messages to players with the log permission.</summary>
		[Description("Disables sending logs as messages to players with the log permission.")]
		public bool DisableSpewLogs = true;

		/// <summary>Prevents OnSecondUpdate checks from writing to the log file.</summary>
		[Description("Prevents OnSecondUpdate checks from writing to the log file.")]
		public bool DisableSecondUpdateLogs = false;

		/// <summary>The chat color for the superadmin group.</summary>
		[Description("The chat color for the superadmin group.\n#.#.# = Red/Blue/Green\nMax value: 255")]
		public int[] SuperAdminChatRGB = { 255, 255, 255 };

		/// <summary>The superadmin chat prefix.</summary>
		[Description("The superadmin chat prefix.")]
		public string SuperAdminChatPrefix = "(Super Admin) ";

		/// <summary>The superadmin chat suffix.</summary>
		[Description("The superadmin chat suffix.")]
		public string SuperAdminChatSuffix = "";

		/// <summary>Whether or not to announce a player's geographic location on join, based on their IP.</summary>
		[Description("Whether or not to announce a player's geographic location on join, based on their IP.")]
		public bool EnableGeoIP;

		/// <summary>Displays a player's IP on join to users with the log permission.</summary>
		[Description("Displays a player's IP on join to users with the log permission.")]
		public bool DisplayIPToAdmins;

		/// <summary>Changes in-game chat format: {0} = Group Name, {1} = Group Prefix, {2} = Player Name, {3} = Group Suffix, {4} = Chat Message.</summary>
		[Description("Changes in-game chat format: {0} = Group Name, {1} = Group Prefix, {2} = Player Name, {3} = Group Suffix, {4} = Chat Message.")]
		public string ChatFormat = "{1}{2}{3}: {4}";

		/// <summary>Changes the player name when using chat above heads. Starts with a player name wrapped in brackets, as per Terraria's formatting.\nSame formatting as ChatFormat without the message.</summary>
		[Description("Changes the player name when using chat above heads. Starts with a player name wrapped in brackets, as per Terraria's formatting.\nSame formatting as ChatFormat without the message.")]
		public string ChatAboveHeadsFormat = "{2}";

		/// <summary>Whether or not to display chat messages above players' heads.</summary>
		[Description("Whether or not to display chat messages above players' heads.")]
		public bool EnableChatAboveHeads = false;

		/// <summary>The RGB values used for the color of broadcast messages.</summary>
		[Description("The RGB values used for the color of broadcast messages.\n#.#.# = Red/Blue/Green\nMax value: 255")]
		public int[] BroadcastRGB = { 127, 255, 212 };

		#endregion


		#region MySQL Settings

		/// <summary>The type of database to use when storing data (either "sqlite" or "mysql").</summary>
		[Description("The type of database to use when storing data (either \"sqlite\" or \"mysql\").")]
		public string StorageType = "sqlite";

		/// <summary>The path of sqlite db.</summary>
		[Description("The path of sqlite db.")]
		public string SqliteDBPath = "tshock.sqlite";

		/// <summary>The MySQL hostname and port to direct connections to.</summary>
		[Description("The MySQL hostname and port to direct connections to.")]
		public string MySqlHost = "localhost:3306";

		/// <summary>The database name to connect to when using MySQL as the database type.</summary>
		[Description("The database name to connect to when using MySQL as the database type.")]
		public string MySqlDbName = "";

		/// <summary>The username used when connecting to a MySQL database.</summary>
		[Description("The username used when connecting to a MySQL database.")]
		public string MySqlUsername = "";

		/// <summary>The password used when connecting to a MySQL database.</summary>
		[Description("The password used when connecting to a MySQL database.")]
		public string MySqlPassword = "";

		/// <summary>Whether or not to save logs to the SQL database instead of a text file.</summary>
		[Description("Whether or not to save logs to the SQL database instead of a text file.\nDefault = false.")]
		public bool UseSqlLogs = false;

		/// <summary>Number of times the SQL log must fail to insert logs before falling back to the text log.</summary>
		[Description("Number of times the SQL log must fail to insert logs before falling back to the text log.")]
		public int RevertToTextLogsOnSqlFailures = 10;

		#endregion


		#region REST API Settings

		/// <summary>Enable or disable the REST API.</summary>
		[Description("Enable or disable the REST API.")]
		public bool RestApiEnabled;

		/// <summary>The port used by the REST API.</summary>
		[Description("The port used by the REST API.")]
		public int RestApiPort = 7878;

		/// <summary>Whether or not to log REST API connections.</summary>
		[Description("Whether or not to log REST API connections.")]
		public bool LogRest = false;

		/// <summary>Whether or not to require token authentication to use the public REST API endpoints.</summary>
		[Description("Whether or not to require token authentication to use the public REST API endpoints.")]
		public bool EnableTokenEndpointAuthentication;

		/// <summary>The maximum REST requests in the bucket before denying requests. Minimum value is 5.</summary>
		[Description("The maximum REST requests in the bucket before denying requests. Minimum value is 5.")]
		public int RESTMaximumRequestsPerInterval = 5;

		/// <summary>How often in minutes the REST requests bucket is decreased by one. Minimum value is 1 minute.</summary>
		[Description("How often in minutes the REST requests bucket is decreased by one. Minimum value is 1 minute.")]
		public int RESTRequestBucketDecreaseIntervalMinutes = 1;

		/// <summary>A dictionary of REST tokens that external applications may use to make queries to your server.</summary>
		[Description("A dictionary of REST tokens that external applications may use to make queries to your server.")]
		public Dictionary<string, SecureRest.TokenData> ApplicationRestTokens = new Dictionary<string, SecureRest.TokenData>();

		#endregion
	}

	/// <summary>
	/// TShock's configuration file
	/// </summary>
	public class TShockConfig : ConfigFile<TShockSettings>
	{
		/// <summary>
		/// Upgrades the configuration file from the old format if required, then reads and returns the currently configured <see cref="TShockSettings"/>
		/// </summary>
		/// <param name="json"></param>
		/// <param name="incompleteSettings"></param>
		/// <returns></returns>
		public override TShockSettings ConvertJson(string json, out bool incompleteSettings)
		{
			var settings = FileTools.LoadConfigAndCheckForChanges<TShockSettings>(json, out incompleteSettings);

			Settings = settings;
			OnConfigRead?.Invoke(this);

			return settings;
		}

		/// <summary>
		/// Dumps all configuration options to a text file in Markdown format
		/// </summary>
		public static void DumpDescriptions()
		{
			var sb = new StringBuilder();
			var defaults = new TShockSettings();

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
