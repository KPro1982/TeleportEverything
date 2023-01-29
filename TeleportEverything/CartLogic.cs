using UnityEngine;
using System.Collections.Generic;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        internal static Vagon? GetAttachedCart()
        {
            currentAttachedCart = null;
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
            return GetAllCarts().FindAll(wagon => (Vector3.Distance(wagon.transform.position, position) < searchRadius));
        }

        internal static List<Vagon> GetAllCarts()
        {
            return Vagon.m_instances;
        }

        internal static void TakeOwnership(Vagon v, long userId)
        {
            if (v.GetComponent<ZNetView>() is { } netView)
            {
                if (netView.IsValid())
                {
                    if (netView.GetZDO()?.m_owner != userId)
                    {
                        netView.GetZDO()?.SetOwner(userId);
                    }
                }
            }
        }

        internal static void TransportCart(Vagon cart, Vector3 pos, Quaternion rot)
        {
            TakeOwnership(cart, ZDOMan.instance.GetMyID());

            var newPosition = pos + SetForwardOffset(rot, 0.5f);
            const float upOffset = 0.3f;
            newPosition.y += upOffset;

            var transform = cart.transform;
            transform.position = newPosition;
            transform.rotation = rot;
            currentAttachedCart = cart;
        }

        internal static void RemoveEmptyItems(Vagon cart)
        {
            RemoveEmptyItemsFromInventory(GetCartInventory(cart));
        }

        internal static Inventory? GetCartInventory(Vagon cart)
        {
            var container = cart.gameObject.GetComponentInChildren<Container>();
            return container != null ? container.m_inventory : null;
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
            if (TransportCartsMode != null && TransportCartsMode.Value.Equals("Enabled", System.StringComparison.OrdinalIgnoreCase)) return true;
            if (TransportCartsMode != null && TransportCartsMode.Value.Equals("Only Dungeons", System.StringComparison.OrdinalIgnoreCase) && IsDungeonTeleport) return true;

            return false;
        }

        internal static bool IsTransportCartsDisabled()
        {
            if (string.IsNullOrEmpty(TransportCartsMode?.Value)) return true;
            return TransportCartsMode != null && TransportCartsMode.Value.Equals("Disabled", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
