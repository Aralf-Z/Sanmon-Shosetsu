using UnityEngine;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class CheatPanelParam: MonoBehaviour
    {
        [SerializeField] private Text nameTxt;
        [SerializeField] private InputField inputTxt;

        public string Param => inputTxt.text;
        
        public void Show(string paramName, string defaultValue = null)
        {
            nameTxt.text = paramName;
            inputTxt.text = defaultValue;
        }
    }
}