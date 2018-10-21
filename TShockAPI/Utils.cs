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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;
using TShockAPI.DB;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using TShockAPI.Localization;

namespace TShockAPI
{
	/// <summary>
	/// Utilities and other TShock core calls that don't fit anywhere else
	/// </summary>
	public class Utils
	{
		/// <summary>
		/// The lowest id for a prefix.
		/// </summary>
		private const int FirstItemPrefix = 1;

		/// <summary>
		/// The highest id for a prefix.
		/// </summary>
		private const int LastItemPrefix = 83;

		/// <summary>instance - an instance of the utils class</summary>
		private static readonly Utils instance = new Utils();

		/// <summary>Utils - Creates a utilities object.</summary>
		private Utils() {}

		/// <summary>Instance - An instance of the utils class.</summary>
		/// <value>value - the Utils instance</value>
		public static Utils Instance { get { return instance; } }

		/// <summary>
		/// Provides the real IP address from a RemoteEndPoint string that contains a port and an IP
		/// </summary>
		/// <param name="mess">A string IPv4 address in IP:PORT form.</param>
		/// <returns>A string IPv4 address.</returns>
		public string GetRealIP(string mess)
		{
			return mess.Split(':')[0];
		}

		/// <summary>
		/// It's a clamp function
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">Value to clamp</param>
		/// <param name="max">Maximum bounds of the clamp</param>
		/// <param name="min">Minimum bounds of the clamp</param>
		/// <returns></returns>
		public T Clamp<T>(T value, T max, T min)
			where T : IComparable<T>
		{
			T result = value;
			if (value.CompareTo(max) > 0)
				result = max;
			if (value.CompareTo(min) < 0)
				result = min;
			return result;
		}

		/// <summary>
		/// Saves the map data by calling the SaveManager and instructing it to save the world.
		/// </summary>
		public void SaveWorld()
		{
			SaveManager.Instance.SaveWorld();
		}

		/// <summary>Broadcast - Broadcasts a message to all players on the server, as well as the server console, and the logs.</summary>
		/// <param name="msg">msg - The message to send</param>
		/// <param name="red">red - The amount of red (0-255) in the color for supported destinations.</param>
		/// <param name="green">green - The amount of green (0-255) in the color for supported destinations.</param>
		/// <param name="blue">blue - The amount of blue (0-255) in the color for the supported destinations.</param>
		public void Broadcast(string msg, byte red, byte green, byte blue)
		{
			TSPlayer.All.SendMessage(msg, red, green, blue);
			TSPlayer.Server.SendMessage(msg, red, green, blue);
			TShock.Log.Info(string.Format("Broadcast: {0}", msg));
		}

		/// <summary>>Broadcast - Broadcasts a message to all players on the server, as well as the server console, and the logs.</summary>
		/// <param name="msg">msg - The message to send</param>
		/// <param name="color">color - The color object for supported destinations.</param>
		public void Broadcast(string msg, Color color)
		{
			Broadcast(msg, color.R, color.G, color.B);
		}

		/// <summary>
		/// Broadcasts a message from a Terraria playerplayer, not TShock
		/// </summary>
		/// <param name="ply">ply - the Terraria player index that will send the packet</param>
		/// <param name="msg">msg - The message to send</param>
		/// <param name="red">red - The amount of red (0-255) in the color for supported destinations.</param>
		/// <param name="green">green - The amount of green (0-255) in the color for supported destinations.</param>
		/// <param name="blue">blue - The amount of blue (0-255) in the color for the supported destinations.</param>
		public void Broadcast(int ply, string msg, byte red, byte green, byte blue)
		{
			TSPlayer.All.SendMessageFromPlayer(msg, red, green, blue, ply);
			TSPlayer.Server.SendMessage(Main.player[ply].name + ": " + msg, red, green, blue);
			TShock.Log.Info(string.Format("Broadcast: {0}", Main.player[ply].name + ": " + msg));
		}

		/// <summary>
		/// Sends message to all players with 'logs' permission.
		/// </summary>
		/// <param name="log">Message to send</param>
		/// <param name="color">Color of the message</param>
		/// <param name="excludedPlayer">The player to not send the message to.</param>
		public void SendLogs(string log, Color color, TSPlayer excludedPlayer = null)
		{
			TShock.Log.Info(log);
			TSPlayer.Server.SendMessage(log, color);
			foreach (TSPlayer player in TShock.Players)
			{
				if (player != null && player != excludedPlayer && player.Active && player.HasPermission(Permissions.logs) &&
						player.DisplayLogs && TShock.Config.DisableSpewLogs == false)
					player.SendMessage(log, color);
			}
		}

		/// <summary>
		/// Gets the number of active players on the server.
		/// </summary>
		/// <returns>The number of active players on the server.</returns>
		public int GetActivePlayerCount()
		{
			return Main.player.Where(p => null != p && p.active).Count();
		}

		//Random should not be generated in a method
		Random r = new Random();

		/// <summary>
		/// Gets a random clear tile in range
		/// </summary>
		/// <param name="startTileX">Bound X</param>
		/// <param name="startTileY">Bound Y</param>
		/// <param name="tileXRange">Range on the X axis</param>
		/// <param name="tileYRange">Range on the Y axis</param>
		/// <param name="tileX">X location</param>
		/// <param name="tileY">Y location</param>
		public void GetRandomClearTileWithInRange(int startTileX, int startTileY, int tileXRange, int tileYRange,
																							out int tileX, out int tileY)
		{
			int j = 0;
			do
			{
				if (j == 100)
				{
					tileX = startTileX;
					tileY = startTileY;
					break;
				}
				tileX = startTileX + r.Next(tileXRange*-1, tileXRange);
				tileY = startTileY + r.Next(tileYRange*-1, tileYRange);
				j++;
			} while (TilePlacementValid(tileX, tileY) && TileSolid(tileX, tileY));
		}

		/// <summary>
		/// Determines if a tile is valid.
		/// </summary>
		/// <param name="tileX">Location X</param>
		/// <param name="tileY">Location Y</param>
		/// <returns>If the tile is valid</returns>
		public bool TilePlacementValid(int tileX, int tileY)
		{
			return tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY;
		}

		/// <summary>
		/// Checks if the tile is solid.
		/// </summary>
		/// <param name="tileX">Location X</param>
		/// <param name="tileY">Location Y</param>
		/// <returns>The tile's solidity.</returns>
		public bool TileSolid(int tileX, int tileY)
		{
			return TilePlacementValid(tileX, tileY) && Main.tile[tileX, tileY] != null &&
				Main.tile[tileX, tileY].active() && Main.tileSolid[Main.tile[tileX, tileY].type] &&
				!Main.tile[tileX, tileY].inActive() && !Main.tile[tileX, tileY].halfBrick() &&
				Main.tile[tileX, tileY].slope() == 0 && Main.tile[tileX, tileY].type != TileID.Bubble;
		}

		/// <summary>
		/// Gets a list of items by ID, Name or Tag.
		/// </summary>
		/// <param name="text">Item ID, Name or Tag.</param>
		/// <returns>A list of matching items.</returns>
		public List<Item> GetItemByIdOrName(string text)
		{
			int type = -1;
			if (Int32.TryParse(text, out type))
			{
				if (type >= Main.maxItemTypes)
					return new List<Item>();
				return new List<Item> {GetItemById(type)};
			}
			Item item = GetItemFromTag(text);
			if (item != null)
				return new List<Item>() { item };
			return GetItemByName(text);
		}

		/// <summary>
		/// Gets an item by ID
		/// </summary>
		/// <param name="id">ID</param>
		/// <returns>Item</returns>
		public Item GetItemById(int id)
		{
			Item item = new Item();
			item.netDefaults(id);
			return item;
		}

		/// <summary>
		/// Gets items by name
		/// </summary>
		/// <param name="name">name</param>
		/// <returns>List of Items</returns>
		public List<Item> GetItemByName(string name)
		{
			var found = new List<Item>();
			Item item = new Item();
			string nameLower = name.ToLowerInvariant();
			var checkEnglish = Language.ActiveCulture != GameCulture.English;

			for (int i = 1; i < Main.maxItemTypes; i++)
			{
				item.netDefaults(i);
				if (!String.IsNullOrWhiteSpace(item.Name))
				{
					if (item.Name.ToLowerInvariant() == nameLower)
						return new List<Item> { item };
					if (item.Name.ToLowerInvariant().StartsWith(nameLower))
						found.Add(item.Clone());
				}

				if (!checkEnglish)
				{
					continue;
				}

				string englishName = EnglishLanguage.GetItemNameById(i).ToLowerInvariant();
				if (!String.IsNullOrEmpty(englishName))
				{
					if (englishName == nameLower)
						return new List<Item> { item };
					if (englishName.StartsWith(nameLower))
						found.Add(item.Clone());
				}
			}
			return found;
		}

		/// <summary>
		/// Gets an item based on a chat item tag.
		/// </summary>
		/// <param name="tag">A tag in the [i/s#/p#:netid] format.</param>
		/// <returns>The item represented by the tag.</returns>
		public Item GetItemFromTag(string tag)
		{
			Regex regex = new Regex(@"\[i(tem)?(?:\/s(?<Stack>\d{1,3}))?(?:\/p(?<Prefix>\d{1,3}))?:(?<NetID>-?\d{1,4})\]");
			Match match = regex.Match(tag);
			if (!match.Success)
				return null;
			Item item = new Item();
			item.netDefaults(Int32.Parse(match.Groups["NetID"].Value));
			if (!String.IsNullOrWhiteSpace(match.Groups["Stack"].Value))
				item.stack = Int32.Parse(match.Groups["Stack"].Value);
			if (!String.IsNullOrWhiteSpace(match.Groups["Prefix"].Value))
				item.prefix = Byte.Parse(match.Groups["Prefix"].Value);
			return item;
		}

		/// <summary>
		/// Gets an NPC by ID or Name
		/// </summary>
		/// <param name="idOrName"></param>
		/// <returns>List of NPCs</returns>
		public List<NPC> GetNPCByIdOrName(string idOrName)
		{
			int type = -1;
			if (int.TryParse(idOrName, out type))
			{
				if (type >= Main.maxNPCTypes)
					return new List<NPC>();
				return new List<NPC> { GetNPCById(type) };
			}
			return GetNPCByName(idOrName);
		}

		/// <summary>
		/// Gets an NPC by ID
		/// </summary>
		/// <param name="id">ID</param>
		/// <returns>NPC</returns>
		public NPC GetNPCById(int id)
		{
			NPC npc = new NPC();
			npc.SetDefaults(id);
			return npc;
		}

		/// <summary>
		/// Gets a NPC by name
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>List of matching NPCs</returns>
		public List<NPC> GetNPCByName(string name)
		{
			var found = new List<NPC>();
			NPC npc = new NPC();
			string nameLower = name.ToLowerInvariant();
			for (int i = -17; i < Main.maxNPCTypes; i++)
			{
				string englishName = EnglishLanguage.GetNpcNameById(i).ToLowerInvariant();

				npc.SetDefaults(i);
				if (npc.FullName.ToLowerInvariant() == nameLower || npc.TypeName.ToLowerInvariant() == nameLower
					|| nameLower == englishName)
					return new List<NPC> { npc };
				if (npc.FullName.ToLowerInvariant().StartsWith(nameLower) || npc.TypeName.ToLowerInvariant().StartsWith(nameLower)
					|| englishName?.StartsWith(nameLower) == true)
					found.Add((NPC)npc.Clone());
			}
			return found;
		}

		/// <summary>
		/// Gets a buff name by id
		/// </summary>
		/// <param name="id">ID</param>
		/// <returns>name</returns>
		public string GetBuffName(int id)
		{
			return (id > 0 && id < Main.maxBuffTypes) ? Lang.GetBuffName(id) : null;
		}

		/// <summary>
		/// Gets the description of a buff
		/// </summary>
		/// <param name="id">ID</param>
		/// <returns>description</returns>
		public string GetBuffDescription(int id)
		{
			return (id > 0 && id < Main.maxBuffTypes) ? Lang.GetBuffDescription(id) : null;
		}

		/// <summary>
		/// Gets a list of buffs by name
		/// </summary>
		/// <param name="name">name</param>
		/// <returns>Matching list of buff ids</returns>
		public List<int> GetBuffByName(string name)
		{
			string nameLower = name.ToLower();
			string buffname;
			for (int i = 1; i < Main.maxBuffTypes; i++)
			{
				buffname = Lang.GetBuffName(i);
				if (!String.IsNullOrWhiteSpace(buffname) && buffname.ToLower() == nameLower)
					return new List<int> {i};
			}
			var found = new List<int>();
			for (int i = 1; i < Main.maxBuffTypes; i++)
			{
				buffname = Lang.GetBuffName(i);
				if (!String.IsNullOrWhiteSpace(buffname) && buffname.ToLower().StartsWith(nameLower))
					found.Add(i);
			}
			return found;
		}

		/// <summary>
		/// Gets a prefix based on its id
		/// </summary>
		/// <param name="id">ID</param>
		/// <returns>Prefix name</returns>
		public string GetPrefixById(int id)
		{
			return id < FirstItemPrefix || id > LastItemPrefix ? "" : Lang.prefix[id].ToString() ?? "";
		}

		/// <summary>
		/// Gets a list of prefixes by name
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>List of prefix IDs</returns>
		public List<int> GetPrefixByName(string name)
		{
			Item item = new Item();
			item.SetDefaults(0);
			string lowerName = name.ToLowerInvariant();
			var found = new List<int>();
			for (int i = FirstItemPrefix; i <= LastItemPrefix; i++)
			{
				item.prefix = (byte)i;
				string prefixName = item.AffixName().Trim().ToLowerInvariant();
				string englishName = EnglishLanguage.GetPrefixById(i).ToLowerInvariant();
				if (prefixName == lowerName || englishName == lowerName)
					return new List<int>() { i };
				else if (prefixName.StartsWith(lowerName) || englishName?.StartsWith(lowerName) == true) // Partial match
					found.Add(i);
			}
			return found;
		}

				/// <summary>
		/// Gets a prefix by ID or name
		/// </summary>
		/// <param name="idOrName">ID or name</param>
		/// <returns>List of prefix IDs</returns>
		public List<int> GetPrefixByIdOrName(string idOrName)
		{
			int type = -1;
			if (int.TryParse(idOrName, out type) && type >= FirstItemPrefix && type <= LastItemPrefix)
			{
				return new List<int> {type};
			}
			return GetPrefixByName(idOrName);
		}

		/// <summary>
		/// Stops the server after kicking all players with a reason message, and optionally saving the world
		/// </summary>
		/// <param name="save">bool perform a world save before stop (default: true)</param>
		/// <param name="reason">string reason (default: "Server shutting down!")</param>
		public void StopServer(bool save = true, string reason = "Server shutting down!")
		{
			TShock.ShuttingDown = true;

			if (save)
				SaveManager.Instance.SaveWorld();

			TSPlayer.All.Kick(reason, true, true, null, true);

			// Broadcast so console can see we are shutting down as well
			TShock.Utils.Broadcast(reason, Color.Red);

			// Disconnect after kick as that signifies server is exiting and could cause a race
			Netplay.disconnect = true;
		}

		/// <summary>
		/// Reloads all configuration settings, groups, regions and raises the reload event.
		/// </summary>
		public void Reload()
		{
			FileTools.SetupConfig();
			TShock.HandleCommandLinePostConfigLoad(Environment.GetCommandLineArgs());
			TShock.Groups.LoadPermisions();
			TShock.Regions.Reload();
			TShock.Itembans.UpdateItemBans();
			TShock.ProjectileBans.UpdateBans();
			TShock.TileBans.UpdateBans();
		}

		/// <summary>
		/// Returns an IPv4 address from a DNS query
		/// </summary>
		/// <param name="hostname">string ip</param>
		public string GetIPv4AddressFromHostname(string hostname)
		{
			try
			{
				//Get the ipv4 address from GetHostAddresses, if an ip is passed it will return that ip
				var ip = Dns.GetHostAddresses(hostname).FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);
				//if the dns query was successful then return it, otherwise return an empty string
				return ip != null ? ip.ToString() : "";
			}
			catch (SocketException)
			{
			}
			return "";
		}

		/// <summary>
		/// Checks if world has hit the max number of chests
		/// </summary>
		/// <returns>True if the entire chest array is used</returns>
		public bool HasWorldReachedMaxChests()
		{
			for (int i = 0; i < Main.chest.Length; i++)
			{
				if (Main.chest[i] == null)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Attempts to parse a string as a timespan (_d_m_h_s).
		/// </summary>
		/// <param name="str">The time string.</param>
		/// <param name="seconds">The seconds.</param>
		/// <returns>Whether the string was parsed successfully.</returns>
		public bool TryParseTime(string str, out int seconds)
		{
			seconds = 0;

			var sb = new StringBuilder(3);
			for (int i = 0; i < str.Length; i++)
			{
				if (Char.IsDigit(str[i]) || (str[i] == '-' || str[i] == '+' || str[i] == ' '))
					sb.Append(str[i]);
				else
				{
					int num;
					if (!int.TryParse(sb.ToString(), out num))
						return false;

					sb.Clear();
					switch (str[i])
					{
						case 's':
							seconds += num;
							break;
						case 'm':
							seconds += num * 60;
							break;
						case 'h':
							seconds += num * 60 * 60;
							break;
						case 'd':
							seconds += num * 60 * 60 * 24;
							break;
						default:
							return false;
					}
				}
			}
			if (sb.Length != 0)
				return false;
			return true;
		}

		/// <summary>
		/// Searches for a projectile by identity and owner
		/// </summary>
		/// <param name="identity">identity</param>
		/// <param name="owner">owner</param>
		/// <returns>projectile ID</returns>
		public int SearchProjectile(short identity, int owner)
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				if (Main.projectile[i].identity == identity && Main.projectile[i].owner == owner)
					return i;
			}
			return 1000;
		}

		/// <summary>
		/// Enumerates boundary points of the given region's rectangle.
		/// </summary>
		/// <param name="regionArea">The region's area to enumerate through.</param>
		/// <returns>The enumerated boundary points.</returns>
		public IEnumerable<Point> EnumerateRegionBoundaries(Rectangle regionArea)
		{
			for (int x = 0; x < regionArea.Width + 1; x++)
			{
				yield return new Point(regionArea.Left + x, regionArea.Top);
				yield return new Point(regionArea.Left + x, regionArea.Bottom);
			}

			for (int y = 1; y < regionArea.Height; y++)
			{
				yield return new Point(regionArea.Left, regionArea.Top + y);
				yield return new Point(regionArea.Right, regionArea.Top + y);
			}
		}

		/// <summary>EncodeColor - Encodes a color as an int.</summary>
		/// <param name="color">color - The color to encode</param>
		/// <returns>int? - The encoded color</returns>
		public int? EncodeColor(Color? color)
		{
			if (color == null)
				return null;

			return BitConverter.ToInt32(new[] { color.Value.R, color.Value.G, color.Value.B, color.Value.A }, 0);
		}

		/// <summary>DecodeColor - Decodes a color encoded by the EncodeColor function.</summary>
		/// <param name="encodedColor">encodedColor - The encoded color</param>
		/// <returns>Color? - The decoded color</returns>
		public Color? DecodeColor(int? encodedColor)
		{
			if (encodedColor == null)
				return null;

			byte[] data = BitConverter.GetBytes(encodedColor.Value);
			return new Color(data[0], data[1], data[2], data[3]);
		}

		/// <summary>
		/// Encodes a Boolean Array as an int.
		/// </summary>
		/// <param name="bools">The boolean array to encode.</param>
		/// <returns>The encoded int.</returns>
		public int? EncodeBoolArray(bool[] bools)
		{
			if (bools == null)
				return null;

			int result = 0;
			for (int i = 0; i < bools.Length; i++)
				if (bools[i])
					result |= (1 << i);

			return result;
		}

		/// <summary>
		/// Decodes a Boolean Array from an int.
		/// </summary>
		/// <param name="encodedbools">The encoded Boolean Array.</param>
		/// <returns>The resulting Boolean Array.</returns>
		public bool[] DecodeBoolArray(int? encodedbools)
		{
			if (encodedbools == null)
				return null;

			bool[] result = new bool[10];
			for (int i = 0; i < result.Length; i++)
				result[i] = (encodedbools & 1 << i) != 0;

			return result;
		}

		/// <summary>EncodeBitsByte - Encodes a BitsByte as a byte.</summary>
		/// <param name="bitsByte">bitsByte - The BitsByte object</param>
		/// <returns>byte? - The converted byte</returns>
		public byte? EncodeBitsByte(BitsByte? bitsByte)
		{
			if (bitsByte == null)
				return null;

			byte result = 0;
			for (int i = 0; i < 8; i++)
				if (bitsByte.Value[i])
					result |= (byte)(1 << i);

			return result;
		}

		/// <summary>DecodeBitsByte - Decodes a bitsbyte from an int.</summary>
		/// <param name="encodedBitsByte">encodedBitsByte - The encoded bitsbyte object.</param>
		/// <returns>BitsByte? - The decoded bitsbyte object</returns>
		public BitsByte? DecodeBitsByte(int? encodedBitsByte)
		{
			if (encodedBitsByte == null)
				return null;

			BitsByte result = new BitsByte();
			for (int i = 0; i < 8; i++)
				result[i] = (encodedBitsByte & 1 << i) != 0;

			return result;
		}

		/// <summary>GetResponseNoException - Gets a web response without generating an exception.</summary>
		/// <param name="req">req - The request to send.</param>
		/// <returns>HttpWebResponse - The response object.</returns>
		public HttpWebResponse GetResponseNoException(HttpWebRequest req)
		{
			try
			{
				return (HttpWebResponse)req.GetResponse();
			}
			catch (WebException we)
			{
				var resp = we.Response as HttpWebResponse;
				if (resp == null)
					throw;
				return resp;
			}
		}

		/// <summary>
		/// Colors the given text by correctly applying the color chat tag.
		/// </summary>
		/// <param name="text">The text to color.</param>
		/// <param name="color">The color to apply.</param>
		/// <returns>The <paramref name="text"/>, surrounded by the color tag with the appropriated hex code.</returns>
		public string ColorTag(string text, Color color)
		{
			return String.Format("[c/{0}:{1}]", color.Hex3(), text);
		}

		/// <summary>
		/// Converts an item into its text representation using the item chat tag.
		/// </summary>
		/// <param name="item">The item to convert.</param>
		/// <returns>The <paramref name="item"/> NetID surrounded by the item tag with proper stack/prefix data.</returns>
		public string ItemTag(Item item)
		{
			int netID = item.netID;
			int stack = item.stack;
			int prefix = item.prefix;
			string options = stack > 1 ? "/s" + stack : prefix != 0 ? "/p" + prefix : "";
			return String.Format("[i{0}:{1}]", options, netID);
		}

		/// <summary>
		/// Gets a list of points selected by a mass-wiring tool.
		/// </summary>
		/// <param name="start">The starting point for the selection.</param>
		/// <param name="end">The ending point for the selection.</param>
		/// <param name="direction">False if facing left, True if facing right.</param>
		/// <returns>
		/// A list of coordinates containing the <paramref name="start"/> and <paramref name="end"/>
		/// points and a list of points between them, forming an L shape based on <paramref name="direction"/>.
		/// </returns>
		public List<Point> GetMassWireOperationRange(Point start, Point end, bool direction = false)
		{
			List<Point> points = new List<Point>();

			#region Tile Selection Logic stolen from Wiring.cs

			// Slightly modified version of Wiring.MassWireOperationInner, ignores a player's wire count
			int num = Math.Sign(end.X - start.X);
			int num2 = Math.Sign(end.Y - start.Y);
			Point pt = new Point();
			int num3;
			int num4;
			int num5;
			if (direction)
			{
				pt.X = start.X;
				num3 = start.Y;
				num4 = end.Y;
				num5 = num2;
			}
			else
			{
				pt.Y = start.Y;
				num3 = start.X;
				num4 = end.X;
				num5 = num;
			}
			int num6 = num3;
			while (num6 != num4)
			{
				if (direction)
				{
					pt.Y = num6;
				}
				else
				{
					pt.X = num6;
				}
				points.Add(pt);
				num6 += num5;
			}
			if (direction)
			{
				pt.Y = end.Y;
				num3 = start.X;
				num4 = end.X;
				num5 = num;
			}
			else
			{
				pt.X = end.X;
				num3 = start.Y;
				num4 = end.Y;
				num5 = num2;
			}
			int num7 = num3;
			while (num7 != num4)
			{
				if (!direction)
				{
					pt.Y = num7;
				}
				else
				{
					pt.X = num7;
				}
				points.Add(pt);
				num7 += num5;
			}
			points.Add(end);

			#endregion

			return points;
		}

		/// <summary>
		/// Dumps information and optionally exits afterwards
		/// </summary>
		/// <param name="exit"></param>
		public void Dump(bool exit = true)
		{
			PrepareLangForDump();
			// Lang.setLang(true);
			ConfigFile.DumpDescriptions();
			Permissions.DumpDescriptions();
			ServerSideCharacters.ServerSideConfig.DumpDescriptions();
			DumpBuffs("BuffList.txt");
			DumpItems("Items-1_0.txt", 1, 235);
			DumpItems("Items-1_1.txt", 235, 604);
			DumpItems("Items-1_2.txt", 604, 2749);
			DumpItems("Items-1_3.txt", 2749, Main.maxItemTypes);
			DumpNPCs("NPCs.txt");
			DumpProjectiles("Projectiles.txt");
			DumpPrefixes("Prefixes.txt");

			if (exit)
			{
				Environment.Exit(1);
			}
		}

		internal void PrepareLangForDump()
		{
			for(int i = 0; i < Main.recipe.Length; i++)
				Main.recipe[i] = new Recipe();
		}
		
		/// <summary>Dumps a matrix of all permissions & all groups in Markdown table format.</summary>
		/// <param name="path">The save destination.</param>
		internal void DumpPermissionMatrix(string path)
		{
			StringBuilder output = new StringBuilder();
			output.Append("|Permission|");

			// Traverse to build group name list
			foreach (Group g in TShock.Groups.groups)
			{
				output.Append(g.Name);
				output.Append("|");
			}

			output.AppendLine();
			output.Append("|-------|");

			foreach (Group g in TShock.Groups.groups)
			{
				output.Append("-------|");
			}
			output.AppendLine();

			foreach (var field in typeof(Permissions).GetFields().OrderBy(f => f.Name))
			{
				output.Append("|");
				output.Append((string) field.GetValue(null));
				output.Append("|");

				foreach (Group g in TShock.Groups.groups)
				{
					if (g.HasPermission((string) field.GetValue(null)))
					{
						output.Append("✔|");
					}
					else
					{
						output.Append("|");
					}
				}
				output.AppendLine();
			}

			File.WriteAllText(path, output.ToString());
		}

		public void DumpBuffs(string path)
		{
			StringBuilder buffer = new StringBuilder();
			buffer.AppendLine("[block:parameters]").AppendLine("{").AppendLine("  \"data\": {");
			buffer.AppendLine("    \"h-0\": \"ID\",");
			buffer.AppendLine("    \"h-1\": \"Name\",");
			buffer.AppendLine("    \"h-2\": \"Description\",");

			List<object[]> elements = new List<object[]>();
			for (int i = 0; i < Main.maxBuffTypes; i++)
			{
				if (!String.IsNullOrEmpty(Lang.GetBuffName(i)))
				{
					object[] element = new object[] { i, Lang.GetBuffName(i), Lang.GetBuffDescription(i) };
					elements.Add(element);
				}
			}

			var rows = elements.Count;
			var columns = 0;
			if (rows > 0)
			{
				columns = elements[0].Length;
			}
			OutputElementsForDump(buffer, elements, rows, columns);

			buffer.AppendLine();
			buffer.AppendLine("  },");
			buffer.AppendLine(String.Format("  \"cols\": {0},", columns)).AppendLine(String.Format("  \"rows\": {0}", rows));
			buffer.AppendLine("}").Append("[/block]");

			File.WriteAllText(path, buffer.ToString());
		}

		public void DumpItems(string path, int start, int end)
		{
			Main.player[Main.myPlayer] = new Player();
			StringBuilder buffer = new StringBuilder();
			Regex newLine = new Regex(@"\n");
			buffer.AppendLine("[block:parameters]").AppendLine("{").AppendLine("  \"data\": {");
			buffer.AppendLine("    \"h-0\": \"ID\",");
			buffer.AppendLine("    \"h-1\": \"Name\",");
			buffer.AppendLine("    \"h-2\": \"Tooltip\",");

			List<object[]> elements = new List<object[]>();
			for (int i = start; i < end; i++)
			{
				Item item = new Item();
				item.SetDefaults(i);

				string tt = "";
				for (int x = 0; x < item.ToolTip.Lines; x++) {
					tt += item.ToolTip.GetLine(x) + "\n";
				}
				if (!String.IsNullOrEmpty(item.Name))
				{
					object[] element = new object[] { i,
													  newLine.Replace(item.Name, @" "),
													  newLine.Replace(tt, @" "),
													};
					elements.Add(element);
				}
			}

			var rows = elements.Count;
			var columns = 0;
			if (rows > 0)
			{
				columns = elements[0].Length;
			}
			OutputElementsForDump(buffer, elements, rows, columns);

			buffer.AppendLine();
			buffer.AppendLine("  },");
			buffer.AppendLine(String.Format("  \"cols\": {0},", columns)).AppendLine(String.Format("  \"rows\": {0}", rows));
			buffer.AppendLine("}").Append("[/block]");

			File.WriteAllText(path, buffer.ToString());
		}

		public void DumpNPCs(string path)
		{
			StringBuilder buffer = new StringBuilder();
			buffer.AppendLine("[block:parameters]").AppendLine("{").AppendLine("  \"data\": {");
			buffer.AppendLine("    \"h-0\": \"ID\",");
			buffer.AppendLine("    \"h-1\": \"Full Name\",");
			buffer.AppendLine("    \"h-2\": \"Type Name\",");

			List<object[]> elements = new List<object[]>();
			for (int i = -65; i < Main.maxNPCTypes; i++)
			{
				NPC npc = new NPC();
				npc.SetDefaults(i);
				if (!String.IsNullOrEmpty(npc.FullName))
				{
					object[] element = new object[] { i, npc.FullName, npc.TypeName };
					elements.Add(element);
				}
			}

			var rows = elements.Count;
			var columns = 0;
			if (rows > 0)
			{
				columns = elements[0].Length;
			}
			OutputElementsForDump(buffer, elements, rows, columns);

			buffer.AppendLine();
			buffer.AppendLine("  },");
			buffer.AppendLine(String.Format("  \"cols\": {0},", columns)).AppendLine(String.Format("  \"rows\": {0}", rows));
			buffer.AppendLine("}").Append("[/block]");

			File.WriteAllText(path, buffer.ToString());
		}

		public void DumpProjectiles(string path)
		{
			Main.rand = new UnifiedRandom();
			StringBuilder buffer = new StringBuilder();
			buffer.AppendLine("[block:parameters]").AppendLine("{").AppendLine("  \"data\": {");
			buffer.AppendLine("    \"h-0\": \"ID\",");
			buffer.AppendLine("    \"h-1\": \"Name\",");

			List<object[]> elements = new List<object[]>();
			for (int i = 0; i < Main.maxProjectileTypes; i++)
			{
				Projectile projectile = new Projectile();
				projectile.SetDefaults(i);
				if (!String.IsNullOrEmpty(projectile.Name))
				{
					object[] element = new object[] { i, projectile.Name };
					elements.Add(element);
				}
			}

			var rows = elements.Count;
			var columns = 0;
			if (rows > 0)
			{
				columns = elements[0].Length;
			}
			OutputElementsForDump(buffer, elements, rows, columns);

			buffer.AppendLine();
			buffer.AppendLine("  },");
			buffer.AppendLine(String.Format("  \"cols\": {0},", columns)).AppendLine(String.Format("  \"rows\": {0}", rows));
			buffer.AppendLine("}").Append("[/block]");

			File.WriteAllText(path, buffer.ToString());
		}

		public void DumpPrefixes(string path)
		{
			StringBuilder buffer = new StringBuilder();
			buffer.AppendLine("[block:parameters]").AppendLine("{").AppendLine("  \"data\": {");
			buffer.AppendLine("    \"h-0\": \"ID\",");
			buffer.AppendLine("    \"h-1\": \"Name\",");

			List<object[]> elements = new List<object[]>();
			for (int i = 0; i < PrefixID.Count; i++)
			{
				string prefix = Lang.prefix[i].ToString();

				if (!String.IsNullOrEmpty(prefix))
				{
					object[] element = new object[] {i, prefix};
					elements.Add(element);
				}
			}

			var rows = elements.Count;
			var columns = 0;
			if (rows > 0)
			{
				columns = elements[0].Length;
			}
			OutputElementsForDump(buffer, elements, rows, columns);

			buffer.AppendLine();
			buffer.AppendLine("  },");
			buffer.AppendLine(String.Format("  \"cols\": {0},", columns)).AppendLine(String.Format("  \"rows\": {0}", rows));
			buffer.AppendLine("}").Append("[/block]");

			File.WriteAllText(path, buffer.ToString());
		}

		private void OutputElementsForDump(StringBuilder buffer, List<object[]> elements, int rows, int columns)
		{
			if (rows > 0)
			{
				columns = elements[0].Length;
				for (int i = 0; i < columns; i++)
				{
					for (int j = 0; j < rows; j++)
					{
						buffer.Append(String.Format("    \"{0}-{1}\": \"{2}\"", j, i, elements[j][i]));
						if (j != rows - 1 || i != columns - 1)
							buffer.AppendLine(",");
					}
				}
			}
		}

		/// <summary>Starts an invasion on the server.</summary>
		/// <param name="type">The invasion type id.</param>
		internal void StartInvasion(int type)
		{
			int invasionSize = 0;

			if (TShock.Config.InfiniteInvasion)
			{
				// Not really an infinite size
				invasionSize = 20000000;
			}
			else
			{
				invasionSize = 100 + (TShock.Config.InvasionMultiplier * GetActivePlayerCount());
			}

			// Order matters
			// StartInvasion will reset the invasion size

			Main.StartInvasion(type);

			// Note: This is a workaround to previously providing the size as a parameter in StartInvasion
			// Have to set start size to report progress correctly
			Main.invasionSizeStart = invasionSize;
			Main.invasionSize = invasionSize;
		}

		/// <summary>Verifies that each stack in each chest is valid and not over the max stack count.</summary>
		internal void FixChestStacks()
		{
			if (TShock.Config.IgnoreChestStacksOnLoad)
				return;

			foreach (Chest chest in Main.chest)
			{
				if (chest != null)
				{
					foreach (Item item in chest.item)
					{
						if (item != null && item.stack > item.maxStack)
							item.stack = item.maxStack;
					}
				}
			}
		}

		/// <summary>Updates the console title with some pertinent information.</summary>
		/// <param name="empty">If the server is empty; determines if we should use Utils.GetActivePlayerCount() for player count or 0.</param>
		internal void SetConsoleTitle(bool empty)
		{
			Console.Title = string.Format("{0}{1}/{2} on {3} @ {4}:{5} (TShock for Terraria v{6})",
					!string.IsNullOrWhiteSpace(TShock.Config.ServerName) ? TShock.Config.ServerName + " - " : "",
					empty ? 0 : GetActivePlayerCount(),
					TShock.Config.MaxSlots, Main.worldName, Netplay.ServerIP.ToString(), Netplay.ListenPort, TShock.VersionNum);
		}

		/// <summary>Determines the distance between two vectors.</summary>
		/// <param name="value1">The first vector location.</param>
		/// <param name="value2">The second vector location.</param>
		/// <returns>The distance between the two vectors.</returns>
		public static float Distance(Vector2 value1, Vector2 value2)
		{
			float num2 = value1.X - value2.X;
			float num = value1.Y - value2.Y;
			float num3 = (num2 * num2) + (num * num);
			return (float)Math.Sqrt(num3);
		}

		/// <summary>Checks to see if a location is in the spawn protection area.</summary>
		/// <param name="x">The x coordinate to check.</param>
		/// <param name="y">The y coordinate to check.</param>
		/// <returns>If the given x,y location is in the spawn area.</returns>
		public static bool IsInSpawn(int x, int y)
		{
			Vector2 tile = new Vector2(x, y);
			Vector2 spawn = new Vector2(Main.spawnTileX, Main.spawnTileY);
			return Distance(spawn, tile) <= TShock.Config.SpawnProtectionRadius;
		}

		/// <summary>Computes the max styles...</summary>
		internal void ComputeMaxStyles()
		{
			var item = new Item();
			for (int i = 0; i < Main.maxItemTypes; i++)
			{
				item.netDefaults(i);
				if (item.placeStyle > 0)
				{
					if (GetDataHandlers.MaxPlaceStyles.ContainsKey(item.createTile))
					{
						if (item.placeStyle > GetDataHandlers.MaxPlaceStyles[item.createTile])
							GetDataHandlers.MaxPlaceStyles[item.createTile] = item.placeStyle;
					}
					else
						GetDataHandlers.MaxPlaceStyles.Add(item.createTile, item.placeStyle);
				}
			}
		}
	}
}
