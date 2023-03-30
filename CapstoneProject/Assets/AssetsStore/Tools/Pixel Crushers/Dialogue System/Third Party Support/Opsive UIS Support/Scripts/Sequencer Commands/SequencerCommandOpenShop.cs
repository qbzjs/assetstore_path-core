﻿using UnityEngine;
using Opsive.UltimateInventorySystem.Exchange.Shops;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.UI.Panels;
using Opsive.UltimateInventorySystem.UI.Menus.Shop;
using Opsive.UltimateInventorySystem.UI.Menus;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Sequencer command: OpenShop([shop], [shopMenu], [playerInventory])
    /// Typically used in NPC's Sequence. Opens shop.
    /// 
    /// Parameters:
    /// - shop: GameObject with ShopBase. Default: speaker.
    /// - shopMenu: GameObject with ShopMenu. Default: ShopMenu in scene.
    /// - playerInventory: GameObject with player's Inventory. Default: listener.
    /// </summary>
    public class SequencerCommandOpenShop : SequencerCommand
    {

        public void Awake()
        {
            var shopSubject = GetSubject(0, speaker);
            var shop = (shopSubject != null) ? shopSubject.GetComponent<ShopBase>() : null;
            var shopMenuTransform = !string.IsNullOrEmpty(GetParameter(1)) ? GetSubject(1) : null;
            var shopMenu = (shopMenuTransform != null) ? shopMenuTransform.GetComponent<ShopMenu>() : null;
            if (shopMenu == null) shopMenu = FindObjectOfType<ShopMenu>();
            if (shopMenu == null)
            {
                var shopMenuObject = GameObjectUtility.GameObjectHardFind("Shop Menu");
                if (shopMenuObject != null) shopMenu = shopMenuObject.GetComponent<ShopMenu>();
            }
            var panelManager = (shopMenu != null) ? shopMenu.GetComponentInParent<DisplayPanelManager>() : null;
            var player = GetSubject(2, listener);
            if (player == null)
            {
                var playerObject = GameObject.FindWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }
            var playerInventory = (player != null) ? player.GetComponentInChildren<Inventory>() : null;

            if (shop == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: OpenShop(" + GetParameters() + ") can't find shop.");
            }
            else if (panelManager == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: OpenShop(" + GetParameters() + ") can't find panel manager parent of shop menu.");
            }
            else if (playerInventory == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Sequencer: OpenShop(" + GetParameters() + ") can't find player inventory.");
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Sequencer: OpenShop(" + GetParameters() + ")");
                var shopMenuOpener = shop.GetComponent<ShopMenuOpener>();
                if (shopMenuOpener != null)
                {
                    shopMenuOpener.Open(playerInventory);
                }
                else
                {
                    shopMenu.BindInventory(playerInventory);
                    shopMenu.SetShop(shop);
                    shopMenu.DisplayPanel.SmartOpen();
                }
            }

            Stop();
        }
    }
}
