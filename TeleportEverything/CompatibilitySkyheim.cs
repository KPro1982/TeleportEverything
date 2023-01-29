using BepInEx.Bootstrap;
using HarmonyLib;
using System;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        #region SkyheimCompatibility

        internal const string SkyheimGuid = "skyheim";
        internal static bool SkyheimAvoidCreatures;

        private void CheckAndPatchSkyheim()
        {
            if (!Chainloader.PluginInfos.ContainsKey(SkyheimGuid)) return;

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

        private static void SkyheimMethodPrefix() => SkyheimAvoidCreatures = true;
        private static void SkyheimMethodPostfix() => SkyheimAvoidCreatures = false;

        [HarmonyPatch(typeof(Game), nameof(Game._RequestRespawn))]
        public class _RequestRespawn_Patch
        {
            static void Prefix()
            {
                if (!IsModEnabled()) return;

                var player = Player.m_localPlayer;
                if (player is not null && !player.IsDead())
                {
                    ApplyTax(player);
                }
            }
            static void Postfix()
            {
                if (!IsModEnabled()) return;

                if (totalContrabandCount > 0)
                {
                    DisplayMessage(Localization.instance.Localize("$te_deducted_items_message", deductedContraband.ToString(), totalContrabandCount.ToString()));
                    deductedContraband = 0;
                    totalContrabandCount = 0;
                }
            }
        }
        #endregion
    }
}
