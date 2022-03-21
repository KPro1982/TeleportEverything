using System.Collections.Generic;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        public static List<DelayedSpawn> Allies;
       
        public static bool IsValidAlly(Character c)
        {
            if (IsEligibleCreature(c) && IsTransportable(c))
            {
                if (HorizontalDistance(c) <= TransportRadius.Value &&
                    VerticalDistance(c) <= TransportVerticalTolerance.Value)
                {
                    return true;
                }
            }

            return false;
        }



        public static string GetName(Character c)

        {
            string r = c?.name.Replace("(Clone)", "").ToLower();
            
            return r;
        }

        public static bool IsEligibleCreature(Character c)
        {
            if (GetName(c).Contains("wolf") && TransportWolves.Value)
            {

                return true;
            }

            if (GetName(c).Contains("boar") && TransportBoar.Value)
            {
                return true;
            }

            if (GetName(c).Contains("lox") && TransportLox.Value)
            {
                return true;
            }

            if (IsInCreatureMask(c) && TransportMask.Value != "")
            {
                return true;
            }

            return false;
        }

        private static bool IsInCreatureMask(Character c)
        {
            var includeList = TransportMask.Value.Split(',');
            foreach (var s in includeList)
            {
                if (GetName(c).Contains(s.ToLower().Trim()))
                {
                    return true;
                }
            }

            return false;
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
