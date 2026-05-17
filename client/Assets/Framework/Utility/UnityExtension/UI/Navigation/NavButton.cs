using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sanmon.Utility.UnityExtension
{
    public class NavButton : Button, ICanNavigated
    {
        public INavSet NavSet { get; set; }
        public bool Interactable => interactable;
        public int Index { get; set; }
        public bool onFocusInvoke = false;
        
        private bool mIsFocused;

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;
            if (mIsFocused) return;

            NavSet.Focus(Index);
            DoStateTransition(SelectionState.Highlighted, true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!interactable) return;
            
            NavSet.Unfocus(Index);
            DoStateTransition(mIsFocused ? SelectionState.Highlighted : SelectionState.Normal, true);
            
            // base.OnPointerExit(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            if (mIsFocused) return;
            base.OnDeselect(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (!mIsFocused) return;
            base.OnSelect(eventData);
        }

        public void SetShow(bool show)
        {
            gameObject.SetActive(show);
        }

        public void Invoke()
        {
            onClick.Invoke();
        }

        /// <summary>
        /// 取highlighted为focused
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void OnFocused()
        {
            mIsFocused = true;

            DoStateTransition(SelectionState.Highlighted, true);

            if (interactable && onFocusInvoke) Invoke();
        }

        public void OnUnfocused()
        {
            mIsFocused = false;
            
            DoStateTransition(interactable ? SelectionState.Normal : SelectionState.Disabled, true);
        }
    }
}