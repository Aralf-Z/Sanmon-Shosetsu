using System;
using System.Collections;
using System.Collections.Generic;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;
using UnityEngine;

namespace GameConsole.GameUI
{
    public class CheatPanelSubmenuBox: MonoBehaviour
    {
        private const string kAllTag = "all";
        
        [SerializeField] private int showCount = 40;
        [SerializeField] private CheatPanelSubmenuButton btnTemplate;
        [SerializeField] private Tagger taggerTemplate;
        [SerializeField] private Searcher searcher;
        [SerializeField] private Page page;
        
        private SimplePool<CheatPanelSubmenuButton> mBtnPool;
        private SimplePool<Tagger> mTaggerPool;
        
        private Action<Command, string> mOnSubmit;

        private CommandWrapper mCommandWrapper;
        private Dictionary<Command, int> mCommandPage = new ();
        
        public void Init(Action<Command, string> onSubmit)
        {
            mOnSubmit = onSubmit;
            
            mBtnPool = new SimplePool<CheatPanelSubmenuButton>(btnTemplate, x => x.Init(OnSubmit));
            mTaggerPool = new SimplePool<Tagger>(taggerTemplate, x => x.Init(OnClickTagger));
            
            page.Init(GotoPage, MovePage, 999999);
        }
        
        public void Show(CommandWrapper commandWrapper)
        {
            gameObject.SetActive(true);

            mCommandWrapper = commandWrapper;

            //page.RefreshPages(mParamInfos.Count /  showCount + 1);
            GotoPage(mCommandPage.GetValueOrDefault(mCommandWrapper.Command));
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Hide();
            }
        }

        private void OnSubmit(string param)
        {
            mOnSubmit?.Invoke(mCommandWrapper.Command, param);
        }

        private void GotoPage(int pageIndex)
        {
            var pageMax = mCommandWrapper.ParameterDefines.Count/ showCount + 1;
            var @goto = Mathf.Clamp(pageIndex, 1, pageMax);
            var start = (@goto - 1)  * showCount;
            
            mCommandPage[mCommandWrapper.Command] = @goto;
            mBtnPool.RecycleAll();
            page.SetInfo($"{@goto}/{pageMax}");
            
            for (var i = 0; i < showCount && i + start < mCommandWrapper.ParameterDefines.Count; i++)
            {
                var param = mCommandWrapper.ParameterDefines[i + start];
                var btn = mBtnPool.Require();
                btn.Show(param);
            }
        }
        
        private void MovePage(int dir)
        {
            var targetPage = mCommandPage[mCommandWrapper.Command] + dir;
            GotoPage(targetPage);
        }
        
        private void OnSearch(List<string> results)
        {
            
        }

        private void OnClearSearch()
        {
            
        }

        private void OnClickTagger(Tagger tagger)
        {
            
        }
    }
}