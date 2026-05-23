using System;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class CheatPanelParamsBox: MonoBehaviour
    {
        [SerializeField] private CheatPanelParam param;
        [SerializeField] private Button onSubmitBtn;
        
        private Action<Command, string> mOnSubmitAct;
        private SimplePool<CheatPanelParam> mParamPool;
        private CommandWrapper mCommandWrapper;
        
        public void Init(Action<Command, string> onSubmit)
        {
            mOnSubmitAct = onSubmit;
            mParamPool = new SimplePool<CheatPanelParam>(param);
            
            onSubmitBtn.onClick.AddListener(Submit);
        }

        public void Show(CommandWrapper wrapper)
        {
            gameObject.SetActive(true);
            mCommandWrapper = wrapper;
            mParamPool.RecycleAll();
            
            foreach (var info in mCommandWrapper.ParameterInfos)
            {
                var p = mParamPool.Require();
                p.Show(info.alias, info.defaultValue);
            }
            
            onSubmitBtn.transform.parent.SetAsLastSibling();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void Submit()
        {
            var @params = string.Empty;

            foreach (var cp in mParamPool.Using)
            {
                @params += cp.Param + " ";
            }
            
            if(@params.Length > 0) @params.Remove(@params.Length - 1);
            
            Hide();
            
            mOnSubmitAct?.Invoke(mCommandWrapper.Command, @params);
        }
    }
}