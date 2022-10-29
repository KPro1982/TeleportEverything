using HarmonyLib;
using static ItemDrop;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        public static void DisplayMessage(string msg)
        {
            switch (MessageMode.Value)
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
                if (!EnableMod.Value)
                {
                    return;
                }

                if (!TransportOres.Value && !TransportDragonEggs.Value)
                {
                    return;
                }

                var width = __instance.GetInventory().GetWidth();
                foreach (ItemData item in __instance.GetInventory().GetAllItems())
                {
                    if (item.m_shared.m_teleportable)
                    {
                        continue;
                    }

                    InventoryGrid.Element element = __instance.GetElement(item.m_gridPos.x, item.m_gridPos.y, width);
                    if (IsDragonEgg(item))
                    {
                        element.m_noteleport.enabled = !TransportDragonEggs.Value;
                    }
                    else
                    {
                        element.m_noteleport.enabled = !TransportOres.Value;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(ItemData), nameof(ItemData.GetTooltip), typeof(ItemData), typeof(int), typeof(bool))]
        public class GetTooltip_Patch
        {
            private static void Postfix(ItemData item, int qualityLevel, bool crafting, ref string __result)
            {
                if (!EnableMod.Value)
                {
                    return;
                }
                if (!TransportOres.Value && !TransportDragonEggs.Value)
                {
                    return;
                }
                if (item.m_shared.m_teleportable)
                {
                    return;
                }
                if (IsDragonEgg(item))
                {
                    if (!TransportDragonEggs.Value)
                    {
                        return;
                    }
                }
                else
                {
                    if (!TransportOres.Value)
                    {
                        return;
                    }
                }
                __result = __result.Replace("\n<color=orange>$item_noteleport</color>", string.Format("\nTransport Fee: <color=orange>{0}%</color>", HasFeeRemoved(item)?0:TransportFee.Value));
            }
        }
    }
}