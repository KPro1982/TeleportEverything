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

                if (TransportAllies && CountAllies() > 0)
                {
                    DisplayMessage($"{CountAllies()} allies will teleport with you!");
                }

                if (CountEnemies() > 0)
                {
                    if (TeleportMode.Value.Contains("Run"))
                    {
                        DisplayMessage(
                            $"Vikings Don't run from a fight: {CountEnemies()} enemies with in {SearchRadius.Value} meters.");
                        return false;
                    }

                    if (TeleportMode.Value.Contains("Take"))

                    {
                        DisplayMessage(
                            $"Beware: {CountEnemies()} enemies may charge the portal!");
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

                if (CountEnemies() > 0 && TeleportMode.Value.Contains("Take"))
                {
                    DisplayMessage(
                        $"Taking Enemies With You! {CountEnemies()} enemies charge the portal!!!");

                    foreach (var e in GetEnemyList(pos, rot))
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

                Debug.Log($"allies: {CountAllies()} and flag {TransportAllies}");
                if (CountAllies() > 0 && TransportAllies)
                {
                    foreach (var ally in GetAllyList(pos, rot, IncludeFollow))
                    {
                        var offset = __instance.transform.forward * SpawnForwardOffset.Value;
                        ally.transform.position = pos + offset;
                        ally.transform.rotation = rot;
                        if (IncludeFollow)
                        {
                            SetFollow(ally.character);
                        }
                    }

                    return __result;
                }

                return __result;
            }
        }
    }
}