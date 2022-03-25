using System.Collections.Generic;
using System.Dynamic;
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

                hasOre = false;

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

                        hasOre = true;
                    }
                }

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(TeleportWorld))]
        [HarmonyPatch(nameof(TeleportWorld.Teleport))]
        public class Teleport_Patch
        {
            private static void Prefix(ref Player player)
            {
                if (!EnableMod.Value)
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
                    DisplayMessage($"{allies.Count} allies will teleport with you!");
                }

                if (enemies.Count > 0 && TeleportMode.Value.Contains("Take"))
                {
                    DisplayMessage($"Beware: {enemies.Count} enemies may charge the portal!");
                }
            }
        }

        [HarmonyPatch(typeof(Humanoid))]
        [HarmonyPatch(nameof(Humanoid.IsTeleportable))]
        public class IsTeleportable_Patch
        {
            private static bool Postfix(bool __result, Humanoid __instance)
            {
                if (!EnableMod.Value)
                {
                    return __result;
                }

                SetIncludeMode();
                GetCreatures();

                if (TransportAllies && allies.Count > 0)
                {
                    ResetDelayTimer();
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

        [HarmonyPatch(typeof(Player))]
        [HarmonyPatch(nameof(Player.TeleportTo))]
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
                EnemiesSpawn = new List<DelayedSpawn>();
                AlliesSpawn = new List<DelayedSpawn>();
                teleportTriggered = true;
                
                if (enemies.Count > 0 && TeleportMode.Value.Contains("Take"))
                {
                    DisplayMessage(
                        $"Taking Enemies With You! {enemies.Count} enemies charge the portal!!!");

                    CreateEnemyList(pos, rot);
                   
                }

                TeleportEverythingLogger.LogInfo(
                    $"Allies: {allies.Count} and flag {TransportAllies}");
                if (allies.Count > 0 && TransportAllies)
                {
                   CreateAllyList(pos, rot, IncludeFollow);

                }

                return __result;
            }
        }

        [HarmonyPatch(typeof(Player))]
        [HarmonyPatch("UpdateTeleport")]
        public class UpdateTeleport_Patch
        {
            static void  Postfix( Player __instance, ref bool ___m_teleporting, float dt)
            {
                if (!ZNetScene.instance.IsAreaReady(__instance.m_teleportTargetPos))
                    return;

                if (!___m_teleporting && teleportTriggered)
                {
                    UpdateDelayTimer(dt);
                }
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