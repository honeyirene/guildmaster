﻿using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuildMaster.UI
{
    public abstract class Window: MonoBehaviour, IPointerDownHandler
    {
        public event Action Closed;
        public bool enableToggle;
        
        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            OnOpen();
        }

        public void Toggle()
        {
            if (!enableToggle) return;
            if (gameObject.activeSelf)
                Close();
            else
                Open();
        }
        public void Close()
        {
            OnClose();
            Closed?.Invoke();
            gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }

        protected virtual void OnOpen(){}

        protected virtual void OnClose(){}
    }
}