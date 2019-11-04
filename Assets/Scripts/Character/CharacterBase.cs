﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GuildMaster
{
    public abstract class CharacterBase : ObjectBase
    {
        public abstract int HP { get; set; }
        public abstract int ATK { get; set; }
        public abstract int DEF { get; set; }
        public abstract int SPD { get; set; }

        public void GetDamage(int damage)
        {
            HP = HP - (damage - DEF);
        }
    }
}