
using System;
using System.Collections.Generic;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        public static bool IsValidAlly(Character c)
        {
            if (!c.IsTamed())
            {
                return false;
            }

            if (IsAllowedAlly(c) && IsTransportable(c))
            {
                if (HorizontalDistance(c) <= TransportRadius?.Value &&
                    VerticalDistance(c) <= TransportVerticalTolerance?.Value)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsAllowedAlly(Character c)
        {
            if (GetPrefabName(c).Equals(WOLF, StringComparison.OrdinalIgnoreCase) && TransportWolves?.Value == false) return false;
            if (GetPrefabName(c).Equals(BOAR, StringComparison.OrdinalIgnoreCase) && TransportBoar?.Value == false) return false;
            if (GetPrefabName(c).Equals(LOX, StringComparison.OrdinalIgnoreCase) && TransportLox?.Value == false) return false;
            
            if (ServerEnableMask != null &&
                PlayerTransportMask != null &&
                PlayerEnableMask != null &&
                ServerTransportMask != null &&
                IsAllowedInMask(c, ServerEnableMask.Value, ServerTransportMask.Value) &&
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

            return IsInMask(GetPrefabName(c), transportMask);
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
            var monsterAI = f.GetComponent<MonsterAI>();

            if (monsterAI != null && monsterAI.GetFollowTarget() != null)
            {
                var target = monsterAI.GetFollowTarget();
                return target.Equals(Player.m_localPlayer.gameObject);
            }

            return false;
        }
        

        public static List<Character> GetAllies(List<Character> creatures)
        {
            var chars = new List<Character>();
            foreach (var c in creatures)
            {
                if (IsValidAlly(c))
                {
                    chars.Add(c);
                }
            }
            return chars;
        }
    }
}
