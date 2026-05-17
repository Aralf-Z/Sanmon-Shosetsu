using UnityEngine;

namespace Sanmon.Module
{
    public interface IUIElement
    {
        bool IsOpening { get; }
        void Open();
        void Hide();
        void Close();
    }
}