﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Terraria;

namespace TShockAPI
{
    public class RegionManager
    {
        public static List<Region> Regions = new List<Region>();

        public static bool AddRegion(int tx, int ty, int width, int height, string regionname, string worldname)
        {
            foreach (Region nametest in Regions)
            {
                if (regionname.ToLower() == nametest.RegionName.ToLower())
                {
                    return false;
                }
            }
            Regions.Add(new Region(new Rectangle(tx, ty, width, height), regionname, true, worldname));
            WriteSettings();
            return true;
        }

        public static bool AddNewUser(string regionName, string name)
        {
            foreach (Region nametest in Regions)
            {
                if (regionName.ToLower() == nametest.RegionName.ToLower())
                {
                    nametest.RegionAllowedNames.Add(name.ToLower());
                    return true;
                }
            }
            return false;
        }

        public static bool DeleteRegion(string name)
        {
            foreach (Region nametest in Regions)
            {
                if (name.ToLower() == nametest.RegionName.ToLower() && nametest.WorldRegionName == Main.worldName)
                {
                    Regions.Remove(nametest);
                    WriteSettings();
                    return true;
                }
            }
            return false;
        }

        public static bool SetRegionState(string name, bool state)
        {
            foreach (Region nametest in Regions)
            {
                if (name.ToLower() == nametest.RegionName.ToLower())
                {
                    nametest.DisableBuild = state;
                    WriteSettings();
                    return true;
                }
            }
            return false;
        }

        public static bool InProtectedArea(int X, int Y, string name)
        {
            foreach(Region region in Regions)
            {
                if (X >= region.RegionArea.Left && X <= region.RegionArea.Right && Y >= region.RegionArea.Top && Y <= region.RegionArea.Bottom && region.DisableBuild && Main.worldName == region.WorldRegionName)
                {
                    if (!AllowedUser(region.RegionName, name.ToLower()) || region.RegionAllowedNames.Count == 0)
                    {
                        Console.WriteLine(name + " try to destroy " + region.RegionName);
                        return true;
                    }
                    }
            }
            return false;
        }

        public static int GetRegionIndex(string regionName)
        {
            for(int i = 0; i< Regions.Count;i++)
            {
                if(Regions[i].RegionName == regionName)
                    return i;
            }
            return -1;
        }

        public static bool AllowedUser(string regionName, string name)
        {
            int ID = -1;
            if ((ID = GetRegionIndex(regionName)) != -1)
            {
                for (int i = 0; i < Regions[ID].RegionAllowedNames.Count; i++)
                {
                    if (Regions[ID].RegionAllowedNames[i].ToLower() == name)
                    {
                        if (TShock.Login.Contains(name))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static void WriteSettings()
        {
            try
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.NewLineChars = Environment.NewLine;

                using (XmlWriter settingsw = XmlWriter.Create(FileTools.RegionsPath, xmlWriterSettings))
                {
                    settingsw.WriteStartDocument();
                    settingsw.WriteStartElement("Regions");

                    foreach (Region region in Regions)
                    {
                        settingsw.WriteStartElement("ProtectedRegion");
                        settingsw.WriteElementString("RegionName", region.RegionName);
                        settingsw.WriteElementString("Point1X", region.RegionArea.X.ToString());
                        settingsw.WriteElementString("Point1Y", region.RegionArea.Y.ToString());
                        settingsw.WriteElementString("Point2X", region.RegionArea.Width.ToString());
                        settingsw.WriteElementString("Point2Y", region.RegionArea.Height.ToString());
                        settingsw.WriteElementString("Protected", region.DisableBuild.ToString());
                        settingsw.WriteElementString("WorldName", region.WorldRegionName);

                        settingsw.WriteElementString("AllowedUserCount", region.RegionAllowedNames.Count.ToString());
                        for (int i = 0; i < region.RegionAllowedNames.Count; i++)
                        {
                            settingsw.WriteElementString("Player", region.RegionAllowedNames[i]);
                        }

                        settingsw.WriteEndElement();
                    }

                    settingsw.WriteEndElement();
                    settingsw.WriteEndDocument();
                }
                Log.Info("Wrote Regions");
            }
            catch
            {
                Log.Warn("Could not write Regions");
            }
        }

        public static void ReadAllSettings()
        {
            try
            {
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.IgnoreWhitespace = true;

                using (XmlReader settingr = XmlReader.Create(FileTools.RegionsPath, xmlReaderSettings))
                {
                    while (settingr.Read())
                    {
                        if (settingr.IsStartElement())
                        {
                            switch (settingr.Name)
                            {                                    
                                case "Regions":
                                    {
                                        break;
                                    }                                
                                case "ProtectedRegion":
                                    {
                                        if (settingr.Read())
                                        {
                                            string name = null;
                                            int x = 0;
                                            int y = 0;
                                            int width = 0;
                                            int height = 0;
                                            bool state = true;
                                            string worldname = null;
                                            int playercount = 0;

                                            settingr.Read();
                                            if (settingr.Value != "" || settingr.Value != null)
                                                name = settingr.Value;
                                            else
                                                Log.Warn("Region name is empty");


                                            settingr.Read();
                                            settingr.Read();
                                            settingr.Read();
                                            if (settingr.Value != "" || settingr.Value != null)
                                                Int32.TryParse(settingr.Value, out x);
                                            else
                                                Log.Warn("x for region " + name + " is empty");

                                            settingr.Read();
                                            settingr.Read();
                                            settingr.Read();
                                            if (settingr.Value != "" || settingr.Value != null)
                                                Int32.TryParse(settingr.Value, out y);
                                            else
                                                Log.Warn("y for region " + name + " is empty");

                                            settingr.Read();
                                            settingr.Read();
                                            settingr.Read();
                                            if (settingr.Value != "" || settingr.Value != null)
                                                Int32.TryParse(settingr.Value, out width);
                                            else
                                                Log.Warn("Width for region " + name + " is empty");

                                            settingr.Read();
                                            settingr.Read();
                                            settingr.Read();
                                            if (settingr.Value != "" || settingr.Value != null)
                                                Int32.TryParse(settingr.Value, out height);
                                            else
                                                Log.Warn("Height for region " + name + " is empty");

                                            settingr.Read();
                                            settingr.Read();
                                            settingr.Read();
                                            if (settingr.Value != "" || settingr.Value != null)
                                                bool.TryParse(settingr.Value, out state);
                                            else
                                                Log.Warn("State for region " + name + " is empty");

                                            settingr.Read();
                                            settingr.Read();
                                            settingr.Read();
                                            if (settingr.Value != "" || settingr.Value != null)
                                                worldname = settingr.Value;                                                
                                            else
                                                Log.Warn("Worldname for region " + name + " is empty");

                                            settingr.Read();
                                            settingr.Read();
                                            settingr.Read();
                                            if (settingr.Value != "" || settingr.Value != null)
                                                Int32.TryParse(settingr.Value, out playercount);
                                            else
                                                Log.Warn("Playercount for region " + name + " is empty");

                                            AddRegion(x, y, width, height, name, worldname);

                                            if (playercount > 0)
                                            {
                                                for (int i = 0; i < playercount; i++)
                                                {
                                                    settingr.Read();
                                                    settingr.Read();
                                                    settingr.Read();
                                                    if (settingr.Value != "" || settingr.Value != null)
                                                    {
                                                        int ID = RegionManager.GetRegionIndex(name);
                                                        Regions[ID].RegionAllowedNames.Add(settingr.Value);
                                                    }
                                                    else
                                                        Log.Warn("PlayerName " + i + " for region " + name + " is empty");
                                                }
                                            }
                                        }
                                        break;
                                  }                                    
                            }
                        }
                    }
                }
                Log.Info("Read Regions");
            }
            catch
            {                
                Log.Warn("Could not read Regions");
                WriteSettings();
            }
        }
    }

    public class Region
    {
        public Rectangle RegionArea { get; set; }
        public string RegionName { get; set; }
        public bool DisableBuild { get; set; }
        public string WorldRegionName { get; set; }
        public List<string> RegionAllowedNames = new List<string>();

        public Region(Rectangle region, string name, bool disablebuild, string worldname)
        {
            RegionArea = region;
            RegionName = name;
            DisableBuild = disablebuild;
            WorldRegionName = worldname;
        }

        public Region()
        {
            RegionArea = Rectangle.Empty;
            RegionName = string.Empty;
            DisableBuild = true;
            WorldRegionName = string.Empty;
        }
    }
}
