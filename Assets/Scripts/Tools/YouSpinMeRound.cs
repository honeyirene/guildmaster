﻿using UnityEngine;

namespace GuildMaster.Tools
{
    // 빙글빙글 돌아요
    public class YouSpinMeRound : MonoBehaviour
    {
        public float x, y, z;

        public void Update()
        {
            transform.Rotate(x, y, z);
        }
    }
}