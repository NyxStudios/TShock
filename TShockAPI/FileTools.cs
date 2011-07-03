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
using System.IO;
using Terraria;

namespace TShockAPI
{
    internal class FileTools
    {
        public static string RulesPath { get { return Path.Combine(TShock.SavePath, "rules.txt"); } }
        public static string MotdPath { get { return Path.Combine(TShock.SavePath, "motd.txt"); } }
        public static string BansPath { get { return Path.Combine(TShock.SavePath, "bans.txt"); } }
        public static string WhitelistPath { get { return Path.Combine(TShock.SavePath, "whitelist.txt"); } }
        public static string GroupsPath { get { return Path.Combine(TShock.SavePath, "groups.txt"); } }
        public static string UsersPath { get { return Path.Combine(TShock.SavePath, "users.txt"); } }
        public static string ItemBansPath { get { return Path.Combine(TShock.SavePath, "itembans.txt"); } }
        public static string RememberedPosPath { get { return Path.Combine(TShock.SavePath, "oldpos.xml"); } }
        public static string ConfigPath { get { return Path.Combine(TShock.SavePath, "config.json"); } }
        public static string RegionsPath { get { return Path.Combine(TShock.SavePath, "regions.xml"); } }
        public static string WarpsPath { get { return Path.Combine(TShock.SavePath, "warps.xml"); } }
        public static string BeggarsPath { get { return Path.Combine(TShock.SavePath, "beggars.txt"); } }
        public static string DictionaryPath { get { return Path.Combine(TShock.SavePath, "dictionary.txt"); } }

        public static void CreateFile(string file)
        {
            File.Create(file).Close();
        }

        public static void CreateIfNot(string file, string data = "")
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, data);
            }
        }

        /// <summary>
        /// Sets up the configuration file for all variables, and creates any missing files.
        /// </summary>
        public static void SetupConfig()
        {
            if (!Directory.Exists(TShock.SavePath))
            {
                Directory.CreateDirectory(TShock.SavePath);
            }

            CreateIfNot(RulesPath, "Respect the admins!\nDon't use TNT!");
            CreateIfNot(MotdPath, "This server is running TShock. Type /help for a list of commands.\n%255,000,000%Current map: %map%\nCurrent players: %players%");
            CreateIfNot(BansPath);
            CreateIfNot(WhitelistPath);
            CreateIfNot(GroupsPath, Resources.groups);
            CreateIfNot(UsersPath, Resources.users);
            CreateIfNot(ItemBansPath, Resources.itembans);
            CreateIfNot(BeggarsPath);
            CreateIfNot(DictionaryPath);

            //Copies if using old paths (Remove in future releases, after everyone is running this version +)
            if (File.Exists("regions.xml") && !File.Exists(RegionsPath))
            {
                File.Move("regions.xml", RegionsPath);
            }
            else
            {
                CreateIfNot(RegionsPath);
            }
            if (File.Exists("warps.xml") && !File.Exists(WarpsPath))
            {
                File.Move("warps.xml", WarpsPath);
            }
            else
            {
                CreateIfNot(WarpsPath);
            }

            try
            {
                if (File.Exists(ConfigPath))
                {
                    TShock.Config = ConfigFile.Read(ConfigPath);
                    // Add all the missing config properties in the json file
                }
                TShock.Config.Write(ConfigPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in config file");
                Log.Error("Config Exception");
                Log.Error(ex.ToString());
            }

            
        }

        /// <summary>
        /// Tells if a user is on the whitelist
        /// </summary>
        /// <param name="ip">string ip of the user</param>
        /// <returns>true/false</returns>
        public static bool OnWhitelist(string ip)
        {
            if (!TShock.Config.EnableWhitelist)
            {
                return true;
            }
            CreateIfNot(WhitelistPath, "127.0.0.1");
            TextReader tr = new StreamReader(WhitelistPath);
            string whitelist = tr.ReadToEnd();
            ip = Tools.GetRealIP(ip);
            bool contains = whitelist.Contains(ip);
            if (!contains)
            {
                var char2 = Environment.NewLine.ToCharArray();
                var array = whitelist.Split(Environment.NewLine.ToCharArray());
                foreach (var line in whitelist.Split(Environment.NewLine.ToCharArray()))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    contains = Tools.GetIPv4Address(line).Equals(ip);
                    if (contains)
                        return true;
                }
                return false;
            }
            else
                return true;
        }
    }
}