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
    , IPointerExitHandler
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
            nameText.text = commandWrapper.Command.name;
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
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mTimer = Config.InfoShowDelay;
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            info.TryHide();
        }
    }
}