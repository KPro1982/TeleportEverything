using HarmonyLib;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        [HarmonyPatch(typeof(Humanoid))]
        [HarmonyPatch("IsTeleportable")]
        public class IsTeleportable_Patch
        {
            private static bool Postfix(bool __result, Humanoid __instance)
            {
                SetIncludeMode();

                if (!EnableMod.Value)
                {
                    return __result;
                }

                if (TransportAllies && GetAllies().Count > 0)
                {
                    DisplayMessage($"{GetAllies().Count} allies will teleport with you!");
                }

                if (GetEnemies().Count > 0)
                {
                    if (TeleportMode.Value.Contains("Run"))
                    {
                        DisplayMessage(
                            $"Vikings Don't run from a fight: {GetEnemies().Count} enemies with in {SearchRadius.Value} meters.");
                        return false;
                    }

                    if (TeleportMode.Value.Contains("Take"))

                    {
                        DisplayMessage(
                            $"Beware: {GetEnemies().Count} enemies may charge the portal!");
                    }
                }

                return __result;
            }
        }
        [HarmonyPatch(typeof(Player))]
        [HarmonyPatch("TeleportTo")]
        public class TeleportTo_Patch
        {
            private static bool Postfix(bool __result, Player __instance, Vector3 pos,
                Quaternion rot, bool distantTeleport)
            {
                if (!EnableMod.Value)
                {
                    return __result;
                }

                SetIncludeMode();

                if (GetEnemies().Count > 0 && TeleportMode.Value.Contains("Take"))
                {
                    DisplayMessage(
                        $"Taking Enemies With You! {GetEnemies().Count} enemies charge the portal!!!");

                    foreach (var e in GetEnemies())
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

                    return __result;
                }

                Debug.Log($"allies: {GetAllies().Count} and flag {TransportAllies}");
                if (GetAllies().Count > 0 && TransportAllies)
                {
                    foreach (var ally in GetAllies())
                    {
                        var offset = __instance.transform.forward * SpawnForwardOffset.Value;
                        ally.transform.position = pos + offset;
                        ally.transform.rotation = rot;
                        if (IncludeFollow)
                        {
                            SetFollow(ally);
                        }
                    }

                    return __result;
                }

                return __result;
            }
        }
    }
}