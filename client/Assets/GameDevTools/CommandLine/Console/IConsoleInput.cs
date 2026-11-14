/* 
    使用接口描述上层逻辑的功能，目的是为了使得该模块能够保持较低的成本移植到其他引擎或者平台
    use interface between the real implemetations to decoupling the whole system's components
*/
namespace RedSaw.CommandLineInterface
{
    /// <summary> define the user input of CommandConsole, cause Unity has two different input solutions. </summary>
    public interface IConsoleInput
    {
        /// <summary> push up to get history input or change selection of alternative options. </summary>
        bool MoveUp { get; }

        /// <summary> push down to get history input or change selection of alternative options. </summary>
        bool MoveDown { get; }

        /// <summary> push ctrl+c or other keys to focus on console. </summary>
        bool Focus { get; }

        /// <summary> push esc or other keys to quit focus on console. </summary>
        bool QuitFocus { get; }

        /// <summary> push F1 or other keys to show or hide console. </summary>
        bool ShowOrHide { get; }
    }
}
