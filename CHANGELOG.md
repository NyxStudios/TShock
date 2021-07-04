# TShock for Terraria

This is the rolling changelog for TShock for Terraria. Use past tense when adding new entries; sign your name off when you add or change something. This should primarily be things like user changes, not necessarily codebase changes unless it's really relevant or large.

## How to add a changelog entry
* Put your entry in terms of what you changed in the past mood. For example: "Changed the world by adding new grommets."
  * Not "fix" or "change".
  * The entry must start with a verb.
  * End your sentence with a period.
  * Write in complete sentences that are understandable by anyone who does not have experience programming, unless the change is related to programming.
  * Do not insert tabs into this file, under any circumstances, ever.
  * Do not forget to sign every line you change with your name. (@hakusaro)
  * If there is no section called "Upcoming changes" below this line, please add one with `## Upcoming changes` as the first line, and then a bulleted item directly after with the first change.

## Upcoming changes
* Type naming for tile & wall ID's in `/grow`, Removing cases: 'cactus' & 'herb' until a fix is found. (@Rozen4334)
* Added pragma notation to remove XML comment warnings in `Commands.cs`. (@Rozen4334)
* Changed the world autosave message so that it no longer warns of a "potential lag spike." (@hakusaro)
* Added `/slay` as an alias for `/kill` to be more consistent with other server mods. (@hakusaro)
* Added `/god` as an alias for `/godmode` to be more consistent with other server mods. (@hakusaro)
* Fixed ridiculous typo in `Amethyst Gemtree` text. (@hakusaro)
* Fixed `CTRL + C` / interactive console interrupt not safely shutting down the server. Now, interrupts will cause a safe shutdown (saving the world and disconnecting all players before fully shutting down). Previously, interrupts caused an unsafe shutdown (not saving the world). (@hakusaro)
* Changed "success message" color to `Color.LimeGreen` instead of `Color.Green`. `Color.Green` looks ugly. `Color.LimeGreen` looks less ugly but isn't as offensively bright as pure green. (@hakusaro)
* Changed the default respawn timer to 10 seconds, so as to not desynchronize from the game by default. (@hakusaro)
* Fixed `/home` allowing players to bypass the respawn timer. (@hakusaro, @moisterrific, @Arthri)
* Added the config option `SuppressPermissionFailureNotices`. When set to `true`, the server will not send warning messages to players when they fail a build permission check from `TSPlayer.HasBuildPermission` (even if `shouldWarnPlayer` is set to true. (@hakusaro)
* Fixed `/warp send` failing a nullcheck if the warp didn't exist. The previous behavior may have always been buggy or broken. In other words, sending someone to a warp that doesn't exist should result in a nicer error. (@hakusaro, @punchready)
* Fixed `/group del` allowing server operators to delete the default group that guests are put into. This is a really critical group and the server doesn't behave correctly when it happens. As a result, it's better to prevent this from happening than not. Additionally, `GroupManagerException`s will be thrown if this is attempted programmatically. Finally, if the exception is thrown in response to `/group del` (or if any other exception is thrown that the command handler can handle), the stack trace will no longer be present. Fixes [#2165](https://github.com/Pryaxis/TShock/issues/2165). (@hakusaro, @DeveloperLuxo, @Rozen4334, @moisterrific, @bartico6, @Quinci135)
* Removed the old `ConfigFile` class. If you are updating a plugin, you should use `TShock.Config.Settings` instead of the accessor you were using. This is typically a really easy change. For most plugin authors, updating to the new config format is as simple as changing the reference to the old static config to point to the new location. If you were using this for your own configs, you should swap to using a `IConfigFile` (see `TShockAPI.Configuration.ConfigFile`). (@hakusaro, @bartico6)
* Added `Main.worldPathName` to `/worldinfo` command. Now, if you need to see what the location on disk for your world file is, you can simply run `/worldinfo` to find out. This is particularly helpful on Linux and macOS, where the world path isn't obvious. (@hakusaro)
* Correct rejection message in LandGolfBallInCupHandler to output the proper expected player id. (@drunderscore)
* Clarified the error mesage that the console is presented if a rate-limit is reached over REST to indicate that "tokens" actually refers to rate-limit tokens, and not auth tokens, and added a hint as to what config setting determines this. (@hakusaro, @patsore)
* Fixed an issue where, when the console was redirected, input was disabled and commands didn't work, in TSAPI. You can now pass `-disable-commands` to disable the input thread, but by default, it will be enabled. Fixes [#1450](https://github.com/Pryaxis/TShock/issues/1450). (@DeathCradle, @QuiCM)

## TShock 4.5.4
* Fixed ridiculous typo in `GetDataHandlers` which caused TShock to read the wrong field in the packet for `usingBiomeTorches`. (@hakusaro, @Arthri)
* Fixed torchgod settings to include whether or not torchgod has been fought by the player before and respect `usingBiomeTorches` setting. (@Quinci135)
* Fixed /worldmode not synchronising data to players after updating the world state (@bartico6, @Arthri)
* Added `OnSendNetData` hook to TSAPI, which enables developers to intercept traffic being sent from the server to clients using the new NetPacket protocol. (@Stealownz)
* Fixed false positive `OnNPCAddBuff` detection when throwing rotten eggs at town NPCs while wearing Frost armor set. (@moisterrific)
* Moved the emoji player index check into a new class of handlers called `IllegalPerSe`, which is designed to help isolate parts of TShock and make it so that "protocol violations" are treated separately from heuristic based anti-cheat checks. (@hakusaro)
* Changed `TSPlayer.FindByNameOrID` so that it will continue searching for players and return a list of many players whem ambiguous matches exist in all cases. Specifically, this avoids a scenario where a griefer names themselves `1` and is difficult to enact justice on, because their name will not be found by the matching system used to kick players. To help with ambiguity, this method now processes requests with prefixes `tsi:` and `tsn:`. `tsi:[number]` will process the search as looking for an exact player by ID. `tsn:` will process the search as looking for an exact name, case sensitive. In both cases, the system will return an exact result in the "old-style" result, i.e., a `List<TSPlayer>` with exactly one result. For example, `/kick tsid:1` will match the player with the ID `1`. `/kick tsn:1` will match the username `1`. In addition, players who attempt to join the server with the name prefixes `tsn:` and `tsi:` will be rejected for having invalid names. (@hakusaro, @Onusai)
* Added warnings for conditions where a password is set at runtime but can be bypassed. The thinking is that if a user sets a password when they're booting the server, that's what they expect to be the password. The only thing is that sometimes, other config options can basically defeat this as a security feature. The goal is just to communicate more and make things clearer. The server also warns users when UUID login is enabled, because it can be confusing and insecure. (@hakusaro, @Onusai)
* Fixed Torch God's Favor biome torch placement being rejected by the server. (@moisterrific)
* Changed backups created by the backup manager to use ISO8601-style timestamps. I say "style" because it's impossible to implement ISO8601 or RFC3389 dates in a filename on most modern filesystems. So instead of the proper ISO separators, we've got dashes and dots. (@hakusaro, change sponsored by @drunderscore)
* Added hook for `OnDoorUse` (`DoorUse`) and associated `DoorUseEventArgs` fired when a door is used. Also added `GetDataHandlers.DoorAction` enum for determining the action of a door. (@hakusaro)
* Disallowed loading of the AutoRegister plugin version 1.2.0 or lower. Versions of this plugin at or equal to 1.2.0 use low entropy material to create passwords. This effectively means that it's possible for any user to be easily impersonated on a server running AutoRegister by simply convincing a user to join a malicious server, even when UUID login is disabled. This was assigned [GHSA-w3h6-j2gm-qf7q](https://github.com/Pryaxis/Plugins/security/advisories/GHSA-w3h6-j2gm-qf7q). (@hakusaro)
* Disallowed loading of another plugin due to [security issue GHSA-qj59-99v9-3gww](https://github.com/Pryaxis/Plugins/security/advisories/GHSA-qj59-99v9-3gww). Due to the importance of this issue and severity, information is not available in the changelog. Information will be available [June 8th, 2021, at 12:00 MDT](https://time.is/1200PM_8_June_2021_in_Littleton?GHSA-qj59-99v9-3gww_information_release). (@hakusaro)

## TShock 4.5.3
* Added permissions for using Teleportation Potions, Magic Conch, and Demon Conch. (@drunderscore)
  * `tshock.tp.tppotion`, `tshock.tp.magicconch`, and `tshock.tp.demonconch` respectively.
* Updated HealOtherPlayer damage check to make more sense by respecting `ignoredamagecap` permission. (@moisterrific)
* Added preliminary support for Terraria 1.4.2.3 (@moisterrific, @Moneylover3246, @DeathCradle)
* Added celebration mk2 explosive to explosives ItemID set in TSAPI. Solves #2304. (@Quinci135)
* TShock now writes its log files to the `logs` folder inside the `tshock` folder by default, as opposed to just the `tshock` folder. (@QuiCM)
* The default MOTD is now prettier. The MOTD format can now contain `%specifier%` to send the command specifier. (@moisterrific)
* The buff commands now support `-1` as a time option to set buffs that last 415 days (the maximum buff time the game supports). (@moisterrific)
* TShock defaults to saving backups every 10 minutes, and defaults to keeping backups for 4 hours. (@hakusaro)
* Updated SSC bypass messaging. Now, when you connect, you're told if you're bypassing SSC. Console logging has been improved to warn when players are not being saved due to the bypass SSC permission. To turn this warning off, change `WarnPlayersAboutBypassPermission` to `false` in the `sscconfig.json` file. (@hakusaro)
* Fix oversight & exploit allowing specially crafted SendTileRectangle packets to perform large-scale world griefing. In addition, `NetTile.Slope` is now the native value (byte), and accessor methods `Slope1`, `Slope2`, and `Slope3` can be used to get the old style of values out. `HalfBrick` and `Actuator` were removed from `NetTile` because these were initialized to zero and never changed or used. (@bartico6)

## TShock 4.5.2
* Added preliminary support for Terraria 1.4.2.2. (@hakusaro)
* Removed `/ungodme` and godmode warning (no longer necessary). Also, godmode now supports silent commands. (@hakusaro)
* Added permission 'tshock.rest.broadcast' to the /v2/server/broadcast REST endpoint. (@TheBambino)

## TShock 4.5.1
* Fixed server crash from `/v2/players/list` & other parameterised REST endpoints. (@QuiCM, reported by @ATFGK)
* Added handling to the PlayerChat hook event. (@QuiCM - Thanks for the suggestion @Arthri)
* Changed the spawnboss command to support silent command specifiers. (@QuiCM, suggested by @nojomyth-dev)
* Updated /godmode to use Journey Mode's Godmode power instead of healing on damage. (requested by @tlworks, backported by @bartico6, implemented preemptive bugfix for creative powers mentioned by @Stealownz)
* Fixed /r attempting to send messages to players that have since disconnected. (@bartico6, reported by @Arthri)
* Added ban ticket ID to ban messages (@QuiCM, suggested by @Bippity)
* Refactored /wallow command. /reply no longer bypasses /wallow (@QuiCM)

## TShock 4.5.0.1
* Fixed conversion from old to new ban system for MySQL hosted ban databases. (@DeathCradle, @ATFGK)
* Fixed wrong identifier used for UUID bans. (@DeathCradle, @ATFGK)
* Fixed conversion from sqlite bans due to locking issue. (@DeathCradle, @Kojirremer)

## TShock 4.5.0
* Updated OTAPI and TSAPI to Terraria 1.4.2.1. (@Stealownz, @DeathCradle)
* Updated TShock with preliminary protocol support for Terraria 1.4.2.1. (@Stealownz)

## TShock 4.4.0 (Pre-release 16)
* Patched protocol issue. Thanks to Off (@tlworks) and @bartico6 for contributions, including packet captures, packet analysis, exploit proof-of-concept testing, patch testing, and detailed reproduction steps. (@hakusaro)
* Disabled debug by default. (@hakusaro)
* Changed "WinVer" field in `/serverinfo` to "Operating System". (@Terrabade)
* Rewritten `/grow`, added every default tree type & changed the default help response. (@Nova4334)
  * Added a new permission: `tshock.world.growevil` to prevent players to grow evil biome trees, these trees spawn with evil biome blocks below them.
* Introduced `/wallow` to disable or enable recieving whispers from other players. (@Nova4334)
* Removed stoned & webbed from disabled status (@QuiCM)
* Fix -forceupdate flag not forcing updates (@Quake)

## TShock 4.4.0 (Pre-release 15)
* Overhauled Bans system. Bans are now based on 'identifiers'. (@QuiCM)
  * The old Bans table (`Bans`) has been deprecated. New bans will go in `PlayerBans`. Old bans will be converted automatically to the new system.
  * All old ban routes in REST are now redirected. Please use `/v3/bans/*` for REST-based ban management.
  * TShock recognizes and acts upon 4 main identifiers: UUID, IP, Player Name, Account name. This can be extended by plugins. New identifiers can be added to the `ban help identifiers` output by registering them in `TShockAPI.DB.Identifier.Register(string, string)`
  * By default, bans are no longer removed upon expiry or 'deletion'. Instead, they remain in the system. A new ban for an indentifier can be added once an existing ban has expired.
* Server Console now understands Terraria color codes (e.g., `[c/FF00FF:Words]`) and prints the colored text to the console. Note that console colors are limited and thus only approximations. (@QuiCM)
* Fixed a bug in `/sudo` that prevented quoted arguments being forwarded properly. Example: `/sudo /user group "user name" "user group"` should now work correctly. (@QuiCM)
* Shutting down the server should now correctly display the shutdown message to players rather than 'Lost connection'. (@QuiCM)
* For developers: TShock now provides `IConfigFile<TSettings>` and `ConfigFile<TSettings>` under the `TShockAPI.Configuration` namespace. No more needing to copy/pasting the same Read/Write code for your plugin configs. (@QuiCM)
  * `ConfigFile<TSettings>` implements `Read` and `Write` for you.
  * Check `TShockConfig` and `ServerSideConfig` for examples on how to use.
* Added URI un-escaping on all inputs into REST. (@QuiCM)
* Attempt to fix platinum coin pickup dupe. (Thanks @Quinci135)

## TShock 4.4.0 (Pre-release 14)
* Terraria v1.4.1.2 (Thanks @Patrikkk and @DeathCradle <3)
* Added Torch God's Favor support in SSC. (@Stealownz)
* SendTileSquare is now SendTileRect and can now send rectangles instead of squares. This is a breaking change (@QuiCM)
* Destroying protected tiles underneath a tile object no longer causes the tile object to disappear for the client (@QuiCM)
* 'RegionProtectGemLocks' config option now works correctly. Gems can now be placed in Gem Locks while this option is enabled (@QuiCM)

## TShock 4.4.0 (Pre-release 13)
* Terraria v1.4.1.1
* Added Gravedigger's Shovel support. (@Zennos)
* You can now start up multiple TShock servers at once without getting a startup error. (@ZakFahey)
* Updated bouncer to include new Magma Stone, Frost Armor, and Spinal Tap inflicted npc debuffs to bouncer. (@Quinci135)

## TShock 4.4.0 (Pre-release 12)
* Fixed various bugs related to Snake Charmer's Flute. (@rustly)  
  * The entirety of the snake now places.  
  * The old snake now removes when placing a new snake.
  * Players are no longer disabled for breaking TilePlace/TileKill thresholds when modifying snakes.  
* Prevented players from seeing the npc spawnrate change permission error on join. (@rustly)
* Installed new sprinklers!
* Organized parameters by category and relevance in the `config.json` file. (@kubedzero)
* Fixed multiple holes in Bouncer OnTileData. (@Patrikkk, @hakusaro)
  * Issue where players could replace tiles with banned tiles without permission. 
  * Including replace action in TilePlace threshold incrementation, so players cannot bypass the threshold while replacing tiles/walls.
  * Including check for maxTileSets when player is replacing tiles, so players cannot send invalid tile data through the replace tile action.
  * Including a check for ReplaceWall when the tile is a Breakable/CutTile.
* Adding checks in Bouncer OnNewProjectile (@Patrikkk):
  * For valid golf club and golf ball creation.
  * Renamed stabProjectile to directionalProjectile for a more accurate naming.
  * Adding staff projectiles to the directionalProjectiles Dictionary to include staffs in the valid projectile creation check.
  * Adding GolfBallItemIDs list in Handlers.LandGolfBallInCupHandler.cs
* Fixed an issue in the SendTileSquare handler that was rejecting valid tile objects. (@QuiCM)
* Fixed the issue where players were unable to place regular ropes because of the valid placement being caught in Bouncer OnTileEdit. (@Patrikkk)
* Added pet license usage permissions to `trustedadmin` and `owner` groups. Do note that this has a high network usage and can be easily be abused so it is not recommended to give out this permission to lower level groups. (@moisterrific) 
* Removed checks that prevented people placing personal storage tiles in SSC as the personal storage is synced with the server. (@Patrikkk)
* Cleaned up a check in Bouner OnTileEdit where it checks for using the respective item when placing a tile to make it clearer. This change also fixed the issue in a previous commit where valid replace action was caught. Moved the check for max tile/wall types to the beginning of the method. (@Patrikkk)
* Improved clarity for insufficient permission related error messages. (@moisterrific)
* Removed redundant Boulder placement check that prevented placing chests on them, as it is no longer possible to place a chest on a boulder, so nothing crashes the server. "1.2.3: Boulders with Chests on them no longer crash the game if the boulder is hit." (@kevzhao2, @Patrikkk)
* `/itemban` - `/projban` - `/tileban` - Added a `default:` case to the commands so an invalid subcommand promts the player to enter the help subcommand to get more information on valid subcommands. (@Patrikkk)
* `/world` - Renamed to /worldinfo to be more accurate to it's function. Command now displays the world's `Seed`. Reformatted the world information so each line isn't repeatedly starting with "World". (@Patrikkk)
* `/who` - Changed the display format of the online players when the `-i` flag is used. From `PlayerName (ID: 0, ID: 0)` to `PlayerName (Index: 0, Account ID: 0)` for clarification. (@Patrikkk)
* Added DisplayDollItemSync event. An event that is called when a player modifies the slot of a DisplayDoll (Mannequin). This event provides information about the current item in the displaydoll, as well as the item that the player is about to set. (@Patrikkk)
* Added DisplayDollItemSyncHandler, which checks for building permissions of the player at the position of the DisplayDoll. (If they do not have permissions, it means they are hacking as they could not even open the doll in the first place.) (@Patrikkk)
* Added RequestTileEntity packet handling. (@Patrikkk)
  * Implemented the OnRequestTileEntityInteraction even hook in GetDataHandler. (@Patrikkk)
  * Created RequestTileEntityInteractionHandler which checks for building permissions when the player is attempting to open a display doll (Mannequin) or a Hat Rack. This now prevents players from opening a Mannequin or a Hat Rack if they have no building permissions at the position of these tile entities. As of 1.4.0.5, these are the only two items that use this packet. (@Patrikkk)

## TShock 4.4.0 (Pre-release 11)
* Added new permission `tshock.tp.pylon` to enable teleporting via Teleportation Pylons (@QuiCM)
* Added new permission `tshock.journey.research` to enable sharing research via item sacrifice (@QuiCM)
* Add Emoji event to GetDataHandler. This packet is received when a player tries to display an emote. (@Patrikkk)
  * Added EmojiHandler to handle an exploit. Adding `tshock.sendemoji` permission and checks. Added this permission to guest group by default. (@Patrikkk)
* Handled SyncCavernMonsterType packet to prevent an exploit where players could modify the server's cavern monster types and make the server spawn any NPCs - including bosses - onto other players. (@Patrikkk)
* Added LandGolfBallInCup event which is accessible for developers to work with, as well as LandGolfBallInCup handler to handle exploits where players could send direct packets to trigger and imitate golf ball cup landing anywhere in the game world. Added two public lists in Handlers.LandGolfBallInCupHandler: GolfBallProjectileIDs and GolfClubItemIDs. (@Patrikkk)
* Added SyncTilePicking event. This is called when a player damages a tile. Implementing SyncTilePickingHandler and patching tile damaging related exploits. (Preventing player sending invalid world position data which disconnects other players.)
* Fixed the issue where mobs could not be fished out during bloodmoon because of Bouncer checks. (@Patrikkk)
  * Fixed the issue where certain fishing rods could not fish out NPCs due to a Bouncer check. (@Patrikkk)
* Update for OTAPI 2.0.0.37 and Terraria 1.4.0.5. (@hakusaro, @Patrikkk)
* Added additional config options for automatically kicking clients from the server upon breaking anti-cheat thresholds. (@moisterrific)
* Added pylon teleportation permission to default group, added `/spawn` permission to admin group, added the new journey mode research permission to trustedadmin, and moved all previous journey mode permissions from owner to trustedadmin. (@moisterrific)

## TShock 4.4.0 (Pre-release 10)
* Fixed all rope coils. (@Olink)
* Fixed a longstanding issue with SendTileSquare that could result in desyncs and visual errors. (@QuiCM)
* Fixed placement issues with Item Frames, Teleportation Pylons, etc. (@QuiCM)
* Fixed doors, and they are good now for real probably. (@QuiCM, @Hakusaro, @Olink)
* Bumped default max damage received cap to 42,000 to accommodate the Empress of Light's instant kill death amount. (@hakusaro, @moisterrific, @Irethia, @Ayrawei)
* Updated `/spawnboss` command to include Empress of Light, Queen Slime, and other additional bosses that have a health bar. (@moisterrific)

## TShock 4.4.0 (Pre-release 9)
* Fixed pet licenses. (@Olink)
* Added initial support for Journey mode in SSC worlds. (@Olink)
* Made TShock database MySQL 8 compatible by escaping column names in our IQueryBuilder code. (Name `Groups` is a reserved element in this version, which is used in our `Region` table.) (@Patrikkk)
* Reintroduced `-worldselectpath` per feedback from @fjfnaranjo. This command line argument should be used to specify the place where the interactive server startup will look for worlds to show on the world select screen. The original version of this argument, `-worldpath`, was removed because several game service providers have broken configurations that stop the server from running with an unhelpful error. This specific configuration was `-world` and `-worldpath`. In the new world, you can do the following:
  * `-worldselectpath` should be used if you want to customize the server interactive boot world list (so that you can select from a number of worlds in non-standard locations).
  * `-world` will behave as an absolute path to the world to load. This is the most common thing you want if you're starting the server and have a specific world in mind.
  * `-worldselectpath` and `-worldname` should work together enabling you to select from a world from the list that you specify. This is *not* a world file name, but a world name as described by Terraria.
  * `-worldselectpath` is identical to the old `-worldpath`. If you specify `-worldselectpath` and `-world` without specifying an absolute path the server will crash for sure.
  * Thank you again to @fjfnaranjo for supplying a [detailed feature request](https://github.com/Pryaxis/TShock/issues/1914) explaining precisely why this option should be available. Without this, we would have had no context as to why this feature was useful or important. Thank you, @fjfnaranjo!
  * This change was implemented by (@QuiCM, @hakusaro).
* Updated Bouncer to include Sparkle Slime debuff that can be applied to town NPCs. (@moisterrific)
* Updated `/spawnboss` command to include Empress of Light, Queen Slime, and other additional bosses that have a health bar. (@moisterrific)
* Added journey mode permissions to owner group by default. (@moisterrific)
* Fixed kick on hardcore death / kick on mediumcore death / ban on either from taking action against journey mode players. (@hakusaro)
* Attempted to fix the problem with the magic mirror spawn problems. You should be able to remove your spawn point in SSC by right clicking on a bed now. (@hakusaro, @AxeelAnder)
* Added HandleFoodPlatterTryPlacing event, which is called whenever a player places a food in a plate. Add antihack to bouncer, to prevent removing food from plates if the region is protected; To prevent placement if they are not in range; To prevent placement if the item is not placed from player hand. (@Patrikkk)
* Fixed an offset error in NetTile that impacted `SendTileSquare`. It was being read as a `byte` and not a `ushort`. (@QuiCM)
* Fixed coins not dropping after being picked up by npcs. The ExtraValue packet was not being read correctly. (@Olink)
* Removed packet monitoring from debug logs. To achieve the same results, install @QuiCM's packet monitor plugin (it does better things). (@hakusaro)
* Updated packet monitoring in send tile square handler for Bouncer debugging. (@hakusaro)
* Added `/sync`, activated with `tshock.synclocalarea`. This is a default guest permission. When the command is issued, the server will resync area around the player in the event of a desync issue. (@hakusaro)
  * If your doors disappear, this command will allow a player to resync without having to disconnect from the server.
  * The default group that gets this permission is `Guest` for the time being.
  * To add this command to your guest group, give them `tshock.synclocalarea`, with `/group addperm guest tshock.synclocalarea`.
  * This command may be removed at any time in the future (and will likely be removed when send tile square handling is fixed).
* Add FishOutNPC event handler, which is called whenever a player fishes out an NPC using a fishing rod. Added antihack to Bouncer, to prevent unathorized and invalid mob spawning, by checking player action, NPC IDs and range. (@Patrikkk, @moisterrific)
* Fixed smart door automatic door desync and deletion issue. (@hakusaro)

## TShock 4.4.0 (Pre-release 8)
* Update for OTAPI 2.0.0.36 and Terraria 1.4.0.4. (@hakusaro, @Patrikkk, @DeathCradle)
* Fixed /wind command. (@AxeelAnder)
* Fixed NPC debuff issue when attempting to fight bosses resulting in kicks. (@AxeelAnder)
* Fixed players are unable to remove an NPC. Change `byte NPCHomeChangeEventArgs.Homeless` to `HouseholdStatus NPCHomeChangeEventArgs.HouseholdStatus`. (@AxeelAnder)
* Fixed lava, wet, honey, and dry bombs;  
  and lava, wet, honey, and dry grenades;  
  and lava, wet, honey, and dry rockets;  
  and lava, wet, honey, and dry mines. (@Olink)
* Fix Bloody Tear displaying the wrong text when used. (@Olink)
* Fix the visibility toggle for the last two accessory slots. (@Olink)
* Adding Journey mode user account permissions. Journey mode must be enabled for these to have any effect. (@Patrikkk)
  * `tshock.journey.time.freeze`
  * `tshock.journey.time.set`
  * `tshock.journey.time.setspeed`
  * `tshock.journey.godmode`
  * `tshock.journey.wind.strength`
  * `tshock.journey.wind.freeze`
  * `tshock.journey.rain.strength`
  * `tshock.journey.rain.freeze`
  * `tshock.journey.placementrange`
  * `tshock.journey.setdifficulty`
  * `tshock.journey.biomespreadfreeze`
  * `tshock.journey.setspawnrate`
* Changed default thresholds for some changes in the config file to accommodate new items & changes to Terraria. (@hakusaro)
* Store projectile type in `ProjectileStruct RecentlyCreatedProjectiles` to identify the recently created projectiles by type. Make `RecentlyCreatedProjectiles` and `ProjectileStruct` public for developers to access from plugins.

## TShock 4.4.0 (Pre-release 7 (Entangled))
* Fixed bed spawn issues when trying to remove spawn point in SSC. (@Olink)
* Fixed Snake Flute. (@Olink)
* Fixed lava absorbant sponge not capturing lava. `LiquidSetEventArgs` now returns a `LiquidType` instead of a byte type. (@hakusaro)
* Fixed bottomless lava bucket from not being able to create lava. (@hakusaro)
  * Ban a lava bucket to ban lava on the server entirely, until we figure out a better way to handle liquids.
* Fixed scarab bombs not detonating on pick style tiles. (@hakusaro)
* Fixed dirt bombs not creating dirt. (@hakusaro)
* Added a ridiculous amount of debug information. If you're experiencing any problems with 1.4 items being caught by the TShock anticheat system, please turn on DebugLogs in your config file and capture log data. It'll be extremely helpful in narrowing down precisely how to fix your problem. (@hakusaro)
* Released with entangled support for 1.4.0.4 based on @Patrikkk local build and latest snapshot gen-dev. (@hakusaro)

## TShock 4.4.0 (Pre-release 6)
* Updates to OTAPI 2.0.0.35 (@DeathCradle).

## TShock 4.4.0 (Pre-release 5)
* Update player spawn related things to 1.4. `Terraria.Player.Spawn` method now has a required argument, `PlayerSpawnContext context`. (@AxeelAnder)
* Make sqlite db path configurable. (@AxeelAnder)
* Terraria 1.4.0.3 experimental support. (@Patrikkk)
* Updated changelog. (@hakusaro)

## TShock 4.4.0 (Pre-release 4)
* Debug logging now provides ConsoleDebug and ILog has been updated to support the concept of debug logs. Debug logs are now controlled by `config.json` instead of by preprocessor debug flag. (@hakusaro)
* Removed `/confuse` command and Terraria player data resync from @Zidonuke. (@hakusaro)
* Attempted to fix the player desync issue by changing `LastNetPosition` logic and disabling a check in Bouncer that would normally reject player update packets from players. (@QuiCM, @hakusaro)

## TShock 4.4.0 (Pre-release 3)
* Fixed `/worldmode` command to correctly target world mode. (@Ristellise)
* The following commands have been removed: `tbloodmoon`, `invade`, `dropmeteor`. `fullmoon`, `sandstorm`, `rain`, `eclipse`
* The following command has been added to replace them: `worldevent`. This command requires the `tshock.world.events` permission.
  * `worldevent` can be used as so: `worldevent [event type] [sub type] [wave (if invasion event)]`
  * Valid event types are `meteor`, `fullmoon`, `bloodmoon`, `eclipse`, `invasion`, `sandstorm`, `rain`
  * Valid sub types are `goblins`, `snowmen`, `pirates`, `pumpkinmoon`, `frostmoon` for invasions, and `slime` for rain.

* A new set of permissions has been added under the node `tshock.world.events`:
  * `tshock.world.events.bloodmoon`: Enables access to the `worldevent bloodmoon` command
  * `tshock.world.events.fullmoon`: Enables access to the `worldevent fullmoon` command
  * `tshock.world.events.invasion`: Enables access to the `worldevent invasion` command
  * `tshock.world.events.eclipse`: Enables access to the `worldevent eclipse` command
  * `tshock.world.events.sandstorm`: Enables access to the `worldevent sandstorm` command
  * `tshock.world.events.rain`: Enables access to the `worldevent rain` command
  * `tshock.world.events.meteor`: Enables access to the `worldevent meteor` command

Please note that the permissions previously tied to the removed commands are also still used to confirm access to the new commands, so if you have existing configurations no one should have any new or lost access.

## TShock 4.4.0 (Pre-release 2)
* Replaced `/expert` with `/worldmode` command. (@QuiCM)
* Fixed NPC buff anticheat issue conflicting with Terraria gameplay changes (whips). (@Patrikkk)

## TShock 4.4.0 (Pre-release 1)
* Added confused debuff to Bouncer for confusion applied from Brain of Confusion
* API: Added return in OnNameCollision if hook has been handled. (@Patrikkk)
* API: Added hooks for item, projectile and tile bans (@deadsurgeon42)
* API: Changed `PlayerHooks` permission hook mechanisms to allow negation from hooks (@deadsurgeon42)
* API: New WorldGrassSpread hook which shold allow corruption/crimson/hallow creep config options to work (@DeathCradle)
* Fixed a missing case in UserManager exception handling, which caused a rather cryptic console error instead of the intended error message (@deadsurgeon42)
* Fixed saving when one player is one the server and another one joins (@MarioE)
* Fixed /spawnmob not spawning negative IDs (@MarioE)
* Validated tile placement on PlaceObject; clients can no longer place frames, paintings etc with dirt blocks (@bartico6, @ProfessorXZ)
* Updated to new stat tracking system with more data so we can actually make informed software decisions (Jordan Coulam)
* Fixed /time display at the end of Terraria hours (@koneko-nyan)
* Added a warning notifying users of the minimum memory required to run TShock (@bartico6)
* Added /group rename to allow changing group names (@ColinBohn, @ProfessorXZ)
* Added /region rename and OnRegionRenamed hook (@koneko-nyan, @deadsurgeon42)
* Rebuilt /ban add. New syntax is /ban add <target> [time] [reason] where target is the target online player, offline player, or IP; where time is the time format or 0 for permanent; and where [reason] is the reason. (@hakusaro)
* Removed /ban addip and /ban addtemp. Now covered under /ban add. (@hakusaro)
* Added /su, which temporarily elevates players with the tshock.su permission to super admin. In addition added, a new group, owner, that is suggested for new users to setup TShock with as opposed to superadmin. Finally, /su is implemented such that a 10 minute timeout will occur preventing people from just camping with it on. (@hakusaro)
* Added /sudo, which runs a command as the superadmin group. If a user fails to execute a command but can sudo, they'll be told that they can override the permission check with sudo. Much better than just telling them to run /su and then re-run the command. (@hakusaro)
* Fixed /savessc not bothering to save ssc data for people who bypass ssc. (@hakusaro)
* Default permission sets for new databases are more modern. (@hakusaro)
* Added the ability to ban by account name instead of just banning a character name assuming its an account name. (@hakusaro)
* Fixed a bug in the CommandLineParser which caused some command lines to fail (@QuicM)
* Renamed TShock.DB.User to TShock.DB.UserAccount, including all the related methods, classes and events. (@Ryozuki)
* Update OTAPI to 2.0.0.31, which also updates Newtonsoft.Json to 10.0.3 (@Ryozuki)
* Fixed DumpItems() from trying to dump older versions of certain items (negative item IDs). (@Zaicon)
* Added the `/dump-reference-data` command, which when run, runs Utils.Dump() and outputs Terraria reference data to the server folder. (@hakusaro)
* Added DateTime datatype support for both MySQL and SQLite. (@Ryozuki)
* Fixed builds to not require a specific version of OTAPI and to not fail when in Release mode (@bartico6)
* Update Assembly Company to Pryaxis (@Ryozuki)
* Removed `/restart` command. (@hakusaro)
* Removed `Permissions.updateplugins` permission. (@hakusaro)
* Removed REST `/v3/server/restart/` route and `/server/restart/` route. (@hakusaro)
* The "auth system" is now referred to as the initial setup system (what it actually is). This is better verbiage for basically all situations. Who really wants to turn off the "authentication system?" In addition, the system now makes it more clear what the point of it is, rather than that it grants permissions. (@hakusaro)
* `GetDataHandlers.SendTileSquare` hook now sends a `TSPlayer` and a `MemoryStream` of raw data. (@hakusaro)
* Added `GetDataHandlers.HealOtherPlayer` hook. (@hakusaro)
* Added `GetDataHandlers.PlaceObject` hook. (@hakusaro)
* `GetDataHandlers.KillMe` now sends a `TSPlayer` and a `PlayerDeathReason`. (@hakusaro)
* Added `GetDataHandlers.ProjectileKill` hook. (@hakusaro)
* Removed `TShock.CheckProjectilePermission`. (@hakusaro)
* Added `TSPlayer` object to `GetDataHandlers.LiquidSetEventArgs`. (@hakusaro)
* Removed `TShock.StartInvasion` for public use (moved to Utils and marked internal). (@hakusaro)
* Fixed invasions started by TShock not reporting size correctly and probably not working at all. (@hakusaro)
* Removed `GetDataHandlers.TileKill` and replaced it with `GetDataHandlers.PlaceChest` as the packet originally designated as tile kill is now only used for chests. (@hakusaro)
* Added `TSPlayer` to `GetDataHandlers.NPCHome`. (@hakusaro)
* Added `TSPlayer` to `GetDataHandlers.ChestItemChanged`. (@hakusaro)
* Fixed chest item changes not triggering any range checks, tile checks, or correct chest checks. (@hakusaro)
* Added `TSPlayer` to `GetDataHandlers.PlayerBuff`. (@hakusaro)
* Added `TSPlayer` and `PlayerDeathReason` to `GetDataHandlers.PlayerDamage`. (@hakusaro)
* Added `TSPlayer` to `GetDataHandlers.NPCStrike`. (@hakusaro)
* Added `TSPlayer` to `GetDataHandlers.PlayerAnimation`. (@hakusaro)
* Added `GetDataHandlers.MassWireOperation` hook and related arguments. (@hakusaro)
* Added `GetDataHandlers.PlaceTileEntity` hook and related arguments. (@hakusaro)
* Added `TSPlayer` to `GetDataHandlers.GemLockToggle`. (@hakusaro)
* Added `GetDataHandlers.PlaceItemFrame` hook and related arguments. (@hakusaro)
* Added `TSPlayer.IsBouncerThrottled()`. (@hakusaro)
* Added `TSPlayer.IsBeingDisabled()` and removed `TShock.CheckIgnores(TSPlayer)`. (@hakusaro)
* Added `TSPlayer.CheckIgnores()` and removed `TShock.CheckIgnores(TSPlayer)`. (@hakusaro)
* Hooks inside TShock can now be registered with their `Register` method and can be prioritized according to the TShock HandlerList system. (@hakusaro)
* Fix message requiring login not using the command specifier set in the config file. (@hakusaro)
* Move `TShock.CheckRangePermission()` to `TSPlayer.IsInRange` which **returns the opposite** of what the previous method did (see updated docs). (@hakusaro)
* Move `TShock.CheckSpawn` to `Utils.IsInSpawn`. (@hakusaro)
* Replace `TShock.CheckTilePermission` with `TSPlayer.HasBuildPermission`, `TSPlayer.HasPaintPermission`, and `TSPlayer.HasModifiedIceSuccessfully` respectively. (@hakusaro)
* Fix stack hack detection being inconsistent between two different check points. Moved `TShock.HackedInventory` to `TSPlayer.HasHackedItemStacks`. Added `GetDataHandlers.GetDataHandledEventArgs` which is where most hooks will inherit from in the future. (@hakusaro)
* All `GetDataHandlers` hooks now inherit from `GetDataHandledEventArgs` which includes a `TSPlayer` and a `MemoryStream` of raw data. (@hakusaro)
* Removed _all obsolete methods in TShock marked obsolete prior to this version (all of them)_ (@hakusaro).
* Removed broken noclip detection and attempted prevention. TShock wasn't doing a good job at stopping noclip. It's always worse to claim that you do something that you can't/don't do, so removing this is better than keeping broken detection in. (@hakusaro)
* Replaced `Utils.FindPlayer` with `TSPlayer.FindByNameOrID` to more appropriately be object orientated. (@hakusaro)
* Moved `Utils.Kick()` to `TSPlayer` since its first argument was a `TSPlayer` object. (@hakusaro)
* Removed `Utils.ForceKick()`. (@hakusaro)
* Removed `Utils.GetPlayerIP()`. (@hakusaro)
* Moved `Utils.Ban()` to `TSPlayer.Ban()`. (@hakusaro)
* Moved `Utils.SendMultipleMatchError()` to `TSPlayer.SendMultipleMatchError`. (@hakusaro)
* Removed `Utils.GetPlayers()`. Iterate over the TSPlayers on the server and make your own list.
* Removed `Utils.HasBanExpired()` and replaced with `Bans.RemoveBanIfExpired()`. (@hakusaro)
* Removed `Utils.SendFileToUser()` and replaced with `TSPlayer.SendFileTextAsMessage()`. (@hakusaro)
* Removed `Utils.GetGroup()` also have you seen `Groups.GetGroupByName()`? (@hakusaro)
* `Utils.MaxChests()` is now `Utils.HasWorldReachedMaxChests()`. (@hakusaro)
* `Utils.GetIPv4Address()` is now `Utils.GetIPv4AddressFromHostname()`. (@hakusaro)
* Fixed the disappearing problem when placing tile entities. (@mistzzt)
* Removed the stat tracking system. (@hakusaro)
* Fixed erroneous kicks and bans when using `KickOnMediumcoreDeath` and `BanOnMediumcoreDeath` options. (@DankRank)
* Removed `TSPlayer.InitSpawn` field. (@DankRank)
* `OnPlayerSpawn`'s player ID field is now `PlayerId`. (@DankRank)
* Fixed null reference console spam in non-SSC mode (@QuiCM)
* `Utils.TryParseTime` can now take spaces (e.g., `3d 5h 2m 3s`) (@QuiCM)
* Enabled banning unregistered users (@QuiCM)
* Added filtering and validation on packet 96 (Teleport player through portal) (@QuiCM)
* Update tracker now uses TLS (@pandabear41)
* When deleting an user account, any player logged in to that account is now logged out properly (@Enerdy)
* Add NPCAddBuff data handler and bouncer (@AxeelAnder)
* Improved config file documentation (@Enerdy)
* Add PlayerZone data handler and bouncer (@AxeelAnder)
* Update sqlite binaries to 32bit 3.27.2 for Windows (@hakusaro)
* Fix banned armour checks not clearing properly (thanks @tysonstrange)
* Added warning message on invalid group comand (@hakusaro, thanks to IcyPhoenix, nuLLzy & Cy on Discord)
* Moved item bans subsystem to isolated file/contained mini-plugin & reorganized codebase accordingly. (@hakusaro)
* Moved bouncer checks for item bans in OnTileEdit to item bans subsystem. (@hakusaro)
* Compatibility with Terraria 1.4.0.2 (@AxeelAnder, @Patrikkk)
  * Multiple fields got slightly renamed.
  * Modifying ToggleExpert command. Main.expertMode is no longer settable. Using a Main.GameMode int property comparsion.
  * GameCulture no longer has static fields to get local language. Using methods to return/compare language.
  * Added permission "tshock.npc.spawnpets" which restricts pet spawns. This can cause high network load, so it's restricted. (@hakusaro)
  * Updated OnTeleport to support new args per protocol changes. (@hakusaro)
  * Disabled anticheat checks for projectile updates due to issues with game changes. (@hakusaro)
  * This update has been brought to you by: Patrikkk, Icy, Chris, Death, Axeel, Zaicon, hakusaro, and Yoraiz0r! <3

## TShock 4.3.26
* Removed the stat tracking system. (@hakusaro)
* Updated SQLite binaries. (@hakusaro)
* Removed server-sided healing when disabled. (@QuiCM)
* Patched an exploit that allowed users to kill town NPCs (@QuiCM)
* [API] Added a patch for the 0-length crash (@QuiCM)

## TShock 4.3.25
* Fixed a critical exploit in the Terraria protocol that could cause massive unpreventable world corruption as well as a number of other problems. Thanks to @bartico6 for reporting. Fixed by the efforts of @QuiCM, @hakusaro, and tips in the right directioon from @bartico6.

## TShock 4.3.24
* Updated OpenTerraria API to 1.3.5.3 (@DeathCradle)
* Updated Terraria Server API to 1.3.5.3 (@WhiteXZ, @hakusaro)
* Updated TShock core components to 1.3.5.3 (@hakusaro)
* Terraria Server API version tick: 2.1
* Added OnNpcKilled hook to Server API: 2.2 (@tylerjwatson)
* Added CreateCombatTextExtended to PacketTypes. This packet allows for the same functionality that packet 82 (CreateCombatText) used to have. (@WhiteXZ)
* Updated ServerBroadcast hook to provide a NetworkText object. (@tylerjwatson)
* Fixed levers and things not updating properly. (@deathcradle)
* Deprecated PacketTypes.ChatText. Chat is now handled using the NetTextModule and packet 82. (@WhiteXZ, @Hakusaro)
* Removed the -lang command-line flag from TShock. It is now a vanilla feature. (@Hakusaro)

## TShock 4.3.23
* Added evil type option during world creation (@mistzzt)
* Bans can be sorted. TShock's default sorting will retrieve bans sorted from newest to oldest based on the date the ban was added (@WhiteXZ)
* Resolved issues with mob and item spawning. Thanks to @OnsenManju for your investigative work :) (@WhiteXZ)
* Patched a crashing exploit (@Simon311)

## TShock 4.3.22
* Compatibility with Terraria 1.3.4.4
* API: Version tick 2.0
* API: Reduced RAM usage by ~80MB (Large server) (@deathcradle)
* API: Added TSPlayer.KillPlayer() (@WhiteXZ)
* API: Added TSPlayer.Logout() (@ProfessorXZ)
* Fixed connections after max slot is reached (@DeathCradle)
* Fixed server crashes caused by client disconnections when attempting to read closed sockets (@Enerdy)
* Added some code to make trapdoors work better (@DogooFalchion)
* AllowCutTilesAndBreakables config option now correctly allows flowers/vines/herbs to be cut in regions without breaking walls (@WhiteXZ)
* REST: `/v3/players/read` now includes a `muted` field (@WhiteXZ)
* REST: Token creation is now more secure (Thanks to @Plazmaz for reporting the issue!)
* REST: Deprecated the RestRequestEvent. If you use this event, please let us know.
* REST: ALL endpoints now have a base route (eg you can use `/server/motd` instead of `/v3/server/motd`). These base routes will never change, but will provide an `upgrade` field describing any newer routes
* REST: Added `/v3/world/autosave` and `/v3/world/bloodmoon` which use GET parameter style arguments. I.e., `/v3/world/autosave?state=[true|false]` & `/v3/world/bloodmoon?state=[true|false]`. The state argument is optional
* Fixed fishing quests not saving/loading correctly when login before join, UUID login, and SSC were enabled together (@DogooFalchion)

## TShock 4.3.21
* Compatibility with Terraria 1.3.4.3 (@Patrikkk, @Zaicon).
* API: Version tick 1.26.
* API: Deprecated PlayerDamage and PlayerKillMe packets (now uses PlayerHurtV2 and PlayerDeathV2).
* API: Main.rand now uses UnifiedRandom instead of Random. This WILL break any existing plugin that uses Main.rand.
* Fixed HealOtherPlayer packet exploit (@Simon311).
* Added associated config option for HealOtherPlayer exploit prevention (@Simon311).
* Added `/accountinfo` command to get account information for a given TShock account (@Simon311).
* Removed TShock color parsing from MOTDs (@WhiteXZ).
* Fixed butterfly statues spawning catchable butterflies (@DogooFalchion).
* Implemented some missing balance changes lost in prior version patches (@DogooFalchion).
* Added alias for server shutdown command: stop (@nicatronTg).
* Removed the old REST model. This includes the following endpoints:
 * `/status`
 * `/v2/players/read`
 * `/v2/server/rawcmd` (@WhiteXZ).
* Fixed `/user group` always giving an unhelpful error messaging telling you to check the console, even if we knew exactly why it failed (@nicatronTg).
* Removed _all obsolete methods in TShock marked obsolete prior to this version (all of them)_ (@nicatronTg).
* Fixed issue where registration + login would fail because KnownIps had 0 items and .Last() doesn't work on collections with 0 items (@DogooFalchion, @nicatronTg, @Simon311).
* Added `/uploadssc [player]` which allows someone to upload SSC data for [player] and store it on the server. Adds `tshock.ssc.upload` and `tshock.ssc.upload.others` permission nodes to match (@DogooFalchion).
* Added hardened stone to the whitelist of tiles editable by players (@DogooFalchion).
* Added conversion system to send convert old MOTD format into smart text, while preserving initial line starting values to keep byte optimization for background colors Thanks to (@WhiteXZ, @Simon311, and especially @DogooFalchion) for the hard work on this issue.

## TShock 4.3.20
* Security improvement: The auth system is now automatically disabled if a superadmin exists in the database (@Enerdy).
* Removed the `auth-verify` command since `auth` now serves its purpose when necessary (@Enerdy).
* Security: `/"` exploit can no longer break chat mute filters (@Simon311).
* Fixed an issue where sometimes players could connect briefly during server shutdown, leading to errors (@Simon311).
* Fixed wyverns despawning & not behaving like normal (@WhiteXZ).
* Fixed major security issue where InvokeClientConnect could be exploited to do terrible, terrible things (@Simon311, @nicatronTg, @popstarfreas, @ProfessorXZ, @WhiteXZ).

## TShock 4.3.19
* Compatibility with Terraria 1.3.3.3 (@Simon311)
* API: Version tick 1.25
* API: Resolved some issues with the ItemForceIntoChest hook (@WhiteXZ, @Patrikkk)
* API: Resolved some shonky code that caused Vitamins and other Ankh Shield related items to drop at strange rates or not at all (@ProfessorXZ, @WhiteXZ, @nicatronTg)
* Fixed magical ice blocks not working correctly (@ProfessorXZ)

## TShock 4.3.18

* Compatibility with Terraria 1.3.3.2
* API: Version tick 1.24
* API: Fixed chat line breaks when using chat tags and long strings of text (@ProfessorXZ)
* API: Added ItemForceIntoChest hook (@WhiteXZ)
* API: Included the player's registration date in REST's players/read endpoints (@ProfessorXZ)
* The setdungeon command correctly uses tshock.world.setdungeon as its permission (@OnsenManju)
* Fixed clients being able to "Catch" and remove NPCs (@ProfessorXZ)
* Fixed clients being able to remove other players' portals (@ProfessorXZ)
* Fixed possible client crashes caused by invalid item netIDs (@ProfessorXZ)
* Fixed players being able to bypass permission checks when placing Tile Entities (@ProfessorXZ)
* Fixed players being able to bypass permission checks when placing items in Item Frames (@ProfessorXZ)
* Fixed a bug involving Item Frames which allowed players to duplicate items (@ProfessorXZ)
* Fixed an issue allowing clients to teleport NPCs to arbitrary locations (@ProfessorXZ)
* Fixed a bug where players would get teleported to their previous location after dismounting the Unicorn Mount (@ProfessorXZ)
* Players can no longer quick stack items into region protected chests (@ProfessorXZ)
* Rope placement is no longer blocked by range checks (@ProfessorXZ)
* The Drill Containment Unit breaks blocks properly now (@ProfessorXZ)
* Fixed item duplications caused by range checks and invalid netIDs (@ProfessorXZ)
* Fixed Expert mode coin duplication (@ProfessorXZ)
* Players are no longer able to place liquids using LoadNetModule packet (@ProfessorXZ)
* Explosives are no longer blocked by range checks (@ProfessorXZ)
* Players can no longer bypass tile checks by using the Tile packet (@ProfessorXZ)
* Fixed a bug where players couldn't hammer a Junction Box without "allowclientsideworldedit" permission (@Patrikkk)
* Fixed the client's UI not being draw when setting wind speed to abnormal values (@ProfessorXZ)
* Added a command to start and stop sandstorms (@WhiteXZ)

## TShock 4.3.17

* Compatibility with Terraria 1.3.2.1
* Updated superadmin behaviour to conform to expected behaviour (@WhiteXZ, @Patrikk)
* Fixed a crash involving teleporters and dressers (@WhiteXZ)
* Fixed pressure plates (@Enerdy, @Patrikk)
* Fixed a deadlock in wiring (@Wolfje)
* Fixed a crash in wiring (@Patrikk)
* Improved network syncing on client joins (@Patrikk)
* The Presserator can now place actuators (@ProfessorXZ)
* Resolved a region error when removing unlisted users from regions (@WhiteXZ)
* Added a `SetDungeon` command to set the dungeon position (@webmilio)
* The currently running world name is now part of the server application's title (@webmilio)
* Gem locks can now be region protected (@mistzzt)
* Players can now place sensors (@mistzzt)
* Repackaged GeoIP with TShock so that GeoIP works (@Enerdy)
* Added permissions to use sundials and start/stop parties (@Patrikk)
* Added an announcement box hook (@mistzzt)
* Added the ability to choose what type of world (crimson/corruption) you generate (@NoNiMad)

## TShock 4.3.16

* Terraria 1.3.1 wiring bugfixes
* Terraria 1.3.1.1 compatibility

## TShock 4.3.15

* This release is actually 4.3.14, but was ticked extra due to a version issue on gen-dev prior to master push.
* Update to 1.3.1

## TShock 4.3.13

* Fixed an issue preventing TShock from starting on certain mono versions (@Wolfje)
* Fixed a deadlock in Wiring (@Wolfje)
* Fixed character styles/gender not being saved properly on first login while SSC is on (@WhiteXZ)
* Added a PlayerPermission hook fired whenever a permission check involving said player occurs (when the new TSPlayer.HasPermission method is called) (@Enerdy)
* Resolved an issue where martian invasions and eclipses would have empty messages if AnonymousBossInvasions was set to true (@WhiteXZ)
* Added an optional `slime` parameter to the `rain` command, allowing slime rain to be started and stopped. New syntax is `rain [slime] <start/stop>` (@WhiteXZ)
* Fixed performance issues due to concurrent dictionary access in TSPlayer (@CoderCow)
* Added an ID property to Regions (@WhiteXZ)
* Fixed an issue where region sizes were calculated incorrectly (@WhiteXZ)
* Fixed a bug in RegionManager preventing regions adding correctly (@pink_panther)
* Fixed another bug in RegionManager preventing regions adding correctly (@WhiteXZ)
* Fixed a routing issue with the `/v2/token/create` REST endpoint
* Removed the `/token/create` REST endpoint. `/v2/token/create` should be used instead.

## TShock 4.3.12

* Fixed issues with TSPlayer.SetTeam not working (@WhiteXZ)
* Fixed /butcher not killing bosses in expert difficulty (@WhiteXZ)
* API: Deprecated PacketBufferer (now obviated by SendQ) (@WhiteXZ)
* API: Building on Windows no longer breaks traps (@Wolfje)
* Fixed bombs, dynamite, and sticky bombs (@Wolfje)
* Removed spammy messages from OnSecondUpdate that confused some server owners (@Wolfje)
* Rewrote some stat tracker code to send actually relevant data to the stats server (@Cleant / George from Multiplay UK)
* Added an opt-out command line switch to disable the stat tracker (--stats-optout) (@Cleant / George from Multiplay UK)
* Added a unique provider token which can be passed to the stat tracker (--provider-token [token]) for tracking servers from the same GSP. (@Cleant / George from Multiplay UK)

## TShock 4.3.11

* This release is actually 4.3.10, but was ticked extra due to a version issue on gen-dev prior to master push.

## TShock 4.3.10

This version features a drop-in tile replacement system by @Wolfje that reduces RAM requirements
by up to 70% on all worlds and CPU requirements up to 10% in the running process.

* Large worlds: from 700MB-1GB -> ~325MB
* Medium worlds: from 500MB -> ~200MB
* Small worlds: from 400MB -> ~125MB

Other notable changes include:

* API: **Drop-in tile storage replacement system** (@Wolfje)
* API: Fixed some possible packet leaks in sendq (@Wolfje)
* API: APIVersion 1.22
* API: Added crash protection around malicious and/or invalid packets (@Wolfje)
* API: Fixed worlds not loading sometimes (@tysonstrange)
* API: Fixed living leaf walls not working as housing
* Fixed an issue preventing some players from joining when the world is saving (@Wolfje)
* Fixed an issue adding a ban on a player who has previously been banned (@Wolfje)
* Fixed /invade martian (@Wolfje)
* Fixed target dummies not working properly (@WhiteXZ)
* Added a config option (DisableSecondUpdateLogs) to prevent log spam from OnSecondUpdate() (@WhiteXZ)
* Added RESTful API login rate limiting (@George)
* Added config options (MaximumRequestsPerInterval, RequestBucketDecreaseIntervalMinutes, LimitOnlyFailedLoginRequests) for rate limiting (@George)
* **DEPRECATION**: Deprecated Disable(string, bool) and added Disable(string, DisableFlags). Please update your plugins accordingly (@WhiteXZ)
* Fixed Halloween and Christmas events not working properly (@TomyLobo)
* Fixed the demon heart's extra accessory slot not working correctly in SSC (@WhiteXZ)
* Fixed gender-changing potions not working correctly in SSC (@hastinbe)
* Fixed IP bans not working correctly (@hastinbe)
* Fixed /reload not using the correct permission (@WhiteXZ)
* Fixed TSPlayer.ActiveChest not being tracked correctly resulting in item dupes while disabled (@WhiteXZ)
* /reload now reloads tile and projectile bans

## TShock 4.3.8
* API: Update to Terraria 1.3.0.8 (@Patrikkk)
* **API: Added a crash reporter which collects memory dumps on Windows** (@Wolfje)
* API: New commandline param: `-crashdir` - Writes crash reports to the specified directory (@Wolfje)
* API: Sendq now doesn't disconnect people when it cant send a packet (@Wolfje)
* API: Fixed more crashes on disconnect in sendq (@Wolfje)
* API: Now ignores unknown server packets (@Wolfje)
* API: Potentially removed arithmetic overflows in server (@Wolfje)

### Using the Crash Reporter

TShock now has a crash reporter built in which writes crash logs to the `crashes` directory
in the event of a catastrophic failure.  **To change where TShock writes its crash logs,
specify the `-crashdir` parameter on the command line**.

1. In the event of a crash, look for a file called `crash_xxxx.zip` in the `crashes` directory
2. Upload the file somewhere, beware the crash file may be quite large (>100MB), anywhere like google drive, dropbox or mega will be fine
3. Post a link to the crash with reproduction steps in the TShock support forum

Alternatively, if you do not want to report the crash, just delete the file.

## TShock 4.3.7

* Auth system kicks players if system is disabled. (@nicatronTg)
* Fixed /login permitting multiple logins without a logout in between. (@nicatronTg)
* Allow[Hallow/Corruption/Crimson]Creep in config now work. (@WhiteXZ)
* API: Treasure bags are now named properly. (@WhiteXZ)
* API: Clients no longer close on disconnect. (@Wolfje)
* API: Add server broadcast hook. (@Patrikk)
* API: Fixed pressure plate hook triggering multiple times. (@Patrikk)
* API: Fixed issues with SendQ writes failing. (@Wolfje)
* API: Version tick to 1.21

## TShock 4.3.6

* API: NPCs shoot the right way (@WhiteXZ)
* API: The server config file works correctly with priority and port (@Patrikkk)
* API: Removed support for favorites and removed JSON dependencies. (@Enerdy)
* API: Removed support for clouds. (@Enerdy)
* API: Fixed a whole lot of bugs with wiring, and in general re-wrote some core bits that were bugged. (@WhiteXZ)
* API: Fixed projectile AI bugs. (@AndrioCelos)
* API: Fixed world saving problems. (WhiteXZ)
* API: Fixed server not accepting more connections once max slots was filled. (@WhiteXZ)
* API: Removed startup parameters and moved them to TShock. (@Cleant)
* API: Item.SetDefaults() no longer kills some tools. (@Enerdy)
* API: Restored chat bubbles. (@WhiteXZ)
* API: Updated to 1.3.0.6. (@Enerdy & @Patrikkk)
* API: Lots and I mean lots of network improvements in the SendQ department. (@tylerjwatson)
* API: Added NpcLootDrop and DropBossBag hooks. (@Patrikkk)
* API: Fixed hook: NpcTriggerPressurePlate (@Patrikkk)
* API: Fixed hook: ProjectileTriggerPressurePlate (@Patrikkk)
* API: Fixed hook: ItemSetDefaultsString (@Patrikkk)
* API: Fixed hook: ItemSetDefaultsInt (@Patrikkk)
* API: Fixed hook: ItemNetDefaults (@Patrikkk)
* API: Fixed hook: GameStatueSpawn (@Patrikkk)
* API: Fixed hook: NpcNetDefaults (@Patrikkk)
* API: Fixed hook: NpcNetSetDefaultsString (@Patrikkk)
* API: Fixed hook: NpcNetSetDefaultsInt (@Patrikkk)
* API: Fixed hook: NpcSpawn (@Patrikkk)
* API: Fixed hook: NpcTransformation (@Patrikkk)
* API: Fixed hook: NpcStrike (@Patrikkk)
* API: Updated AssemblyInfo to 1.3.0.6. (@nicatronTg)
* API: Moved to .NET Framework 4.5. (@tylerjwatson)
* API: Dedicated server input thread doesn't run if input is redirected/piped. (@tylerjwatson)
* API: Wiring.cs methods are now public. (@Stealownz)
* API: Added PlayerTriggerPressurePlate hook. (@Patrikkk)
* API: API Version Tick to 1.20.
* The config option disabling the DCU has been deprecated and will be removed in a future release. (@nicatronTg)
* Fixed bubble tile triggering noclip checks. (@Enerdy)
* Updated projectile handling in GetDataHandlers. (@WhiteXZ)
* Fixed issue #992. (@WhiteXZ)
* Teleport handler now handles wormholes. (@WhiteXZ)
* Fixed tall gates and trap doors (issue #998). (@WhiteXZ)
* Added monoliths to orientable tiles (issue #999). (@WhiteXZ)
* Fixed vortex stealth armor (issue #964). (@WhiteXZ)
* Added moon lord to spawn boss. (@WhiteXZ)
* Fixed serverpassword syntax error error message. (@JordyMoos)
* Fixed issue #1019. (@WhiteXZ)
* Fix: Region protection prevents placement of objects. (@Patrikkk)
* Moved all startup parameters to TShock. (@Cleant)
* Fix: Target dummies are no longer butchered. (@Denway)
* Added projectile 465 to the ignore list, which fixes some other issues. (@Enerdy)
* Fix: Logging out is now safe with SSC (/logout) (issue #1037). (@WhiteXZ)
* API/TShock: Removed -world parameter from TShock, put it back in the API. (@tylerjwatson)

## TShock 4.3.5

* Fix HandleSpawnBoss, and as a result the spawnboss command and boss spawning items. (@Ijwu)
* Rewrite SendQ for more network stack improvements (@tylerjwatson)
* Update to Terraria 1.3.0.5 (@Patrikkk)

## TShock 4.3.4

* Fix invasion progress messages (@WhiteXZ)
* Completely rewrite SendQ to have less deadlocks (@tylerjwatson)

## TShock 4.3.3

* Fix dihydrogen monoxide (@tylerjwatson)
* Whitelist another boss projectile (@Patrikkk, @WhiteXZ)

## TShock 4.3.2

* Fixed the issue where using the Super Absorbent Sponge would disable users (@WhiteXZ)
* Fixed an issue in NetGetData where e.Length - 1 would be -1 (@WhiteXZ)
* Fixed /who -i and /userinfo (@Enerdy)
* API: OnRegionEntered hook now returns the region entered (@Patrikkk)
* Support for Terraria 1.3.0.4 (@nicatronTg)
* Fixed dressers being unbreakable. (@nicatronTg)
* Fixed wall placement mechanics (@nicatronTg, @Ijwu, @WhiteXZ)
* Fixed Moon Lord projectiles disabling players (@k0rd, @nicatronTg)
* Fixed several potential crashes in server (@Patrikkk)
* Fixed -autocreate command line argument (@WhiteXZ, @nicatronTg)
* Added more world data to world load menu (@WhiteXZ)
* Moved server password to TShock config (@Enerdy)
* Fixed world delete in server (@benjiro)
* Fixed disappearing NPCs (@WhiteXZ)
* Added much more performant code, SendQ, to server module. Reduces downstream network overhead by at least 40% (@tylerjwatson)
* API: Updated TSPlayer.Disable to use new buffs (@Enerdy)
* Updated default max damage & projectile damage to 1,175 (based on 625 people)
* Fixed support for SSC (@WhiteXZ)

## TShock 4.3.1

* Fixed a bug where /user group failing would output no error. (@nicatronTg)
* Fixed a bug where /user group would fail. @(Enerdy)
* Added the ability to disable backup autosave messages. (@nicatronTg)
* Fixed /buff malfunctioning when entering an invalid buff name. (@Enerdy)
* Fixed projectiles 435-438 (martian invasion) freezing everyone under certain conditions. (@Enerdy)
* DisableTombstones now works properly with the new golden gravestones. (@Enerdy)
* REST module now properly catches exceptions during Start(). (@Patrikkk)
* Added /expert command to toggle expert mode. (@WhiteXZ)
* Fixed pirate invasions. (@patrik)
* Fixed worldinfo packet. (@WhiteXZ)
* Fixed server passwords. (@Enerdy)

## TShock 4.3.0.0

* API: Modifed NetItem so that it's actually useful. (@MarioE)
* Updated prebuilts (SQLite, JSON, MySQL) to latest versions. (@nicatronTg)
* Added a minimum password length to prevent blank passwords. (@nicatronTg)
* Modified item ban checks to provide which item is disabling a player in the logs. (@Enerdy)
* API: Modified TSPlayer to store a user, and deprecated calls to TSPlayer.User.ID. (@WhiteXZ)
* Modified chat color specs in config file to be int arrays rather than floats. (@nicatronTg)
* Modified verbiage for ```/auth``` and ```/auth-verify``` to make it clearer how they operate. (@nicatronTg)
* API: Added fuzzy name searching for users. (@WhiteXZ)
* API: Fixed ```OnPlayerLogout``` not being fired when a player disconnects. (@nicatronTg)
* API: Deprecated ```ValidString``` and ```SanitizeString``` methods in Utils. (@nicatronTg)
* Added BCrypt password hashing and related systems for it. BCrypt replaces the old system using non-password hashing algorithms for storing passwords. It breaks implementations of the login code that were manually recreated, but is otherwise seamless in transition. (@nicatronTg)
* API: Added ```User.VerifyPassword(string password)``` which verifies if the user's password matches their stored hash. It automatically upgrades a users' password to BCrypt if called and the password stored is not a BCrypt hash. (@nicatronTg)
* API: Deprecated ```Utils.HashPassword``` and related password hashing functions as those are no longer needed for plugin access. (@nicatronTg)
* Fixed ```UseServerName``` config option so that it correctly sends the config server name any time that Main.WorldName is used. (@Olink)
* Fixed a bug where people could ban themselves. (@nicatronTg)
* Fixed a bug where banning a player who never logged in caused problems. (@nicatronTg)
* Terraria 1.3.0.3 support.
