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
using System.Linq;
using System.Collections.Generic;

namespace TShockAPI
{
	/// <summary>
	/// A class used to group multiple users' permissions and settings.
	/// </summary>
	public class Group
	{
		// NOTE: Using a const still suffers from needing to recompile to change the default
		// ideally we would use a static but this means it can't be used for the default parameter :(
		/// <summary>
		/// Default chat color.
		/// </summary>
		public const string defaultChatColor = "255,255,255";

		/// <summary>
		/// List of permissions available to the group.
		/// </summary>
		public readonly List<string> permissions = new List<string>();

		/// <summary>
		/// List of permissions that the group is explicitly barred from.
		/// </summary>
		public readonly List<string> negatedpermissions = new List<string>();

		/// <summary>
		/// The group's name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The group that this group inherits permissions from.
		/// </summary>
		public Group Parent { get; set; }

		/// <summary>
		/// The chat prefix for this group.
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// The chat suffix for this group.
		/// </summary>
		public string Suffix { get; set; }

		/// <summary>
		/// The chat color of the group.
		/// Returns "255,255,255", sets "255,255,255"
		/// </summary>
		public string ChatColor
		{
			get { return string.Format("{0},{1},{2}", R.ToString("D3"), G.ToString("D3"), B.ToString("D3")); }
			set
			{
				if (null != value)
				{
					string[] parts = value.Split(',');
					if (3 == parts.Length)
					{
						byte r, g, b;
						if (byte.TryParse(parts[0], out r) && byte.TryParse(parts[1], out g) && byte.TryParse(parts[2], out b))
						{
							R = r;
							G = g;
							B = b;
							return;
						}
					}
				}
			}
		}

		/// <summary>
		/// The permissions of the user in string form.
		/// </summary>
		public string Permissions
		{
			get
			{
				List<string> all = new List<string>(permissions);
				negatedpermissions.ForEach(p => all.Add("!" + p));
				return string.Join(",", all);
			}
			set
			{
				permissions.Clear();
				negatedpermissions.Clear();
				if (null != value)
					value.Split(',').ForEach(p => AddPermission(p.Trim()));
			}
		}

		/// <summary>
		/// The permissions of this group and all that it inherits from.
		/// </summary>
		public virtual List<string> TotalPermissions
		{
			get
			{
				var cur = this;
				var traversed = new List<Group>();
				HashSet<string> all = new HashSet<string>();
				while (cur != null)
				{
					foreach (var perm in cur.permissions)
					{
						all.Add(perm);
					}

					foreach (var perm in cur.negatedpermissions)
					{
						all.Remove(perm);
					}

					if (traversed.Contains(cur))
					{
						throw new Exception("Infinite group parenting ({0})".SFormat(cur.Name));
					}
					traversed.Add(cur);
					cur = cur.Parent;
				}
				return all.ToList();
			}
		}

		/// <summary>
		/// The group's chat color red byte.
		/// </summary>
		public byte R = 255;
		/// <summary>
		/// The group's chat color green byte.
		/// </summary>
		public byte G = 255;
		/// <summary>
		/// The group's chat color blue byte.
		/// </summary>
		public byte B = 255;

		/// <summary>
		/// The default group attributed to unregistered users.
		/// </summary>
		public static Group DefaultGroup = null;

		/// <summary>
		/// Initializes a new instance of the group class.
		/// </summary>
		/// <param name="groupname">The name of the group.</param>
		/// <param name="parentgroup">The parent group, if any.</param>
		/// <param name="chatcolor">The chat color, in "RRR,GGG,BBB" format.</param>
		/// <param name="permissions">The list of permissions associated with this group, separated by commas.</param>
		public Group(string groupname, Group parentgroup = null, string chatcolor = "255,255,255", string permissions = null)
		{
			Name = groupname;
			Parent = parentgroup;
			ChatColor = chatcolor;
			Permissions = permissions;
		}

		/// <summary>
		/// Checks to see if a group has a specified permission.
		/// </summary>
		/// <param name="permission">The permission to check.</param>
		/// <returns>True if the group has that permission.</returns>
		public virtual bool HasPermission(string permission)
		{
			bool negated = false;
			if (String.IsNullOrEmpty(permission) || (RealHasPermission(permission, ref negated) && !negated))
			{
				return true;
			}

			if (negated)
				return false;

			string[] nodes = permission.Split('.');
			for (int i = nodes.Length - 1; i >= 0; i--)
			{
				nodes[i] = "*";
				if (RealHasPermission(String.Join(".", nodes, 0, i + 1), ref negated))
				{
					return !negated;
				}
			}
			return false;
		}
		private bool RealHasPermission(string permission, ref bool negated)
		{
			negated = false;
			if (string.IsNullOrEmpty(permission))
				return true;

			var cur = this;
			var traversed = new List<Group>();
			while (cur != null)
			{
				if (cur.negatedpermissions.Contains(permission))
				{
					negated = true;
					return false;
				}
				if (cur.permissions.Contains(permission))
					return true;
				if (traversed.Contains(cur))
				{
					throw new InvalidOperationException("Infinite group parenting ({0})".SFormat(cur.Name));
				}
				traversed.Add(cur);
				cur = cur.Parent;
			}
			return false;
		}

		/// <summary>
		/// Adds a permission to the list of negated permissions.
		/// </summary>
		/// <param name="permission">The permission to negate.</param>
		public void NegatePermission(string permission)
		{
			// Avoid duplicates
			if (!negatedpermissions.Contains(permission))
			{
				negatedpermissions.Add(permission);
				permissions.Remove(permission); // Ensure we don't have conflicting definitions for a permissions
			}
		}

		/// <summary>
		/// Adds a permission to the list of permissions.
		/// </summary>
		/// <param name="permission">The permission to add.</param>
		public void AddPermission(string permission)
		{
			if (permission.StartsWith("!"))
			{
				NegatePermission(permission.Substring(1));
				return;
			}
			// Avoid duplicates
			if (!permissions.Contains(permission))
			{
				permissions.Add(permission);
				negatedpermissions.Remove(permission); // Ensure we don't have conflicting definitions for a permissions
			}
		}

		/// <summary>
		/// Clears the permission list and sets it to the list provided, 
		/// will parse "!permssion" and add it to the negated permissions.
		/// </summary>
		/// <param name="permission">The new list of permissions to associate with the group.</param>
		public void SetPermission(List<string> permission)
		{
			permissions.Clear();
			negatedpermissions.Clear();
			permission.ForEach(p => AddPermission(p));
		}

		/// <summary>
		/// Will remove a permission from the respective list,
		/// where "!permission" will remove a negated permission.
		/// </summary>
		/// <param name="permission"></param>
		public void RemovePermission(string permission)
		{
			if (permission.StartsWith("!"))
			{
				negatedpermissions.Remove(permission.Substring(1));
				return;
			}
			permissions.Remove(permission);
		}

		/// <summary>
		/// Assigns all fields of this instance to another.
		/// </summary>
		/// <param name="otherGroup">The other instance.</param>
		public void AssignTo(Group otherGroup)
		{
			otherGroup.Name = Name;
			otherGroup.Parent = Parent;
			otherGroup.Prefix = Prefix;
			otherGroup.Suffix = Suffix;
			otherGroup.R = R;
			otherGroup.G = G;
			otherGroup.B = B;
			otherGroup.Permissions = Permissions;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}

	/// <summary>
	/// This class is the SuperAdminGroup, which has access to everything.
	/// </summary>
	public class SuperAdminGroup : Group
	{
		/// <summary>
		/// The superadmin class has every permission, represented by '*'.
		/// </summary>
		public override List<string> TotalPermissions
		{
			get { return new List<string> { "*" }; }
		}

		/// <summary>
		/// Initializes a new instance of the SuperAdminGroup class with the configured parameters.
		/// Those can be changed in the config file.
		/// </summary>
		public SuperAdminGroup()
			: base("superadmin")
		{
			R = (byte)TShock.Config.SuperAdminChatRGB[0];
			G = (byte)TShock.Config.SuperAdminChatRGB[1];
			B = (byte)TShock.Config.SuperAdminChatRGB[2];
			Prefix = TShock.Config.SuperAdminChatPrefix;
			Suffix = TShock.Config.SuperAdminChatSuffix;
		}

		/// <summary>
		/// Override to allow access to everything.
		/// </summary>
		/// <param name="permission">The permission</param>
		/// <returns>True</returns>
		public override bool HasPermission(string permission)
		{
			return true;
		}
	}
}
