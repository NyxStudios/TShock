/*
TShock, a server mod for Terraria
Copyright (C) 2011-2019 Pryaxis & TShock Contributors

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

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TShockAPI.Configuration
{
	/// <summary>
	/// Settings used to configure server side characters
	/// </summary>
	public class SscSettings
	{
		/// <summary>
		/// Enable server side characters, causing client data to be saved on the server instead of the client.
		/// </summary>
		[Description("Enable server side characters, causing client data to be saved on the server instead of the client.")]
		public bool Enabled = false;

		/// <summary>
		/// How often SSC should save, in minutes.
		/// </summary>
		[Description("How often SSC should save, in minutes.")]
		public int ServerSideCharacterSave = 5;

		/// <summary>
		/// Time, in milliseconds, to disallow discarding items after logging in when ServerSideCharacters is ON.
		/// </summary>
		[Description("Time, in milliseconds, to disallow discarding items after logging in when ServerSideCharacters is ON.")]
		public int LogonDiscardThreshold = 250;

		/// <summary>
		/// The starting default health for new players when SSC is enabled.
		/// </summary>
		[Description("The starting default health for new players when SSC is enabled.")]
		public int StartingHealth = 100;

		/// <summary>
		/// The starting default mana for new players when SSC is enabled.
		/// </summary>
		[Description("The starting default mana for new players when SSC is enabled.")]
		public int StartingMana = 20;

		/// <summary>
		/// The starting default inventory for new players when SSC is enabled.
		/// </summary>
		[Description("The starting default inventory for new players when SSC is enabled.")]
		public List<NetItem> StartingInventory = new List<NetItem>();

		/// <summary>
		/// Warns players that they have the bypass SSC permission enabled. To disable warning, turn this off.
		/// </summary>
		[Description("Warns players and the console if a player has the tshock.ignore.ssc permission with data in the SSC table.")]
		public bool WarnPlayersAboutBypassPermission = true;
	}

	/// <summary>
	/// Configuration for the server side characters system
	/// </summary>
	public class ServerSideConfig : ConfigFile<SscSettings>
	{
		/// <summary>
		/// Upgrades the configuration file from the old format if required, then reads and returns the currently configured <see cref="SscSettings"/>
		/// </summary>
		/// <param name="json"></param>
		/// <param name="incompleteSettings"></param>
		/// <returns></returns>
		public override SscSettings ConvertJson(string json, out bool incompleteSettings)
		{
			var settings = FileTools.LoadConfigAndCheckForChanges<SscSettings>(json, out incompleteSettings);

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
			var defaults = new ServerSideConfig();

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

			File.WriteAllText("ServerSideConfigDescriptions.txt", sb.ToString());
		}
	}
}
