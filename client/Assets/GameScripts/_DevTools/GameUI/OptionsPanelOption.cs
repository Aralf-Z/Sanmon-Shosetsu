using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class OptionsPanelOption: MonoBehaviour
    {
        [SerializeField] private Text titleTxt;
        [SerializeField] private Toggle toggle;
        [SerializeField] private Page number;
        
        private int mCurrentNumber;
        private int mNumberMin;
        private int mNumberMax;
        
        private Action<int> mSetNumberAct;
        private Action<bool> mSetBoolAct;
        
        public void Init(string text, int current, Action<int> setCallback, int min = 1, int max = 99, int mulStep = 10)
        {
            CommonInit(text);
            
            mCurrentNumber = current;
            mSetNumberAct = setCallback;
            mNumberMin = min;
            mNumberMax = max;
            
            number.gameObject.SetActive(true);
            number.SetInfo(mCurrentNumber.ToString());
            number.Init(null, ChangeNumber, mulStep);
        }
        
        public void Init(string text, bool current, Action<bool> setCallback)
        {
            CommonInit(text);
            mSetBoolAct = setCallback;
            toggle.gameObject.SetActive(true);
            toggle.isOn = current;
            toggle.onValueChanged.AddListener(ChangeToggle);
        }

        private void CommonInit(string text)
        {
            titleTxt.text = text;
            toggle.gameObject.SetActive(false);
            number.gameObject.SetActive(false);
        }

        private void ChangeNumber(int delta)
        {
            mCurrentNumber = Mathf.Clamp(delta + mCurrentNumber, mNumberMin, mNumberMax);
            number.SetInfo(mCurrentNumber.ToString());
            mSetNumberAct?.Invoke(mCurrentNumber);
        }
        
        private void ChangeToggle(bool isOn)
        {
            mSetBoolAct?.Invoke(isOn);
        }
    }
}