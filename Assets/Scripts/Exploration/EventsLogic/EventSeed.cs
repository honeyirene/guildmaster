using System;
using UnityEngine;
using Random = System.Random;

namespace GuildMaster.Exploration.Events
{
    /// <summary>
    /// 이벤트 하나를 랜덤적으로 생성하는 클래스
    /// </summary>
    [Serializable]
    public abstract class EventSeed
    {
        public abstract Event Generate(Random random);
    }

    [Serializable]
    public class AsIsEventSeed : EventSeed
    {
        [SerializeField] private Event _event;

        public override Event Generate(Random random)
        {
            return _event;
        }
    }
    //TODO : 이벤트 발생 확률 등이 이 class에 추가되어야 할지 논의 필요.
}