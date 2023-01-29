using HarmonyLib;
using static ItemDrop;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        public static void DisplayMessage(string msg)
        {
            switch (MessageMode?.Value)
            {
                case "top left":
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, msg);
                    break;
                case "centered":
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, msg);
                    break;
            }
        }

        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
        public class UpdateGui_Patch
        {
            private static void Postfix(InventoryGrid __instance)
            {
                if (!IsModEnabled()) return;

                if (!OresEnabled() && !DragonEggsEnabled()) return;

                var width = __instance.GetInventory().GetWidth();
                foreach (var item in __instance.GetInventory().GetAllItems())
                {
                    if(item?.m_shared == null || item.m_shared.m_teleportable == true)
                    {
                        continue;
                    }

                    InventoryGrid.Element element = __instance.GetElement(item.m_gridPos.x, item.m_gridPos.y, width);
                    if (IsDragonEgg(item))
                    {
                        element.m_noteleport.enabled = !DragonEggsEnabled();
                    }
                    else
                    {
                        element.m_noteleport.enabled = !OresEnabled();
                    }
                }
            }
        }
        [HarmonyPatch(typeof(ItemData), nameof(ItemData.GetTooltip), typeof(ItemData), typeof(int), typeof(bool))]
        public class GetTooltip_Patch
        {
            private static void Postfix(ItemData item, ref string __result)
            {
                if (!IsModEnabled()) return;

                if (!OresEnabled() && !DragonEggsEnabled()) return;

                if (item?.m_dropPrefab == null)
                {
                    if (OresEnabled())
                    {
                        __result = __result.Replace("\n<color=orange>$item_noteleport</color>", "");
                    }
                    return;
                }
                if (item?.m_shared == null || item.m_shared.m_teleportable)
                {
                    return;
                }

                if (!ItemPermitted(item)) return;
                
                __result = __result.Replace("\n<color=orange>$item_noteleport</color>", 
                    string.Concat("\n", Localization.instance.Localize("$te_item_transport_fee",
                    HasFeeRemoved(item)?"0":TransportFee?.Value.ToString()))
                );
            }
        }
    }
}