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
            var isDragonEgg = IsDragonEgg(item);
            if (isDragonEgg && !DragonEggsEnabled()) return false;
            if (!isDragonEgg && !OresEnabled()) return false;

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
            if (item.m_dropPrefab == null)
            {
                return false;
            }
            return GetItemPrefabName(item).Equals(DRAGON_EGG, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool HasFeeRemoved(ItemDrop.ItemData item)
        {
            if (item?.m_dropPrefab == null)
            {
                return false;
            }
            return RemoveTransportFeeFrom?.Value != null && IsInMask(item.m_dropPrefab.name, RemoveTransportFeeFrom.Value);
        }

        private static int CalculateDeductionValue(int oreQuantity)
        {
            decimal deductionPercentage = (decimal)TransportFee.Value / 100;
            decimal valueToDeduct = oreQuantity * deductionPercentage;

            // Ensure a minimum deduction of 1 and convert to integer
            return Math.Max((int)Math.Ceiling(valueToDeduct), 1);
        }

        internal static void ReduceStacks(Player player)
        {
            var ores = RegisterOreQuantities(player.GetInventory());

            var cart = GetAttachedCart();

            if (CanTransportCarts() && ShouldTaxCarts?.Value == true && cart != null)
            {
                var cartInventory = GetCartInventory(cart);
                ores = RegisterOreQuantities(cartInventory, true, ores);
            }

            //deduct from inventory
            foreach (var ore in ores)
            {
                if (TransportFee == null) continue;
                var valueToDeduct = CalculateDeductionValue(ore.Value);
                var deducted = 0;

                if (ShouldTaxCarts?.Value == true && currrentCartBeingTaxed && cart != null && valueToDeduct > 0)
                {
                    var container = cart.gameObject.GetComponentInChildren<Container>();
                    if (container != null)
                        ReduceFromInventory(container.GetInventory(), ore.Key, ref valueToDeduct, ref deducted);
                }
                if (valueToDeduct > 0)
                {
                    ReduceFromInventory(player.GetInventory(), ore.Key, ref valueToDeduct, ref deducted);
                }

                deductedContraband += deducted;

                LogDeductionInfo(deducted, ore.Value, ore.Key);
            }

            currrentCartBeingTaxed = false;
        }

        private static void LogDeductionInfo(int deducted, int oreQuantity, string oreKey)
        {
            var message = Localization.instance.Localize("$te_deducted_items_detailed_message",
                deducted.ToString(), oreQuantity.ToString(), oreKey);

            TeleportEverythingLogger.LogInfo(message);
        }

        internal static Dictionary<string, int> RegisterOreQuantities(Inventory? inventory, bool isFromCart = false, Dictionary<string, int> ores = null)
        {
            ores ??= new Dictionary<string, int>();
            if (inventory == null) return ores;

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
            if (inventory == null)
            {
                return;
            }

            List<ItemDrop.ItemData> itemsToDeduct = inventory
                .GetAllItems()
                .Where(item => !item.m_shared.m_teleportable && GetItemTranslatedName(item).Equals(oreKey) && item.m_stack > 0)
                .ToList();

            foreach (var item in itemsToDeduct)
            {
                DeductItemFromInventory(item, ref valueToDeduct, ref deducted);

                // Only remove the item if m_stack is 0
                if (item.m_stack == 0)
                {
                    inventory.RemoveItem(item);
                }
            }

            inventory.Changed();
        }

        private static void DeductItemFromInventory(ItemDrop.ItemData item, ref int valueToDeduct, ref int deducted)
        {
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

        public static bool DragonEggsEnabled() => TransportDragonEggs?.Value == true;
        public static bool OresEnabled() => TransportOres?.Value == true;
    }
}