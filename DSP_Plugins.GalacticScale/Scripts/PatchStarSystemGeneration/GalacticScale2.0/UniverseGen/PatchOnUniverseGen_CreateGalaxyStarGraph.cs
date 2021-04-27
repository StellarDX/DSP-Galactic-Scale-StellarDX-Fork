﻿using HarmonyLib;
using System.Collections.Generic;


namespace GalacticScale.Scripts.PatchStarSystemGeneration
{
    [HarmonyPatch(typeof(UniverseGen))]
    public class PatchOnUniverseGenStarGraph
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreateGalaxyStarGraph")]
        public static bool CreateGalaxyStarGraph(GalaxyData galaxy)
        {
            GS2.Log("CreateGalaxyStarGraph - "+galaxy.starCount);
            galaxy.graphNodes = new StarGraphNode[galaxy.starCount];
            for (int index1 = 0; index1 < galaxy.starCount; ++index1)
            {
                GS2.Log("adding node " + index1 + " " + galaxy.stars[index1].position) ;
                galaxy.graphNodes[index1] = new StarGraphNode(galaxy.stars[index1]);
                StarGraphNode graphNode1 = galaxy.graphNodes[index1];
                for (int index2 = 0; index2 < index1; ++index2)
                {
                    StarGraphNode graphNode2 = galaxy.graphNodes[index2];
                    if ((graphNode1.pos - graphNode2.pos).sqrMagnitude < 64.0)
                    {
                        GS2.Log("Adding Stargraphnode connection - "+ graphNode1.pos +" - " + graphNode2.pos);
                        UniverseGen.list_sorted_add(graphNode1.conns, graphNode2);
                        UniverseGen.list_sorted_add(graphNode2.conns, graphNode1);
                    }
                }
                UniverseGen.line_arragement_for_add_node(graphNode1);
            }
            return false;
        }
    }
}
