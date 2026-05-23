using System;
using System.Collections.Generic;
using GameConsole.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class Searcher: MonoBehaviour
    {
        [SerializeField] private InputField inputField;

        [SerializeField] [Range(0.05f, 1f)] private float typeDelay = 0.5f;

        public float TypeDelay
        {
            get => typeDelay;
            set => typeDelay = Mathf.Clamp(value, 0.05f, 1f);
        }
        
        private SimpleSearcher mSearcher;

        private string mStringCache;

        private float mDelayTimer;
        
        private Action<List<string>> mOnType;
        
        private Action mOnClear;
        
        public void Init(List<string> commands, Action<List<string>> onType, Action onClear)
        {
            mSearcher = new SimpleSearcher(commands);
            mOnType = onType;
            mOnClear = onClear;
            
            inputField.onValueChanged.AddListener(str =>
            {
                mStringCache = str;
                mDelayTimer = typeDelay;
            });
        }

        private void Update()
        {
            if(mSearcher == null) return;
            
            //todo 做成undo redo
            //
            // if (inputField.isFocused)
            // {
            //     if (Input.GetKeyDown(KeyCode.UpArrow))
            //     {
            //         OnSearchHistory(1);
            //     }
            //     else if (Input.GetKeyDown(KeyCode.DownArrow))
            //     {
            //         OnSearchHistory(-1);
            //     }
            // }
            
            mDelayTimer -= Time.deltaTime;
        }

        private void LateUpdate()
        {
            if(mSearcher == null) return;
            
            if (mDelayTimer > 0) return;
            
            if(string.IsNullOrEmpty(mStringCache))
                mOnClear?.Invoke();
            else
                mOnType?.Invoke(mSearcher.Search(mStringCache));
            
            mDelayTimer = float.MaxValue;
        }

        private void OnSearchHistory(int step)
        {
            var str = step switch
            {
                < 0 => mSearcher.PreviousQuery,
                > 0 => mSearcher.NextQuery,
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(str)) return;
            inputField.text = str;
        }
    }
}