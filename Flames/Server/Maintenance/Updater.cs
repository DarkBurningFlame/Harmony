/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using Flames.Network;
using Flames.Tasks;

namespace Flames
{
    /// <summary> Checks for and applies software updates. </summary>
    public static class Updater
    {

        public static string SourceURL = "https://github.com/DarkBurningFlame/Harmony/";
        public const string BaseURL = "https://github.com/DarkBurningFlame/Harmony/blob/main/";
        public const string UploadsURL = "https://github.com/DarkBurningFlame/Harmony/tree/main/Uploads";
        public const string UpdatesURL = "https://github.com/DarkBurningFlame/Harmony/raw/main/Uploads/";
        public static string WikiURL = "https://github.com/ClassiCube/MCGalaxy/wiki/";
        const string CurrentVersionURL = UpdatesURL + "dev.txt";
        const string guiURL = UpdatesURL + "Harmony.exe";
        const string cliURL = UpdatesURL + "Harmony_.exe";

        public static event EventHandler NewerVersionDetected;

        public static void UpdaterTask(SchedulerTask task)
        {
            UpdateCheck();
            task.Delay = TimeSpan.FromHours(2);
        }

        static void UpdateCheck()
        {
            if (!Server.Config.CheckForUpdates) return;
            WebClient client = HttpUtil.CreateWebClient();

            try
            {
                string latest = client.DownloadString(CurrentVersionURL);

                if (new Version(Server.Version) >= new Version(latest))
                {
                    Logger.Log(LogType.SystemActivity, "No update found!");
                }
                else if (NewerVersionDetected != null)
                {
                    NewerVersionDetected(null, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error checking for updates", ex);
            }

            client.Dispose();
        }

        public static void PerformUpdate()
        {
            try
            {
                try
                {
                    DeleteFiles("Harmony_.update", "Harmony.update", "prev_Harmony.exe", "prev_Harmony_.exe");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error deleting files", ex);
                }

                WebClient client = HttpUtil.CreateWebClient();
                client.DownloadFile(guiURL, "Harmony_.update");
                client.DownloadFile(cliURL, "Harmony.update");
                Level[] levels = LevelInfo.Loaded.Items;
                foreach (Level lvl in levels)
                {
                    if (!lvl.SaveChanges) continue;
                    lvl.Save();
                    lvl.SaveBlockDBChanges();
                }

                Player[] players = PlayerInfo.Online.Items;
                foreach (Player pl in players) pl.SaveStats();

                // Move current files to previous files (by moving instead of copying, 
                //  can overwrite original the files without breaking the server)

                AtomicIO.TryMove("Harmony_.exe", "prev_Harmony_.exe");
                AtomicIO.TryMove("Harmony.exe", "prev_Harmony.exe");
                File.Move("Harmony_.update", "Harmony_.exe");
                File.Move("Harmony.update", "Harmony.exe");
                Server.Stop(true, "Updating server.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error performing update", ex);
            }
        }

        static void DeleteFiles(params string[] paths)
        {
            foreach (string path in paths) { AtomicIO.TryDelete(path); }
        }
    }
}
