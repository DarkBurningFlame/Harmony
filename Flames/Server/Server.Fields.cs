/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    
    Dual-licensed under the    Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Collections.Generic;
using Flames.Network;
using Flames.Tasks;

namespace Flames 
{
    public sealed partial class Server 
    {
        public static bool cancelcommand;        
        public delegate void OnFlameCommand(string cmd, string message);
        public static event OnFlameCommand FlameCommand;
        public delegate void MessageEventHandler(string message);
        public delegate void VoidHandler();
        
        public static event MessageEventHandler OnURLChange;
        public static event VoidHandler OnSettingsUpdate;
        public static ServerConfig Config = new ServerConfig();
        public static DateTime StartTime;
        public static TimeSpan UpTime = DateTime.UtcNow - StartTime;
        public static PlayerExtList AutoloadMaps;
        public static PlayerMetaList RankInfo = new PlayerMetaList("text/rankinfo.txt");
        public static PlayerMetaList Notes = new PlayerMetaList("text/notes.txt");
        /// <summary> *** DO NOT USE THIS! *** Use VersionString, as this field is a constant and is inlined if used. </summary>
        public const string InternalVersion = "0.0.7.8";
        public static string SoftwareName = "&4H&6a&5r&0m&7o&2n&dy&a Dev";
        public static string Version { get { return InternalVersion; } }
        static string fullName;
        public static string SoftwareNameVersioned
        {
            get { return fullName ?? SoftwareName + " " + Version; }
            set { fullName = value; }
        }
        public static INetListen Listener = new TcpListen();

        //Other
        public static bool SetupFinished, CLIMode;
        
        public static PlayerList whiteList, invalidIds;
        public static PlayerList ignored, hidden, agreed, vip, noEmotes, lockdown;
        public static PlayerExtList models, skins, reach, rotations, modelScales;
        public static PlayerExtList bannedIP, frozen, muted, tempBans, tempRanks;
        
        public static readonly List<string> Devs = new List<string>() {
            "DarkBurningFlame", "BurningFlame", "SuperNova", "DeadNova",
            "HyperNova", "RandomStranger05", "GoldenSparks", "AurumStellae",
            "sethbatman05", "sethbatman2005", "jackstage1", "Pattykaki45",
            "jaketheidiot", "RandomStrangers", "ArgenteaeLunae", "Argenteae",
            "HarmonyNetwork" , "krowteNynomraH", "UserTaken123", "UserNotFree",
            "onedez"};
        public static readonly List<string> Opstats = new List<string>() { "ban", "tempban", "xban", "banip", "kick", "warn", "mute", "freeze", "setrank" };

        public static Level mainLevel;

        public static PlayerList reviewlist = new PlayerList();
        static string[] announcements = new string[0];
        public static string RestartPath;

        // Extra storage for custom commands
        public static ExtrasCollection Extras = new ExtrasCollection();
        
        public static int YesVotes, NoVotes;
        public static bool voting;
        public const int MAX_PLAYERS = int.MaxValue;
        
        public static Scheduler MainScheduler = new Scheduler("H_MainScheduler");
        public static Scheduler Background = new Scheduler("H_BackgroundScheduler");
        public static Scheduler Critical = new Scheduler("H_CriticalScheduler");
        public static Scheduler Heartbeats = new Scheduler("H_HeartbeatsScheduler");
        public static Server s = new Server();

        public const byte VERSION_0016 = 3; // classic 0.0.16
        public const byte VERSION_0017 = 4; // classic 0.0.17 / 0.0.18
        public const byte VERSION_0019 = 5; // classic 0.0.19
        public const byte VERSION_0020 = 6; // classic 0.0.20 / 0.0.21 / 0.0.23
        public const byte VERSION_0030 = 7; // classic 0.30 (final)
        // base 64 encoded Harmony_full.ico
        public const string GUIIcon_source = "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";
        public static bool chatmod, flipHead;
        public static bool shuttingDown;
    }
}
