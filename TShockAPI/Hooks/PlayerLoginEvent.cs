﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TShockAPI.Hooks
{
    class PlayerLoginEventArgs
    {
        public TSPlayer Player { get; set; }
        public PlayerLoginEventArgs(TSPlayer ply)
        {
            Player = ply;
        }
    }

    class PlayerLoginEvent
    {
        public delegate void PlayerLoginD(PlayerLoginEventArgs e);
        public static event PlayerLoginD PlayerLogin;
        public static void OnPlayerLogin(TSPlayer ply)
        {
            if(PlayerLogin == null)
            {
                return;
            }

            PlayerLoginEventArgs args = new PlayerLoginEventArgs(ply);
            PlayerLogin(args);
        }
    }
}
