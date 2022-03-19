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
            int deductedCount = 0, totalCount = 0;

            foreach (var item in player.GetInventory().GetAllItems())
            {
                if (item.m_shared.m_teleportable || IsDragonEgg(item))
                    continue;

                int totalStack = item.m_stack;
                item.m_stack = System.Convert.ToInt32(totalStack * (1-(float)TransportFee.Value / 100));

                TeleportEverythingLogger.LogMessage($"{totalStack - item.m_stack} out of {totalStack} {item.m_dropPrefab.name} deducted as a fee for transporting contraband.");
                //counts
                deductedCount += totalStack - item.m_stack;
                totalCount += totalStack;
            }
            DisplayMessage($"{deductedCount} out of {totalCount} items deducted as a fee for transporting contraband.");
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
