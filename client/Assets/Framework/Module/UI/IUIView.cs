using UnityEngine;

namespace Sanmon.Module
{
    public interface IUIView
    {
        UIHandle Handle { get; }
        
        void Open();
        void Hide();
        void Close();
    }
}