using UnityEngine;
using RedSaw.CommandLineInterface;

namespace GameConsole.UI
{
    /// <summary> 隔离 TextField 默认导航和控制台的历史导航. </summary>
    public sealed class ToolkitConsoleInput : IConsoleInput
    {
        public bool MoveUp { get; private set; }

        public bool MoveDown { get; private set; }

        public bool Focus => false;

        public bool QuitFocus => false;

        public bool ShowOrHide => false;

        /// <summary> 从原 Input 系统采集, 不重复处理 UI Toolkit 导航事件. </summary>
        public void Capture(bool visible, bool focused, bool detailFocused = false)
        {
            Reset();
            if (!visible) return;

            MoveUp = focused && UnityEngine.Input.GetKeyDown(KeyCode.UpArrow);
            MoveDown = focused && UnityEngine.Input.GetKeyDown(KeyCode.DownArrow);
        }

        public void Reset()
        {
            MoveUp = false;
            MoveDown = false;
        }
    }
}
