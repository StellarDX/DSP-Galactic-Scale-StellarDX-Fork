﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Patch = GalacticScale.Scripts.PatchStarSystemGeneration.PatchForStarSystemGeneration;
using System.Diagnostics;
namespace GalacticScale
{
    public static partial class GS2
    {
        public static PlanetData CreatePlanet(ref StarData star, int index, int orbitAround)
        {
            Patch.Debug("Start of CreatePlanet(" + settings.Stars[star.index].counter + ", " + star.index + "," + index + "," + orbitAround+")");
            StackTrace stackTrace = new StackTrace();
            Patch.Debug("***"+stackTrace.GetFrame(1).GetMethod().Name);
            System.Random ran = new System.Random(index);
            float r = (float)ran.NextDouble();
            PlanetData planetData = new PlanetData();
            GSplanet gsPlanet;
            if (orbitAround == 0) gsPlanet = settings.Stars[star.index].Planets[index];
            else gsPlanet = settings.Stars[star.index].Planets[orbitAround].Moons[index];
            planetData.index = index;
            planetData.galaxy = galaxy;
            planetData.star = star;
            planetData.seed = ran.Next();
            planetData.orbitAround = orbitAround;
            //Patch.Debug("orbitAround set to " + orbitAround);
            if (orbitAround > 0) planetData.orbitAroundPlanet = star.planets[orbitAround];
            
            int counter = settings.Stars[star.index].counter;
            planetData.number = counter;// index;
            planetData.id = star.id * 100 + settings.Stars[star.index].counter++ + 1;
            Patch.Debug("Planet ID = " + planetData.id + " index = " + index);
            string roman = "";
            if (orbitAround > 0) roman = Scripts.RomanNumbers.roman[orbitAround + 1] + " - ";
            roman += Scripts.RomanNumbers.roman[index + 1];
            Patch.Debug("roman = " + roman + "  name = " + gsPlanet.Name);
            planetData.name = (gsPlanet.Name != "") ? gsPlanet.Name : star.name + " " + roman;
            planetData.orbitRadius = gsPlanet.OrbitRadius;
            planetData.orbitIndex = (orbitAround > 0) ? settings.Stars[star.index].Planets[orbitAround].GetOrbitIndex(planetData.orbitRadius) : settings.Stars[star.index].GetOrbitIndex(planetData.orbitRadius);
            Patch.Debug(planetData.orbitIndex + " is orbitindex for orbitRadius " + planetData.orbitRadius);
            planetData.orbitInclination = gsPlanet.OrbitInclination;
            planetData.orbitLongitude = gsPlanet.OrbitLongitude;// 1+(index * (360/8));//
            planetData.orbitalPeriod = gsPlanet.OrbitalPeriod;
            planetData.orbitPhase = gsPlanet.OrbitPhase;//1+(index * (360/star.planetCount));
            planetData.orbitAround = orbitAround;
            Patch.Debug(planetData.orbitPhase + " <- phase");
            planetData.obliquity = gsPlanet.Obliquity;
            //planetData.singularity |= gsPlanet.singularity.Layside;
            planetData.rotationPeriod = gsPlanet.RotationPeriod;
            planetData.rotationPhase = gsPlanet.RotationPhase;
            //Patch.Debug("SunDistance");
            planetData.sunDistance = orbitAround > 0 ? star.planets[orbitAround].orbitRadius : planetData.orbitRadius; //moon use gsPlanet.orbitAroundPlanet.orbitalRadius;
            //Patch.Debug("Scale");
            planetData.scale = 1f;
            planetData.radius = gsPlanet.Radius;
            planetData.segment = 5;
            //planetData.singularity |= gsPlanet.singularity.TidalLocked;
            //planetData.singularity |= gsPlanet.singularity.TidalLocked2;
            //planetData.singularity |= gsPlanet.singularity.TidalLocked4;
            //planetData.singularity |= gsPlanet.singularity.ClockwiseRotate;
            //planetData.singularity |= gsPlanet.singularity.TidalLocked;
            planetData.runtimeOrbitRotation = Quaternion.AngleAxis(planetData.orbitLongitude, Vector3.up) * Quaternion.AngleAxis(planetData.orbitInclination, Vector3.forward); // moon gsPlanet.runtimeOrbitRotation = gsPlanet.orbitAroundPlanet.runtimeOrbitRotation * gsPlanet.runtimeOrbitRotation;
            planetData.runtimeSystemRotation = planetData.runtimeOrbitRotation * Quaternion.AngleAxis(planetData.obliquity, Vector3.forward);
            //Patch.Debug("Setting type");
            planetData.type = gsPlanet.Theme.type;
            //Patch.Debug("Type set to " + planetData.type);
            GS2.DumpObjectToJson(System.IO.Path.Combine(GS2.DataDir, "test.json"), GS2.planetThemes["Mediterranian"]);

            if (planetData.type == EPlanetType.Gas) planetData.scale = 10f;
            planetData.precision = (int)gsPlanet.Radius;
            //planetData.temperatureBias = gsPlanet.temperatureBias;
            planetData.luminosity = gsPlanet.Luminosity;
            //Patch.Debug("Setting Theme " + gsPlanet.Theme + " " + gsPlanet.Theme.theme);
            GS2.DumpObjectToJson(GS2.DataDir + "\\Planet" + planetData.id + ".json", gsPlanet);

            SetPlanetTheme(planetData, star, gameDesc, gsPlanet.Theme.theme, 0, ran.NextDouble(), ran.NextDouble(), ran.NextDouble(), ran.NextDouble(), ran.Next());
            //PlanetGen.SetPlanetTheme(planetData, star, gameDesc, 1, 0, ran.NextDouble(), ran.NextDouble(), ran.NextDouble(), ran.NextDouble(), ran.Next());

            //Patch.Debug("theme set");
            star.galaxy.astroPoses[planetData.id].uRadius = planetData.realRadius;
            //Patch.Debug("astropose radius set");

            star.planets[counter] = planetData;
            Patch.Debug("star.planets["+counter+"] successfully set");
            if (gsPlanet.MoonCount > 0)
            {
                Patch.Debug("Creating moons for gsPlanet " + index + " of star " + star.index + ". Star.counter = " + counter + " and star.planets.Length = " + star.planets.Length);

                CreateMoons(ref planetData, gsPlanet);
                Patch.Debug("Moons Created, returning planetData");
            }
            DebugPlanet(planetData);
            Patch.Debug("FINISHED CREATING PLANET " + planetData.id);
            return planetData;
        }
        public static void CreateMoons(ref PlanetData planetData, GSplanet planet)
        {
            Patch.Debug("Start of CreateMoons");
            StackTrace stackTrace = new StackTrace();
            Patch.Debug("***" + stackTrace.GetFrame(1).GetMethod().Name);


            for (var i = 0; i < planet.MoonCount; i++)
            {
                Patch.Debug("creating moon " + i + " of " + planet.MoonCount);
                PlanetData moon = CreatePlanet(ref planetData.star, i, planetData.index);
                Patch.Debug("finished creating moon " + moon.name);
                moon.orbitAroundPlanet = planetData;
                if (i > 1) planetData.singularity |= EPlanetSingularity.MultipleSatellites;
                BCE.console.WriteLine("Changed Singulatiry", ConsoleColor.Cyan);
            }
        }
        public static void DebugPlanet(PlanetData planet)
        {
            BCE.console.WriteLine("Creating Planet " + planet.id, ConsoleColor.Red);
            BCE.console.WriteLine("Index " + planet.index, ConsoleColor.Green);
            BCE.console.WriteLine("OrbitAround " + planet.orbitAround, ConsoleColor.Green);
            BCE.console.WriteLine("Seed " + planet.seed, ConsoleColor.Green);
            BCE.console.WriteLine("Period " + planet.orbitalPeriod, ConsoleColor.Green);
            BCE.console.WriteLine("Inclination " + planet.orbitInclination, ConsoleColor.Green);
            BCE.console.WriteLine("Orbit Index " + planet.orbitIndex, ConsoleColor.Green);
            BCE.console.WriteLine("Orbit Longitude " + planet.orbitLongitude, ConsoleColor.Green);
            BCE.console.WriteLine("Orbit Phase " + planet.orbitPhase, ConsoleColor.Green);
            BCE.console.WriteLine("Orbit Radius " + planet.orbitRadius, ConsoleColor.Green);
            BCE.console.WriteLine("Precision " + planet.precision, ConsoleColor.Green);
            BCE.console.WriteLine("Radius " + planet.radius, ConsoleColor.Green);
            BCE.console.WriteLine("Rotation Period " + planet.rotationPeriod, ConsoleColor.Green);
            BCE.console.WriteLine("Rotation Phase " + planet.rotationPhase, ConsoleColor.Green);
            BCE.console.WriteLine("RuntimeLocalSunDirection " + planet.runtimeLocalSunDirection, ConsoleColor.Green);
            BCE.console.WriteLine("RuntimeOrbitPhase " + planet.runtimeOrbitPhase, ConsoleColor.Green);
            BCE.console.WriteLine("RuntimeOrbitRotation " + planet.runtimeOrbitRotation, ConsoleColor.Green);
            BCE.console.WriteLine("RuntimePosition " + planet.runtimePosition, ConsoleColor.Green);
            BCE.console.WriteLine("RuntimePositionNext " + planet.runtimePositionNext, ConsoleColor.Green);
            BCE.console.WriteLine("RuntimeRotation " + planet.runtimeRotation, ConsoleColor.Green);
            BCE.console.WriteLine("RuntimeRotationNext " + planet.runtimeRotationNext, ConsoleColor.Green);
            BCE.console.WriteLine("RuntimeRotationPhase " + planet.runtimeRotationPhase, ConsoleColor.Green);
            BCE.console.WriteLine("RuntimeSystemRotation " + planet.runtimeSystemRotation, ConsoleColor.Green);
            BCE.console.WriteLine("SingularityString " + planet.singularityString, ConsoleColor.Green);
            BCE.console.WriteLine("Sundistance " + planet.sunDistance, ConsoleColor.Green);
            BCE.console.WriteLine("TemperatureBias " + planet.temperatureBias, ConsoleColor.Green);
            BCE.console.WriteLine("Theme " + planet.theme, ConsoleColor.Green);
            BCE.console.WriteLine("Type " + planet.type, ConsoleColor.Green);
            BCE.console.WriteLine("uPosition " + planet.uPosition, ConsoleColor.Green);
            BCE.console.WriteLine("uPositionNext " + planet.uPositionNext, ConsoleColor.Green);
            BCE.console.WriteLine("Wanted " + planet.wanted, ConsoleColor.Green);
            BCE.console.WriteLine("WaterHeight " + planet.waterHeight, ConsoleColor.Green);
            BCE.console.WriteLine("WaterItemID " + planet.waterItemId, ConsoleColor.Green);
            BCE.console.WriteLine("WindStrength " + planet.windStrength, ConsoleColor.Green);
            BCE.console.WriteLine("Number " + planet.number, ConsoleColor.Green);
            BCE.console.WriteLine("Obliquity " + planet.obliquity, ConsoleColor.Green);
            BCE.console.WriteLine("Name " + planet.name, ConsoleColor.Green);
            BCE.console.WriteLine("mod_y " + planet.mod_y, ConsoleColor.Green);
            BCE.console.WriteLine("mod_x " + planet.mod_x, ConsoleColor.Green);
            BCE.console.WriteLine("Luminosity " + planet.luminosity, ConsoleColor.Green);
            BCE.console.WriteLine("Levelized " + planet.levelized, ConsoleColor.Green);
            BCE.console.WriteLine("Land% " + planet.landPercent, ConsoleColor.Green);
            BCE.console.WriteLine("Ion Height " + planet.ionHeight, ConsoleColor.Green);
            BCE.console.WriteLine("HabitableBias " + planet.habitableBias, ConsoleColor.Green);
            BCE.console.WriteLine("GasTotalHeat " + planet.gasTotalHeat, ConsoleColor.Green);
            BCE.console.WriteLine("BirthResourcePoint1 " + planet.birthResourcePoint1, ConsoleColor.Green);
            BCE.console.WriteLine("BirthResourcePoint0 " + planet.birthResourcePoint0, ConsoleColor.Green);
            BCE.console.WriteLine("BirthPoint " + planet.birthPoint, ConsoleColor.Green);
            BCE.console.WriteLine("Algo ID " + planet.algoId, ConsoleColor.Green);
            BCE.console.WriteLine("---------------------", ConsoleColor.Red);
        }
    }
}