﻿/*
TShock, a server mod for Terraria
Copyright (C) 2011-2013 Nyx Studios (fka. The TShock Team)

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
using System.Threading;
using Terraria;

namespace TShockAPI.PluginUpdater
{
    class PluginUpdaterThread
    {
        private TSPlayer invoker;
        public PluginUpdaterThread(TSPlayer player)
        {
            invoker = player;
            PluginVersionCheck.PluginUpdate += PluginUpdate;
            HandleUpdate();
        }

        private void HandleUpdate()
        {
            foreach(PluginContainer cont in ProgramServer.Plugins)
                new Thread(PluginVersionCheck.CheckPlugin).Start(cont.Plugin);
        }

        private int Updates = 0;
        private void PluginUpdate(UpdateArgs args)
        {
            Updates++;
            if(args.Success && String.IsNullOrEmpty(args.Error))
            {
                invoker.SendSuccessMessage(String.Format("{0} was downloaded successfully.", args.Plugin.Name));
            }
            else if(args.Success)
            {
                invoker.SendSuccessMessage(String.Format("{0} was skipped. Reason: {1}", args.Plugin.Name, args.Error));
            }
            else
            {
                invoker.SendSuccessMessage(String.Format("{0} failed to downloaded. Error: {1}", args.Plugin.Name, args.Error));
            }

            if(Updates >= Terraria.ProgramServer.Plugins.Count)
            {
                PluginVersionCheck.PluginUpdate -= PluginUpdate;

                invoker.SendSuccessMessage("All plugins have been downloaded.  Now copying them to the plugin folder...");

                string folder = Path.Combine(TShock.SavePath, "UpdatedPlugins");
                string dest = Path.Combine(TShock.SavePath, "..", "ServerPlugins");

                foreach (string dir in Directory.GetDirectories(folder, "*", System.IO.SearchOption.AllDirectories))
                {
                    string new_folder = dest + dir.Substring(folder.Length);
                    if (!Directory.Exists(new_folder))
                        Directory.CreateDirectory(new_folder);
                }

                foreach (string file_name in Directory.GetFiles(folder, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    TSPlayer.Server.SendSuccessMessage(String.Format("Copied {0}", file_name));
                    File.Copy(file_name, dest + file_name.Substring(folder.Length), true);
                } 
                
                
                Directory.Delete(folder, true);

                invoker.SendSuccessMessage("All plugins have been processed.  Restart the server to have access to the new plugins.");
            }
        }
    }
}
