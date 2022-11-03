using System;
using System.Collections.Generic;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        internal static int deductedContraband = 0, totalContrabandCount = 0;
        public static void ApplyTax(Player player)
        {
            if (!player.IsTeleportable() || ZoneSystem.instance.GetGlobalKey("noportals")) //avoid fee if player is not teleportable
            {
                return;
            }

            if (TransportFee.Value == 0)
            {
                return;
            }

            ReduceStacks(player);
            RemoveEmptyItems(player);
        }

        internal static string GetItemPrefabName(ItemDrop.ItemData item) => item.m_dropPrefab.name; //item name, no use of tolower here

        internal static bool IsDragonEgg(ItemDrop.ItemData item)
        {
            if (item?.m_dropPrefab == null)
            {
                return false;
            }
            return GetItemPrefabName(item).Equals("DragonEgg");
        }

        internal static bool HasFeeRemoved(ItemDrop.ItemData item)
        {
            if (item?.m_dropPrefab == null)
            {
                return false;
            }
            return IsInMask(item.m_dropPrefab.name, RemoveTransportFeeFrom.Value);
        }

        internal static void ReduceStacks(Player player)
        {
            var ores = new Dictionary<string, int>();

            //register ore quantities in a dictionary
            foreach (var item in player.GetInventory().GetAllItems())
            {
                if (item.m_shared.m_teleportable || HasFeeRemoved(item))
                {
                    continue;
                }

                AddOrCreateKey(ores, GetItemPrefabName(item), item.m_stack);
                totalContrabandCount += item.m_stack;
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
                        if (item.m_shared.m_teleportable) continue;
                        if (!GetItemPrefabName(item).Equals(ore.Key) || item.m_stack == 0 || valueToDeduct == 0) continue;

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
                deductedContraband += deducted;
                TeleportEverythingLogger.LogInfo(
                    $"{deducted} out of {ore.Value} {ore.Key} deducted as a fee for transporting contraband.");
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