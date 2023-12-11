using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace TeleportEverything
{
    internal partial class Plugin
    {
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        public static class InventoryIsTeleportablePatch
        {
            private static bool Prefix(ref bool __result, ref Inventory __instance)
            {
                if (!IsModEnabled()) return true; //go to original method

                if (DragonEggsEnabled() && OresEnabled())
                {
                    __result = true;
                    return false; //skip original method
                }

                if (__instance.GetAllItems().Any(item => !ItemPermitted(item)))
                {
                    return true; //go to original method
                }

                __result = true;
                return false; //skip original method
            }
        }

        [HarmonyPatch(typeof(Teleport), nameof(Teleport.Interact))]
        public static class TeleportInteractPatch
        {
            private static void Prefix(Humanoid character, bool hold, Teleport __instance)
            {
                if (!IsModEnabled()) return;
 
                if (hold)
                {
                    return;
                }

                if (__instance.m_targetPoint == null)
                {
                    return;
                }
                IsDungeonTeleport = true;
            }

            private static void Postfix()
            {
                if (!IsModEnabled()) return;
                IsDungeonTeleport = false;
            }
        }

        [HarmonyPatch(typeof(Teleport), nameof(Teleport.GetHoverText))]
        public static class TeleportGetHoverTextPatch
        {
            private static void Postfix()
            {
                if (!IsModEnabled()) return;

                SetIncludeMode();
                GetCreatures();

                DisplayAlliesMessage();
                DisplayEnemiesMessage();
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.IsTeleportable))]
        public class IsTeleportablePatch
        {
            private static bool Postfix(bool __result, Humanoid __instance)
            {
                if (!IsModEnabled()) return __result;

                if (Game.instance.m_firstSpawn) //if player is still spawning
                {
                    return __result;
                }

                if (ShowVikingsDontRun)
                {
                    ShowVikingsDontRun = false;
                }
                
                if (!__result) //if player inventory teleportable is false
                {
                    return __result;
                }

                if (SkyheimAvoidCreatures)
                {
                    return __result; //skip if it is skyheim blink/recall
                }

                SetIncludeMode();
                GetCreatures();

                DisplayAlliesMessage();

                if (Enemies?.Count > 0)
                {
                    if (TeleportMode != null && TeleportMode.Value.Contains("Run"))
                    {
                        ShowVikingsDontRun = true;
                        return false;
                    }

                    DisplayEnemiesMessage();
                }

                if (!IsTransportCartsDisabled() && !IsTeleportingCart())
                {
                    var cart = GetAttachedCart();
                    if (cart != null)
                    {
                        if (!CartIsTeleportable(cart)) return false;
                    }
                }

                return __result;
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
        public class TeleportWorldTeleport_Patch
        {
            private static void Postfix()
            {
                if (!IsModEnabled()) return;

                if(ShowVikingsDontRun)
                {
                    DisplayMessage(Localization.instance.Localize("$te_vikings_dont_run", Enemies?.Count.ToString(), SearchRadius?.Value.ToString()));
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
        public class TeleportToPatch
        {
            private static bool Postfix(bool __result, Player __instance, Vector3 pos,
                Quaternion rot, bool distantTeleport)
            {
                if (!IsModEnabled()) return __result;

                if (!__instance.IsTeleporting())
                {
                    return __result;
                }

                if (SkyheimAvoidCreatures) {
                    return __result; //skip if it is skyheim blink/recall
                }

                SetIncludeMode();
                GetCreatures();
                TeleportTriggered = true;

                if (!IsDungeonTeleport)
                {
                    ApplyTax(__instance);
                }

                var attachedCart = GetAttachedCart();
                if (attachedCart != null)
                {
                    if (CanTransportCarts())
                    {
                        attachedCart.Detach();
                        TransportCart(attachedCart, pos, rot);

                        __instance.m_teleportTargetPos += SetForwardOffset(rot, (CART_SIZE + CART_FORWARD_OFFSET));
                    }
                }

                if (TeleportMode != null && Enemies?.Count > 0 && TeleportMode.Value.Contains("Take"))
                {
                    TeleportCreatures(__instance, Enemies, true);
                }

                TeleportEverythingLogger.LogInfo(Localization.instance.Localize("$te_transported_allies_message", Allies?.Count.ToString(), TransportAllies.ToString()));
                if (Allies?.Count > 0 && TransportAllies)
                {
                    TeleportCreatures(__instance, Allies);
                }
                return __result;
            }
        }

        [HarmonyPatch(typeof(Tameable), nameof(Tameable.RPC_Command))]
        public class TameableRPCCommandPatch
        {
            private static void Postfix(Tameable __instance)
            {
                if (!IsModEnabled()) return;

                __instance.m_character.m_nview.ClaimOwnership();
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateTeleport))]
        public class UpdateTeleportPatch
        {
            static void Prefix(ref bool ___m_distantTeleport)
            {
                if (IsModEnabled() && ShowTransportAnimationScreen?.Value == false && ___m_distantTeleport)
                {
                    ___m_distantTeleport = false;
                }
            }

            static void Postfix(Player __instance, ref bool ___m_teleporting)
            {
                if (!IsModEnabled()) return;

                if (!___m_teleporting && TeleportTriggered && ZNetScene.instance.IsAreaReady(__instance.m_teleportTargetPos))
                {
                    TeleportTriggered = false;
                    //TeleportEverythingLogger.LogInfo("Teleport ended");
                    if (CanTransportCarts())
                    {
                        if (attachedTeleportingCartId != null) 
                        {
                            var attachedCart = GetNearbyCarts(__instance.transform.position, SearchRadius.Value).Where(cart => cart.m_nview.GetZDO().m_uid == attachedTeleportingCartId).FirstOrDefault();
                            if (attachedCart?.CanAttach(__instance.gameObject) == true)
                            {
                                attachedCart.AttachTo(__instance.gameObject);
                            }
                            attachedTeleportingCartId = null;
                        }
                    }
                    if (totalContrabandCount > 0)
                    {
                        DisplayLongMessage(Localization.instance.Localize("$te_deducted_items_message", deductedContraband.ToString(), totalContrabandCount.ToString()));
                        deductedContraband = 0;
                        totalContrabandCount = 0;
                    }
                }
            }
        }

        [HarmonyPatch]
        public class TeleportWorldPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Awake))]
            private static void Postfix(TeleportWorld __instance)
            {
                if (!IsModEnabled()) return;

                ChangePortalVolume(__instance);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.UpdatePortal))]
            private static void Prefix(TeleportWorld __instance, ref float ___m_activationRange)
            {
                if (!IsModEnabled()) return;
                
                if (PortalActivationRange != null && !IsFloatEqual(___m_activationRange, PortalActivationRange.Value))
                {
                    ___m_activationRange = PortalActivationRange.Value;
                }
            }

            private static void ChangePortalVolume(TeleportWorld portal)
            {
                var audio = portal.GetComponentInChildren<AudioSource>();

                if (audio != null && PortalSoundVolume != null && !IsFloatEqual(audio.volume, PortalSoundVolume.Value))
                {
                    audio.volume = PortalSoundVolume.Value;
                }
            }
        }
    }
}