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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using MySql.Data.MySqlClient;

namespace TShockAPI.DB
{
	/// <summary>
	/// Represents the GroupManager, which is in charge of group management.
	/// </summary>
	public class GroupManager : IEnumerable<Group>
	{
		private IDbConnection database;
		public readonly List<Group> groups = new List<Group>();

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupManager"/> class with the specified database connection.
		/// </summary>
		/// <param name="db">The connection.</param>
		public GroupManager(IDbConnection db)
		{
			database = db;

			var table = new SqlTable("GroupList",
				new SqlColumn("GroupName", MySqlDbType.VarChar, 32) { Primary = true },
				new SqlColumn("Parent", MySqlDbType.VarChar, 32),
				new SqlColumn("Commands", MySqlDbType.Text),
				new SqlColumn("ChatColor", MySqlDbType.Text),
				new SqlColumn("Prefix", MySqlDbType.Text),
				new SqlColumn("Suffix", MySqlDbType.Text)
			);
			var creator = new SqlTableCreator(db,
				db.GetSqlType() == SqlType.Sqlite
					? (IQueryBuilder)new SqliteQueryCreator()
					: new MysqlQueryCreator());
			if (creator.EnsureTableStructure(table))
			{
				// Add default groups if they don't exist
				AddDefaultGroup("guest", "",
					string.Join(",",
						Permissions.canbuild,
						Permissions.canregister,
						Permissions.canlogin,
						Permissions.canpartychat,
						Permissions.cantalkinthird,
						Permissions.canchat,
						Permissions.synclocalarea,
						Permissions.sendemoji));

				AddDefaultGroup("default", "guest",
					string.Join(",",
						Permissions.warp,
						Permissions.canchangepassword,
						Permissions.canlogout,
						Permissions.summonboss,
						Permissions.whisper,
						Permissions.wormhole,
						Permissions.canpaint,
						Permissions.pylon,
						Permissions.tppotion,
						Permissions.magicconch,
						Permissions.demonconch));

				AddDefaultGroup("vip", "default",
					string.Join(",",
						Permissions.reservedslot,
						Permissions.renamenpc,
						Permissions.startinvasion,
						Permissions.summonboss,
						Permissions.whisper,
						Permissions.wormhole));

				AddDefaultGroup("newadmin", "vip",
					string.Join(",",
						Permissions.kick,
						Permissions.editspawn,
						Permissions.reservedslot,
						Permissions.annoy,
						Permissions.checkaccountinfo,
						Permissions.getpos,
						Permissions.mute,
						Permissions.rod,
						Permissions.savessc,
						Permissions.seeids,
						"tshock.world.time.*"));

				AddDefaultGroup("admin", "newadmin",
					string.Join(",",
						Permissions.ban,
						Permissions.whitelist,
						Permissions.spawnboss,
						Permissions.spawnmob,
						Permissions.managewarp,
						Permissions.time,
						Permissions.tp,
						Permissions.slap,
						Permissions.kill,
						Permissions.logs,
						Permissions.immunetokick,
						Permissions.tpothers,
						Permissions.advaccountinfo,
						Permissions.broadcast,
						Permissions.home,
						Permissions.tpallothers,
						Permissions.tpallow,
						Permissions.tpnpc,
						Permissions.tppos,
						Permissions.tpsilent,
						Permissions.userinfo,
						Permissions.spawn));

				AddDefaultGroup("trustedadmin", "admin",
					string.Join(",",
						Permissions.maintenance,
						"tshock.cfg.*",
						"tshock.world.*",
						Permissions.butcher,
						Permissions.item,
						Permissions.give,
						Permissions.heal,
						Permissions.immunetoban,
						Permissions.usebanneditem,
						Permissions.allowclientsideworldedit,
						Permissions.buff,
						Permissions.buffplayer,
						Permissions.clear,
						Permissions.clearangler,
						Permissions.godmode,
						Permissions.godmodeother,
						Permissions.ignoredamagecap,
						Permissions.ignorehp,
						Permissions.ignorekilltiledetection,
						Permissions.ignoreliquidsetdetection,
						Permissions.ignoremp,
						Permissions.ignorepaintdetection,
						Permissions.ignoreplacetiledetection,
						Permissions.ignoreprojectiledetection,
						Permissions.ignorestackhackdetection,
						Permissions.invade,
						Permissions.startdd2,
						Permissions.uploaddata,
						Permissions.uploadothersdata,
						Permissions.spawnpets,
						Permissions.journey_timefreeze,
						Permissions.journey_timeset,
						Permissions.journey_timespeed,
						Permissions.journey_godmode,
						Permissions.journey_windstrength,
						Permissions.journey_windfreeze,
						Permissions.journey_rainstrength,
						Permissions.journey_rainfreeze,
						Permissions.journey_placementrange,
						Permissions.journey_setdifficulty,
						Permissions.journey_biomespreadfreeze,
						Permissions.journey_setspawnrate,
						Permissions.journey_contributeresearch));

				AddDefaultGroup("owner", "trustedadmin",
					string.Join(",",
						Permissions.su,
						Permissions.allowdroppingbanneditems,
						Permissions.antibuild,
						Permissions.canusebannedprojectiles,
						Permissions.canusebannedtiles,
						Permissions.managegroup,
						Permissions.manageitem,
						Permissions.manageprojectile,
						Permissions.manageregion,
						Permissions.managetile,
						Permissions.maxspawns,
						Permissions.serverinfo,
						Permissions.settempgroup,
						Permissions.spawnrate,
						Permissions.tpoverride,
						Permissions.createdumps));
			}

			// Load Permissions from the DB
			LoadPermisions();

			Group.DefaultGroup = GetGroupByName(TShock.Config.Settings.DefaultGuestGroupName);
		}

		internal void EnsureCoreGroupsPresent()
		{
			if (!GroupExists(TShock.Config.Settings.DefaultGuestGroupName))
			{
				TShock.Log.ConsoleError("The guest group could not be found. This may indicate a typo in the configuration file, or that the group was renamed or deleted.");
				throw new Exception("The guest group could not be found.");
			}

			if(!GroupExists(TShock.Config.Settings.DefaultRegistrationGroupName))
            {
				TShock.Log.ConsoleError("The default usergroup could not be found. This may indicate a typo in the configuration file, or that the group was renamed or deleted.");
				throw new Exception("The default usergroup could not be found.");
            }
		}

		private void AddDefaultGroup(string name, string parent, string permissions)
		{
			if (!GroupExists(name))
				AddGroup(name, parent, permissions, Group.defaultChatColor);
		}

		/// <summary>
		/// Determines whether the given group exists.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns><c>true</c> if it does; otherwise, <c>false</c>.</returns>
		public bool GroupExists(string group)
		{
			if (group == "superadmin")
				return true;

			return groups.Any(g => g.Name.Equals(group));
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<Group> GetEnumerator()
		{
			return groups.GetEnumerator();
		}

		/// <summary>
		/// Gets the group matching the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The group.</returns>
		public Group GetGroupByName(string name)
		{
			var ret = groups.Where(g => g.Name == name);
			return 1 == ret.Count() ? ret.ElementAt(0) : null;
		}

		/// <summary>
		/// Adds group with name and permissions if it does not exist.
		/// </summary>
		/// <param name="name">name of group</param>
		/// <param name="parentname">parent of group</param>
		/// <param name="permissions">permissions</param>
		/// <param name="chatcolor">chatcolor</param>
		public void AddGroup(String name, string parentname, String permissions, String chatcolor)
		{
			if (GroupExists(name))
			{
				throw new GroupExistsException(name);
			}

			var group = new Group(name, null, chatcolor);
			group.Permissions = permissions;
			if (!string.IsNullOrWhiteSpace(parentname))
			{
				var parent = groups.FirstOrDefault(gp => gp.Name == parentname);
				if (parent == null || name == parentname)
				{
					var error = "Invalid parent {0} for group {1}".SFormat(parentname, group.Name);
					TShock.Log.ConsoleError(error);
					throw new GroupManagerException(error);
				}
				group.Parent = parent;
			}

			string query = (TShock.Config.Settings.StorageType.ToLower() == "sqlite")
				? "INSERT OR IGNORE INTO GroupList (GroupName, Parent, Commands, ChatColor) VALUES (@0, @1, @2, @3);"
				: "INSERT IGNORE INTO GroupList SET GroupName=@0, Parent=@1, Commands=@2, ChatColor=@3";
			if (database.Query(query, name, parentname, permissions, chatcolor) == 1)
			{
				groups.Add(group);
			}
			else
				throw new GroupManagerException("Failed to add group '" + name + ".'");
		}

		/// <summary>
		/// Updates a group including permissions
		/// </summary>
		/// <param name="name">name of the group to update</param>
		/// <param name="parentname">parent of group</param>
		/// <param name="permissions">permissions</param>
		/// <param name="chatcolor">chatcolor</param>
		/// <param name="suffix">suffix</param>
		/// <param name="prefix">prefix</param> //why is suffix before prefix?!
		public void UpdateGroup(string name, string parentname, string permissions, string chatcolor, string suffix, string prefix)
		{
			Group group = GetGroupByName(name);
			if (group == null)
				throw new GroupNotExistException(name);

			Group parent = null;
			if (!string.IsNullOrWhiteSpace(parentname))
			{
				parent = GetGroupByName(parentname);
				if (parent == null || parent == group)
					throw new GroupManagerException("Invalid parent \"{0}\" for group \"{1}\".".SFormat(parentname, name));

				// Check if the new parent would cause loops.
				List<Group> groupChain = new List<Group> { group, parent };
				Group checkingGroup = parent.Parent;
				while (checkingGroup != null)
				{
					if (groupChain.Contains(checkingGroup))
						throw new GroupManagerException(
							string.Format("Invalid parent \"{0}\" for group \"{1}\" would cause loops in the parent chain.", parentname, name));

					groupChain.Add(checkingGroup);
					checkingGroup = checkingGroup.Parent;
				}
			}

			// Ensure any group validation is also persisted to the DB.
			var newGroup = new Group(name, parent, chatcolor, permissions);
			newGroup.Prefix = prefix;
			newGroup.Suffix = suffix;
			string query = "UPDATE GroupList SET Parent=@0, Commands=@1, ChatColor=@2, Suffix=@3, Prefix=@4 WHERE GroupName=@5";
			if (database.Query(query, parentname, newGroup.Permissions, newGroup.ChatColor, suffix, prefix, name) != 1)
				throw new GroupManagerException(string.Format("Failed to update group \"{0}\".", name));

			group.ChatColor = chatcolor;
			group.Permissions = permissions;
			group.Parent = parent;
			group.Prefix = prefix;
			group.Suffix = suffix;
		}

		/// <summary>
		/// Renames the specified group.
		/// </summary>
		/// <param name="name">The group's name.</param>
		/// <param name="newName">The new name.</param>
		/// <returns>The result from the operation to be sent back to the user.</returns>
		public String RenameGroup(string name, string newName)
		{
			if (!GroupExists(name))
			{
				throw new GroupNotExistException(name);
			}

			if (GroupExists(newName))
			{
				throw new GroupExistsException(newName);
			}

			using (var db = database.CloneEx())
			{
				db.Open();
				using (var transaction = db.BeginTransaction())
				{
					try
					{
						using (var command = db.CreateCommand())
						{
							command.CommandText = "UPDATE GroupList SET GroupName = @0 WHERE GroupName = @1";
							command.AddParameter("@0", newName);
							command.AddParameter("@1", name);
							command.ExecuteNonQuery();
						}

						var oldGroup = GetGroupByName(name);
						var newGroup = new Group(newName, oldGroup.Parent, oldGroup.ChatColor, oldGroup.Permissions)
						{
							Prefix = oldGroup.Prefix,
							Suffix = oldGroup.Suffix
						};
						groups.Remove(oldGroup);
						groups.Add(newGroup);

						// We need to check if the old group has been referenced as a parent and update those references accordingly 
						using (var command = db.CreateCommand())
						{
							command.CommandText = "UPDATE GroupList SET Parent = @0 WHERE Parent = @1";
							command.AddParameter("@0", newName);
							command.AddParameter("@1", name);
							command.ExecuteNonQuery();
						}
						foreach (var group in groups.Where(g => g.Parent != null && g.Parent == oldGroup))
						{
							group.Parent = newGroup;
						}

						// Read the config file to prevent the possible loss of any unsaved changes
						TShock.Config.Read(FileTools.ConfigPath, out bool writeConfig);
						if (TShock.Config.Settings.DefaultGuestGroupName == oldGroup.Name)
						{
							TShock.Config.Settings.DefaultGuestGroupName = newGroup.Name;
							Group.DefaultGroup = newGroup;
						}
						if (TShock.Config.Settings.DefaultRegistrationGroupName == oldGroup.Name)
						{
							TShock.Config.Settings.DefaultRegistrationGroupName = newGroup.Name;
						}
						if (writeConfig)
						{
							TShock.Config.Write(FileTools.ConfigPath);
						}

						// We also need to check if any users belong to the old group and automatically apply changes
						using (var command = db.CreateCommand())
						{
							command.CommandText = "UPDATE Users SET Usergroup = @0 WHERE Usergroup = @1";
							command.AddParameter("@0", newName);
							command.AddParameter("@1", name);
							command.ExecuteNonQuery();
						}
						foreach (var player in TShock.Players.Where(p => p?.Group == oldGroup))
						{
							player.Group = newGroup;
						}

						transaction.Commit();
						return $"Group \"{name}\" has been renamed to \"{newName}\".";
					}
					catch (Exception ex)
					{
						TShock.Log.Error($"An exception has occured during database transaction: {ex.Message}");
						try
						{
							transaction.Rollback();
						}
						catch (Exception rollbackEx)
						{
							TShock.Log.Error($"An exception has occured during database rollback: {rollbackEx.Message}");
						}
					}
				}
			}

			throw new GroupManagerException($"Failed to rename group \"{name}\".");
		}

		/// <summary>
		/// Deletes the specified group.
		/// </summary>
		/// <param name="name">The group's name.</param>
		/// <param name="exceptions">Whether exceptions will be thrown in case something goes wrong.</param>
		/// <returns>The result from the operation to be sent back to the user.</returns>
		public String DeleteGroup(String name, bool exceptions = false)
		{
			if (!GroupExists(name))
			{
				if (exceptions)
					throw new GroupNotExistException(name);
				return "Error: Group doesn't exist.";
			}

			if (name == Group.DefaultGroup.Name)
			{
				if (exceptions)
					throw new GroupManagerException("Unable to remove default guest group.");
				return "Error: Unable to remove the default guest group.";
			}

			if (database.Query("DELETE FROM GroupList WHERE GroupName=@0", name) == 1)
			{
				groups.Remove(TShock.Groups.GetGroupByName(name));
				return "Group " + name + " has been deleted successfully.";
			}

			if (exceptions)
				throw new GroupManagerException("Failed to delete group '" + name + ".'");
			return "Failed to delete group '" + name + ".'";
		}

		/// <summary>
		/// Enumerates the given permission list and adds permissions for the specified group accordingly.
		/// </summary>
		/// <param name="name">The group name.</param>
		/// <param name="permissions">The permission list.</param>
		/// <returns>The result from the operation to be sent back to the user.</returns>
		public String AddPermissions(String name, List<String> permissions)
		{
			if (!GroupExists(name))
				return "Error: Group doesn't exist.";

			var group = TShock.Groups.GetGroupByName(name);
			var oldperms = group.Permissions; // Store old permissions in case of error
			permissions.ForEach(p => group.AddPermission(p));

			if (database.Query("UPDATE GroupList SET Commands=@0 WHERE GroupName=@1", group.Permissions, name) == 1)
				return "Group " + name + " has been modified successfully.";

			// Restore old permissions so DB and internal object are in a consistent state
			group.Permissions = oldperms;
			return "";
		}

		/// <summary>
		/// Enumerates the given permission list and removes valid permissions for the specified group accordingly.
		/// </summary>
		/// <param name="name">The group name.</param>
		/// <param name="permissions">The permission list.</param>
		/// <returns>The result from the operation to be sent back to the user.</returns>
		public String DeletePermissions(String name, List<String> permissions)
		{
			if (!GroupExists(name))
				return "Error: Group doesn't exist.";

			var group = TShock.Groups.GetGroupByName(name);
			var oldperms = group.Permissions; // Store old permissions in case of error
			permissions.ForEach(p => group.RemovePermission(p));

			if (database.Query("UPDATE GroupList SET Commands=@0 WHERE GroupName=@1", group.Permissions, name) == 1)
				return "Group " + name + " has been modified successfully.";

			// Restore old permissions so DB and internal object are in a consistent state
			group.Permissions = oldperms;
			return "";
		}

		/// <summary>
		/// Enumerates the group list and loads permissions for each group appropriately.
		/// </summary>
		public void LoadPermisions()
		{
			try
			{
				List<Group> newGroups = new List<Group>(groups.Count);
				Dictionary<string, string> newGroupParents = new Dictionary<string, string>(groups.Count);
				using (var reader = database.QueryReader("SELECT * FROM GroupList"))
				{
					while (reader.Read())
					{
						string groupName = reader.Get<string>("GroupName");
						if (groupName == "superadmin")
						{
							TShock.Log.ConsoleInfo("WARNING: Group \"superadmin\" is defined in the database even though it's a reserved group name.");
							continue;
						}

						newGroups.Add(new Group(groupName, null, reader.Get<string>("ChatColor"), reader.Get<string>("Commands"))
						{
							Prefix = reader.Get<string>("Prefix"),
							Suffix = reader.Get<string>("Suffix"),
						});

						try
						{
							newGroupParents.Add(groupName, reader.Get<string>("Parent"));
						}
						catch (ArgumentException)
						{
							// Just in case somebody messed with the unique primary key.
							TShock.Log.ConsoleError("ERROR: Group name \"{0}\" occurs more than once. Keeping current group settings.");
							return;
						}
					}
				}

				try
				{
					// Get rid of deleted groups.
					for (int i = 0; i < groups.Count; i++)
						if (newGroups.All(g => g.Name != groups[i].Name))
							groups.RemoveAt(i--);

					// Apply changed group settings while keeping the current instances and add new groups.
					foreach (Group newGroup in newGroups)
					{
						Group currentGroup = groups.FirstOrDefault(g => g.Name == newGroup.Name);
						if (currentGroup != null)
							newGroup.AssignTo(currentGroup);
						else
							groups.Add(newGroup);
					}

					// Resolve parent groups.
					Debug.Assert(newGroups.Count == newGroupParents.Count);
					for (int i = 0; i < groups.Count; i++)
					{
						Group group = groups[i];
						string parentGroupName;
						if (!newGroupParents.TryGetValue(group.Name, out parentGroupName) || string.IsNullOrEmpty(parentGroupName))
							continue;

						group.Parent = groups.FirstOrDefault(g => g.Name == parentGroupName);
						if (group.Parent == null)
						{
							TShock.Log.ConsoleError(
								"ERROR: Group \"{0}\" is referencing non existent parent group \"{1}\", parent reference was removed.",
								group.Name, parentGroupName);
						}
						else
						{
							if (group.Parent == group)
								TShock.Log.ConsoleInfo(
									"WARNING: Group \"{0}\" is referencing itself as parent group, parent reference was removed.", group.Name);

							List<Group> groupChain = new List<Group> { group };
							Group checkingGroup = group;
							while (checkingGroup.Parent != null)
							{
								if (groupChain.Contains(checkingGroup.Parent))
								{
									TShock.Log.ConsoleError(
										"ERROR: Group \"{0}\" is referencing parent group \"{1}\" which is already part of the parent chain. Parent reference removed.",
										checkingGroup.Name, checkingGroup.Parent.Name);

									checkingGroup.Parent = null;
									break;
								}
								groupChain.Add(checkingGroup);
								checkingGroup = checkingGroup.Parent;
							}
						}
					}
				}
				finally
				{
					if (!groups.Any(g => g is SuperAdminGroup))
						groups.Add(new SuperAdminGroup());
				}
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError("Error on reloading groups: " + ex);
			}
		}
	}

	/// <summary>
	/// Represents the base GroupManager exception.
	/// </summary>
	[Serializable]
	public class GroupManagerException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GroupManagerException"/> with the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		public GroupManagerException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupManagerException"/> with the specified message and inner exception.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner exception.</param>
		public GroupManagerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// Represents the GroupExists exception.
	/// This exception is thrown whenever an attempt to add an existing group into the database is made.
	/// </summary>
	[Serializable]
	public class GroupExistsException : GroupManagerException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GroupExistsException"/> with the specified group name.
		/// </summary>
		/// <param name="name">The group name.</param>
		public GroupExistsException(string name)
			: base("Group '" + name + "' already exists")
		{
		}
	}

	/// <summary>
	/// Represents the GroupNotExist exception.
	/// This exception is thrown whenever we try to access a group that does not exist.
	/// </summary>
	[Serializable]
	public class GroupNotExistException : GroupManagerException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GroupNotExistException"/> with the specified group name.
		/// </summary>
		/// <param name="name">The group name.</param>
		public GroupNotExistException(string name)
			: base("Group '" + name + "' does not exist")
		{
		}
	}
}
