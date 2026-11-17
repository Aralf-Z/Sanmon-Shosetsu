using UnityEngine;
using RedSaw.CommandLineInterface;

namespace GameConsole.UI
{
    /// <summary> 隔离 TextField 默认导航和控制台的历史导航. </summary>
    public sealed class ToolkitConsoleInput : IConsoleInput
    {
        public bool MoveUp { get; private set; }

        public bool MoveDown { get; private set; }

        public bool Focus { get; private set; }

        public bool QuitFocus { get; private set; }

        public bool ShowOrHide => false;

        /// <summary> 从原 Input 系统采集, 不重复处理 UI Toolkit 导航事件. </summary>
        public void Capture(bool visible, bool focused, bool detailFocused = false)
        {
            Reset();
            if (!visible) return;

            Focus = !focused && !detailFocused && UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.C);
            MoveUp = focused && UnityEngine.Input.GetKeyDown(KeyCode.UpArrow);
            MoveDown = focused && UnityEngine.Input.GetKeyDown(KeyCode.DownArrow);
            QuitFocus = focused && UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        }

        public void Reset()
        {
            MoveUp = false;
            MoveDown = false;
            Focus = false;
            QuitFocus = false;
        }
    }
}
