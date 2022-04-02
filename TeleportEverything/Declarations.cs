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
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    internal partial class Plugin : BaseUnityPlugin
    {
        internal const string ModName = "TeleportEverything";
        internal const string ModVersion = "1.6.0";
        internal const string Author = "kpro";
        private const string ModGUID = "com."+ Author + "." + ModName;

        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource TeleportEverythingLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
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

        //Enemies
        public static ConfigEntry<float>? SpawnEnemiesForwardOffset;
        public static ConfigEntry<float>? MaximumDisplacement;

        //Portal
        public static ConfigEntry<float>? PortalActivationRange;
        public static ConfigEntry<float>? PortalSoundVolume;

        //Teleport Self
        public static ConfigEntry<float>? SearchRadius;
        public static List<Character>? enemies;
        public static List<Character>? allies;

        //Items
        public static ConfigEntry<bool>? TransportDragonEggs;
        public static ConfigEntry<bool>? TransportOres;
        public static ConfigEntry<int>? TransportFee;

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

        private void Awake()
        {
            _harmony.PatchAll();
            CreateConfigValues();
            SetupWatcher();

            enemies = new List<Character>();
            allies = new List<Character>();

            ClearIncludeVars();
            Debug.Log($"{ModName} Loaded...");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
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

            //Portal
            PortalSoundVolume = config("--- Portal ---", "Portal Sound Volume", 0.8f,
                new ConfigDescription("Portal sound effect volume.",
                    new AcceptableValueRange<float>(0, 1)), false);

            PortalActivationRange = config("--- Portal ---", "Portal Activation Range", 5f,
                new ConfigDescription("Portal activation range in meters.",
                    new AcceptableValueRange<float>(0, 20f),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 8 }), false);

            // Transport
            IncludeMode = config("--- Transport ---", "Ally Mode", "No Allies",
                new ConfigDescription("Ally Mode",
                    new AcceptableValueList<string>("No Allies", "All tamed", "Only Follow",
                        "All tamed except Named", "Only Named"),
                    new ConfigurationManagerAttributes { IsAdvanced = false, Order = 7 }), false);

            TransportRadius = config("--- Transport ---", "Transport Radius", 10f,
                new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 6 }));

            TransportVerticalTolerance = config("--- Transport ---", "Vertical Tolerance", 2f,
                new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));

            SpawnForwardOffset = config("--- Transport ---", "Spawn Forward Tolerance", .5f,
            new ConfigDescription("Allies spawn forward offset",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            PlayerEnableMask = config("--- Transport ---", "Player Filter By Mask", false,
                new ConfigDescription("Enable to filter which tameable creatures can teleport.", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }), false);

            PlayerTransportMask = config("--- Transport ---", "Player Transport Mask", "",
                new ConfigDescription("Add the prefab names to filter creatures to transport", null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }), false);

            TransportBoar = config("--- Transport ---", "Transport Boars", true, "", false);
            TransportLox = config("--- Transport ---", "Transport Loxes", true, "", false);
            TransportWolves = config("--- Transport ---", "Transport Wolves", true, "", false);

            //Enemies
            SpawnEnemiesForwardOffset = config("--- Transport Enemies ---", "Spawn Enemies Forward Tolerance", 6.2f,
            new ConfigDescription("Spawn forward in meters if Take Enemies With you is enabled.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));

            MaximumDisplacement = config("--- Transport Enemies ---", "Max Enemy Displacement", .5f,
            new ConfigDescription("Max Enemy Displacement if Take Enemies With you is enabled.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));


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
                new ConfigDescription("Allows transporting dragon eggs."));
            TransportOres = config("--- Transport Items ---", "Transport Ores", false,
                new ConfigDescription(
                    "Allows transporting ores, ingots and other restricted items."));
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

        #region ConfigWatcher
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
        #endregion

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