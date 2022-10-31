using BepInEx.Bootstrap;
using HarmonyLib;
using System;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        #region SkyheimCompatibility

        internal const string skyheimName = "skyheim";
        internal static bool isSkyheimBlink = false;

        private void CheckAndPatchSkyheim()
        {
            if (!Chainloader.PluginInfos.ContainsKey(skyheimName)) return;

            var blinkClass = Type.GetType("SkyheimAttackBlink, skyheim");
            if (blinkClass != null)
            {
                try
                {
                    var method = AccessTools.Method(blinkClass, "OnAttackTrigger");
                    _harmony.Patch(method,
                        new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(SkyheimBlinkPrefix))),
                        new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(SkyheimBlinkPostfix)))
                    );
                    TeleportEverythingLogger.LogInfo("Skyheim settings applied");
                }
                catch (Exception ex)
                {
                    TeleportEverythingLogger.LogInfo($"Failed to apply settings to skyheim: {ex.Message}");
                }
            }
        }

        private static void SkyheimBlinkPrefix() => isSkyheimBlink = true;
        private static void SkyheimBlinkPostfix() => isSkyheimBlink = false;

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
