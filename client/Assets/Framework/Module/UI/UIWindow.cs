using System;
using UnityEngine;

namespace Sanmon.Module
{
    public abstract class UIWindow: UIElement
        , IUIWindow
    {
        public abstract EmUIOrder Order { get; }
    }
}