﻿/*   
TShock, a server mod for Terraria
Copyright (C) 2011 The TShock Team

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
using System.Data;
using System.IO;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;
using TShockAPI.DB;

namespace TShockAPI.DB
{
    public class BanManager
    {
        private IDbConnection database;

        public BanManager(IDbConnection db)
        {
            database = db;

            using (var com = database.CreateCommand())
            {
                if (TShock.Config.StorageType.ToLower() == "sqlite")
                    com.CommandText =
                        "CREATE TABLE IF NOT EXISTS 'Bans' ('IP' TEXT PRIMARY KEY, 'Name' TEXT, 'Reason' TEXT);";
                else if (TShock.Config.StorageType.ToLower() == "mysql")
                    com.CommandText =
                        "CREATE TABLE IF NOT EXISTS  Bans (`IP` VARCHAR(255), `Name` VARCHAR(255), `Reason` VARCHAR(255), PRIMARY KEY (`IP`));";

                com.ExecuteNonQuery();

                String file = Path.Combine( TShock.SavePath, "bans.txt" );
                if (File.Exists(file))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        String line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            String[] info = line.Split('|');
                            if (TShock.Config.StorageType.ToLower() == "sqlite")
                                com.CommandText = "INSERT OR IGNORE INTO Bans (IP, Name, Reason) VALUES (@ip, @name, @reason);";
                            else if (TShock.Config.StorageType.ToLower() == "mysql")
                                com.CommandText = "INSERT IGNORE INTO Bans SET IP=@ip, Name=@name, Reason=@reason;";
                            com.AddParameter("@ip", info[0].Trim());
                            com.AddParameter("@name", info[1].Trim());
                            com.AddParameter("@reason", info[2].Trim());
                            com.ExecuteNonQuery();
                            com.Parameters.Clear();
                        }
                    }
                    String path = Path.Combine(TShock.SavePath, "old_configs");
                    String file2 = Path.Combine(path, "bans.txt");
                    if (!Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);
                    if (File.Exists(file2))
                        File.Delete(file2);
                    File.Move(file, file2);
                }
            }
        }

        public Ban GetBanByIp(string ip)
        {
            try
            {
                using (var com = database.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM Bans WHERE IP=@ip";
                    com.AddParameter("@ip", ip);
                    using (var reader = com.ExecuteReader())
                    {
                        if (reader.Read())
                            return new Ban((string)reader["IP"], (string)reader["Name"], (string)reader["Reason"]);

                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return null;
        }

        public Ban GetBanByName(string name, bool casesensitive = true)
        {
            if (!TShock.Config.EnableBanOnUsernames)
            {
                return null;
            }
            try
            {
                using (var com = database.CreateCommand())
                {
                    var namecol = casesensitive ? "Name" : "UPPER(Name)";
                    if (!casesensitive)
                        name = name.ToUpper();
                    com.CommandText = "SELECT * FROM Bans WHERE " + namecol + "=@name";
                    com.AddParameter("@name", name);
                    using (var reader = com.ExecuteReader())
                    {
                        if (reader.Read())
                            return new Ban((string)reader["IP"], (string)reader["Name"], (string)reader["Reason"]);

                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return null;
        }

        public bool AddBan(string ip, string name = "", string reason = "")
        {
            try
            {
                using (var com = database.CreateCommand())
                {
                    com.CommandText = "INSERT INTO Bans (IP, Name, Reason) VALUES (@ip, @name, @reason);";
                    com.AddParameter("@ip", ip);
                    com.AddParameter("@name", name);
                    com.AddParameter("@reason", reason);
                    com.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return false;
        }

        public bool RemoveBan(string ip)
        {
            try
            {
                using (var com = database.CreateCommand())
                {
                    com.CommandText = "DELETE FROM Bans WHERE IP=@ip";
                    com.AddParameter("@ip", ip);
                    com.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return false;
        }
        public bool ClearBans()
        {
            try
            {
                using (var com = database.CreateCommand())
                {
                    com.CommandText = "DELETE FROM Bans";
                    com.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return false;
        }
    }

    public class Ban
    {
        public string IP { get; set; }

        public string Name { get; set; }

        public string Reason { get; set; }

        public Ban(string ip, string name, string reason)
        {
            IP = ip;
            Name = name;
            Reason = reason;
        }

        public Ban()
        {
            IP = string.Empty;
            Name = string.Empty;
            Reason = string.Empty;
        }
    }
}