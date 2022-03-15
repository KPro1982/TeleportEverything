using HarmonyLib;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        public static class Inventory_IsTeleportable_Patch
        {
            private static bool Prefix(ref bool __result)
            {
                if (EnableMod.Value)
                {
                    if (TransportOres.Value)
                    {
                        __result = true;
                        return false; //skip original method
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Teleport), nameof(Teleport.GetHoverText))]
        public static class Teleport_GetHoverText_Patch
        {
            private static void Postfix()
            {
                if (!EnableMod.Value)
                    return;

                PopulateEntityLists();

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

                PopulateEntityLists();

                if (TransportAllies && allies.Count > 0)
                {
                    DisplayMessage($"{allies.Count} allies will teleport with you!");
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
                        DisplayMessage(
                            $"Beware: {enemies.Count} enemies may charge the portal!");
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
                    return __result;

                PopulateEntityLists();

                if (enemies.Count > 0 && TeleportMode.Value.Contains("Take"))
                {
                    DisplayMessage(
                        $"Taking Enemies With You! {enemies.Count} enemies charge the portal!!!");

                    foreach (var e in enemies)
                    {
                        if (Random.Range(0, 100) <= 25)
                        {
                            var displacement = Random.insideUnitSphere * MaximumDisplacement.Value;
                            displacement.y = 0;
                            var offset = __instance.transform.forward * SpawnForwardOffset.Value;
                            e.transform.position = pos + offset + displacement;
                            e.transform.rotation = rot;
                        }
                    }
                }

                Debug.Log($"allies: {allies.Count} and flag {TransportAllies}");
                if (allies.Count > 0 && TransportAllies)
                {
                    foreach (var ally in allies)
                    {
                        var offset = __instance.transform.forward * SpawnForwardOffset.Value;
                        ally.transform.position = pos + offset;
                        ally.transform.rotation = rot;
                        if (IncludeFollow)
                        {
                            SetFollow(ally);
                        }
                    }
                }

                return __result;
            }
        }
    }
}