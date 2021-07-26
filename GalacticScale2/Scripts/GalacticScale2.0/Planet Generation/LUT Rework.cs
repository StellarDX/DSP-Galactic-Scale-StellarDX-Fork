﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace GalacticScale
{
    public static partial class GS2
    {
        public static Dictionary<int, int[]> keyedLUTs = new Dictionary<int, int[]>();

        public static void SetLuts(int segments, float planetRadius)
        {
            if (!DSPGame.IsMenuDemo && !Vanilla)
            {
                GS2.Warn("Setting LUTS");
                // Prevent special LUT's being created in main menu
                if (keyedLUTs.ContainsKey(segments) && keyedLUTs.ContainsKey(segments) &&
                    PatchOnUIBuildingGrid.LUT512.ContainsKey(segments)) return;
                var numSegments =
                    segments / 4; //Number of segments on a quarter circle (the other 3/4 will result by mirroring)
                var lut = new int[numSegments];
                var segmentAngle =
                    Mathf.PI / 2f / numSegments; //quarter circle divided by num segments is the angle per segment

                var lastMajorRadius = planetRadius;
                var lastMajorRadiusCount = numSegments * 4;
                
                var classicLUT = new int[512];
                classicLUT[0] = 1;

                for (var cnt = 0; cnt < numSegments; cnt++)
                {
                    var ringradius = Mathf.Cos(cnt * segmentAngle) * planetRadius; //cos of the nth segment is the x-distance of the point in a 2d circle
                    var classicIdx = Mathf.CeilToInt(Mathf.Abs(Mathf.Cos((float) ((cnt + 1) / (segments / 4f) * Math.PI * 0.5))) * segments);

                    //If the new radius is smaller than 90% of the currently used radius, use it as the new segment count to avoid tile squishing
                    //if (ringradius < 0.9 * lastMajorRadius)
                    var ringcircumference = ringradius * 2 * Mathf.PI;
                    GS2.Log($"{lastMajorRadiusCount} ClassicIdx:{classicIdx} Cnt:{cnt} RingRadius:{ringradius} RingCircumference:{ringcircumference} {(ringcircumference / ((lastMajorRadiusCount) * 5))}");
                    //if (ringcircumference / ((lastMajorRadiusCount) *5) < 1.0f)
                    if (ringradius < 0.9 * lastMajorRadius)
                    {
                        lastMajorRadius = ringradius;
                        lastMajorRadiusCount = (int) ((ringradius-20f) / 20.0) * 20 + 20 - ((ringcircumference / (lastMajorRadiusCount * 5) < 1)?20:0);
                        if (lastMajorRadiusCount <= 0) lastMajorRadiusCount = 20;

                    }

                    lut[cnt] = lastMajorRadiusCount;
                    classicLUT[classicIdx] = lastMajorRadiusCount;
                }

                var last = 1;
                for (var oldlLutIdx = 1; oldlLutIdx < 512; oldlLutIdx++)
                    if (classicLUT[oldlLutIdx] > last)
                    {
                        //Offset of 1 is required to avoid mismatch of some longitude circles
                        var temp = classicLUT[oldlLutIdx];
                        classicLUT[oldlLutIdx] = last;
                        last = temp;
                    }
                    else
                    {
                        classicLUT[oldlLutIdx] = last;
                    }

                //Fill all Look Up Tables (Dictionaries really)
                if (!keyedLUTs.ContainsKey(segments)) keyedLUTs.Add(segments, lut);
                if (!keyedLUTs.ContainsKey(segments)) keyedLUTs.Add(segments, lut);
                if (!PatchOnUIBuildingGrid.LUT512.ContainsKey(segments))
                {
                    GS2.Warn("Adding Segments to LUT512");
                    PatchOnUIBuildingGrid.LUT512.Add(segments, classicLUT);
                }
            }
        }
    }
}