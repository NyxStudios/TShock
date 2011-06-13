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
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;

namespace TShockAPI
{
    internal class Tools
    {
        private static List<Group> groups = new List<Group>();

        /// <summary>
        /// Provides the real IP address from a RemoteEndPoint string that contains a port and an IP
        /// </summary>
        /// <param name="mess">A string IPv4 address in IP:PORT form.</param>
        /// <returns>A string IPv4 address.</returns>
        public static string GetRealIP(string mess)
        {
            return mess.Split(':')[0];
        }

        /// <summary>
        /// Gets the IP from a player index
        /// </summary>
        /// <param name="plr">Player Index</param>
        /// <returns>IP</returns>
        public static string GetPlayerIP(int plr)
        {
            return GetRealIP(Netplay.serverSock[plr].tcpClient.Client.RemoteEndPoint.ToString());
        }

        /// <summary>
        /// Used for some places where a list of players might be used.
        /// </summary>
        /// <returns>String of players seperated by commas.</returns>
        public static string GetPlayers()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(Main.player[i].name);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// It's a clamp function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to clamp</param>
        /// <param name="max">Maximum bounds of the clamp</param>
        /// <param name="min">Minimum bounds of the clamp</param>
        /// <returns></returns>
        public static T Clamp<T>(T value, T max, T min)
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
        /// Broadcasts a message to all players
        /// </summary>
        /// <param name="msg">string message</param>
        public static void Broadcast(string msg)
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                SendMessage(i, msg);
            }
            Log.Info(string.Format("Broadcast: {0}", msg));
        }

        public static void Broadcast(string msg, float red, float green, float blue)
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                SendMessage(i, msg, red, green, blue);
            }
            Log.Info(string.Format("Broadcast: {0}", msg));
        }

        /// <summary>
        /// Sends a message out to a single player
        /// </summary>
        /// <param name="ply">int socket thingy for the player from the server socket</param>
        /// <param name="msg">String message</param>
      
        public static void SendMessage(int ply, string msg, float red, float green, float blue)
        {
            NetMessage.SendData(0x19, ply, -1, msg, 255, red, green, blue);
        }

        /// <summary>
        /// Sends a message out to a single player
        /// </summary>
        /// <param name="ply">int socket thingy for the player from the server socket</param>
        /// <param name="msg">String message</param>
        /// <param name="color">Float containing red, blue, and green color values</param>
        public static void SendMessage(int ply, string msg, Color color)
        {
            NetMessage.SendData(0x19, ply, -1, msg, 255, color.R, color.G, color.B);
        }

        /// <summary>
        /// Sends message to all users with 'logs' permission.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="color"></param>
        public static void SendLogs(string log, Color color)
        {
            Log.Info(log);
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (TShock.players[i] == null)
                    continue;
                if (!TShock.players[i].group.HasPermission("logs"))
                    continue;

                SendMessage(i, log, color);
            }
        }

        /// <summary>
        /// Sends a green message to a player
        /// </summary>
        /// <param name="ply">int socket thingy for the player from the server socket</param>
        /// <param name="message">string message</param>
        public static void SendMessage(int ply, string message)
        {
            NetMessage.SendData(0x19, ply, -1, message, 255, 0f, 255f, 0f);
        }

        /// <summary>
        /// The number of active players on the server.
        /// </summary>
        /// <returns>int playerCount</returns>
        public static int activePlayers()
        {
            int num = 0;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    num++;
                }
            }
            return num;
        }

        /// <summary>
        /// Finds the name of the player of the int given
        /// </summary>
        /// <param name="ply">string player name</param>
        /// <returns>int player</returns>
        public static int FindPlayer(string ply)
        {
            List<int> found = new List<int>();
            for (int i = 0; i < Main.player.Length; i++)
            {
                if (Main.player[i].name.ToLower().Equals(ply.ToLower()))
                    return i;
                if (Main.player[i].name.ToLower().Contains(ply.ToLower()))
                    found.Add(i);
            }
            if (found.Count == 1)
                return found[0];
            else if (found.Count > 1)
                return -2;
            else
                return -1;
        }

        /// <summary>
        /// Gets the given player's name
        /// </summary>
        /// <param name="ply">int player</param>
        /// <returns>string name</returns>
        public static string FindPlayer(int ply)
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                if (i == ply)
                {
                    return Main.player[i].name;
                }
            }
            return "null";
        }

        /// <summary>
        /// Creates an NPC
        /// </summary>
        /// <param name="type">Type is defined in the enum NPC list</param>
        /// <param name="x">X coord of the desired npc</param>
        /// <param name="y">Y coord of the desired npc</param>
        /// <param name="target">int player that the npc targets</param>
        public static void NewNPC(int type, int x, int y, int target)
        {
            switch (type)
            {
                case 0: //World Eater
                    WorldGen.shadowOrbSmashed = true;
                    WorldGen.shadowOrbCount = 3;
                    int w = NPC.NewNPC(x, y, 13, 1);
                    Main.npc[w].target = target;
                    break;
                case 1: //Eye
                    Main.time = 4861;
                    Main.dayTime = false;
                    WorldGen.spawnEye = true;
                    break;
                case 2: //Skeletron
                    int enpeecee = NPC.NewNPC(x, y, 0x23, 0);
                    Main.npc[enpeecee].netUpdate = true;
                    break;
            }
        }

        /// <summary>
        /// Kicks a player from the server without checking for immunetokick permission.
        /// </summary>
        /// <param name="ply">int player</param>
        /// <param name="reason">string reason</param>
        public static void ForceKick(int ply, string reason)
        {
            string ip = GetPlayerIP(ply);
            NetMessage.SendData(0x2, ply, -1, reason, 0x0, 0f, 0f, 0f);
            Log.Info(string.Format("{0} was force kicked for : {1}", ip, reason));
        }

        /// <summary>
        /// Kicks a player from the server.
        /// </summary>
        /// <param name="ply">int player</param>
        /// <param name="reason">string reason</param>
        public static bool Kick(int ply, string reason, string adminUserName = "")
        {
            if (!Netplay.serverSock[ply].active || Netplay.serverSock[ply].kill)
                return true;
            if (!TShock.players[ply].group.HasPermission("immunetokick"))
            {
                string playerName = Main.player[ply].name;
                NetMessage.SendData(0x2, ply, -1, string.Format("Kicked: {0}", reason), 0x0, 0f, 0f, 0f);
                Log.Info(string.Format("Kicked {0} for : {1}", playerName, reason));
                if (adminUserName.Length == 0)
                    Broadcast(string.Format("{0} was kicked for {1}", playerName, reason.ToLower()));
                else
                    Tools.Broadcast(string.Format("{0} kicked {1} for {2}", adminUserName, playerName, reason.ToLower()));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Bans and kicks a player from the server.
        /// </summary>
        /// <param name="ply">int player</param>
        /// <param name="reason">string reason</param>
        public static bool Ban(int plr, string reason, string adminUserName = "")
        {
            if (!Netplay.serverSock[plr].active || Netplay.serverSock[plr].kill)
                return true;
            if (!TShock.players[plr].group.HasPermission("immunetoban"))
            {
                string ip = GetPlayerIP(plr);
                string playerName = Main.player[plr].name;
                TShock.Bans.AddBan(ip, playerName, reason);
                NetMessage.SendData(0x2, plr, -1, string.Format("Banned: {0}", reason), 0x0, 0f, 0f, 0f);
                Log.Info(string.Format("Banned {0} for : {1}", playerName, reason));
                if (adminUserName.Length == 0)
                    Broadcast(string.Format("{0} was banned for {1}", playerName, reason.ToLower()));
                else
                    Tools.Broadcast(string.Format("{0} banned {1} for {2}", adminUserName, playerName, reason.ToLower()));
                return true;
            }
            return false;
        }

        public static bool HandleCheater(int ply, string reason)
        {
            return HandleBadPlayer(ply, "ignorecheatdetection", ConfigurationManager.banCheater, ConfigurationManager.kickCheater, reason);
        }

        public static bool HandleGriefer(int ply, string reason)
        {
            return HandleBadPlayer(ply, "ignoregriefdetection", ConfigurationManager.banGriefer, ConfigurationManager.kickGriefer, reason);
        }

        public static bool HandleTntUser(int ply, string reason)
        {
            return HandleBadPlayer(ply, "ignoregriefdetection", ConfigurationManager.banTnt, ConfigurationManager.kickTnt, reason);
        }

        public static bool HandleExplosivesUser(int ply, string reason)
        {
            return HandleBadPlayer(ply, "ignoregriefdetection", ConfigurationManager.banBoom, ConfigurationManager.kickBoom, reason);
        }

        private static bool HandleBadPlayer(int ply, string overridePermission, bool ban, bool kick, string reason)
        {
            if (!TShock.players[ply].group.HasPermission(overridePermission))
            {
                if (ban)
                {
                    return Ban(ply, reason);
                }
                else if (kick)
                {
                    return Kick(ply, reason);
                }
            }
            return false;
        }

        /// <summary>
        /// Shows a MOTD to the player
        /// </summary>
        /// <param name="ply">int player</param>
        public static void ShowMOTD(int ply)
        {
            string foo = "";
            TextReader tr = new StreamReader(FileTools.SaveDir + "motd.txt");
            while ((foo = tr.ReadLine()) != null)
            {
                foo = foo.Replace("%map%", Main.worldName);
                foo = foo.Replace("%players%", GetPlayers());
                if (foo.Substring(0, 1) == "%" && foo.Substring(12, 1) == "%") //Look for a beginning color code.
                {
                    string possibleColor = foo.Substring(0, 13);
                    foo = foo.Remove(0, 13);
                    float[] pC = { 0, 0, 0 };
                    possibleColor = possibleColor.Replace("%", "");
                    string[] pCc = possibleColor.Split(',');
                    if (pCc.Length == 3)
                    {
                        try
                        {
                            SendMessage(ply, foo, Clamp(Convert.ToInt32(pCc[0]), 255, 0), Clamp(Convert.ToInt32(pCc[1]), 255, 0), Clamp(Convert.ToInt32(pCc[2]), 255, 0));
                            continue;
                        }
                        catch (Exception e)
                        {
                            FileTools.WriteError(e.Message);
                        }
                    }
                }
                SendMessage(ply, foo);
            }
            tr.Close();
        }

        public static void LoadGroups()
        {
            groups = new List<Group>();
            groups.Add(new SuperAdminGroup("superadmin"));

            StreamReader sr = new StreamReader(FileTools.SaveDir + "groups.txt");
            string data = sr.ReadToEnd();
            data = data.Replace("\r", "");
            string[] lines = data.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#"))
                {
                    continue;
                }
                string[] args = lines[i].Split(' ');
                if (args.Length < 2)
                {
                    continue;
                }
                string name = args[0];
                string parent = args[1];
                Group group = null;
                if (parent.Equals("null"))
                {
                    group = new Group(name);
                }
                else
                {
                    for (int j = 0; j < groups.Count; j++)
                    {
                        if (groups[j].GetName().Equals(parent))
                        {
                            group = new Group(name, groups[j]);
                            break;
                        }
                    }
                }
                if (group == null)
                {
                    throw new Exception("Something in the groups.txt is fucked up");
                }
                else
                {
                    for (int j = 2; j < args.Length; j++)
                    {
                        string permission = args[j];
                        if (permission.StartsWith("!"))
                        {
                            group.NegatePermission(args[j].Replace("!", ""));
                        }
                        else
                        {
                            group.AddPermission(args[j]);
                        }
                    }
                }
                groups.Add(group);
            }
            sr.Close();
        }

        /// <summary>
        /// Returns a Group from the name of the group
        /// </summary>
        /// <param name="ply">string groupName</param>
        public static Group GetGroup(string groupName)
        {
            //first attempt on cached groups
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].GetName().Equals(groupName))
                {
                    return groups[i];
                }
            }
            //shit, it didnt work, reload and try again
            LoadGroups();

            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].GetName().Equals(groupName))
                {
                    return groups[i];
                }
            }

            //sigh :(, ok, you fucked up the config files, congrats.
            return null;
        }

        /// <summary>
        /// Returns a Group for a ip from users.txt
        /// </summary>
        /// <param name="ply">string ip</param>
        public static Group GetGroupForIP(string ip)
        {
            ip = GetRealIP(ip);

            StreamReader sr = new StreamReader(FileTools.SaveDir + "users.txt");
            string data = sr.ReadToEnd();
            data = data.Replace("\r", "");
            string[] lines = data.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string[] args = lines[i].Split(' ');
                if (args.Length < 2)
                {
                    continue;
                }
                if (lines[i].StartsWith("#"))
                {
                    continue;
                }
                if (lines[i].StartsWith("!"))
                {
                    continue;
                }                
                if (args[0].Equals(ip))
                {
                    return GetGroup(args[1]);
                }
            }
            sr.Close();
            return GetGroup("default");
        }

        /// <summary>
        /// Returns a Group for a name from users.txt
        /// </summary>
        /// <param name="who">int who</param>
        public static Group GetGroupbyName(int who)
        {
            StreamReader sr = new StreamReader(FileTools.SaveDir + "users.txt");
            string data = sr.ReadToEnd();
            data = data.Replace("\r", "");
            string[] lines = data.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string[] args = lines[i].Split(' ');
                if (args.Length < 2)
                {
                    continue;
                }
                if (lines[i].StartsWith("#"))
                {
                    continue;
                }
                if (lines[i].StartsWith("!"))
                {
                    if (args[1].Equals(FindPlayer(who)))
                    {
                        return GetGroup(args[2]);
                    }
                }
            }
            sr.Close();
            return GetGroup("default");
        }
    }
}