using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class ActionButton: MonoBehaviour
        , IPointerEnterHandler
        , IPointerExitHandler
        , IPointerClickHandler
    {
        [SerializeField] private Image graphic;
        [SerializeField] private Color onHoverColor = Color.cyan;
        [SerializeField] private Color onNormalColor = Color.gray;
        [SerializeField] private Color onMouse0ClickColor = Color.yellow;
        [SerializeField] private Color onMouse1ClickColor = Color.green;
        [SerializeField] private GameObject onHoverInfo;

        private bool mIsHover = false;
        private float mClickTimer;
        
        /// <summary> 左键行为 </summary>
        public Action onMouse0Act;
        /// <summary> 右键行为 </summary>
        public Action onMouse1Act;

        private void Awake()
        {
            HoverInfoEnable(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mIsHover = true;
            HoverInfoEnable(true);
            graphic.color = onHoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mIsHover = false;
            HoverInfoEnable(false);
            graphic.color = onNormalColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                Debug.Log("OnPointerClickLeft");
                onMouse0Act?.Invoke();
                graphic.color = onMouse0ClickColor;
                mClickTimer = 0.2f;
            }
            else  if(eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log("OnPointerClickRight");
                onMouse1Act?.Invoke();
                graphic.color = onMouse1ClickColor;
                mClickTimer = 0.2f;
            }
        }

        private void Update()
        {
            mClickTimer -= Time.deltaTime;
            if (mClickTimer <= 0f)
            {
                graphic.color = mIsHover ? onHoverColor : onNormalColor;
                mClickTimer = float.MaxValue;
            }
        }

        private void HoverInfoEnable(bool enable)
        {
            if(onHoverInfo) onHoverInfo.SetActive(enable);
        }
    }
}