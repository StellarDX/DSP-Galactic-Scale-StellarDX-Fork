﻿using HarmonyLib;
using System;
using System.IO;

namespace GalacticScale
{
    public class PatchOnGameSave
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSave), "SaveCurrentGame")]
        public static void SaveCurrentGame(string saveName)
{
            var path = Path.Combine(GS2.DataDir, "GalaxyBackups");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, saveName);
            path = path+ "-" + DateTime.Now.ToString("dd") + ".json";
            if (saveName.Length < 10) return;
            if (saveName.Substring(1,8) != "autosave" && saveName.Substring(1,8) != "lastexit") GS2.SaveSettingsToJson(path);
        }
    }

}