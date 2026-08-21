using System;
using UnityEngine;

namespace Sanmon.Module
{
    /// <summary>
    /// ui视窗
    /// </summary>
    public abstract class UIWindow: UIPart
        , IUIWindow
    {
        public abstract UIOrder Order { get; }
    }
}