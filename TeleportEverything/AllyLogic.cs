using System;
using System.Collections.Generic;
using System.Linq;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        public static string GetName(Character c)
        {
            return c?.name.Replace("(Clone)", "").ToLower();
        }
        
        public static bool IsAllowedAlly(Character c)
        {
            if (!ServerEnableMask.Value && !UserEnableMask.Value)
                return true;

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
                return true;

            if (String.IsNullOrWhiteSpace(transportMask))
                return false;

            if (IsInFilterMask(c, transportMask))
                return true;

            return false;
        }

        private static bool IsInFilterMask(Character c, string mask)
        {
            List<string> maskList = mask.Split(',').Select(p => p.Trim().ToLower()).ToList();
            string isInMask = maskList.FirstOrDefault(s => s.Contains(GetName(c)));

            return isInMask != null;
        }

        public static bool IsAllyTransportable(Character ally)
        {
            if (IsNamed(ally) && ExcludeNamed)
            {
                return false;
            }

            if (IsFollow(ally) && IncludeFollow)
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

        public static bool IsFollow(Character f)
        {
            var mAi = f.GetComponent<MonsterAI>();

            if (mAi != null && mAi.GetFollowTarget() != null &&
                mAi.GetFollowTarget().Equals(Player.m_localPlayer.gameObject))
            {
                return true;
            }

            return false;
        }

        public static void SetFollow(Character f)
        {
            var mAi = f.GetComponent<MonsterAI>();

            mAi?.SetFollowTarget(Player.m_localPlayer.gameObject);
        }
        
    }
}