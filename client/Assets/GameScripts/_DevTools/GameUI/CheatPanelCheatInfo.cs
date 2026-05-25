using System;
using System.Linq;
using GameConsole.Extension;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class CheatPanelCheatInfo: MonoBehaviour
        , IPointerExitHandler
    {
        [SerializeField] private Text infoText;
        //[SerializeField] private Vector2 anchor;
        
        private RectTransform mRectTransform;

        private void Awake()
        {
            mRectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        public void Show(CheatPanelCheat cheat)
        {
            gameObject.SetActive(true);

            // var cRt = (RectTransform)cheat.transform;
            // var cp1Rt = (RectTransform)cRt.transform;
            // var cp2Rt = (RectTransform)cp1Rt.transform;
            // var center = cRt.rect.center + cp1Rt.rect.center + cp2Rt.rect.center;
            //
            // mRectTransform.anchoredPosition = center;
            
            infoText.text = $"name: \n{cheat.CommandWrapper.Alias}\n" +
                            $"cmd: \n{cheat.CommandWrapper.Command.name}\n" +
                            $"desc: \n{cheat.CommandWrapper.Command.description}\n" +
                            $"params: \n{string.Join(",\n", cheat.CommandWrapper.ParameterInfos.Select(x => x.alias))}";
        }

        public void Hide()
        {
            
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            //gameObject.SetActive(false);
        }
    }
}