using System;
using System.Collections.Generic;
using System.Reflection;
using Alchemy.Inspector;
using Sanmon.Core;
using Sanmon.Helper;
using UnityEngine;

namespace Sanmon.Module
{
    public class UIModule: MonoBehaviour
        , IModule
        , IGetModule
    {
        internal const string TITLE = "ui";

        [LabelText("UI控制器")] [SerializeField] internal UIController controller;
        internal Transform root;
        
        private bool _isInit;

        private readonly Dictionary<Type, UIHandle> _handles = new();
        
        public UIHandle GetWindow<T>() where T : UIWindow
        {
            var type = typeof(T);
            
            if (!_handles.TryGetValue(type, out var handle))
            {
                var metaData = type.GetCustomAttribute<UIMetaDataAttribute>();
                handle = new UIHandle();
                
                if (metaData == null)
                {
                    SanmonLogger.LogWarning($"UI: [{type.Name}] 未设置元数据.", TITLE);
                }
                else
                {
                    handle.metaData = metaData;
                    handle.TryLoad();
                }
                
                _handles.Add(type, handle);
            }
            
            return handle;
        }
        
        int IModule.InitOrder => InitOrderDefine.UI;
        bool IModule.IsInit => _isInit;
        
        void IModule.Init()
        {
            DontDestroyOnLoad(controller.gameObject);
            root = controller.canvas.transform;
            _isInit = true;
        }

        void IModule.Deinit()
        {
            _isInit = false;
        }

        void IModule.OnLogicUpdate(float dt)
        {
            
        }
    }
}