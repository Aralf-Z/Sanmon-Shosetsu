using RedSaw.CommandLineInterface;
using UnityEngine;

namespace GameConsole.UI
{
    /// <summary> 隔离 TextField 默认导航和控制台的历史导航. </summary>
    public sealed class ToolkitConsoleInput : IConsoleInput
    {
        /// <summary> 当前帧向上导航请求. </summary>
        public bool MoveUp { get; private set; }

        /// <summary> 当前帧向下导航请求. </summary>
        public bool MoveDown { get; private set; }

        /// <summary> 当前帧请求聚焦控制台. </summary>
        public bool Focus { get; private set; }

        /// <summary> 当前帧请求退出文本输入. </summary>
        public bool QuitFocus { get; private set; }

        /// <summary> 窗口开关由 GameDevToolsController 统一处理. </summary>
        public bool ShowOrHide => false;

        /// <summary> 从原 Input 系统采集, 不重复处理 UI Toolkit 导航事件. </summary>
        public void Capture(bool visible, bool focused, bool detailFocused = false)
        {
            Reset();
            if (!visible)
                return;
            Focus = !focused && !detailFocused && UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.C);
            MoveUp = focused && UnityEngine.Input.GetKeyDown(KeyCode.UpArrow);
            MoveDown = focused && UnityEngine.Input.GetKeyDown(KeyCode.DownArrow);
            QuitFocus = focused && UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        }

        /// <summary> 隐藏或切换窗口时清除所有输入请求. </summary>
        public void Reset()
        {
            MoveUp = false;
            MoveDown = false;
            Focus = false;
            QuitFocus = false;
        }
    }
}
