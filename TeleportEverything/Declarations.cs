using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TeleportEverything
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal partial class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.kpro.TeleportEverything";
        public const string PluginName = "TeleportEverything";
        public const string PluginVersion = "1.3.0";

        // General
        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<string> TeleportMode;
        public static ConfigEntry<string> MessageMode;

        // Transport Allies
        public static bool TransportAllies;
        public static ConfigEntry<bool> TransportBoar;
        public static ConfigEntry<bool> TransportWolves;
        public static ConfigEntry<bool> TransportLox;
        public static ConfigEntry<string> TransportMask;
        public static ConfigEntry<float> TransportRadius;
        public static ConfigEntry<float> TransportVerticalTolerance;
        public static ConfigEntry<float> SpawnForwardOffset;
        public static bool IncludeTamed;
        public static bool IncludeNamed;
        public static bool IncludeWild;
        public static bool IncludeFollow;
        public static bool ExcludeNamed;
        public static ConfigEntry<string> IncludeMode;

        //Teleport Self
        public static ConfigEntry<float> SearchRadius;
        public static ConfigEntry<float> MaximumDisplacement;
        public static List<Character> enemies;
        public static List<Character> allies;

        //Items
        public static ConfigEntry<bool> RemoveItemsRestriction;
        public static ConfigEntry<bool> TransportDragonEggs;
        public static ConfigEntry<bool> TransportOres;
        public static ConfigEntry<int> TransportOreKeepPct;
        public static bool hasOre;

        private readonly Harmony harmony = new Harmony(PluginGUID);

        private void Awake()
        {
            harmony.PatchAll();
            CreateConfigValues();
            enemies = new List<Character>();
            allies = new List<Character>();

            ClearIncludeVars();
        }

        private void OnDestroy()
        {
            harmony.UnpatchSelf();
        }


        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;

            // General
            EnableMod = Config.Bind("Mod", "Enable Mod", true);
            MessageMode = Config.Bind("Mod", "Message Mode", "No messages",
                new ConfigDescription("Ally Mode",
                    new AcceptableValueList<string>("No messages", "top left", "centered")));


            // Transport

            TransportBoar = Config.Bind("Transport", "Transport Boar", false);
            TransportWolves = Config.Bind("Transport", "Transport Wolves", false);
            TransportLox = Config.Bind("Transport", "Transport Lox", false);
            TransportMask = Config.Bind("Transport", "Transport Mask", "");

            IncludeMode = Config.Bind("Transport", "Ally Mode", "No Allies",
                new ConfigDescription("Ally Mode",
                    new AcceptableValueList<string>("No Allies", "All tamed", "Only Follow",
                        "All tamed except Named", "Only Named")));

            TransportRadius = Config.Bind("Transport", "Transport Radius", 10f);
            TransportVerticalTolerance =
                Config.Bind("Transport", "Transport Vertical Tolerance", 2f);
            SpawnForwardOffset = Config.Bind("Transport", "Spawn forward Tolerance", .5f);

            // Transport.Items
            RemoveItemsRestriction = Config.Bind("Transport Items", "Remove Items Restriction", false,
                new ConfigDescription("Allows transporting all items."));
            TransportDragonEggs = Config.Bind("Transport Items", "Transport Dragon Eggs", false);
            TransportOres = Config.Bind("Transport Items", "Transport Ores", false,
                new ConfigDescription("Allows transporting ores, ingots and other restricted items."));
            TransportOreKeepPct = Config.Bind("Transport Items Config", "Transport Ore Keep Percentage", 100,
                new ConfigDescription("Percentage of ore to be keept on inventory when teleporting. (%)",
                new AcceptableValueRange<int>(0, 100)));

            // Teleport Self
            SearchRadius = Config.Bind("Teleport Self", "Search Radius", 10f);
            MaximumDisplacement = Config.Bind("Teleport Self", "Max Enemy Displacement", 5f);
            TeleportMode = Config.Bind("Teleport Self", "Teleport Mode", "Standard",
                new ConfigDescription("Teleport Mode",
                    new AcceptableValueList<string>("Standard", "Vikings Don't Run",
                        "Take Them With You")));
        }

        private static void ClearIncludeVars()
        {
            TransportAllies = false;
            IncludeTamed = false;
            IncludeNamed = false;
            IncludeWild = false;
            IncludeFollow = false;
            ExcludeNamed = false;
        }

        public static void SetIncludeMode()
        {
            ClearIncludeVars();

            if (!IncludeMode.Value.Contains("No Allies"))
            {
                TransportAllies = true;
            }

            if (IncludeMode.Value.Equals("All tamed"))
            {
                IncludeTamed = true;
                IncludeNamed = true;
                IncludeFollow = true;
            }

            if (IncludeMode.Value.Contains("Only Follow"))
            {
                IncludeFollow = true;
            }

            if (IncludeMode.Value.Contains("All tamed except Named"))
            {
                IncludeTamed = true;
                ExcludeNamed = true;
                IncludeFollow = true;
            }

            if (IncludeMode.Value.Contains("Only Named"))
            {
                IncludeNamed = true;
            }
        }

       

        
    }
}