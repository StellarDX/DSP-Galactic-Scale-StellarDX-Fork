﻿using HarmonyLib;
using UnityEngine.UI;
using System.Reflection;
using System.IO;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System;

namespace GalacticScale.Scripts.PatchUI {
    [BepInPlugin("dsp.galactic-scale.ui", "Galactic Scale Plug-In - UI", "1.3.3")]
    public class PatchUI : BaseUnityPlugin {
        public new static ManualLogSource Logger;
        public static string Version = "1.3.3";

        internal void Awake()
        {
            var harmony = new Harmony("dsp.galactic-scale.ui");
            Logger = new ManualLogSource("PatchUI");
            BepInEx.Logging.Logger.Sources.Add(Logger);
            Logger.LogMessage("Galactic Scale Version " + Version + " loading");
            try
            {
                harmony.PatchAll(typeof(PatchUI));
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
            }
        }

        public static Sprite GetEmbeddedSprite(string location)
        {
            byte[] buffer;
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream s = assembly.GetManifestResourceStream(location))
            {
                long length = s.Length;
                buffer = new byte[length];
                s.Read(buffer, 0, (int)length);
            }
            Texture2D tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, buffer);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 100f);
        }
        //public static byte[] GetSplashImage()
        //{
        //    byte[] buffer;
        //    Assembly assembly = Assembly.GetExecutingAssembly();
        //    using (Stream s = assembly.GetManifestResourceStream("GalacticScale.Scripts.PatchUI.splash.jpg"))
        //    {
        //        long length = s.Length;
        //        buffer = new byte[length];
        //        s.Read(buffer, 0, (int)length);
        //    }
        //    return buffer;
        //}
        //public static Sprite GetRulerSprite()
        //{
        //    byte[] buffer;
        //    Assembly assembly = Assembly.GetExecutingAssembly();
        //    using (Stream s = assembly.GetManifestResourceStream("GalacticScale.Scripts.PatchUI.ruler.png"))
        //    {
        //        long length = s.Length;
        //        buffer = new byte[length];
        //        s.Read(buffer, 0, (int)length);
        //    }
        //    Texture2D tex = new Texture2D(2, 2);
        //    ImageConversion.LoadImage(tex, buffer);
        //    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 100f);
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(UIEscMenu), "_OnOpen")]
        public static void _OnOpen(ref Text ___stateText)
        {
            ___stateText.text += "\r\nGalactic Scale v" + Version;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIPlanetGlobe), "_OnUpdate")]
        public static void PatchOnUpdate(ref Text ___geoInfoText)
        {
            if (GameMain.localPlanet != null && VFInput.alt) ___geoInfoText.text = "\r\nRadius " + GameMain.localPlanet.radius;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIGameLoadingSplash), "OnEnable")]
        public static void OnEnable(ref Text ___promptText)
        {
            ___promptText.text = "WARNING - Galactic Scale savegames can be broken by updates. Read the FAQ @ http://customizing.space";
            //string dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(PatchUI)).Location) + "\\splash.jpg";
            //Texture2D tex = null;
            //byte[] fileData;
            //if (File.Exists(dir))
            //{
            //    fileData = File.ReadAllBytes(dir);
            //} else
            //{
            //    fileData = GetSplashImage();
            //}
            //tex = new Texture2D(2, 2);
            //ImageConversion.LoadImage(tex, fileData);
            Image[] images = UIRoot.instance.overlayCanvas.GetComponentsInChildren<Image>();
            RawImage[] rimages = UIRoot.instance.overlayCanvas.GetComponentsInChildren<RawImage>();
            foreach (Image image in images)
            {
                if (image.name == "black-bg")
                {
                    image.sprite = GetEmbeddedSprite("GalacticScale.Scripts.PatchUI.splash.jpg"); // Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 100f);
                    image.color = Color.white;
                }
                else if (image.name == "bg" || image.name == "dots" || image.name == "dsp") image.enabled = false;
            }
            foreach (RawImage rimage in rimages)
            {
                if (rimage.name == "vignette") rimage.enabled = false;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(UIVersionText), "Refresh")]
        public static bool Refresh (ref Text ___textComp, string ___prefix, ref AccountData ___displayAccount, ref bool ___firstFrame)
        {
            if (___textComp != null)
            {
                bool flag = false;
                if (GameMain.data != null && !GameMain.instance.isMenuDemo && GameMain.isRunning)
                {
                    if (___displayAccount != GameMain.data.account)
                    {
                       ___displayAccount = GameMain.data.account;
                        flag = true;
                    }
                }
                else if (___displayAccount.userId != 0UL)
                {
                    ___displayAccount = AccountData.NULL;
                    flag = true;
                }
                if (___firstFrame || flag)
                {
                    //string empty = string.Empty;
                    string userName = ___displayAccount.detail.userName;
                    if (string.IsNullOrEmpty(userName))
                    {
                        ___textComp.fontSize = 18;
                        ___textComp.text = ___prefix.Translate() + " " + GameConfig.gameVersion.ToFullString() + "\nGalactic Scale v" + Version;
                    }
                    else
                    {
                        ___textComp.fontSize = 24;
                        ___textComp.text = ___prefix.Translate() + " " + GameConfig.gameVersion.ToFullString() + "\r\n" + userName + " - Galactic Scale v" + Version;
                    }

                }
            }
            ___firstFrame = false;
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIPlanetDetail), "OnPlanetDataSet")]
        public static void OnPlanetDataSet (ref UIPlanetDetail __instance, Text ___obliquityValueText)
        {
            // Add the planets radius to the Planet Detail UI
            if (___obliquityValueText.transform.parent.transform.parent.childCount == 6) 
            {
                GameObject radiusLabel;
                GameObject obliquityLabel = ___obliquityValueText.transform.parent.gameObject;
                radiusLabel = Instantiate(obliquityLabel, obliquityLabel.transform.parent.transform);
                radiusLabel.transform.localPosition += (Vector3.down * 20);
                Text radiusLabelText = radiusLabel.GetComponent<Text>();
                radiusLabelText.GetComponent<Localizer>().enabled = false;
                Image radiusIcon = radiusLabel.transform.GetChild(1).GetComponent<Image>();
                radiusIcon.sprite = GetEmbeddedSprite("GalacticScale.Scripts.PatchUI.ruler.png"); //  GetRulerSprite();
                Text radiusValueText = radiusLabel.transform.GetChild(0).GetComponent<Text>();        
                radiusLabelText.text = "Planetary Radius";
                radiusValueText.text = __instance.planet.radius.ToString();
            }
            if (___obliquityValueText.transform.parent.transform.parent.childCount == 7)
            {
                Transform p = ___obliquityValueText.transform.parent.parent;
                GameObject radiusLabel = p.GetChild(p.childCount - 1).gameObject;
                Text radiusValueText = radiusLabel.transform.GetChild(0).GetComponent<Text>();
                if (__instance.planet != null) radiusValueText.text = __instance.planet.radius.ToString();
            }
        }
        [HarmonyPrefix, HarmonyPatch(typeof(UIPlanetDetail), "SetResCount")]
        public static bool SetResCount(int count, ref RectTransform ___rectTrans, ref RectTransform ___paramGroup) // Adjust the height of the PlanetDetail UI to allow for Radius Text
        {
            ___rectTrans.sizeDelta = new Vector2(___rectTrans.sizeDelta.x, (float)(190 + count * 20) + 20f);
            ___paramGroup.anchoredPosition = new Vector2(___paramGroup.anchoredPosition.x, (float)(-90 - count * 20));
            return false;
        }

    }
}
