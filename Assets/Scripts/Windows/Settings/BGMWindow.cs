﻿
using GuildMaster.UI;

namespace GuildMaster.UI
{
    public class BGMWindow : DraggableWindow, IToggleableWindow
    {
        public void Open()
        {
            base.OpenWindow();
            Refresh();
        }

        // Update is called once per frame
        void Refresh()
        {
            
        }
    }
}