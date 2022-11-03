using BepInEx.Bootstrap;
using HarmonyLib;
using System;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        #region SkyheimCompatibility

        internal const string skyheimGUID = "skyheim";
        internal static bool skyheimAvoidCreatures = false;

        private void CheckAndPatchSkyheim()
        {
            if (!Chainloader.PluginInfos.ContainsKey(skyheimGUID)) return;

            var blinkClass = Type.GetType($"SkyheimAttackBlink, skyheim");
            if (blinkClass != null)
            {
                try
                {
                    var method = AccessTools.Method(blinkClass, "OnAttackTrigger");
                    _harmony.Patch(method,
                        new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(SkyheimMethodPrefix))),
                        new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(SkyheimMethodPostfix)))
                    );
                    TeleportEverythingLogger.LogInfo("Skyheim blink settings applied");
                }
                catch (Exception ex)
                {
                    TeleportEverythingLogger.LogInfo($"Failed to apply blink settings to skyheim: {ex.Message}");
                }
            }

            var recallClass = Type.GetType($"SkyheimAttackRecall, skyheim");
            if (recallClass != null)
            {
                try
                {
                    var method = AccessTools.Method(recallClass, "OnAttackStart");
                    _harmony.Patch(method,
                        new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(SkyheimMethodPrefix))),
                        new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(SkyheimMethodPostfix)))
                    );
                    TeleportEverythingLogger.LogInfo("Skyheim recall settings applied");
                }
                catch (Exception ex)
                {
                    TeleportEverythingLogger.LogInfo($"Failed to apply recall settings to skyheim: {ex.Message}");
                }
            }
        }

        private static void SkyheimMethodPrefix() => skyheimAvoidCreatures = true;
        private static void SkyheimMethodPostfix() => skyheimAvoidCreatures = false;

        [HarmonyPatch(typeof(Game), nameof(Game._RequestRespawn))]
        public class _RequestRespawn_Patch
        {
            static void Prefix()
            {
                if (!EnableMod.Value) return;

                var player = Player.m_localPlayer;
                if (player is not null && !player.IsDead())
                {
                    ApplyTax(player);
                }
            }
            static void Postfix()
            {
                if (!EnableMod.Value) return;

                if (totalContrabandCount > 0)
                {
                    DisplayMessage(
                        $"{deductedContraband} out of {totalContrabandCount} items deducted as a fee for transporting contraband.");
                    deductedContraband = 0;
                    totalContrabandCount = 0;
                }
            }
        }
        #endregion
    }
}
