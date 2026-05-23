using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class Page: MonoBehaviour
    {
        [SerializeField] private Button mulPreBtn;
        [SerializeField] private Button mulNextBtn;
        [SerializeField] private Button preBtn;
        [SerializeField] private Button nextBtn;
        
        [SerializeField] private InputField inputField;
        [SerializeField] private Button gotoPageBtn;

        [SerializeField] private Text infoText;
        
        // [SerializeField] private Tagger pageTagger;
        //
        // private SimplePool<Tagger> mPageBtnPool;
        private Action<int> mGotoPageAct;
        private Action<int> mMovePageAct;

        public void Init(Action<int> gotoPageAct, Action<int> movePageAct, int mulMoveStep) 
        {
            mGotoPageAct = gotoPageAct;
            mMovePageAct = movePageAct;
            mulPreBtn.onClick.AddListener(() => mMovePageAct?.Invoke(-mulMoveStep));
            mulNextBtn.onClick.AddListener(() => mMovePageAct?.Invoke(mulMoveStep));
            preBtn.onClick.AddListener(() => mMovePageAct?.Invoke(-1));
            nextBtn.onClick.AddListener(() => mMovePageAct?.Invoke(1));
            inputField?.onSubmit.AddListener(txt => GotoPage());
            gotoPageBtn?.onClick.AddListener(GotoPage);

            // mPageBtnPool = new SimplePool<Tagger>(pageTagger, tagger =>
            // {
            //     var index = mPageBtnPool.Index;
            //     tagger.SetTagName(index.ToString());
            //     mGotoPageAct?.Invoke(index);
            // });
        }

        public void SetInfo(string info)
        {
            infoText.text = info;
        }
        
        // public void RefreshPages(int count)
        // {
        //     mPageBtnPool.RecycleAll();
        //
        //     while (count > 0)
        //     {
        //         count--;
        //         mPageBtnPool.Require();
        //     }
        // }
        
        private void GotoPage()
        {
            if (int.TryParse(inputField.text, out var page))
            {
                mGotoPageAct?.Invoke(page);
            }
        }
    }
}