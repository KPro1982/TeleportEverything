using HarmonyLib;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        public static class InventoryIsTeleportablePatch
        {
            private static bool Prefix(ref bool __result, ref Inventory __instance)
            {
                if (!EnableMod.Value)
                {
                    return true; //go to original method
                }

                if (TransportDragonEggs.Value && TransportOres.Value)
                {
                    __result = true;
                    return false; //skip original method
                }

                foreach (var item in __instance.GetAllItems())
                {
                    if (item.m_shared.m_teleportable)
                    {
                        continue;
                    }

                    if (IsDragonEgg(item))
                    {
                        if (!TransportDragonEggs.Value)
                        {
                            return true; //go to original method
                        }
                    }
                    else
                    {
                        if (!TransportOres.Value)
                        {
                            return true; //go to original method
                        }
                    }
                }

                __result = true;
                return false; //skip original method
            }
        }

        [HarmonyPatch(typeof(Teleport), nameof(Teleport.Interact))]
        public static class TeleportInteractPatch
        {
            private static void Prefix(Humanoid character, bool hold, bool alt, Teleport __instance)
            {
                if (!EnableMod.Value)
                {
                    return;
                }
                if (hold)
                {
                    return;
                }
                if (__instance.m_targetPoint == null)
                {
                    return;
                }
                IsDungeonTeleport = true;
            }

            private static void Postfix()
            {
                if (!EnableMod.Value)
                {
                    return;
                }
                IsDungeonTeleport = false;
            }
        }

        [HarmonyPatch(typeof(Teleport), nameof(Teleport.GetHoverText))]
        public static class TeleportGetHoverTextPatch
        {
            private static void Postfix()
            {
                if (!EnableMod.Value)
                {
                    return;
                }

                SetIncludeMode();
                GetCreatures();

                DisplayAlliesMessage();
                DisplayEnemiesMessage();
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.IsTeleportable))]
        public class IsTeleportablePatch
        {
            private static bool Postfix(bool __result, Humanoid __instance)
            {
                if (!EnableMod.Value)
                {
                    return __result;
                }

                if (Game.instance.m_firstSpawn) //if player is still spawning
                {
                    return __result;
                }

                if (ShowVikingsDontRun)
                {
                    ShowVikingsDontRun = false;
                }
                
                if (!__result) //if player inventory teleportable is false
                {
                    return __result;
                }

                if (skyheimAvoidCreatures)
                {
                    return __result; //skip if it is skyheim blink/recall
                }

                SetIncludeMode();
                GetCreatures();

                DisplayAlliesMessage();

                if (Enemies.Count > 0)
                {
                    if (TeleportMode.Value.Contains("Run"))
                    {
                        ShowVikingsDontRun = true;
                        return false;
                    }

                    DisplayEnemiesMessage();
                }

                return __result;
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
        public class TeleportWorldTeleport_Patch
        {
            static void Postfix()
            {
                if (!EnableMod.Value)
                {
                    return;
                }

                if(ShowVikingsDontRun)
                {
                    DisplayMessage(Localization.instance.Localize("$te_vikings_dont_run", Enemies.Count.ToString(), SearchRadius.Value.ToString()));
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
        public class TeleportToPatch
        {
            private static bool Postfix(bool __result, Player __instance, Vector3 pos,
                Quaternion rot, bool distantTeleport)
            {
                if (!EnableMod.Value)
                {
                    return __result;
                }

                if (!__instance.IsTeleporting())
                {
                    return __result;
                }

                if (skyheimAvoidCreatures) {
                    return __result; //skip if it is skyheim blink/recall
                }

                SetIncludeMode();
                GetCreatures();
                TeleportTriggered = true;

                if (!IsDungeonTeleport)
                {
                    ApplyTax(__instance);
                }

                if (Enemies.Count > 0 && TeleportMode.Value.Contains("Take"))
                {
                    TeleportCreatures(__instance, Enemies, true);
                }

                TeleportEverythingLogger.LogInfo(Localization.instance.Localize("$te_transported_allies_message", Allies.Count.ToString(), TransportAllies.ToString()));
                if (Allies.Count > 0 && TransportAllies)
                {
                    TeleportCreatures(__instance, Allies);
                }
                return __result;
            }
        }

        [HarmonyPatch(typeof(Tameable), nameof(Tameable.RPC_Command))]
        public class TameableRPCCommandPatch
        {
            private static void Postfix(Tameable __instance, ZDOID characterID)
            {
                if (!EnableMod.Value)
                {
                    return;
                }

                if (__instance.m_character.GetComponent<ZNetView>() is { } netView)
                {
                    TakeOwnership(__instance.m_character, characterID.m_userID);                   
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateTeleport))]
        public class UpdateTeleportPatch
        {
            static void Postfix(Player __instance, ref bool ___m_teleporting, float dt)
            {
                if (!EnableMod.Value)
                {
                    return;
                }

                if (!ZNetScene.instance.IsAreaReady(__instance.m_teleportTargetPos))
                    return;

                //set enemies unalerted while teleporting?
                //if(___m_teleporting && teleportTriggered)
                //{
                //    if (TeleportMode.Value.Contains("Take"))
                //    {
                //        foreach (Character c in enemies)
                //        {
                //            c.GetComponent<MonsterAI>()?.SetAlerted(false);
                //        }
                //    }
                //}

                if (!___m_teleporting && TeleportTriggered)
                {
                    TeleportTriggered = false;
                    //TeleportEverythingLogger.LogInfo("Teleport ended");
                    if (totalContrabandCount > 0)
                    {
                        DisplayMessage(Localization.instance.Localize("$te_deducted_items_message", deductedContraband.ToString(), totalContrabandCount.ToString()));
                        deductedContraband = 0;
                        totalContrabandCount = 0;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Awake))]
        public class TeleportWorldAwakePatch
        {
            static void Postfix(TeleportWorld __instance)
            {
                if (!EnableMod.Value)
                {
                    return;
                }

                AudioSource audio = __instance.GetComponentInChildren<AudioSource>();
                if(audio != null)
                {
                    audio.volume = PortalSoundVolume.Value;
                }
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.UpdatePortal))]
        public class UpdatePortalPatch
        {
            static void Prefix(ref float ___m_activationRange)
            {
                if (!EnableMod.Value)
                {
                    return;
                }

                ___m_activationRange = PortalActivationRange.Value;
            }
        }
    }
}