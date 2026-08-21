using UnityEngine;

namespace Sanmon.Module
{
    /// <summary>
    /// ui视窗子元素
    /// </summary>
    public abstract partial class UIPart: MonoBehaviour
        , IUIView
    {
        public UIHandle Handle { get; }

        public void Open()
        {
            gameObject.SetActive(true);
            OnOpen();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }

        public void Close()
        {
            Hide();
            OnClose();
        }
        
        protected internal abstract void OnCreate();
        protected abstract void OnOpen();
        protected abstract void OnHide();
        protected abstract void OnClose();
    }
}