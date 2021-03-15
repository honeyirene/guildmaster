﻿using GuildMaster.Data;
using GuildMaster.Databases;
using GuildMaster.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using static GuildMaster.Data.PlayerInventory;

namespace GuildMaster.Windows
{
    public class ShopWindow : DraggableWindow, IToggleableWindow
    {
        [SerializeField] private Text ShopName; 
        [SerializeField] private ShopItemListView shopItemListView;
        [SerializeField] private PlayerShopItemListView playerShopItemListView;
        [SerializeField] private Text PlayerGold;
        [SerializeField] private Text BuyText;
        [SerializeField] private Text SellText;
        [SerializeField] private Text TotalText;
        public void CloseWindow()
        {
            UiWindowsManager.Instance.shopItemPanel.Close();
            UiWindowsManager.Instance.itemInfoPanel.Close();
            base.Close();
        }
        /// <summary>
        /// 외부에서 열 땐 이 메서드 말고 오버라이딩 된 다른 Open 메서드로 윈도우를 오픈해주세요.
        /// </summary>
        public void Open()
        {
            if (!initialized) throw new ArgumentException("ShopWindow는 npc 인벤토리를 넘겨받은 후 실행되어야 합니다.");
            base.OpenWindow();
        }
        /// <summary>
        /// 거래하는 npc의 인벤토리를 넘겨받으면서 열리는 open 코드
        /// </summary>
        /// <param name="npcInventoryInf"></param>
        /// <param name="npcInventoryNotInf"></param>
        /// <param name="shopname"></param>
        public void Open(ReadOnlyCollection<Item> npcInventoryInf, ReadOnlyCollection<ItemCount> npcInventoryNotInf, string shopname)
        {
            this.npcInventoryInf = npcInventoryInf;
            this.npcInventoryNotInf = new List<ItemCount>();
            foreach (ItemCount itemCount in npcInventoryNotInf)
            {
                this.npcInventoryNotInf.Add(new ItemCount(itemCount.Item, itemCount.Number));
            }
            this.shopname = shopname;
            BuildNPCInventory();
            Initialize();
            Open();
            EventRefresh();
            Refresh();
            shopItemListView.ResetQuantity(true);
            playerShopItemListView.ResetQuantity();
        }
        public void GetPanelInfo(ItemStack itemStack, bool isbuying, int index)
        {
            if (isbuying)
            {
                shopItemListView.setItemStack(index, itemStack, isbuying);
                if (itemStack.Quantity == 0)
                {
                    if (shopOnDict.TryGetValue(index, out ItemStack _itemStack))
                        shopOnDict.Remove(index);
                    shopItemListView.ActiveItemIcon(false, index, true);
                }
                else if (shopOnDict.TryGetValue(index, out ItemStack _itemStack))
                {
                    shopOnDict[index].Quantity = itemStack.Quantity;
                    shopItemListView.ActiveItemIcon(true, index, true);
                }
                else
                {
                    shopOnDict.Add(index, itemStack);
                    shopItemListView.ActiveItemIcon(true, index, true);
                }
            }
            else
            {
                playerShopItemListView.setItemStack(index, itemStack, isbuying);
                if (itemStack.Quantity == 0)
                {
                    if (playerOnDict.TryGetValue(_currentCategory, out var PDict) && PDict.TryGetValue(index, out ItemStack _itemStack))
                        PDict.Remove(index);
                    playerShopItemListView.ActiveItemIcon(false, index, false);
                }
                else if (playerOnDict.TryGetValue(_currentCategory, out var PDict))
                {
                    if (PDict.TryGetValue(index, out ItemStack _itemStack))
                        PDict[index].Quantity = itemStack.Quantity;
                    else
                        PDict.Add(index, itemStack);
                    playerShopItemListView.ActiveItemIcon(true, index, false);
                }
                else
                {
                    Dictionary<int, ItemStack> Dict = new Dictionary<int, ItemStack>();
                    Dict.Add(index, itemStack);
                    playerOnDict.Add(_currentCategory, Dict);
                    playerShopItemListView.ActiveItemIcon(true, index, false);
                }
            }
            Refresh();
            return;
        }
        public void Deal()
        {
            if (Player.Instance.PlayerGuild.Balance.Value + totalcost < 0) return;
            foreach (KeyValuePair<int, ItemStack> keyValuePair in shopOnDict)
            {
                if (!keyValuePair.Value.isInfinite) npcInventory.TryDeleteItem(keyValuePair.Value.Item, keyValuePair.Value.Quantity);
                Player.Instance.PlayerInventory.TryAddItem(keyValuePair.Value.Item, keyValuePair.Value.Quantity);
            }
            shopOnDict.Clear();
            int size = npcInventory.Num;
            List<ItemStack> npcShopItemList = new List<ItemStack>();
            foreach (ItemStack itemStack in npcInventory.InventoryList)
            {
                npcShopItemList.Add(itemStack);
            }
            foreach (KeyValuePair<ItemCategory, Dictionary<int, ItemStack>> categoryDict in playerOnDict)
            {
                foreach (KeyValuePair<int, ItemStack> items in categoryDict.Value)
                {
                    if (!npcShopItemList.Exists(x => x.Item == items.Value.Item && !x.isInfinite))
                    {
                        size++;
                    }
                }
            }
            Inventory inventory = new Inventory(size, false);
            foreach (ItemStack itemstack in npcInventory.InventoryList)
            {
                if (itemstack.Item == null) return;
                if (itemstack.isInfinite)
                    inventory.TryAddInfiniteItem(itemstack.Item);
                else
                    inventory.TryAddItem(itemstack.Item, itemstack.ItemNum);
            }
            foreach (KeyValuePair<ItemCategory, Dictionary<int, ItemStack>> categoryDict in playerOnDict)
            {
                foreach (KeyValuePair<int, ItemStack> keyValuePair in categoryDict.Value)
                {
                    Player.Instance.PlayerInventory.TryDeleteItem(keyValuePair.Value.Item, keyValuePair.Value.Quantity);
                    inventory.TryAddItem(keyValuePair.Value.Item, keyValuePair.Value.Quantity);
                }
            }
            playerOnDict.Clear();
            //Refresh Player Inventory
            npcInventory = inventory;
            shopItemListView.SetInventory(npcInventory);
            Refresh();
            shopItemListView.ResetQuantity(true);
            playerShopItemListView.ResetQuantity();
            ChangeCategory(_currentCategory);
            Player.Instance.PlayerGuild.Balance.Value += totalcost;
        }
        private void PointerEntered(Item item)
        {
            if (item != null)
                _panelRequestId = UiWindowsManager.Instance.itemInfoPanel.Open(item.Code);
        }

        private void PointerExited()
        {
            if (_panelRequestId == 0) return;
            UiWindowsManager.Instance.itemInfoPanel.Close(_panelRequestId);
            _panelRequestId = 0;
        }
        private void ShopClicked(int index)
        {
            shopItemListView.ActiveItemIcon(true, index, true);
            if (shopOnDict.TryGetValue(index, out ItemStack itemStack))
                UiWindowsManager.Instance.shopItemPanel.Open(itemStack, true, index);
            else
                UiWindowsManager.Instance.shopItemPanel.Open(shopItemListView.getItemStack(index), true, index);
        }
        private void PlayerClicked(int index)
        {
            if (playerShopItemListView.getItemStack(index) == null || playerShopItemListView.getItemStack(index).Item == null) 
                return;
            playerShopItemListView.ActiveItemIcon(true, index, false);
            if (playerOnDict.TryGetValue(_currentCategory, out var pDict) && pDict.TryGetValue(index, out ItemStack itemStack))
                UiWindowsManager.Instance.shopItemPanel.Open(itemStack, false, index);
            else
                UiWindowsManager.Instance.shopItemPanel.Open(playerShopItemListView.getItemStack(index), false, index);
        }

        private bool _changeCategoryBlock = false;
        private void ChangeCategory(PlayerInventory.ItemCategory category)
        {
            if (_changeCategoryBlock) return;
            _changeCategoryBlock = true;
            _currentCategory = category;
            foreach (var ict in GetComponentsInChildren<ItemCategoryToggle>())
            {
                ict.Toggle.isOn = ict.category == category;
            }
            playerShopItemListView.ChangeCategory((int)category);

            _changeCategoryBlock = false;
        }
        private void Start()
        {
            foreach (var ict in GetComponentsInChildren<ItemCategoryToggle>())
            {
                var cat = ict.category;
                ict.Toggle.onValueChanged.AddListener(b =>
                {
                    if (b) ChangeCategory(cat);
                    Refresh();
                });
            }
            ChangeCategory(PlayerInventory.ItemCategory.Equipable);
        }
        private void Initialize()
        {
            initialized = true;
            ShopName.text = shopname;
            playerShopItemListView.SetPlayerInventory(Player.Instance.PlayerInventory);
            shopItemListView.SetInventory(npcInventory);
            shopOnDict.Clear();
            playerOnDict.Clear();
        }
        private void BuildNPCInventory()
        {
            originalShopSize = npcInventoryInf.Count + npcInventoryNotInf.Count;
            npcInventory = new Inventory(originalShopSize, false);
            foreach (Item item in npcInventoryInf)
            {
                if (item == null) throw new ArgumentException("npcInventoryInf의 item이 null입니다.");
                npcInventory.TryAddInfiniteItem(item);
            }
            foreach (ItemCount itemcount in npcInventoryNotInf)
            {
                if (itemcount.Item == null) throw new ArgumentException("npcInventoryNotInf의 item이 null입니다.");
                npcInventory.TryAddItem(itemcount.Item, itemcount.Number);
            }
        }
        private void Refresh()
        {
            shopItemListView.Refresh();
            playerShopItemListView.Refresh();
            //Gold Refresh
            PlayerGold.text = Player.Instance.PlayerGuild.Balance.Value.ToString();
            buycost = 0;
            sellcost = 0;
            totalcost = 0;
            foreach (KeyValuePair<int, ItemStack> pair in shopOnDict)
            {
                shopItemListView.ActiveItemIcon(true, pair.Key, true);
                buycost += pair.Value.Quantity * pair.Value.BuyCost;
            }
            foreach (var Dictpair in playerOnDict)
            {
                foreach (var pair in Dictpair.Value)
                {
                    if (Dictpair.Key == _currentCategory) 
                        playerShopItemListView.ActiveItemIcon(true, pair.Key, false);
                    sellcost += pair.Value.Quantity * pair.Value.SellCost;
                }
            }
            totalcost = sellcost - buycost;
            BuyText.text = buycost.ToString();
            SellText.text = sellcost.ToString();
            TotalText.text = totalcost.ToString();
            //ICons Refresh
        }
        private void EventRefresh()
        {
            shopItemListView.Refresh();
            playerShopItemListView.Refresh();
            //이벤트 구독
            shopItemListView.PointerEntered -= PointerEntered;
            shopItemListView.PointerEntered += PointerEntered;
            shopItemListView.PointerExited -= PointerExited;
            shopItemListView.PointerExited += PointerExited;
            shopItemListView.SClick -= ShopClicked;
            shopItemListView.SClick += ShopClicked;
            playerShopItemListView.PointerEntered -= PointerEntered;
            playerShopItemListView.PointerEntered += PointerEntered;
            playerShopItemListView.PointerExited -= PointerExited;
            playerShopItemListView.PointerExited += PointerExited;
            playerShopItemListView.SClick -= PlayerClicked;
            playerShopItemListView.SClick += PlayerClicked;
        }
        private int _panelRequestId;
        private int buycost = 0, sellcost = 0, totalcost;
        private int originalShopSize;
        private ReadOnlyCollection<Item> npcInventoryInf;
        private List<ItemCount> npcInventoryNotInf;
        private Dictionary<int, ItemStack> shopOnDict = new Dictionary<int, ItemStack>();
        private Dictionary<ItemCategory, Dictionary<int, ItemStack>> playerOnDict = new Dictionary<ItemCategory, Dictionary<int, ItemStack>>();
        private Inventory npcInventory;
        private string shopname;
        private bool initialized = false;
        private PlayerInventory.ItemCategory _currentCategory;
    }
}
