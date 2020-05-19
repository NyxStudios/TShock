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
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using BCrypt.Net;
using System.Security.Cryptography;

namespace TShockAPI.DB
{
	/// <summary>UserAccountManager - Methods for dealing with database user accounts and other related functionality within TShock.</summary>
	public class UserAccountManager
	{
		/// <summary>database - The database object to use for connections.</summary>
		private IDbConnection _database;

		/// <summary>Creates a UserAccountManager object. During instantiation, this method will verify the table structure against the format below.</summary>
		/// <param name="db">The database to connect to.</param>
		/// <returns>A UserAccountManager object.</returns>
		public UserAccountManager(IDbConnection db)
		{
			_database = db;

			var table = new SqlTable("Users",
				new SqlColumn("ID", MySqlDbType.Int32) {Primary = true, AutoIncrement = true},
				new SqlColumn("Username", MySqlDbType.VarChar, 32) {Unique = true},
				new SqlColumn("Password", MySqlDbType.VarChar, 128),
				new SqlColumn("UUID", MySqlDbType.VarChar, 128),
				new SqlColumn("Usergroup", MySqlDbType.Text),
				new SqlColumn("Registered", MySqlDbType.Text),
				new SqlColumn("LastAccessed", MySqlDbType.Text),
				new SqlColumn("KnownIPs", MySqlDbType.Text)
				);
			var creator = new SqlTableCreator(db,
				db.GetSqlType() == SqlType.Sqlite
				? (IQueryBuilder) new SqliteQueryCreator()
				: new MysqlQueryCreator());
			creator.EnsureTableStructure(table);
		}

		/// <summary>
		/// Adds the given user account to the database
		/// </summary>
		/// <param name="account">The user account to be added</param>
		public void AddUserAccount(UserAccount account)
		{
			if (!TShock.Groups.GroupExists(account.Group))
				throw new GroupNotExistsException(account.Group);

			int ret;
			try
			{
				ret = _database.Query("INSERT INTO Users (Username, Password, UUID, UserGroup, Registered) VALUES (@0, @1, @2, @3, @4);", account.Name,
					account.Password, account.UUID, account.Group, DateTime.UtcNow.ToString("s"));
			}
			catch (Exception ex)
			{
				// Detect duplicate user using a regexp as Sqlite doesn't have well structured exceptions
				if (Regex.IsMatch(ex.Message, "Username.*not unique|UNIQUE constraint failed: Users\\.Username"))
					throw new UserAccountExistsException(account.Name);
				throw new UserAccountManagerException("AddUser SQL returned an error (" + ex.Message + ")", ex);
			}

			if (1 > ret)
				throw new UserAccountExistsException(account.Name);

			Hooks.AccountHooks.OnAccountCreate(account);
		}

		/// <summary>
		/// Removes all user accounts from the database whose usernames match the given user account
		/// </summary>
		/// <param name="account">The user account</param>
		public void RemoveUserAccount(UserAccount account)
		{
			try
			{
				// Logout any player logged in as the account to be removed
				TShock.Players.Where(p => p?.IsLoggedIn == true && p.Account.Name == account.Name).ForEach(p => p.Logout());

				UserAccount tempuser = GetUserAccount(account);
				int affected = _database.Query("DELETE FROM Users WHERE Username=@0", account.Name);

				if (affected < 1)
					throw new UserAccountNotExistException(account.Name);

				Hooks.AccountHooks.OnAccountDelete(tempuser);

			}
			catch (Exception ex)
			{
				throw new UserAccountManagerException("RemoveUser SQL returned an error", ex);
			}
		}

		/// <summary>
		/// Sets the Hashed Password for a given username
		/// </summary>
		/// <param name="account">The user account</param>
		/// <param name="password">The user account password to be set</param>
		public void SetUserAccountPassword(UserAccount account, string password)
		{
			try
			{
				account.CreateBCryptHash(password);

				if (
					_database.Query("UPDATE Users SET Password = @0 WHERE Username = @1;", account.Password,
						account.Name) == 0)
					throw new UserAccountNotExistException(account.Name);
			}
			catch (Exception ex)
			{
				throw new UserAccountManagerException("SetUserPassword SQL returned an error", ex);
			}
		}

		/// <summary>
		/// Sets the UUID for a given username
		/// </summary>
		/// <param name="account">The user account</param>
		/// <param name="uuid">The user account uuid to be set</param>
		public void SetUserAccountUUID(UserAccount account, string uuid)
		{
			try
			{
				if (
					_database.Query("UPDATE Users SET UUID = @0 WHERE Username = @1;", uuid,
								   account.Name) == 0)
					throw new UserAccountNotExistException(account.Name);
			}
			catch (Exception ex)
			{
				throw new UserAccountManagerException("SetUserUUID SQL returned an error", ex);
			}
		}

		/// <summary>
		/// Sets the group for a given username
		/// </summary>
		/// <param name="account">The user account</param>
		/// <param name="group">The user account group to be set</param>
		public void SetUserGroup(UserAccount account, string group)
		{
			Group grp = TShock.Groups.GetGroupByName(group);
			if (null == grp)
				throw new GroupNotExistsException(group);

			if (_database.Query("UPDATE Users SET UserGroup = @0 WHERE Username = @1;", group, account.Name) == 0)
				throw new UserAccountNotExistException(account.Name);

			try
			{
				// Update player group reference for any logged in player
				foreach (var player in TShock.Players.Where(p => p != null && p.Account != null && p.Account.Name == account.Name))
				{
					player.Group = grp;
				}
			}
			catch (Exception ex)
			{
				throw new UserAccountManagerException("SetUserGroup SQL returned an error", ex);
			}
		}

		/// <summary>Updates the last accessed time for a database user account to the current time.</summary>
		/// <param name="account">The user account object to modify.</param>
		public void UpdateLogin(UserAccount account)
		{
			try
			{
				if (_database.Query("UPDATE Users SET LastAccessed = @0, KnownIps = @1 WHERE Username = @2;", DateTime.UtcNow.ToString("s"), account.KnownIps, account.Name) == 0)
					throw new UserAccountNotExistException(account.Name);
			}
			catch (Exception ex)
			{
				throw new UserAccountManagerException("UpdateLogin SQL returned an error", ex);
			}
		}

		/// <summary>Gets the database ID of a given user account object from the database.</summary>
		/// <param name="username">The username of the user account to query for.</param>
		/// <returns>The user account ID</returns>
		public int GetUserAccountID(string username)
		{
			try
			{
				using (var reader = _database.QueryReader("SELECT * FROM Users WHERE Username=@0", username))
				{
					if (reader.Read())
					{
						return reader.Get<int>("ID");
					}
				}
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError("FetchHashedPasswordAndGroup SQL returned an error: " + ex);
			}
			return -1;
		}

		/// <summary>Gets a user account object by name.</summary>
		/// <param name="name">The user's name.</param>
		/// <returns>The user account object returned from the search.</returns>
		public UserAccount GetUserAccountByName(string name)
		{
			try
			{
				return GetUserAccount(new UserAccount {Name = name});
			}
			catch (UserAccountManagerException)
			{
				return null;
			}
		}

		/// <summary>Gets a user account object by their user account ID.</summary>
		/// <param name="id">The user's ID.</param>
		/// <returns>The user account object returned from the search.</returns>
		public UserAccount GetUserAccountByID(int id)
		{
			try
			{
				return GetUserAccount(new UserAccount {ID = id});
			}
			catch (UserAccountManagerException)
			{
				return null;
			}
		}

		/// <summary>Gets a user account object by a user account object.</summary>
		/// <param name="account">The user account object to search by.</param>
		/// <returns>The user object that is returned from the search.</returns>
		public UserAccount GetUserAccount(UserAccount account)
		{
			bool multiple = false;
			string query;
			string type;
			object arg;
			if (account.ID != 0)
			{
				query = "SELECT * FROM Users WHERE ID=@0";
				arg = account.ID;
				type = "id";
			}
			else
			{
				query = "SELECT * FROM Users WHERE Username=@0";
				arg = account.Name;
				type = "name";
			}

			try
			{
				using (var result = _database.QueryReader(query, arg))
				{
					if (result.Read())
					{
						account = LoadUserAccountFromResult(account, result);
						// Check for multiple matches
						if (!result.Read())
							return account;
						multiple = true;
					}
				}
			}
			catch (Exception ex)
			{
				throw new UserAccountManagerException("GetUser SQL returned an error (" + ex.Message + ")", ex);
			}
			if (multiple)
				throw new UserAccountManagerException(String.Format("Multiple user accounts found for {0} '{1}'", type, arg));

			throw new UserAccountNotExistException(account.Name);
		}

		/// <summary>Gets all the user accounts from the database.</summary>
		/// <returns>The user accounts from the database.</returns>
		public List<UserAccount> GetUserAccounts()
		{
			try
			{
				List<UserAccount> accounts = new List<UserAccount>();
				using (var reader = _database.QueryReader("SELECT * FROM Users"))
				{
					while (reader.Read())
					{
						accounts.Add(LoadUserAccountFromResult(new UserAccount(), reader));
					}
					return accounts;
				}
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
			}
			return null;
		}

		/// <summary>
		/// Gets all user accounts from the database with a username that starts with or contains <see cref="username"/>
		/// </summary>
		/// <param name="username">Rough username search. "n" will match "n", "na", "nam", "name", etc</param>
		/// <param name="notAtStart">If <see cref="username"/> is not the first part of the username. If true then "name" would match "name", "username", "wordsnamewords", etc</param>
		/// <returns>Matching users or null if exception is thrown</returns>
		public List<UserAccount> GetUserAccountsByName(string username, bool notAtStart = false)
		{
			try
			{
				List<UserAccount> accounts = new List<UserAccount>();
				string search = notAtStart ? string.Format("%{0}%", username) : string.Format("{0}%", username);
				using (var reader = _database.QueryReader("SELECT * FROM Users WHERE Username LIKE @0",
					search))
				{
					while (reader.Read())
					{
						accounts.Add(LoadUserAccountFromResult(new UserAccount(), reader));
					}
				}
				return accounts;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
			}
			return null;
		}

		/// <summary>Fills out the fields of a User account object with the results from a QueryResult object.</summary>
		/// <param name="account">The user account to add data to.</param>
		/// <param name="result">The QueryResult object to add data from.</param>
		/// <returns>The 'filled out' user object.</returns>
		private UserAccount LoadUserAccountFromResult(UserAccount account, QueryResult result)
		{
			account.ID = result.Get<int>("ID");
			account.Group = result.Get<string>("Usergroup");
			account.Password = result.Get<string>("Password");
			account.UUID = result.Get<string>("UUID");
			account.Name = result.Get<string>("Username");
			account.Registered = result.Get<string>("Registered");
			account.LastAccessed = result.Get<string>("LastAccessed");
			account.KnownIps = result.Get<string>("KnownIps");
			return account;
		}
	}

	/// <summary>A database user account.</summary>
	public class UserAccount : IEquatable<UserAccount>
	{
		/// <summary>The database ID of the user account.</summary>
		public int ID { get; set; }

		/// <summary>The user's name.</summary>
		public string Name { get; set; }

		/// <summary>The hashed password for the user account.</summary>
		public string Password { get; internal set; }

		/// <summary>The user's saved Univerally Unique Identifier token.</summary>
		public string UUID { get; set; }

		/// <summary>The group object that the user account is a part of.</summary>
		public string Group { get; set; }

		/// <summary>The unix epoch corresponding to the registration date of the user account.</summary>
		public string Registered { get; set; }

		/// <summary>The unix epoch corresponding to the last access date of the user account.</summary>
		public string LastAccessed { get; set; }

		/// <summary>A JSON serialized list of known IP addresses for a user account.</summary>
		public string KnownIps { get; set; }

		/// <summary>Constructor for the user account object, assuming you define everything yourself.</summary>
		/// <param name="name">The user's name.</param>
		/// <param name="pass">The user's password hash.</param>
		/// <param name="uuid">The user's UUID.</param>
		/// <param name="group">The user's group name.</param>
		/// <param name="registered">The unix epoch for the registration date.</param>
		/// <param name="last">The unix epoch for the last access date.</param>
		/// <param name="known">The known IPs for the user account, serialized as a JSON object</param>
		/// <returns>A completed user account object.</returns>
		public UserAccount(string name, string pass, string uuid, string group, string registered, string last, string known)
		{
			Name = name;
			Password = pass;
			UUID = uuid;
			Group = group;
			Registered = registered;
			LastAccessed = last;
			KnownIps = known;
		}

		/// <summary>Default constructor for a user account object; holds no data.</summary>
		/// <returns>A user account object.</returns>
		public UserAccount()
		{
			Name = "";
			Password = "";
			UUID = "";
			Group = "";
			Registered = "";
			LastAccessed = "";
			KnownIps = "";
		}

		/// <summary>
		/// Verifies if a password matches the one stored in the database.
		/// If the password is stored in an unsafe hashing algorithm, it will be converted to BCrypt.
		/// If the password is stored using BCrypt, it will be re-saved if the work factor in the config
		/// is greater than the existing work factor with the new work factor.
		/// </summary>
		/// <param name="password">The password to check against the user account object.</param>
		/// <returns>bool true, if the password matched, or false, if it didn't.</returns>
		public bool VerifyPassword(string password)
		{
			try
			{
				if (BCrypt.Net.BCrypt.Verify(password, Password))
				{
					// If necessary, perform an upgrade to the highest work factor.
					UpgradePasswordWorkFactor(password);
					return true;
				}
			}
			catch (SaltParseException)
			{
				if (String.Equals(HashPassword(password), Password, StringComparison.InvariantCultureIgnoreCase))
				{
					// Return true to keep blank passwords working but don't convert them to bcrypt.
					if (Password == "non-existant password") {
						return true;
					}
					// The password is not stored using BCrypt; upgrade it to BCrypt immediately
					UpgradePasswordToBCrypt(password);
					return true;
				}
				return false;
			}
			return false;
		}

		/// <summary>Upgrades a password to BCrypt, from an insecure hashing algorithm.</summary>
		/// <param name="password">The raw user account password (unhashed) to upgrade</param>
		protected void UpgradePasswordToBCrypt(string password)
		{
			// Save the old password, in the event that we have to revert changes.
			string oldpassword = Password;

			try
			{
				TShock.UserAccounts.SetUserAccountPassword(this, password);
			}
			catch (UserAccountManagerException e)
			{
				TShock.Log.ConsoleError(e.ToString());
				Password = oldpassword; // Revert changes
			}
		}

		/// <summary>Upgrades a password to the highest work factor available in the config.</summary>
		/// <param name="password">The raw user account password (unhashed) to upgrade</param>
		protected void UpgradePasswordWorkFactor(string password)
		{
			// If the destination work factor is not greater, we won't upgrade it or re-hash it
			int currentWorkFactor;
			try
			{
				currentWorkFactor = Int32.Parse((Password.Split('$')[2]));
			}
			catch (FormatException)
			{
				TShock.Log.ConsoleError("Warning: Not upgrading work factor because bcrypt hash in an invalid format.");
				return;
			}

			if (currentWorkFactor < TShock.Config.BCryptWorkFactor)
			{
				try
				{
					TShock.UserAccounts.SetUserAccountPassword(this, password);
				}
				catch (UserAccountManagerException e)
				{
					TShock.Log.ConsoleError(e.ToString());
				}
			}
		}

		/// <summary>Creates a BCrypt hash for a user account and stores it in this object.</summary>
		/// <param name="password">The plain text password to hash</param>
		public void CreateBCryptHash(string password)
		{
			if (password.Trim().Length < Math.Max(4, TShock.Config.MinimumPasswordLength))
			{
				throw new ArgumentOutOfRangeException("password", "Password must be > " + TShock.Config.MinimumPasswordLength + " characters.");
			}
			try
			{
				Password = BCrypt.Net.BCrypt.HashPassword(password.Trim(), TShock.Config.BCryptWorkFactor);
			}
			catch (ArgumentOutOfRangeException)
			{
				TShock.Log.ConsoleError("Invalid BCrypt work factor in config file! Creating new hash using default work factor.");
				Password = BCrypt.Net.BCrypt.HashPassword(password.Trim());
			}
		}

		/// <summary>Creates a BCrypt hash for a user account and stores it in this object.</summary>
		/// <param name="password">The plain text password to hash</param>
		/// <param name="workFactor">The work factor to use in generating the password hash</param>
		public void CreateBCryptHash(string password, int workFactor)
		{
			if (password.Trim().Length < Math.Max(4, TShock.Config.MinimumPasswordLength))
			{
				throw new ArgumentOutOfRangeException("password", "Password must be > " + TShock.Config.MinimumPasswordLength + " characters.");
			}
			Password = BCrypt.Net.BCrypt.HashPassword(password.Trim(), workFactor);
		}

		/// <summary>
		/// A dictionary of hashing algorithms and an implementation object.
		/// </summary>
		protected readonly Dictionary<string, Func<HashAlgorithm>> HashTypes = new Dictionary<string, Func<HashAlgorithm>>
			{
					{"sha512", () => new SHA512Managed()},
					{"sha256", () => new SHA256Managed()},
					// {"md5", () => new MD5Cng()},
					{"sha512-xp", () => SHA512.Create()},
					{"sha256-xp", () => SHA256.Create()},
					{"md5-xp", () => MD5.Create()},
			};

		/// <summary>
		/// Returns a hashed string for a given string based on the config file's hash algo
		/// </summary>
		/// <param name="bytes">bytes to hash</param>
		/// <returns>string hash</returns>
		protected string HashPassword(byte[] bytes)
		{
			if (bytes == null)
				throw new NullReferenceException("bytes");
			Func<HashAlgorithm> func;
			if (!HashTypes.TryGetValue(TShock.Config.HashAlgorithm.ToLower(), out func))
				throw new NotSupportedException("Hashing algorithm {0} is not supported".SFormat(TShock.Config.HashAlgorithm.ToLower()));

			using (var hash = func())
			{
				var ret = hash.ComputeHash(bytes);
				return ret.Aggregate("", (s, b) => s + b.ToString("X2"));
			}
		}

		/// <summary>
		/// Returns a hashed string for a given string based on the config file's hash algo
		/// </summary>
		/// <param name="password">string to hash</param>
		/// <returns>string hash</returns>
		protected string HashPassword(string password)
		{
			if (string.IsNullOrEmpty(password) && Password == "non-existant password")
				return "non-existant password";
			return HashPassword(Encoding.UTF8.GetBytes(password));
		}

		#region IEquatable

		/// <summary>Indicates whether the current <see cref="UserAccount"/> is equal to another <see cref="UserAccount"/>.</summary>
		/// <returns>true if the <see cref="UserAccount"/> is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An <see cref="UserAccount"/> to compare with this <see cref="UserAccount"/>.</param>
		public bool Equals(UserAccount other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return ID == other.ID && string.Equals(Name, other.Name);
		}

		/// <summary>Indicates whether the current <see cref="UserAccount"/> is equal to another object.</summary>
		/// <returns>true if the <see cref="UserAccount"/> is equal to the <paramref name="obj" /> parameter; otherwise, false.</returns>
		/// <param name="obj">An <see cref="object"/> to compare with this <see cref="UserAccount"/>.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((UserAccount)obj);
		}

		/// <summary>Serves as the hash function. </summary>
		/// <returns>A hash code for the current <see cref="UserAccount"/>.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (ID * 397) ^ (Name != null ? Name.GetHashCode() : 0);
			}
		}

		/// <summary>
		/// Compares equality of two <see cref="UserAccount"/> objects.
		/// </summary>
		/// <param name="left">Left hand of the comparison.</param>
		/// <param name="right">Right hand of the comparison.</param>
		/// <returns>true if the <see cref="UserAccount"/> objects are equal; otherwise, false.</returns>
		public static bool operator ==(UserAccount left, UserAccount right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// Compares equality of two <see cref="UserAccount"/> objects.
		/// </summary>
		/// <param name="left">Left hand of the comparison.</param>
		/// <param name="right">Right hand of the comparison.</param>
		/// <returns>true if the <see cref="UserAccount"/> objects aren't equal; otherwise, false.</returns>
		public static bool operator !=(UserAccount left, UserAccount right)
		{
			return !Equals(left, right);
		}

		#endregion

		/// <summary>
		/// Converts the UserAccount to it's string representation
		/// </summary>
		/// <returns>Returns the UserAccount string representation</returns>
		public override string ToString() => Name;
	}

	/// <summary>UserAccountManagerException - An exception generated by the user account manager.</summary>
	[Serializable]
	public class UserAccountManagerException : Exception
	{
		/// <summary>Creates a new UserAccountManagerException object.</summary>
		/// <param name="message">The message for the object.</param>
		/// <returns>A new UserAccountManagerException object.</returns>
		public UserAccountManagerException(string message)
			: base(message)
		{
		}

		/// <summary>Creates a new UserAccountManager Object with an internal exception.</summary>
		/// <param name="message">The message for the object.</param>
		/// <param name="inner">The inner exception for the object.</param>
		/// <returns>A new UserAccountManagerException with a defined inner exception.</returns>
		public UserAccountManagerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>A UserExistsException object, used when a user account already exists when attempting to create a new one.</summary>
	[Serializable]
	public class UserAccountExistsException : UserAccountManagerException
	{
		/// <summary>Creates a new UserAccountExistsException object.</summary>
		/// <param name="name">The name of the user account that already exists.</param>
		/// <returns>A UserAccountExistsException object with the user's name passed in the message.</returns>
		public UserAccountExistsException(string name)
			: base("User account '" + name + "' already exists")
		{
		}
	}

	/// <summary>A UserNotExistException, used when a user does not exist and a query failed as a result of it.</summary>
	[Serializable]
	public class UserAccountNotExistException : UserAccountManagerException
	{
		/// <summary>Creates a new UserAccountNotExistException object, with the user account name in the message.</summary>
		/// <param name="name">The user account name to be pasesd in the message.</param>
		/// <returns>A new UserAccountNotExistException object with a message containing the user account name that does not exist.</returns>
		public UserAccountNotExistException(string name)
			: base("User account '" + name + "' does not exist")
		{
		}
	}

	/// <summary>A GroupNotExistsException, used when a group does not exist.</summary>
	[Serializable]
	public class GroupNotExistsException : UserAccountManagerException
	{
		/// <summary>Creates a new GroupNotExistsException object with the group's name in the message.</summary>
		/// <param name="group">The group name.</param>
		/// <returns>A new GroupNotExistsException with the group that does not exist's name in the message.</returns>
		public GroupNotExistsException(string group)
			: base("Group '" + group + "' does not exist")
		{
		}
	}
}
