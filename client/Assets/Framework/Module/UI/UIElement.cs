using UnityEngine;

namespace Sanmon.Module
{
    public abstract partial class UIElement: MonoBehaviour
        , IUIElement
    {
        public bool IsOpening { get; private set; }
        
        public void Open()
        {
            gameObject.SetActive(true);
            IsOpening = true;
            OnOpen();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            IsOpening = false;
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