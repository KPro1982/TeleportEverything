using System;
using System.Collections.Generic;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        internal static bool IsDragonEgg(ItemDrop.ItemData item) =>
            item.m_dropPrefab.name.Equals("DragonEgg");

        internal static void ReduceStacks(Player player)
        {
            int deductedCount = 0, totalCount = 0;
            var ores = new Dictionary<string, int>();

            //register ore quantities in a dictionary
            foreach (var item in player.GetInventory().GetAllItems())
            {
                if (item.m_shared.m_teleportable || IsDragonEgg(item))
                {
                    continue;
                }

                AddOrCreateKey(ores, item.m_dropPrefab.name, item.m_stack);

                totalCount += item.m_stack;
            }

            //deduct from inventory
            foreach (var ore in ores)
            {
                int valueToDeduct = Convert.ToInt32(ore.Value * (float)TransportFee.Value / 100);
                valueToDeduct = valueToDeduct > 0 ? valueToDeduct : 1;
                
                int deducted = 0;
                while (valueToDeduct > 0)
                {
                    foreach (var item in player.GetInventory().GetAllItems())
                    {
                        if (item.m_shared.m_teleportable || IsDragonEgg(item)) continue;
                        if (ore.Key != item.m_dropPrefab.name || item.m_stack == 0 || valueToDeduct == 0) continue;

                        if (item.m_stack >= valueToDeduct)
                        {
                            deducted += valueToDeduct;
                            item.m_stack -= valueToDeduct;  
                            valueToDeduct = 0;
                        }
                        else
                        {
                            deducted += item.m_stack;
                            valueToDeduct -= item.m_stack;
                            item.m_stack = 0;
                        }
                    }
                }
                deductedCount += deducted;
                TeleportEverythingLogger.LogInfo(
                    $"{deducted} out of {ore.Value} {ore.Key} deducted as a fee for transporting contraband.");
            }

            if (totalCount > 0)
            {
                DisplayMessage(
                    $"{deductedCount} out of {totalCount} items deducted as a fee for transporting contraband.");
            }
        }

        internal static void AddOrCreateKey(Dictionary<string, int> dict, string key, int value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] += value;
            }
            else
            {
                dict.Add(key, value);
            }
        }

        internal static void RemoveEmptyItems(Player player)
        {
            var items = player.GetInventory().GetAllItems();
            items.RemoveAll(item => item.m_stack == 0);
        }
    }
}