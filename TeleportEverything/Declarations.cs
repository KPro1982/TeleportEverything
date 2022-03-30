using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;

#nullable enable
namespace TeleportEverything
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal partial class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.kpro.TeleportEverything";
        public const string PluginName = "TeleportEverything";
        public const string PluginVersion = "1.5.0";
        private static string ConfigFileName = PluginGUID + ".cfg";

        private static string ConfigFileFullPath =
            Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        // Mod
        private static ConfigEntry<bool>? _serverConfigLocked;
        public static ConfigEntry<bool>? EnableMod;
        public static ConfigEntry<string>? TeleportMode;

        // Transport Allies
        public static ConfigEntry<bool>? ServerEnableMask;
        public static ConfigEntry<string>? ServerTransportMask;
        public static ConfigEntry<float>? TransportRadius;
        public static ConfigEntry<float>? TransportVerticalTolerance;
        public static ConfigEntry<float>? SpawnForwardOffset;
        public static ConfigEntry<bool>? TransportWolves;
        public static ConfigEntry<bool>? TransportBoar;
        public static ConfigEntry<bool>? TransportLox;
        public static ConfigEntry<int>? SpawnEnemiesDelay;

        public const float DEFAULT_DELAY = 2f;

        //Teleport Self
        public static ConfigEntry<float>? SearchRadius;
        public static List<Character>? enemies;
        public static List<Character>? allies;

        //Items
        public static ConfigEntry<bool>? TransportDragonEggs;
        public static ConfigEntry<bool>? TransportOres;
        public static ConfigEntry<int>? TransportFee;
        public static bool hasOre;

        //User Settings
        public static ConfigEntry<string>? IncludeMode;
        public static ConfigEntry<string>? MessageMode;
        public static ConfigEntry<bool>? PlayerEnableMask;
        public static ConfigEntry<string>? PlayerTransportMask;

        // Include vars
        public static bool TransportAllies;
        public static bool IncludeTamed;
        public static bool IncludeNamed;
        public static bool IncludeWild;
        public static bool IncludeFollow;
        public static bool ExcludeNamed;

        private readonly Harmony harmony = new(PluginGUID);
        
        //Teleport Timer
        public static float DelayTimer;


        public static readonly ManualLogSource TeleportEverythingLogger =
            BepInEx.Logging.Logger.CreateLogSource(PluginName);

        private static readonly ConfigSync ConfigSync = new(PluginGUID)
        {
            DisplayName = PluginName, CurrentVersion = PluginVersion,
            MinimumRequiredVersion = PluginVersion
        };


        private void Awake()
        {
            harmony.PatchAll();
            CreateConfigValues();
            SetupWatcher();

            enemies = new List<Character>();
            allies = new List<Character>();

            ClearIncludeVars();
            DelayTimer = 0;
            Debug.Log($"{PluginName} Loaded...");
        }

        private void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        #region CreateConfigValues
        private void CreateConfigValues()
        {
            //Mod
            _serverConfigLocked = config("--- Mod ---", "Force Server Config", true, "Force Server Config");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            EnableMod = config("--- Mod ---", "Enable Mod", true, "Enable/Disable mod");
            MessageMode = config("--- Mod ---", "Message Mode", "No messages",
                new ConfigDescription("Message Mode",
                    new AcceptableValueList<string>("No messages", "top left", "centered")), false);

            // Transport
            IncludeMode = config("--- Transport ---", "Ally Mode", "No Allies",
                new ConfigDescription("Ally Mode",
                    new AcceptableValueList<string>("No Allies", "All tamed", "Only Follow",
                        "All tamed except Named", "Only Named"),
                    new ConfigurationManagerAttributes { IsAdvanced = false, Order = 7 }), false);

            SpawnEnemiesDelay = config("--- Transport ---", "Spawn Enemies Delay", 3,
                new ConfigDescription("Add extra seconds to enemies spawn when teleporting",
                    new AcceptableValueRange<int>(0, 10),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 6 }));

            TransportRadius = config("--- Transport ---", "Transport Radius", 10f,
                new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));

            TransportVerticalTolerance = config("--- Transport ---", "Vertical Tolerance", 2f,
                new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            SpawnForwardOffset = config("--- Transport ---", "Spawn Forward Tolerance", .5f,
            new ConfigDescription("", null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

            PlayerEnableMask = config("--- Transport ---", "Player Filter By Mask", false,
                new ConfigDescription("Enable to filter which tameable creatures can teleport.", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }), false);

            PlayerTransportMask = config("--- Transport ---", "Player Transport Mask", "",
                new ConfigDescription("Add the prefab names to filter creatures to transport", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }), false);

            TransportBoar = config("--- Transport ---", "Transport Boars", true, "", false);
            TransportLox = config("--- Transport ---", "Transport Loxes", true, "", false);
            TransportWolves = config("--- Transport ---", "Transport Wolves", true, "", false);

            //Server
            ServerEnableMask = config("--- Server ---", "Server Filter By Mask", false,
                new ConfigDescription(
                    "Enable to filter which tameable creatures can teleport on server.", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }));
            ServerTransportMask = config("--- Server ---", "Server Transport Mask", "",
                new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }));

            // Transport Items
            TransportDragonEggs = config("--- Transport Items ---", "Transport Dragon Eggs", false,
                new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));
            TransportOres = config("--- Transport Items ---", "Transport Ores", false,
                new ConfigDescription(
                    "Allows transporting ores, ingots and other restricted items.", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));
            TransportFee = config("--- Transport Items ---", "Transport fee", 10,
                new ConfigDescription("Transport Fee in (%) ore",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));

            // Teleport Self
            SearchRadius = config("--- Portal Behavior ---", "Search Radius", 10f,
                new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
            TeleportMode = config("--- Portal Behavior ---", "Teleport Mode", "Standard",
                new ConfigDescription("Teleport Mode",
                    new AcceptableValueList<string>("Standard", "Vikings Don't Run",
                        "Take Them With You"), new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));            
        }
        #endregion

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

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath))
            {
                return;
            }

            try
            {
                TeleportEverythingLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                TeleportEverythingLogger.LogError(
                    $"There was an issue loading your {ConfigFileName}");
                TeleportEverythingLogger.LogError(
                    "Please check your config entries for spelling and format!");
            }
        }

        #region ConfigOptions

        private ConfigEntry<T> config<T>(string group, string name, T value,
            ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description + (synchronizedSetting
                        ? " [Synced with Server]"
                        : " [Not Synced with Server]"), description.AcceptableValues,
                    description.Tags);
            var configEntry = Config.Bind(group, name, value, extendedDescription);

            var syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true) => config(group, name, value,
            new ConfigDescription(description), synchronizedSetting);

        #endregion
    }
}