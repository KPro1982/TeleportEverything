using UnityEngine;
using System.Collections.Generic;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        internal static Vagon? GetAttachedCart()
        {
            if (SearchRadius == null) return null;
            var carts = GetNearbyCarts(Player.m_localPlayer.transform.position, SearchRadius.Value);
            foreach (var cart in carts)
            {
                if (cart.IsAttached(Player.m_localPlayer.gameObject.GetComponent<Character>()))
                {
                    return cart;
                }
            }

            return null;
        }

        internal static List<Vagon> GetNearbyCarts(Vector3 position, float searchRadius)
        {
            return GetAllCarts().FindAll(cart => (Vector3.Distance(cart.transform.position, position) < searchRadius));
        }

        internal static List<Vagon> GetAllCarts()
        {
            return Vagon.m_instances;
        }

        internal static void TransportCart(Vagon cart, Vector3 pos, Quaternion rot)
        {
            cart.m_nview.ClaimOwnership();

            var newPosition = pos + SetForwardOffset(rot, CART_FORWARD_OFFSET);
            if (!ZoneSystem.instance.FindFloor(newPosition, out _))
            {
                newPosition.y = ZoneSystem.instance.GetSolidHeight(newPosition) + 0.5f;
            }

            SetPosition(cart, newPosition, rot);
            cart.m_body.velocity = Vector3.zero;
            attachedTeleportingCartId = cart.m_nview.GetZDO().m_uid;
        }

        internal static bool IsTeleportingCart() => attachedTeleportingCartId != null;

        internal static void RemoveEmptyItems(Vagon cart)
        {
            TeleportEverythingLogger.LogInfo("Taking fee from cart");
            RemoveEmptyItemsFromInventory(GetCartInventory(cart));
        }

        internal static Inventory? GetCartInventory(Vagon cart)
        {
            var container = cart.gameObject.GetComponentInChildren<Container>();
            return container?.m_inventory;
        }

        internal static bool CartIsTeleportable(Vagon cart)
        {
            if (DragonEggsEnabled() && OresEnabled())
            {
                return true;
            }

            var inventory = GetCartInventory(cart);
            if (inventory == null) return true;
            foreach (var item in inventory.GetAllItems())
            {
                if (!ItemPermitted(item)) return false;
            }

            return true;
        }

        internal static bool CanTransportCarts()
        {
            if (IsTransportCartsDisabled()) return false;
            if (TransportCartsMode.Value.Equals("Enabled", System.StringComparison.OrdinalIgnoreCase)) return true;
            if (TransportCartsMode.Value.Equals("Only Dungeons", System.StringComparison.OrdinalIgnoreCase) && IsDungeonTeleport) return true;

            return false;
        }

        internal static bool IsTransportCartsDisabled()
        {
            if (TransportCartsMode == null || string.IsNullOrEmpty(TransportCartsMode?.Value) || TransportCartsMode.Value.Equals("Disabled", System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
