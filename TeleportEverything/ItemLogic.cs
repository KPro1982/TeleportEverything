using System;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        internal static bool IsDragonEgg(ItemDrop.ItemData item)
        {
            return item.m_dropPrefab.name.Equals("DragonEgg");
        }

        internal static void ReduceStacks(Player player)
        {
            foreach (var item in player.GetInventory().GetAllItems())
            {
                if (item.m_shared.m_teleportable || IsDragonEgg(item))
                    continue;

                var totalStack = item.m_stack;
                item.m_stack = Convert.ToInt32(totalStack * (1-(float)TransportFee.Value / 100));

                DisplayMessage($"{totalStack - item.m_stack} out of {totalStack} deducted as a fee for transporting contraband.");
            }
        }

        internal static void RemoveEmptyItems(Player player)
        {
            var items = player.GetInventory().GetAllItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].m_stack == 0)
                    items.RemoveAt(i);
            }
        }
    }
}
