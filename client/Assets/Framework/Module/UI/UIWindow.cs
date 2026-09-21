using System;
using UnityEngine;

namespace Sanmon.Module
{
    /// <summary>
    /// ui视窗
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public abstract class UIWindow: UIPart
        , IUIWindow
    {
        public UIHandle Handle { get; internal set; }
    }
}