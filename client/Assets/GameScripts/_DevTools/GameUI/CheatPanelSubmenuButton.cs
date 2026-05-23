using System;
using GameConsole.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class CheatPanelSubmenuButton: MonoBehaviour
    {
        [SerializeField] private Button cheatBtn;
        [SerializeField] private Text cheatTxt;

        private ParamDefine mDefine;

        public void Init(Action<string> onClickAct)
        {
            cheatBtn.onClick.AddListener(() => onClickAct?.Invoke(mDefine.param));
        }
        
        public void Show(ParamDefine define)
        {
            mDefine = define;
            cheatTxt.text = define.name;
        }
    }
}