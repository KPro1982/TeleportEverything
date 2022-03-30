
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        public static List<DelayedSpawn> AlliesSpawn;
        public static bool IsValidAlly(Character c)
        {
            if (!c.IsTamed())
            {
                return false;
            }

            if (IsAllowedAlly(c) && IsTransportable(c))
            {
                if (HorizontalDistance(c) <= TransportRadius.Value &&
                    VerticalDistance(c) <= TransportVerticalTolerance.Value)
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetPrefabName(Character c) => c?.name.Replace("(Clone)", "").ToLower();

        public static bool IsAllowedAlly(Character c)
        {
            if (GetPrefabName(c).Equals("wolf") && !TransportWolves.Value)
            {
                return false;
            }
            if (GetPrefabName(c).Equals("boar") && !TransportBoar.Value)
            {
                return false;
            }
            if (GetPrefabName(c).Equals("lox") && !TransportLox.Value)
            {
                return false;
            }
            if (!ServerEnableMask.Value && !PlayerEnableMask.Value)
            {
                return true;
            }

            if (IsAllowedInMask(c, ServerEnableMask.Value, ServerTransportMask.Value) &&
                IsAllowedInMask(c, PlayerEnableMask.Value, PlayerTransportMask.Value))
            {
                return true;
            }

            return false;
        }

        private static bool IsAllowedInMask(Character c, bool enableMask, string transportMask)
        {
            if (!enableMask)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(transportMask))
            {
                return false;
            }

            if (IsInFilterMask(c, transportMask))
            {
                return true;
            }

            return false;
        }


        private static bool IsInFilterMask(Character c, string mask)

        {
            List<string> maskList = mask.Split(',').Select(p => p.Trim().ToLower()).ToList();
            var isInMask = maskList.FirstOrDefault(s => s.Contains(GetPrefabName(c)));

            return isInMask != null;
        }

        public static bool IsTransportable(Character ally)
        {
            if (IsNamed(ally) && ExcludeNamed)
            {
                return false;
            }

            if (IsFollowing(ally) && IncludeFollow)
            {
                return true;
            }

            if (IsNamed(ally) && IncludeNamed)
            {
                return true;
            }

            if (ally.IsTamed() && IncludeTamed)
            {
                return true;
            }

            return false;
        }


        public static bool IsNamed(Character t)
        {
            var name = t.GetComponent<Tameable>()?.GetText();

            return !string.IsNullOrEmpty(name);
        }

        public static bool IsFollowing(Character f)
        {
            var mAi = f.GetComponent<MonsterAI>();

            if (mAi != null && mAi.GetFollowTarget() != null &&
                mAi.GetFollowTarget().Equals(Player.m_localPlayer.gameObject))
            {

                return true;
            }

            return false;
        }
        

        public static List<Character> GetAllies(List<Character> creatures)
        {
            return creatures.FindAll(IsValidAlly);
        }

        public static void CreateAllyList(Vector3 pos, Quaternion rot, bool follow)
        {
            Vector3 offset = Player.m_localPlayer.transform.forward * SpawnForwardOffset.Value;

            float addDelay = 0f;
            foreach (Character c in allies)
            {
                AlliesSpawn.Add(new DelayedSpawn(c, true, DEFAULT_DELAY + addDelay, GetDelayTimer(), pos, rot, offset, IsFollowing(c)));
                addDelay += 0.8f;
            } 
        }
    }
}
