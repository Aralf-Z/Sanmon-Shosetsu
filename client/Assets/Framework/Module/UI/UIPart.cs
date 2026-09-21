using UnityEngine;

namespace Sanmon.Module
{
    /// <summary>
    /// ui视窗元素
    /// </summary>
    public abstract partial class UIPart: MonoBehaviour
        , IUIView
    {
        //todo focus & unfocus 功能开发
        
        /// <summary>
        /// 显示
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        /// <summary>
        /// 隐藏
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            Hide();
            OnClose();
        }
        
        /// <summary>
        /// 创建时
        /// </summary>
        protected internal abstract void OnCreate();
        /// <summary>
        /// 显示时
        /// </summary>
        protected abstract void OnShow();
        /// <summary>
        /// 隐藏时
        /// </summary>
        protected abstract void OnHide();
        /// <summary>
        /// 关闭时
        /// </summary>
        protected abstract void OnClose();
    }
}