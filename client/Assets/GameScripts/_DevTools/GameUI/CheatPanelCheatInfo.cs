using GameConsole.Extension;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameConsole.GameUI
{
    public class CheatPanelCheatInfo: MonoBehaviour
    , IPointerEnterHandler, IPointerExitHandler
    {
        public void Show(CheatPanelCheat commandWrapper)
        {
            if (Config.ShowInfo)
            {
                gameObject.SetActive(true);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Exit cheat info");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("Enter cheat info");
            gameObject.SetActive(false);
        }

        public void TryHide()
        {
            
        }
    }
}