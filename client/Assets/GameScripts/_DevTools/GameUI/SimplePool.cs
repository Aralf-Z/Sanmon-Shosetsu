using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameConsole.GameUI
{
    public class SimplePool<T> where T : MonoBehaviour
    {
        private readonly List<T> mPool = new ();
        private readonly List<T> mUsing = new ();
        private readonly Action<T> mInitAct;
        private readonly T mTemplate;

        public IReadOnlyList<T> Using => mUsing;
        public int Index { get; private set; }
        
        public SimplePool(T template, Action<T> initAct = null)
        {
            mTemplate = template;
            mInitAct = initAct;
            
            mTemplate.gameObject.SetActive(false);
        }

        public T Require()
        {
            if (Index < mPool.Count)
            {
                var tar = mPool[Index++];
                tar.gameObject.SetActive(true);
                mUsing.Add(tar);
                return tar;
            }

            var go = UnityEngine.Object.Instantiate(mTemplate.gameObject, mTemplate.transform.parent);
            var obj = go.GetComponent<T>();
            Index++;
            mInitAct?.Invoke(obj);
            go.gameObject.SetActive(true);
            mPool.Add(obj);
            mUsing.Add(obj);
            return obj;
        }

        public void RecycleAll()
        {
            foreach (var obj in mUsing)
            {
                obj.gameObject.SetActive(false);
            }
            
            mUsing.Clear();
            Index = 0;
        }
    }
}