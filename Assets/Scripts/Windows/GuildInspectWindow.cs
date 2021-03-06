using System;
using System.Linq;
using GuildMaster.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Windows
{
    public class GuildInspectWindow: DraggableWindow, IToggleableWindow
    {
        [SerializeField] private Text rankLabel;
        [SerializeField] private Text membersNumberLabel;
        [SerializeField] private Text balanceLabel;
        [SerializeField] private Text reputationLabel;

        public void OnEnable()
        {
            Player.Instance.PlayerGuild.Changed += Refresh;
        }
        public void OnDisable()
        {
            Player.Instance.PlayerGuild.Changed -= Refresh;
        }

        public void Open()
        {
            base.OpenWindow();
            Refresh();
        }

        private void Refresh()
        {
            var guild = Player.Instance.PlayerGuild;
            rankLabel.text = guild.Rank.ToString();
            membersNumberLabel.text = $"{guild._guildMembers.GuildMemberList.Count()}/{guild.MemberNumberLimit}";
            balanceLabel.text = guild.Balance.ToString();
            reputationLabel.text = guild.Reputation.ToString();
        }

    }
}