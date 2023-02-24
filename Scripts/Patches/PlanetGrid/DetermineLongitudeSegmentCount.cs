﻿using HarmonyLib;
using UnityEngine;

namespace GalacticScale
{
    public partial class PatchOnPlanetGrid
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetGrid), "DetermineLongitudeSegmentCount")]
        public static bool DetermineLongitudeSegmentCount(PlanetGrid __instance, int latitudeIndex, int segment, ref int __result)
        {
            if (!DSPGame.IsMenuDemo)
                if (!GS2.Vanilla)
                {
                    if (!GS2.keyedLUTs.ContainsKey(segment))
                        GS2.SetLuts(segment, segment);
                    if (GS2.keyedLUTs.ContainsKey(segment))
                    {
                        var index = Mathf.Abs(latitudeIndex) % (segment / 2);

                        if (index >= segment / 4) index = segment / 4 - index;
                        if (index < 0 || GS2.keyedLUTs[segment].Length == 0)
                        {
                            __result = 4;
                            return false;
                        }

                        if (index > GS2.keyedLUTs[segment].Length - 1) __result = GS2.keyedLUTs[segment][GS2.keyedLUTs[segment].Length - 1];
                        else __result = GS2.keyedLUTs[segment][index];
                        //a = 1 * 1;
                    }
                    else
                    {
                        //Original algorithm. Really shouldn't be used anymore... but just in case it's still here.
                        // GS2.Warn("Using original algo");
                        var index = Mathf.CeilToInt(Mathf.Abs(Mathf.Cos((float)(latitudeIndex / (double)(segment / 4f) * 3.14159274101257 * 0.5))) * segment);
                        __result = index < 500 ? PlatformSystem.segmentTable[index] : (index + 49) / 100 * 100;
                        //a = 2;
                    }
                    if (__result < 4) __result = 4;
                    return false;
                }

            return true;
        }
    }
}