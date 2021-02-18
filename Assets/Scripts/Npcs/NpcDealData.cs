﻿using GuildMaster.Data;
using GuildMaster.Databases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace GuildMaster.Npcs
{
    [Serializable]
    public class NpcDealData
    {
        public bool HasDeal => npcInventoryLimit.Count > 0 || npcInventoryUnlimit.Count > 0;
        [SerializeField] public string shopname;
        [SerializeField] private List<ItemCode> npcInventoryUnlimit = new List<ItemCode>();
        [SerializeField] private List<ItemCodeStack> npcInventoryLimit = new List<ItemCodeStack>();
        public ReadOnlyCollection<ItemCode> NpcInventoryUnlimit => npcInventoryUnlimit.AsReadOnly();
        public ReadOnlyCollection<ItemCodeStack> NpcInventoryLimit => npcInventoryLimit.AsReadOnly();
    }
}