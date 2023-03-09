using System;
using System.Collections.Generic;
using System.Linq;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        private static int deductedContraband, totalContrabandCount;
        
        private static bool ItemPermitted(ItemDrop.ItemData item)
        {
            if (item.m_shared.m_teleportable) return true;
            if (IsDragonEgg(item) && !DragonEggsEnabled()) return false;
            if (!OresEnabled()) return false;

            return true;
        }
        public static void ApplyTax(Player player)
        {
            if (!player.IsTeleportable() || ZoneSystem.instance.GetGlobalKey("noportals")) //avoid fee if player is not teleportable
            {
                return;
            }

            if (TransportFee != null && TransportFee.Value == 0)
            {
                return;
            }

            ReduceStacks(player);
        }

        internal static string GetItemPrefabName(ItemDrop.ItemData item) => item.m_dropPrefab.name; //item name, no use of tolower here
        internal static string GetItemTranslatedName(ItemDrop.ItemData item) => Localization.instance.Localize(item.m_shared.m_name);

        internal static bool IsDragonEgg(ItemDrop.ItemData item)
        {
            if (item?.m_dropPrefab == null)
            {
                return false;
            }
            return GetItemPrefabName(item).Equals(DRAGON_EGG);
        }

        internal static bool HasFeeRemoved(ItemDrop.ItemData item)
        {
            if (item?.m_dropPrefab == null)
            {
                return false;
            }
            return RemoveTransportFeeFrom?.Value != null && IsInMask(item.m_dropPrefab.name, RemoveTransportFeeFrom.Value);
        }

        internal static void ReduceStacks(Player player)
        {
            var ores = new Dictionary<string, int>();
            ores = RegisterOreQuantities(player.GetInventory(), ores);

            var cart = GetAttachedCart();

            if (CanTransportCarts() && ShouldTaxCarts?.Value == true && cart != null)
            {
                var cartInventory = GetCartInventory(cart);
                ores = RegisterOreQuantities(cartInventory, ores, true);
            }

            //deduct from inventory
            foreach (var ore in ores)
            {
                if (TransportFee == null) continue;
                var valueToDeduct = Convert.ToInt32(ore.Value * (float)TransportFee.Value / 100);
                valueToDeduct = valueToDeduct > 0 ? valueToDeduct : 1;
                
                var deducted = 0;
                if (ShouldTaxCarts?.Value == true && currrentCartBeingTaxed && cart != null)
                {
                    var container = cart.gameObject.GetComponentInChildren<Container>();
                    if (container != null)
                        ReduceFromInventory(container.m_inventory, ore.Key, ref valueToDeduct, ref deducted);
                }
                if (valueToDeduct > 0)
                {
                    ReduceFromInventory(player.GetInventory(), ore.Key, ref valueToDeduct, ref deducted);
                }
                deductedContraband += deducted;
                TeleportEverythingLogger.LogInfo(
                    Localization.instance.Localize("$te_deducted_items_detailed_message",
                        deducted.ToString(), ore.Value.ToString(), ore.Key)
                );
            }
            RemoveEmptyItems(player);
            if (ShouldTaxCarts?.Value == true && currrentCartBeingTaxed && cart != null)
            {
                RemoveEmptyItems(cart);
                currrentCartBeingTaxed = false;
            }
        }

        internal static Dictionary<string, int> RegisterOreQuantities(Inventory? inventory, Dictionary<string, int> ores, bool isFromCart=false)
        {
            if(inventory == null) return ores;
            //register ore quantities in a dictionary
            foreach (var item in inventory.GetAllItems().Where(item => !item.m_shared.m_teleportable && !HasFeeRemoved(item)))
            {
                if(isFromCart)
                {
                    currrentCartBeingTaxed = true;
                }
                AddOrCreateKey(ores, GetItemTranslatedName(item), item.m_stack);
                totalContrabandCount += item.m_stack;
            }
            return ores;
        }

        internal static void ReduceFromInventory(Inventory? inventory, string oreKey, ref int valueToDeduct, ref int deducted)
        {
            if (inventory == null) return;
            foreach (var item in inventory.GetAllItems().Where(item => !item.m_shared.m_teleportable))
            {
                if (!GetItemTranslatedName(item).Equals(oreKey) || item.m_stack == 0 || valueToDeduct == 0) continue;

                if (item.m_stack > valueToDeduct)
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
            RemoveEmptyItemsFromInventory(player.GetInventory());
        }

        internal static void RemoveEmptyItemsFromInventory(Inventory? inventory)
        {
            if (inventory == null) return;
            var items = inventory.GetAllItems();
            items.RemoveAll(item => item.m_stack == 0);
        }

        public static bool DragonEggsEnabled() => TransportDragonEggs?.Value == true;
        public static bool OresEnabled() => TransportOres?.Value == true;
    }
}