using HarmonyLib;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        public static bool teleportTriggered = false;
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        public static class Inventory_IsTeleportable_Patch
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
                            __result = false;
                            return false;
                        }
                    }
                    else
                    {
                        if (!TransportOres.Value)
                        {
                            __result = false;
                            return false;
                        }
                    }
                }

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(TeleportWorld))]
        [HarmonyPatch(nameof(TeleportWorld.Teleport))]
        public class TeleportWorld_Teleport_Patch
        {
            private static void Prefix(ref Player player, TeleportWorld __instance)
            {
                if (!EnableMod.Value)
                {
                    return;
                }

                if (!__instance.TargetFound() || !player.IsTeleportable() || ZoneSystem.instance.GetGlobalKey("noportals")) //avoid fee if player is not teleportable
                {
                    return;
                }

                if (!TransportOres.Value)
                {
                    return;
                }

                if (TransportFee.Value == 0)
                {
                    return;
                }

                ReduceStacks(player);

                RemoveEmptyItems(player);
            }
        }

        [HarmonyPatch(typeof(Teleport), nameof(Teleport.GetHoverText))]
        public static class Teleport_GetHoverText_Patch
        {
            private static void Postfix()
            {
                if (!EnableMod.Value)
                {
                    return;
                }

                SetIncludeMode();
                GetCreatures();

                if (TransportAllies && allies.Count > 0)
                {
                    DisplayMessage($"Transporting {allies.Count} allies!");
                }

                if (enemies.Count > 0 && TeleportMode.Value.Contains("Take"))
                {
                    DisplayMessage($"Beware: {enemies.Count} enemies may charge the portal!");
                }
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.IsTeleportable))]
        public class IsTeleportable_Patch
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
                
                if (!__result) //if player inventory teleportable is false
                {
                    return __result;
                }

                SetIncludeMode();
                GetCreatures();

                if (TransportAllies && allies.Count > 0)
                {
                    DisplayMessage($"Transporting {allies.Count} allies!");
                }

                if (enemies.Count > 0)
                {
                    if (TeleportMode.Value.Contains("Run"))
                    {
                        DisplayMessage(
                            $"Vikings Don't run from a fight: {enemies.Count} enemies with in {SearchRadius.Value} meters.");

                        return false;
                    }

                    if (TeleportMode.Value.Contains("Take"))

                    {
                        DisplayMessage($"Beware: {enemies.Count} enemies may charge the portal!");
                    }
                }

                return __result;
            }
        }


        [HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
        public class TeleportTo_Patch
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

                SetIncludeMode();
                GetCreatures();

                teleportTriggered = true;
                
                if (enemies.Count > 0 && TeleportMode.Value.Contains("Take"))
                {
                    DisplayMessage(
                        $"Taking Enemies With You! {enemies.Count} enemies charge the portal!!!");

                    TeleportCreatures(__instance, enemies, true);
                }

                TeleportEverythingLogger.LogInfo(
                    $"Allies: {allies.Count} and flag {TransportAllies}");
                if (allies.Count > 0 && TransportAllies)
                {
                    TeleportCreatures(__instance, allies);
                }
                return __result;
            }
        }

        [HarmonyPatch(typeof(Tameable), nameof(Tameable.RPC_Command))]
        public class Tameable_RPC_Command_Patch
        {
            private static void Postfix(Tameable __instance, ZDOID characterID)
            {
                if (__instance.m_character.GetComponent<ZNetView>() is { } netView)
                {
                    TakeOwnership(__instance.m_character, characterID.m_userID);                   
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateTeleport))]
        public class UpdateTeleport_Patch
        {
            static void Postfix(Player __instance, ref bool ___m_teleporting, float dt)
            {
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

                if (!___m_teleporting && teleportTriggered)
                {
                    teleportTriggered = false;
                    CreateDelayedSpawn();
                }
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Awake))]
        public class TeleportWorld_Awake_Patch
        {
            static void Postfix(TeleportWorld __instance)
            {
                AudioSource audio = __instance.GetComponentInChildren<AudioSource>();
                if(audio != null)
                {
                    audio.volume = PortalSoundVolume.Value;
                }
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.UpdatePortal))]
        public class UpdatePortal_Patch
        {
            static void Prefix(ref float ___m_activationRange)
            {
                ___m_activationRange = PortalActivationRange.Value;
            }
        }

        public static DelayedAction delayedAction;
        [HarmonyPatch(typeof(Game), nameof(Game.Awake))]
        public class GameAwake_Patch
        {
            static void Postfix(Game __instance)
            {
                if (delayedAction == null)
                {
                    delayedAction = __instance.gameObject.AddComponent<DelayedAction>();
                }
            }
        }
    }
}