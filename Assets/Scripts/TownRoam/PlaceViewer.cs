﻿using System;
using System.Collections.Generic;
using GuildMaster.Npcs;
using GuildMaster.Tools;
using GuildMaster.UI;
using UnityEngine;
using UnityEngine.Events;

namespace GuildMaster.TownRoam
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class PlaceViewer: MonoBehaviour
    {
        private const int PlaceBeingViewedLayer = 8;
        
        private void Start()
        {
            _camera = GetComponent<Camera>();
        }
        
        private void Update()
        {
            if (_currentPlace == null) return;
            // if (ReferenceEquals(_currentPlace, null)) return;
            
            // 화면 비율이 바뀌는 것을 걱정해 Update에 넣습니다. 화면 비율이 바뀌지 않는 게 확실하면 Goto에 넣어주세요.
            _camera.orthographicSize = Math.Max(_currentPlace.Size.y/2, _currentPlace.Size.x/2/_camera.aspect);
            transform.position = _currentPlace.Center;
        }
        public void Goto(Place p)
        {
            // 현재는 카메라를 이동시키며 그 Place를 활성화시키는 방법을 사용하고 있으나,
            // 그 장소를 복제하여 그곳을 비추는 것도 괜찮아 보입니다.    
            if (p == null) throw new Exception("PlaceViewer cannot Goto null");
            
            // 전에 있던 장소 비활성화&현재 장소 활성화.
            if (_currentPlace != null)
                SetLayersOfChildren(_currentPlace.gameObject, _originalLayer);         
            _currentPlace = p;

            var gObj = p.gameObject;
            _originalLayer = gObj.layer;
            SetLayersOfChildren(gObj, PlaceBeingViewedLayer);
            
            UpdateSubscribedButtons<PlaceMoveButton, PlaceMoveButton>
                (_subscribedMoveButtons, gObj, btn => Goto(btn.connectedPlace));
            UpdateSubscribedButtons<RoamingNpc, NpcStaticData>
                (_subscribedRoamingNpcs, gObj, npc=> UiWindowsManager.Instance.OpenNpcInteractWindow(npc));
        }

        
        /*
         * 원래 구독하던 버튼 이벤트들을 모두 구독 해제하고,
         * 주어진 parentObject와 그 모든 자녀에서 TComponent 타입의 버튼 컴포턴트를 찾고,
         * 그 전체에게 주어진 핸들러 부착.
         */
        private static void UpdateSubscribedButtons<TComponent, TEventParam>
            (List<TComponent> list, GameObject parentObject, GenericButton<TEventParam>.ClickedHandler handler) 
            where TComponent: GenericButton<TEventParam>
        {
            list.ForEach(e =>
            {
                if (e != null)
                    e.Clicked -= handler;
            });
            list.Clear();
            foreach (var button in parentObject.GetComponentsInChildren<TComponent>())
            {
                button.Clicked += handler;
                list.Add(button);
            }
        }

        private void SetLayersOfChildren(GameObject gObj, int layer)
        {
            foreach (var t in gObj.GetComponentsInChildren<Transform>())
                t.gameObject.layer = layer;
        }
        private int _originalLayer;
        private Place _currentPlace;
        private readonly List<PlaceMoveButton> _subscribedMoveButtons = new List<PlaceMoveButton>();
        private readonly List<RoamingNpc> _subscribedRoamingNpcs = new List<RoamingNpc>();
        private Camera _camera;
    }
}