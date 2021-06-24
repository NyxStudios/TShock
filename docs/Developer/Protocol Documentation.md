## Connect Request [1]
###  Client -> Server
| Size | Description | Type | Notes |
|------|----------|------|-------|
|?|Version|String|"Terraria" + Main.curRelease|

## Disconnect [2]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|?|Reason|[NetworkText](#networktext)|-|

## Set User Slot [3]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|

## Player Info [4]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|1|Skin Varient|Byte|-|
|1|Hair|Byte|If >162 then Set To 0|
|?|Name|String|-|
|1|Hair Dye|Byte|-|
|1|Hide Visuals|Byte|-|
|1|Hide Visuals 2|Byte|-|
|1|Hide Misc|Byte|-|
|3|Hair Color|[Color](#color-structure)|-|
|3|Skin Color|[Color](#color-structure)|-|
|3|Eye Color|[Color](#color-structure)|-|
|3|Shirt Color|[Color](#color-structure)|-|
|3|Under Shirt Color|[Color](#color-structure)|-|
|3|Pants Color|[Color](#color-structure)|-|
|3|Shoe Color|[Color](#color-structure)|-|
|1|Difficulty Flags|Byte|BitFlags: 0 = Softcore, 1 = Mediumcore, 2 = Hardcore, 4 = ExtraAccessory, 8 = Creative|
|1|Torch Flags|Byte|BitFlags: 1 = UsingBiomeTorches, 2 = HappyFunTorchTime, 4 = unlockedBiomeTorches|

## Player Inventory Slot [5]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Slot ID|Int16|0 - 58 = Inventory, 59 - 78 = Armor, 79 - 88 = Dye, 89 - 93 MiscEquips, 94 - 98 = MiscDyes, 99 - 138 = Piggy bank, 139 - 178 = Safe, 179 = Trash, 180 - 219 = Defender's Forge, 220 - 259 = Void Vault
|2|Stack|Int16|-|
|1|Prefix|Byte|-|
|2|Item NetID|Int16|-|

## Request World Data [6]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
||||

## World Info [7]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|Time|Int32|-|
|1|Day and Moon Info|Byte|BitFlags: 1 = Day Time, 2 = Blood Moon, 4 = Eclipse|
|1|Moon Phase|Byte|-|
|2|Max Tiles X|Int16|-|
|2|Max Tiles Y|Int16|-|
|2|Spawn X|Int16|-|
|2|Spawn Y|Int16|-|
|2|WorldSurface|Int16|-|
|2|RockLayer|Int16|-|
|4|World ID|Int32|-|
|?|World Name|String|-|
|1|Game Mode|Byte|-|
|16|World Unique ID|Byte[]|-|
|8|World Generator Version|UInt64|-|
|1|Moon Type|Byte|-|
|1|Tree Background|Byte|-|
|1|Corruption Background|Byte|-|
|1|Jungle Background|Byte|-|
|1|Snow Background|Byte|-|
|1|Hallow Background|Byte|-|
|1|Crimson Background|Byte|-|
|1|Desert Background|Byte|-|
|1|Ocean Background|Byte|-|
|1|?? Background|Byte|-|
|1|?? Background|Byte|-|
|1|?? Background|Byte|-|
|1|?? Background|Byte|-|
|1|?? Background|Byte|-|
|1|Ice Back Style|Byte|-|
|1|Jungle Back Style|Byte|-|
|1|Hell Back Style|Byte|-|
|4|Wind Speed Set|Single|-|
|1|Cloud Number|Byte|-|
|4|Tree 1|Int32|-|
|4|Tree 2|Int32|-|
|4|Tree 3|Int32|-|
|1|Tree Style 1|Byte|-|
|1|Tree Style 2|Byte|-|
|1|Tree Style 3|Byte|-|
|1|Tree Style 4|Byte|-|
|4|Cave Back 1|Int32|-|
|4|Cave Back 2|Int32|-|
|4|Cave Back 3|Int32|-|
|1|Cave Back Style 1|Byte|-|
|1|Cave Back Style 2|Byte|-|
|1|Cave Back Style 3|Byte|-|
|1|Cave Back Style 4|Byte|-|
|4|Forest 1 Tree Top Style|Byte|-|
|4|Forest 2 Tree Top Style|Byte|-|
|4|Forest 3 Tree Top Style|Byte|-|
|4|Forest 4 Tree Top Style|Byte|-|
|4|Corruption Tree Top Style|Byte|-|
|4|Jungle Tree Top Style|Byte|-|
|4|Snow Tree Top Style|Byte|-|
|4|Hallow Tree Top Style|Byte|-|
|4|Crimson Tree Top Style|Byte|-|
|4|Desert Tree Top Style|Byte|-|
|4|Ocean Tree Top Style|Byte|-|
|4|Glowing Mushroom Tree Top Style|Byte|-|
|4|Underworld Tree Top Style|Byte|-|
|4|Rain|Single|-|
|1|Event Info|Byte|BitFlags: 1 = Shadow Orb Smashed, 2 = Downed Boss 1, 4 = Downed Boss 2, 8 = Downed Boss 3, 16 = Hard Mode, 32 = Downed Clown, 64 = Server Side Character, 128 = Downed Plant Boss|
|1|Event Info 2|Byte|BitFlags: 1 = Mech Boss Downed, 2 = Mech Boss Downed 2, 4 = Mech Boss Downed 3, 8 = Mech Boss Any Downed, 16 = Cloud BG, 32 = Crimson, 64 = Pumpkin Moon, 128 = Snow Moon|
|1|Event Info 3|Byte|BitFlags: 1 = Expert Mode, 2 = FastForwardTime, 4 = Slime Rain, 8 = Downed Slime King, 16 = Downed Queen Bee, 32 = Downed Fishron, 64 = Downed Martians, 128 = Downed Ancient Cultist|
|1|Event Info 4|Byte|BitFlags: 1 = Downed Moon Lord, 2 = Downed Pumking, 4 = Downed Mourning Wood, 8 = Downed Ice Queen, 16 = Downed Santank, 32 = Downed Everscream, 64 = Downed Golem, 128 = Birthday Party|
|1|Event Info 5|Byte|BitFlags: 1 = Downed Pirates, 2 = Downed Frost Legion, 4 = Downed Goblins, 8 = Sandstorm, 16 = DD2 Event, 32 = Downed DD2 Tier 1, 64 = Downed DD2 Tier 2, 128 = Downed DD2 Tier 3|
|1|Event Info 6|Byte|BitFlags: 1 = Combat Book Used, 2 = Manual Lanterns, 4 = Downed Solar Tower, 8 = Downed Vortex Tower, 16 = Downed Tower Nebula, 32 = Downed Stardust Tower, 64 = Force Halloween (day), 128 = Force XMas (day)
|1|Event Info 7|Byte|BitFlags: 1 = Bought Cat, 2 = Bought Dog, 4 = Bought Bunny, 8 = Free Cake, 16 = Drunk World, 32 = Downed Empress of Light, 64 = Downed Queen Slime, 128 = GetGoodWorld
|2|Copper Ore Tier|Int16|Tile ID 7 or 166|
|2|Iron Ore Tier|Int16|Tile ID 6 or 167|
|2|Silver Ore Tier|Int16|Tile ID 9 or 168|
|2|Gold Ore Tier|Int16|Tile ID 8 or 169|
|2|Cobalt Ore Tier|Int16|Tile ID 107 or 221|
|2|Mythril Ore Tier|Int16|Tile ID 108 or 222|
|2|Adamantite Ore Tier|Int16|Tile ID 111 or 223|
|1|Invasion Type|SByte|-|
|8|Lobby ID|UInt64|-|
|4|Sandstorm Severity|Single|-|

## Request Essential Tiles [8]
This packet is used once in the connecting phase and does the following:
1. Sends you the spawn sections
2. Optionally, if spawn coords aren't -1 - sends you the sections of the selected position (which is the player's spawnpoint)
3. Synchronises all portals and sections around them

### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|X|Int32|Player Spawn X|
|4|Y|Int32|Player Spawn Y|

## Status [9]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|StatusMax|Int32|Status only increases|
|?|Status Text|[NetworkText](#networktext)|-|
|1|Status Text Flags|Byte|1 = HideStatusTextPercent, 2 = StatusTextHasShadows, 4 = ServerWantsToRunCheckBytesInClientLoopThread|

## Send Section [10]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Compressed|Boolean|-|
|4|X Start|Int32|-|
|4|Y Start|Int32|-|
|2|Width|Int16|-|
|2|Height|Int16|-|
|?|Tiles|-||
|2|Chest Count|Int16|-|
|?|Chests|-||
|2|Sign Count|Int16|-|
|?|Signs|-||
|2|TileEntity Count|Int16|-|
|?|TileEntities|-|-|

## Section Tile Frame [11]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Start X|Int16|-|
|2|Start Y|Int16|-|
|2|End X|Int16|-|
|2|End Y|Int16|-|

## Spawn Player [12]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Spawn X|Int16|-|
|2|Spawn Y|Int16|-|
|4|Respawn Time Remaining|Int32|If > 0, then player is still dead|
|1|Player Spawn Context|Byte|Enum: 0 = ReviveFromDeath, 1 = SpawningIntoWorld, 2 = RecallFromItem|

## Update Player [13]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|1|Control|Byte|BitFlags: 1 = ControlUp, 2 = ControlDown, 4 = ControlLeft, 8 = ControlRight, 16 = ControlJump, 32 = ControlUseItem, 64 = Direction|
|1|Pulley|Byte|BitFlags: 1 = Pulley Enabled, 2 = Direction, 4 = UpdateVelocity, 8 = VortexStealthActive, 16 = GravityDirection, 32 = ShieldRaised|
|1|Misc|Byte|BitFlags: 1 = HoveringUp, 2 = VoidVaultEnabled, 4 = Sitting, 8 = DownedDD2Event, 16 = IsPettingAnimal, 32 = IsPettingSmallAnimal, 64 = UsedPotionofReturn, 128 = HoveringDown|
|1|SleepingInfo|Byte|BitFlags: 1 = IsSleeping|
|1|Selected Item|Byte|-|
|4|Position X|Single|-|
|4|Position Y|Single|-|
|4|Velocity X|Single|Not sent if UpdateVelocity is not set|
|4|Velocity Y|Single|Not sent if UpdateVelocity is not set|
|4|Original Position X|Single|Original Position for Potion of Return, only sent if UsedPotionofReturn flag is true|
|4|Original Position Y|Single|Original Position for Potion of Return, only sent if UsedPotionofReturn flag is true|
|4|Home Position X|Single|Home Position for Potion of Return, only sent if UsedPotionofReturn flag is true|
|4|Home Position Y|Single|Home Position for Potion of Return, only sent if UsedPotionofReturn flag is true|

## Player Active [14]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|1|Active|Byte|-|

## Null [15]
### Never sent
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## Player HP [16]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|HP|Int16|-|
|2|Max HP|Int16|-|

## Modify Tile [17]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Action|Byte|Values: 0 = KillTile, 1 = PlaceTile, 2 = KillWall, 3 = PlaceWall, 4 = KillTileNoItem, 5 = PlaceWire, 6 = KillWire, 7 = PoundTile, 8 = PlaceActuator, 9 = KillActuator, 10 = PlaceWire2, 11 = KillWire2, 12 = PlaceWire3, 13 = KillWire3, 14 = SlopeTile, 15 = FrameTrack, 16 = PlaceWire4, 17 = KillWire4, 18 = PokeLogicGate, 19 = Actuate, 20 = KillTile, 21 = ReplaceTile, 22 = ReplaceWall, 23 = SlopePoundTile|
|2|Tile X|Int16|-|
|2|Tile Y|Int16|-|
|2|Flags1|Int16|KillTile (Fail: Bool), PlaceTile (Type: Byte), KillWall (Fail: Bool), PlaceWall (Type: Byte), KillTileNoItem (Fail: Bool), SlopeTile (Slope: Byte), ReplaceTile (Type: Int16), ReplaceWall (Type: Int16)|
|1|Flags2|Byte|PlaceTile (Style: Byte), ReplaceTile (Style: Byte)|

## Time [18]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|DayTime|Boolean|-|
|4|TimeValue|Int32|-|
|2|SunModY|Int16|-|
|2|MoonModY|Int16|-|

## Door Toggle [19]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Action|Byte|0 = Open Door, 1 = Close Door, 2 = Open Trapdoor, 3 = Close Trapdoor, 4 = Open Tall Gate, 5 = Close Tall Gate|
|2|Tile X|Int16|-|
|2|Tile Y|Int16|-|
|1|Direction|Byte|If (Action == 0) then (if (Direction == -1) then OpenToLeft else OpenToRight) if (Action == 2) then (if (Direction == 1) then PlayerIsAboveTrapdoor) if (Action == 3) then (if (Direction == 1) then PlayerIsAboveTrapdoor)|

## Send Tile Square [20]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Size|UInt16|-|
|1|TileChangeType|Byte|Only if (Size & 0x8000) != 0|
|2|Tile X|Int16|-|
|2|Tile Y|Int16|-|
|?|Tiles|-||

## Update Item Drop [21]
*See Update Item Drop 2 [90]*

## Update Item Owner [22]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Item ID|Int16|-|
|1|Player ID|Byte|-|

## NPC Update [23]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|
|4|Position X|Single|-|
|4|Position Y|Single|-|
|4|Velocity X|Single|-|
|4|Velocity Y|Single|-|
|2|Target|UInt16|Player ID|
|1|NpcFlags1|Byte|BitFlags: 1 = Direction, 2 = DirectionY, 4 = AI[0], 8 = AI[1], 16 = AI[2], 32 = AI[3], 64 = SpriteDirection, 128 = LifeMax|
|1|NpcFlags2|Byte|BitFlags: 1 = StatsScaled, 2 = SpawnedFromStatue, 4 = StrengthMultiplier|
|?|AI[?]|Single[]|Only sent for each true AI flag in NpcFlags1|
|2|NPC NetID|Int16|-|
|1|playerCountForMultiplayerDifficultyOverride|Byte|Only sent if StatsScaled flag is true|
|4|Strength Multiplier|Single|Only sent if StrengthMultiplier flag is true|
|1|LifeBytes|Byte|The size of Life (in bytes), only sent if LifeMax flag is not true|
|?|Life|Variable|Byte, Int16, or Int32 according to LifeBytes, only sent if LifeMax flag is not true|
|1|ReleaseOwner|Byte|Only present if NPC is catchable|

## Strike NPCwith Held Item [24]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|
|1|Player ID|Byte|-|

## Null [25]
### Never sent
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## Null [26]
### Never sent
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## Projectile Update [27]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Projectile ID|Int16|-|
|4|Position X|Single|-|
|4|Position Y|Single|-|
|4|Velocity X|Single|-|
|4|Velocity Y|Single|-|
|1|Owner|Byte|Player ID|
|2|Type|Int16|-|
|1|ProjFlags|Byte|BitFlags: 1 = AI[0], 2 = AI[1], 16 = Damage, 32 = Knockback, 64 = OriginalDamage, 128 = ProjUUID|
|4|AI0|Single|Only sent if AI[0] flag is true|
|4|AI1|Single|Only sent if AI[1] flag is true|
|2|Damage|Int16|Only sent if Damage flag is true|
|4|Knockback|Single|Only sent if Knockback flag is true|
|2|Original Damage|Int16|Only sent if OriginalDamage flag is true|
|2|Proj UUID|Int16|Only sent if ProjUUID flag is true|

## NPC Strike [28]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|
|2|Damage|Int16|-1 = Kill|
|4|Knockback|Single|-|
|1|Hit Direction|Byte|-|
|1|Crit|Boolean|-|

## Destroy Projectile [29]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Projectile ID|Int16|-|
|1|Owner|Byte|Player ID|

## Toggle P V P [30]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|1|PVP Enabled|Boolean|-|

## Open Chest [31]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Tile X|Int16|-|
|2|Tile Y|Int16|-|

Packet [31] is always used to "open" a world chest (that is, an item container placed in the world). When this packet is received the server will send the chest's contents, and sync the active chest ID to the player using packet [33].

## Update Chest Item [32]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Chest ID|Int16|-|
|1|Item Slot|Byte|-|
|2|Stack|Int16|-|
|1|Prefix|Byte|-|
|2|Item Net ID|Int16|-|

## Sync Active Chest [33]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Chest ID|Int16|-|
|2|Chest X|Int16|-|
|2|Chest Y|Int16|-|
|1|Name Length|Byte|-|
|?|Chest Name|String|Only if length > 0 && <= 20|

This packet is used to tell the server that you've exited the chest view (sending ID -1), that you're looking at your piggy bank (sending ID -2), that you're looking at your safe (sending ID -3) and that you're looking at your defender's forge (sending ID -4). Those are sent at every chest interaction. Packet [33]'s main function is to synchronize the sending client's active chest to the server, and its side function is to rename the chest.
It should be noted that this packet is *not* sent when you open a regular chest. The server knows which chest you opened when you send [31], so the [33] is only sent upon exit to unblock the chest (as opposed to both open & exit for banks like piggy, safe & defender forge)

## PlaceChest [34]
### Server <-> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Action|Byte|BitFlags:0 = Place Chest, 1 = Kill Chest, 2 = Place Dresser, 3 = Kill Dresser. 4 = Place Containers2, 5 = Kill Containers2|
|2|Tile X|Int16|-|
|2|Tile Y|Int16|-|
|2|Style|Int16|FrameX(Chest type)|
|2|Chest ID to destroy|Int16|ID if client is receiving packet, else 0|

## Heal Effect [35]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Heal Amount|Int16|-|

## Player Zone [36]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|1|ZoneFlags1|Byte|1 = Dungeon, 2 = Corruption, 4 =Holy, 8 = Meteor, 16 = Jungle, 32 = Snow, 64 = Crimson, 128 = Water Candle|
|1|ZoneFlags2|Byte|1 = Peace Candle, 2 = Solar Tower, 4 = Vortex Tower, 8 = Nebula Tower, 16 = Stardust Tower, 32 = Desert, 64 = Glowshroom, 128 = Underground Desert|
|1|ZoneFlags3|Byte|1 = Sky, 2 = Overworld, 4 = Dirt Layer, 8 = Rock Layer, 16 = Underworld, 32 = Beach, 64 = Rain, 128 = Sandstorm|
|1|ZoneFlags4|Byte|1 = Old One's Army, 2 = Granite, 4 = Marble, 8 = Hive, 16 = Gem Cave, 32 = Lihzhard Temple, 64 = Graveyard|

## Request Password [37]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## Send Password [38]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|?|Password|String|-|

## Remove Item Owner [39]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Item Index|Int16|-|

## Set Active NPC [40]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|NPC Talk Target|Int16|-|

## Player Item Animation [41]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|4|Item Rotation|Single|-|
|2|Item Animation|Int16|-|

## Player Mana [42]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Mana|Int16|-|
|2|Max Mana|Int16|-|

## Mana Effect [43]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Mana Amount|Int16|-|

## Null [44]
### Never sent
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## Player Team [45]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|1|Team|Byte|-|

## Request Sign [46]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|

## Update Sign [47]
### Updates sign if sent from client otherwise displays sign to client.
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Sign ID|Int16|-|
|2|X|Int16|-|
|2|Y|Int16|-|
|?|Text|String|-|
|1|Player ID|Byte|-|
|1|SignFlags|Byte|BitFlags: 1 = TBD|

## Set Liquid [48]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|
|1|Liquid|Byte|-|
|1|Liquid Type|Byte|-|

## Complete Connection and Spawn [49]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|-|-|-|-|

## Update Player Buff [50]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2 * 22|BuffType|UInt16[22]|-|

## Special NPC Effect [51]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|1|Type|Byte|Values: 1 = Spawn Skeletron, 2 = Cause sound at player, 3 = Start Sundialing (Only works if server is receiving), 4 = BigMimcSpawnSmoke, 5 = Register Torch God in Bestiary (from client when they use the item)|

## Unlock [52]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Type|Byte|Values: 1 = Chest Unlock, 2 = Door Unlock|
|2|X|Int16|-|
|2|Y|Int16|-|

## Add NPC Buff [53]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|
|2|Buff|UInt16|-|
|2|Time|Int16|-|

## Update NPC Buff [54]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|
|2|Buff ID 1|UInt16|-|
|2|Time 1|Int16|-|
|2|Buff ID 2|UInt16|-|
|2|Time 2|Int16|-|
|2|Buff ID 3|UInt16|-|
|2|Time 3|Int16|-|
|2|Buff ID 4|UInt16|-|
|2|Time 4|Int16|-|
|2|Buff ID 5|UInt16|-|
|2|Time 5|Int16|-|

## Add Player Buff [55]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Buff|UInt16|-|
|4|Time|Int32|-|

## Update NPC Name [56]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|
|?|Name|String|Only if client is receiving packet|
|4|TownNpcVariationIndex|Int32|Only if client is receiving packet|

## Update Good Evil [57]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Good|Byte|-|
|1|Evil|Byte|-|
|1|Crimson|Byte|-|

## Play Music Item [58]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|4|Note|Single|-|

## Hit Switch [59]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|

## NPC Home Update [60]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|
|2|Home Tile X|Int16|-|
|2|Home Tile Y|Int16|-|
|1|Homeless|Byte|-|

## Spawn Boss Invasion [61]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Player ID|Int16|-|
|2|Type|Int16|Negative Values: -1 = GoblinInvasion, -2 = FrostInvasion, -3 = PirateInvasion, -4 = PumpkinMoon, -5 = SnowMoon, -6 = Eclipse, -7 = Martian Moon, -8 = Impending Doom, -10 = Blood Moon, -11 = Combat Book Used, -12 = Bought Cat, -13 = Bought Dog, -14 = Bought Bunny, Positive Values: Spawns any of these NPCs: 4, 13, 50, 126, 125, 134, 127, 128, 131, 129, 130, 222, 245, 266, 370, 657|

## Player Dodge [62]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|1|Flag|Byte|1 = Ninja Dodge 2 = Shadow Dodge|

## Paint Tile [63]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|
|1|Color|Byte|-|

## Paint Wall [64]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|
|1|Color|Byte|-|

## Player NPC Teleport [65]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Flags|Byte|BitFlags: 0 = Player Teleport (Neither 1 or 2), 1 = NPC Teleport, 2 = Player Teleport to Other Player, 4 = GetPositionFromTarget, 8 = HasExtraInfo |
|2|Target ID|Int16|-|
|4|X|Single|-|
|4|Y|Single|-|
|1|Style|Byte|-|
|4|ExtraInfo|Int32|Only sent if HasExtraInfo flag is true|

## Heal Other Player [66]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Heal Amount|Int16|-|

## Placeholder [67]
### Does not exist in the official client. Exists solely for the purpose of being used by custom clients and servers.
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|-| |-| |

## Client UUID [68]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|?|UUID|String||

## Get Chest Name [69]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Chest ID|Int16|-|
|2|Chest X|Int16|-|
|2|Chest Y|Int16|-|
|?|Name|String|-|

## Catch NPC [70]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|
|1|Player ID|Byte|-|

## Release NPC [71]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|X|Int32|-|
|4|Y|Int32|-|
|2|NPC Type|Int16|-|
|1|Style|Byte|Sent to NPC AI[2]|

## Travelling Merchant Inventory [72]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2*40|Items|Int16[40]|Each short related to an item type NetID.|

## Teleportation Potion [73]
### Server <-> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Type|Byte|0 = Teleportation Potion, 1 = Magic Conch, 2 = Demon Conch|

## Angler Quest [74]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Quest|Byte|-|
|1|Completed|Boolean|-|

## Complete Angler Quest Today [75]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## Number Of Angler Quests Completed [76]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|4|Angler Quests Completed|Int32|-|
|4|Golfer Score|Int32|-|

## Create Temporary Animation [77]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Animation Type|Int16|-|
|2|Tile Type|UInt16|-|
|2|X|Int16|-|
|2|Y|Int16|-|

## Report Invasion Progress [78]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|Progress|Int32|-|
|4|Max Progress|Int32|-|
|1|Icon|SByte|-|
|1|Wave|SByte|-|

## Place Object [79]
### Server <-> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|
|2|Type|Int16|-|
|2|Style|Int16|-|
|1|Alternate|Byte|-|
|1|Random|SByte|-|
|1|Direction|Boolean|-|

## Sync Player Chest Index [80]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player|Byte|-|
|2|Chest|Int16|-|

## Create Combat Text [81]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|X|Single|-|
|4|Y|Single|-|
|3|Color|[Color](#color-structure)|-|
|4|Heal Amount|Int32|-|

## Load Net Module [82]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Module ID|Unsigned Short||
|??|Payload|??||

Note: 82 is sent prior to chat packets in 1.3.5.x. Instructs the server to load a net module.

## Set NPC Kill Count [83]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC Type|Int16|-|
|4|Kill Count|Int32|-|

## Set Player Stealth [84]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player|Byte|-|
|4|Stealth|Single|-|

## Force Item Into Nearest Chest [85]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Inventory Slot|Byte|-|

## Update Tile Entity [86]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|TileEntityId|Int32|-|
|1|UpdateTileFlag|Boolean|If UpdateTileFlag is false, TileEntity is removed|
|1|TileEntity Type|Byte|Only sent if UpdateTileFlag is false|
|2|X|Int16|Only sent if UpdateTileFlag is false|
|2|Y|Int16|Only sent if UpdateTileFlag is false|

## Place Tile Entity [87]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|
|1|TileEntityType|Byte|2 = Logic Sensor 1 = Item Frame 0 = Training Dummy|

## Tweak Item (FKA. Alter Item Drop) [88]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Item Index|Int16|-|
|1|Flags1|Byte|BitFlags: 1 = Color, 2 = Damage, 4 = Knockback, 8 = UseAnimation, 16 = UseTime, 32 = Shoot, 64 = ShootSpeed, 128 = NextFlags|
|4|Packed Color Value|UInt32|if Flags1.Color|
|2|Damage|UInt16|if Flags1.Damage|
|4|Knockback|Single|if Flags1.Knockback|
|2|Use Animation|UInt16|if Flags1.UseAnimation|
|2|Use Time|UInt16|if Flags1.UseTime|
|2|Shoot|Int16|if Flags1.Shoot|
|4|ShootSpeed|Float|if Flags1.ShootSpeed|
|1|Flags2|Byte|if Flags1.NextFlags, BitFlags: 1 = Width, 2 = Height, 4 = Scale, 8 = Ammo, 16 = UseAmmo, 32 = NotAmmo|
|2|Width|Int16|if Flags2.Width|
|2|Height|Int16|if Flags2.Height|
|4|Scale|Float|if Flags2.Scale|
|2|Ammo|Int16|If Flags2.Ammo|
|2|UseAmmo|Int16|If Flags2.UseAmmo|
|1|NotAmmo|Bool|If Flags2.NotAmmo|

## Place Item Frame [89]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|
|2|ItemId|Int16|-|
|1|Prefix|Byte|-|
|2|Stack|Int16|-|

## Update Item Drop 2 [90]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Item ID|Int16|If below 400 and NetID 0 Then Set NullIf ItemID is 400 Then New Item|
|4|Position X|Single|-|
|4|Position Y|Single|-|
|4|Velocity X|Single|-|
|4|Velocity Y|Single|-|
|2|Stack Size|Int16|-|
|1|Prefix|Byte|-|
|1|NoDelay|Byte|If 0 then ownIgnore = 0 and ownTime = 100|
|2|Item Net ID|Int16|-|

## Sync Emote Bubble [91]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|Emote ID|Int32|-|
|1|Anchor Type|Byte|-|
|2|Player ID|UInt16|Only sent if AnchorType != 255|
|2|Emote LifeTime|UInt16|Only sent if AnchorType != 255|
|1|Emote|Byte|Only sent if AnchorType != 255|
|2|Emote MetaData|Int16|Only sent if AnchorType != 255 and Emote < 0|

## Sync Extra Value [92]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC Index|Int16|-|
|4|Extra Value|Int32|-|
|4|X|Single|-|
|4|Y|Single|-|

## Social Handshake [93]
### Not used
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## Deprecated [94]
### Not used
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## Kill Portal [95]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Projectile Owner|UInt16|-|
|1|Projectile AI|Byte|-|

## Player Teleport Portal [96]
### Server <-> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Portal Color Index|Int16|-|
|4|New Position X|Single|-|
|4|New Position Y|Single|-|
|4|Velocity X|Single|-|
|4|Velocity Y|Single|-|

## Notify Player NPC Killed [97]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|

## Notify Player Of Event [98]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Event ID|Int16|-|

## Update Minion Target [99]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|4|Target X|Single|-|
|4|Target Y|Single|-|

## NPC Teleport Portal [100]
### Server <-> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|UInt16|-|
|2|Portal Color Index|Int16|-|
|4|New Position X|Single|-|
|4|New Position Y|Single|-|
|4|Velocity X|Single|-|
|4|Velocity Y|Single|-|

## Update Shield Strengths [101]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Solar Tower Shield Strength|UInt16|-|
|2|Vortex Tower Shield Strength|UInt16|-|
|2|Nebula Tower Shield Strength|UInt16|-|
|2|Stardust Tower Shield Strength|UInt16|-|

## Nebula Level Up [102]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Level Up Type|UInt16|-|
|4|Origin X|Single|In world coordinate pixels.|
|4|Origin Y|Single|In world coordinate pixels.|

## Moon Lord Countdown [103]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|Moon Lord Countdown|Int32|-|

## NPC Shop Item [104]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Slot|Byte|-|
|2|Item Type|Int16|-|
|2|Stack|Int16|-|
|1|Prefix|Byte|-|
|4|Value|Int32|-|
|1|Flags|Byte|BitFlags: 1 = BuyOnce|

## Gem Lock Toggle [105]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|
|1|On|Boolean|-|

## Poof of Smoke [106]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|PackedVector|UInt32|Two Int16's packed into 4 bytes.|

## Smart Text Message (FKA. Chat Message v2) [107] 
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|3|Message Color|[Color](#color-structure)|Client cannot change colors|
|?|Message|[NetworkText](#networktext)||-|
|2|Message Length|Int16|

## Wired Cannon Shot [108]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Damage|Int16|-|
|4|Knockback|Single|-|
|2|X|Int16|-|
|2|Y|Int16|-|
|2|Angle|Int16|-|
|2|Ammo|Int16|-|
|1|Player ID|Byte|Shooter's Player ID|

## Mass Wire Operation [109]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Start X|Int16|-|
|2|Start Y|Int16|-|
|2|End X|Int16|-|
|2|End Y|Int16|-|
|1|ToolMode|Byte|BitFlags: 1 = Red, 2 = Green, 4 = Blue, 8 = Yellow, 16 = Actuator, 32 = Cutter|

## Mass Wire Operation Consume [110]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Item Type|Int16|-|
|2|Quantity|Int16|-|
|1|Player ID|Byte|-|

## Toggle Birthday Party [111]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## GrowFX [112]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|EffectFlags|Byte|1 = Tree Growth Effects, 2 = Fairy Effects|
|4|X|Int32|-|
|4|Y|Int32|-|
|1|Data|Byte| if EffectFlag is TreeGrowth, data is Height; if EffectFlag is Fairy Effects, data is effect Type |
|2|Tree Gore|Int16| Always 0 unless it is TreeGrowth |

## CrystalInvasionStart [113]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|

## CrystalInvasionWipeAll [114]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## MinionAttackTargetUpdate [115]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|Minion Attack Target|Int16|-|

## CrystalInvasionSendWaitTime [116]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|Time Until Next Wave|Int32|1800 (30s) between waves, 30 (5s) when starting|

## PlayerHurtV2 [117]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|?|Player Death Reason|[PlayerDeathReason](#playerdeathreason)|-|
|2|Damage|Int16|-|
|1|Hit Direction|Byte|-|
|1|Flags|Byte|BitFlags: 1 = Crit, 2 = PvP|
|1|Cooldown Counter|SByte|-|

## PlayerDeathV2 [118]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|?|Player Death Reason|[PlayerDeathReason](#playerdeathreason)|-|
|2|Damage|Int16|-|
|1|Hit Direction|Byte|-|
|1|Flags|Byte|BitFlags: 1 = PvP|

## CombatTextString [119]
### Client <-> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|X|Single|-|
|4|Y|Single|-|
|3|Color|[Color](#color-structure)|-|
|?|Combat Text|[NetworkText](#networktext)|-|

## Emoji [120]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player Index|Byte|-|
|1|Emoticon ID|Byte|-|

## TEDisplayDollItemSync [121]
### Client <-> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|4|TileEntity ID|Int32|-|
|1|Item Index|Byte|-|
|2|Item ID|UInt16|-|
|2|Stack|UInt16|-|
|1|Prefix|Byte|-|

## RequestTileEntityInteraction [122]
### Client <-> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|TileEntityID|Int32|-|
|1|Player ID|Byte|-|

## WeaponsRackTryPlacing [123]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|
|2|Net ID|Int16|-|
|1|Prefix|Byte|-|
|2|Stack|Int16|-|

## TEHatRackItemSync [124]
### Client <-> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|4|TileEntityID|Int32|-|
|1|Item Index|Byte|-|
|2|Item ID|UInt16|-|
|2|Stack|UInt16|-|
|1|Prefix|Byte|-|

## SyncTilePicking [125]
### Client <-> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|X|Int16|-|
|2|Y|Int16|-|
|1|Pick Damage|Byte|-|

## SyncRevengeMarker [126]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|Unique ID|Int32|-|
|4|X|Single|-|
|4|Y|Single|-|
|4|NPC ID|Int32|-|
|4|NPC HP Percent|Single|-|
|4|NPC Type|Int32|-|
|4|NPC AI|Int32|-|
|4|Coin Value|Int32|-|
|4|Base Value|Single|-|
|1|SpawnedFromStatue|Boolean|-|

## RemoveRevengeMarker [127]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|Unique ID|Int32|-|

## LandGolfBallInCup [128]
### Client <-> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|2|X|UInt16|-|
|2|Y|UInt16|-|
|2|Number of Hits|UInt16|-|
|2|Proj ID|UInt16|-|

## FinishedConnectingToServer [129]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|

## FishOutNPC [130]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|UInt16|-|
|2|Y|UInt16|-|
|2|NPC ID|Int16|-|

## TamperWithNPC [131]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|UInt16|-|
|1|SetNPCImmunity|Byte|-|
|4|Immunity Time|Int32|Only sent if SetNPCImmunity flag is true|
|2|Immunity Player ID|Int16|Set to -1 for immunity from all players|

## PlayLegacySound [132]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|4|X|Single|-|
|4|Y|Single|-|
|2|Sound ID|UInt16|-|
|1|Sound Flags|Byte|BitFlags: 1 = Style, 2 = Volume Scale, 3 = Pitch Offset|

## FoodPlatterTryPlacing [133]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|X|Int16|-|
|2|Y|Int16|-|
|2|Item ID|Int16|-|
|1|Prefix|Byte|-|
|2|Stack|Int16|-|

## UpdatePlayerLuckFactors [134]
### Client <-> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|4|Ladybug Luck Time Remaining|Int32|-|
|4|Torch Luck|Single|-|
|1|Luck Potion|Byte|-|
|1|HasGardenGnomeNearby|Boolean|-|

## DeadPlayer [135]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|

## SyncCavernMonsterType [136]
### Client <-> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Net ID|UInt16|NPC.cavernMonsterType[0,0]|
|2|Net ID|UInt16|NPC.cavernMonsterType[0,1]|
|2|Net ID|UInt16|NPC.cavernMonsterType[0,2]|
|2|Net ID|UInt16|NPC.cavernMonsterType[1,0]|
|2|Net ID|UInt16|NPC.cavernMonsterType[1,1]|
|2|Net ID|UInt16|NPC.cavernMonsterType[1,2]|

## RequestNPCBuffRemoval [137]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC ID|Int16|-|
|2|Buff ID|UInt16|-|

## ClientFinishedInventoryChangesOnThisTick (formerly ClientSyncedInventory) [138]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|

Sent by the client code in TrySyncingMyPlayer twice when a player moves an item around in their inventory. Packet actually has no data. Total payload size is 2 packets per inventory item drag, with 3 bytes each (2 for length, 1 for packet ID). This is a functionally useless packet.

## SetCountsAsHostForGameplay [139]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|-|
|1|CountsAsHost|Boolean|-|

## SetMiscEventValues [140]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Unused|Byte|Value is never used but set as 0. Possible flexible design pattern?|
|4|creditsRollRemainingTime|Int32|Clamped. Min 0, Max 28800.|
***

## Color Structure
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Red|Byte|-|
|1|Green|Byte|-|
|1|Blue|Byte|-|

## Tile Structure
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Flags1|Byte|BitFlags: 1= Active, 2= Lighted,  4 = HasWall, 8 = HasLiquid, 16 = Wire1, 32 = HalfBrick, 64 = Actuator, 128 = Inactive|
|1|Flags2|Byte|BitFlags: 1 = Wire2, 2 = Wire3, 4 = HasColor, 8 = HasWallColor, 16 = Slope1, 32 = Slope2, 64 = Slope3, 128 = Wire4|
|1|Color|Byte|Only if HasColor|
|1|Wall Color|Byte|Only if HasWallColor|
|2|Type|UInt16|Only if Active|
|2|FrameX|Int16|Only if Active && tileFrameImportant|
|2|FrameY|Int16|Only if Active && tileFrameImportant|
|1|Wall|UInt16|Only if HasWall|
|1|Liquid|Byte|Only if HasLiquid|
|1|LiquidType|Byte|Only if HasLiquid|

## Chest Structure
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Index|Int16|-|
|2|X|Int16|-|
|2|Y|Int16|-|
|?|Chest Name|String|-|

## Sign Structure
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Index|Int16|-|
|2|X|Int16|-|
|2|Y|Int16|-|
|?|Sign Text|String|-|

## TileEntity Structure
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Type|Byte|0 = Training Dummy, 1 = Item Frame, 2 = Logic Sensor, 3 = Display Doll, 4 = Weapons Rack, 5 = Hat Rack, 6 = Food Platter, 7 = Teleportation Pylon|
|4|ID|Int32|-|
|2|X|Int16|-|
|2|Y|Int16|-|
|?|ExtraData|Variable|See TE types below.|

### TrainingDummy ExtraData
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|NPC Index|Int16|-|

### ItemFrame ExtraData
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Item Type|Int16|-|
|1|Item Prefix|Byte|-|
|2|Item Stack|Int16|-|

### LogicSensor ExtraData
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|LogicCheckType|Byte|-|
|1|On|Bool|-|

### DisplayDoll ExtraData
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|DollFlags1|Byte|BitFlags: 0-8 items|
|1|DollFlags2|Byte|BitFlags: 0-8 items|
|2|Item Type|Int16|*See note*|
|1|Item Prefix|Byte|*See note*|
|2|Item Stack|Int16|*See note*|
|2|Dye Type|Int16|*See note*|
|1|Dye Prefix|Byte|*See note*|
|2|Dye Stack|Int16|*See note*|

Note: Each bit in DollFlags1 represents the presence of one item on the doll. For each item, it will loop through and read the item type, prefix, and stack. Then it will move on to the dyes and read the type, prefix, and stack for the dye items.

### WeaponsRack ExtraData
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Item Type|Int16|-|
|1|Item Prefix|Byte|-|
|2|Item Stack|Int16|-|

### HatRack ExtraData
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|HatFlags|Byte|BitFlags: 0-2 items|
|2|Item Type|Int16|*See note*|
|1|Item Prefix|Byte|*See note*|
|2|Item Stack|Int16|*See note*|
|2|Dye Type|Int16|*See note*|
|1|Dye Prefix|Byte|*See note*|
|2|Dye Stack|Int16|*See note*|

Note: Each bit in HatFlags represents the presence of one item on the hat rack. For each item, it will loop through and read the item type, prefix, and stack. Then it will move on to the dyes and read the type, prefix, and stack for the dye items.

### FoodPlatter ExtraData
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Item Type|Int16|-|
|1|Item Prefix|Byte|-|
|2|Item Stack|Int16|-|

## PlayerDeathReason
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player Death Reason|Byte|BitFlags: 1 = Killed via PvP, 2 = Killed via NPC, 4 = Killed via Projectile, 8 = Killed via Other, 16 = Killed via Projectile, 32 = Killed via PvP, 64 = Killed via PvP, 128 = Killed via Custom Modification|
|2|Killer's Player ID|Int16|Only if BitFlags[0] is true|
|2|Killing NPC's Index|Int16|Only if BitFlags[1] is true|
|2|Projectile Index|Int16|Only if BitFlags[2] is true|
|1|Type of Death (Other)|Byte|Only if BitFlags[3] is true: 0 = Fall Damage, 1 = Drowning, 2 = Lava Damage, 3 = Fall Damage, 4 = Demon Altar, 6 = Companion Cube, 7 = Suffocation, 8 = Burning,  9 = Poison/Venom, 10 = Electrified,  11 = WoF (Escape), 12  = WoF (Licked), 13 = Chaos State,  14 = Chaos State v2 (Male),  15 = Chaos State v2 (Female)|
|2|Projectile Type|Int16|Only if BitFlags[4] is true|
|2|Item Type|Int16|Only if BitFlags[5] is true|
|1|Item Prefix|Byte|Only if BitFlags[6] is true|
|?|Death Reason|String|Only if BitFlags[7] is true|

## NetworkText
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Mode|Byte|Enum: 0 = Literal, 1 = Formattable, 2 = LocalizationKey|
|?|Text|String|-|
|1|SubstitutionList Length|Byte|Only if Mode != Literal|
|?|SubstitutionList|NetworkText[]|Only if Mode != Literal|

## ParticleOrchestraType
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|8|Position|Vector2|-|
|8|Movement Vector|Vector2|-|
|4|PackedShaderIndex|Int32|-|
|1|Invoking Player ID|Byte|-|

# Net Modules
## Liquid [0]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Total changes|UInt16|Number of times the following has been repeated.|
|4|Position|Int32|Y is the first 2 bytes, X is the last 2 bytes.|
|1|Liquid Amount|Byte|0-255|
|1|Liquid Type|Byte|1 = Water, 2 = Lava, 3 = Honey|

## Text [1]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Author Index|Byte|Only 0-254 shows chat above heads. Not sent by client, client's index is used for id.|
|?|Message Text|[NetworkText](#networktext)|-|
|3|Color|[Color](#color-structure)|Not sent by client|

## Ping [2]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|8|Position|Vector2|-|

## Ambience [3]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Player ID|Byte|Player the effect centers around|
|4|Seed|Int32|Seed for FastRandom|
|1|SkyEntity Type|Byte|0 = BirdsV, 1 = Wyvern, 2 = Airship, 3 = AirBalloon, 4 = Eyeball, 5 = Meteor, 6 = BoneSerpent, 7 = Bats, 8 = Butterflies, 9 = LostKite, 10 = Vulture, 11 = PixiePosse, 12 = Seagulls, 13 = SlimeBalloons, 14 = Gastropods, 15 = Pegasus, 16 = EaterOfSouls, 17 = Crimera, 18 = Hellbats|

## Bestiary [4]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|BestiaryUnlockType|Byte|0 = Kill, 1 = Sight, 2 = Chat|
|2|NPC NetID|Int16|-|
|2|Kill Count|UInt16|Only included if BestiaryUnlockType is Kill|

## CreativeUnlocks [5]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Item ID|Int16|-|
|2|Sacrificed/Researched Item Amount|UInt16|-|

Note: This does not appear to be used in code, Journey mode uses CreativeUnlocksPlayerReport instead.

## CreativePowers [6]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|2|Power Type|UInt16|0 = FreezeTime, 1 = StartDayImmediately, 2 = StartNoonImmediately, 3 = StartNightImmediately, 4 = StartMidnightImmediately, 5 = GodmodePower, 6 = ModifyWindDirectionAndStrength, 7 = ModifyRainPower, 8 = ModifyTimeRate, 9 = FreezeRainPower, 10 = FreezeWindDirectionAndStrength, 11 = FarPlacementRangePower, 12 = DifficultySliderPower, 13 = StopBiomeSpreadPower, 14 = SpawnRateSliderPerPlayerPower|

Note: There are permission checks in place, but all powers only use `CanBeChangedByEveryone` so they all pass unless a plugin changes this.
```cs
namespace Terraria.GameContent.Creative
{
	public enum PowerPermissionLevel
	{
		LockedForEveryone,       //0 Will always reject
		CanBeChangedByHostAlone, //1 Will only accept if on singleplayer (Main.netMode = 0)
		CanBeChangedByEveryone   //2 Default value for all
	}
}
```

## CreativeUnlocksPlayerReport [7]
### Client -> Server
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|0|Byte|Always zero, value here was intended for other potential unlock reports. Within NetCreativePowerPermissionsModule, private const _requestItemSacrificeId = 0|
|2|Item ID|UInt16|-|
|2|Sacrificed/Researched Item Amount|UInt16|-|

## TeleportPylon [8]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Pylon Packet Type|Byte|0 = PylonWasAdded, 1 = PylonWasRemoved, 2 = PlayerRequestsTeleport|
|2|Position X|Int16|-|
|2|Position Y|Int16|-|
|1|Pylon Type|Byte|0 = SurfacePurity, 1 = Jungle, 2 = Hallow, 3 = Underground, 4 = Beach, 5 = Desert, 6 = Snow, 7 = GlowingMushroom, 8 = Victory (Universal Pylon)|

## Particles [9]
### Server <-> Client (Sync)
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|Particle Type|Byte|0 = Keybrand, 1 = FlameWaders, 2 = StellarTune, 3 = WallOfFleshGoatMountFlames, 4 = BlackLightningHit, 5 = RainbowRodHit, 6 = BlackLightningSmall, 7 = StardustPunch, 8 = PrincessWeapon|
|21|ParticleOrchestraType|[ParticleOrchestraType](#particleorchestratype)|-|

## CreativePowerPermissions [10]
### Server -> Client
| Size | Description | Type | Notes |
|------|-------------|------|-------|
|1|0|Byte|Always zero, value here maybe intended for other potential unlock reports. Within NetCreativePowerPermissionsModule, private const _setPermissionLevelId = 0|
|2|Power Type|UInt16|0 = FreezeTime, 1 = StartDayImmediately, 2 = StartNoonImmediately, 3 = StartNightImmediately, 4 = StartMidnightImmediately, 5 = GodmodePower, 6 = ModifyWindDirectionAndStrength, 7 = ModifyRainPower, 8 = ModifyTimeRate, 9 = FreezeRainPower, 10 = FreezeWindDirectionAndStrength, 11 = FarPlacementRangePower, 12 = DifficultySliderPower, 13 = StopBiomeSpreadPower, 14 = SpawnRateSliderPerPlayerPower|
|1|Power Level|Byte|0 = LockedForEveryone, 1 = CanBeChangedByHostAlone, 2 = CanBeChangedByEveryone|

Note: All powers use only `CanBeChangedByEveryone`. If the server tells the client a power has a permission other than default, it will be stopped client-side and notified in the client's localization.
```cs
namespace Terraria.GameContent.Creative
{
	public enum PowerPermissionLevel
	{
		LockedForEveryone,       //0 Will always reject
		CanBeChangedByHostAlone, //1 Will only accept if on singleplayer (Main.netMode = 0)
		CanBeChangedByEveryone   //2 Default value for all
	}
}
```