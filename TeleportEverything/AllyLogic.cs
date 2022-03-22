
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
         public static List<DelayedSpawn> Allies;
       
        public static bool IsValidAlly(Character c)
        {
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

        public static string GetName(Character c) => c?.name.Replace("(Clone)", "").ToLower();

        public static bool IsAllowedAlly(Character c)
        {
            if (GetName(c).Equals("wolf") && !TransportWolves.Value)
            {
                return false;
            }
            if (GetName(c).Equals("boar") && !TransportBoar.Value)
            {
                return false;
            }
            if (GetName(c).Equals("lox") && !TransportLox.Value)
            {
                return false;
            }
            if (!ServerEnableMask.Value && !UserEnableMask.Value)
            {
                return true;
            }

            if (IsAllowedInMask(c, ServerEnableMask.Value, ServerTransportMask.Value) &&
                IsAllowedInMask(c, UserEnableMask.Value, UserTransportMask.Value))
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
            var isInMask = maskList.FirstOrDefault(s => s.Contains(GetName(c)));

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
        

        public static int CountAllies()
        {
            var characters = new List<Character>();
            Character.GetCharactersInRange(Player.m_localPlayer.transform.position,
                SearchRadius.Value, characters);

            List<Character> lAlly = characters.FindAll(IsValidAlly);

            return lAlly.Count;
        }

        public static void CreateAllyList(Vector3 pos, Quaternion rot, bool follow)
        {
            var characters = new List<Character>();
            Allies = new List<DelayedSpawn>();

            Character.GetCharactersInRange(Player.m_localPlayer.transform.position,
                SearchRadius.Value, characters);

            var characterList = characters.FindAll(c => IsValidAlly(c) == true);

            Vector3 offset = Player.m_localPlayer.transform.forward * SpawnForwardOffset.Value;

            float addDelay = 0f;
            foreach (Character c in characterList)
            {
                Allies.Add(new DelayedSpawn(c, true, 10f + addDelay, GetDelayTimer(), pos, rot, offset, follow));
                addDelay += 0.5f;
            }
            
        }

        public static List<DelayedSpawn> GetAllyList()
        {
            return Allies;
        }
    }
}
