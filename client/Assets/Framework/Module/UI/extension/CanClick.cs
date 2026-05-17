using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sanmon.Module
{
    public class CanClick : MonoBehaviour
    ,IPointerClickHandler
    {
        public Action onClickAct;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            onClickAct?.Invoke();
        }
    }
}
