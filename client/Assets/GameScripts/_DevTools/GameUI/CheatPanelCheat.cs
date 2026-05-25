using System;
using System.Collections.Generic;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class CheatPanelCheat : MonoBehaviour
    , IPointerEnterHandler
    {
        [SerializeField] private Button submitBtn;
        [SerializeField] private Text nameText;
        [SerializeField] private CheatPanelCheatInfo info;
        [SerializeField] private CheatPanelParamsBox paramBox;
        
        public CommandWrapper CommandWrapper { get; private set; }
        
        private Action<CommandWrapper> mSubmitAct;

        private float mTimer = float.MaxValue;

        public void Init(Action<CommandWrapper> submitAct)
        {
            mSubmitAct = submitAct;
            submitBtn.onClick.AddListener(OnSubmit);
        }

        public void SetCommand(CommandWrapper commandWrapper)
        {
            CommandWrapper = commandWrapper;
            nameText.text = commandWrapper.Alias;
        }

        private void OnSubmit()
        {
            mSubmitAct?.Invoke(CommandWrapper);
        }

        private void Update()
        {
            mTimer -= Time.deltaTime;

            if (mTimer <= 0f)
            {
                info.Show(this);
                mTimer = float.MaxValue;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Config.ShowInfo)
            {
                mTimer = Config.InfoShowDelay;
            }
        }
    }
}