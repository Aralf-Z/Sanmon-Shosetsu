using System;
using System.Collections.Generic;
using Sanmon.Core;
using UnityEngine;

namespace Sanmon.Module
{
    public class UIModule: MonoBehaviour
        , IModule
        , IGetModule
    {
        int IModule.InitOrder => InitOrderDefine.UI;

        void IModule.Init()
        {
            foreach (EmUIOrder order in Enum.GetValues(typeof(EmUIOrder)))
            {
                var orderName = Enum.GetName(typeof(EmUIOrder), order);
                var canvas = transform.Find(orderName).GetComponent<Canvas>();
                canvas.sortingOrder = (int)order;
                mOrdersRoot.Add(order, canvas.transform);
            }
        }

        void IModule.Deinit()
        {
            
        }

        void IModule.OnUpdate(float dt)
        {
            
        }
        
        private readonly Dictionary<EmUIOrder, Transform> mOrdersRoot = new();//层级根节点
        private readonly Dictionary<Type, UIWindow> mCachedWindows = new();//缓存的window
        private readonly Dictionary<Type, UIWindow> mOpeningWindows = new();//打开中的window
        
        public T Open<T>() where T : UIWindow
        {
            TryLoad<T>();
            
            var type = typeof(T);
            
            if(mOpeningWindows.TryGetValue(type, out var openingWindow)) return (T)openingWindow;
            
            var target = mCachedWindows[type];
            target.Open();
            target.transform.SetParent(mOrdersRoot[target.Order], false);
            target.transform.SetAsLastSibling();
            mOpeningWindows.Add(type, target);
            
            return (T)target;
        }

        public T Hide<T>() where T : UIWindow
        {
            TryLoad<T>();
            
            var type = typeof(T);
            
            if (mOpeningWindows.ContainsKey(type))
            {
                var window = mOpeningWindows[type];
                window.Hide();
                mOpeningWindows.Remove(type);
                return (T)window;
            }

            return (T)mCachedWindows[type];
        }

        private void TryLoad<T>() where T : UIWindow
        {
            var type = typeof(T);
            var windowName = type.Name;

            if (mCachedWindows.ContainsKey(type)) return;
            
            var prefab = this.Module().Asset.LoadSync<GameObject>(windowName);
            
            var window = Instantiate(prefab).GetComponent<T>();

            if (window == null)
            {
                throw new NullReferenceException($"can not find window : [{windowName}]");
            }
            
            window.OnCreate();
            mCachedWindows.Add(type, window);
        }
    }
}