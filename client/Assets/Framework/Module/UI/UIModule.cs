using System;
using System.Collections.Generic;
using System.Reflection;
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
        
        internal Transform Root { get; private set; }
        
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
            Root = transform;
            _isInit = true;
        }

        void IModule.Deinit()
        {
            
        }

        void IModule.OnLogicUpdate(float dt)
        {
            
        }
    }
}