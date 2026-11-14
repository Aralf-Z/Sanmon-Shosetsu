using System;
using System.Collections.Generic;

/* 
    使用接口描述上层逻辑的功能，目的是为了使得该模块能够保持较低的成本移植到其他引擎或者平台
    use interface between the real implemetations to decoupling the whole system's components
*/
namespace RedSaw.CommandLineInterface
{
    /// <summary> define the interface of console renderer. </summary>
    public interface IConsoleRenderer
    {
        /// <summary> Is visible. </summary>
        bool IsVisible { get; set; }

#region InputField
        /// <summary> check if input field is focused. </summary>
        bool IsInputFieldFocus { get; }

        /// <summary> input text. </summary>
        string InputText { get; set; }

        /// <summary> input text to selection. </summary>
        string InputTextToCursor { get; set; }

        /// <summary> focus on input field. </summary>
        void Focus();

        /// <summary> activate input field. </summary>
        void ActivateInput();

        /// <summary> quit focus on input field. </summary>
        void QuitFocus();

        /// <summary> add listener on input field. </summary>
        void BindOnTextChanged(Action<string> callback);

        /// <summary> add listener on input field. </summary>
        void BindOnSubmit(Action<string> callback);

        /// <summary> set cursor position. </summary>
        void SetInputCursorPosition(int pos);

        /// <summary> make scrollbar position at last position. </summary>
        void MoveScrollBarToEnd();

        /// <summary> Move cursor to end. </summary>
        void MoveCursorToEnd()
        {
            if (InputText != null)
                SetInputCursorPosition(InputText.Length);
        }

#endregion
#region OutputPanel
        /// <summary> output message. </summary>
        void Output(string msg);

        /// <summary> output message. </summary>
        void Output(string[] msg);

        /// <summary> output message with given color. </summary>
        void Output(string msg, string color = "#ffffff");

        /// <summary> output message. </summary>
        void Output(string[] msg, string color = "#ffffff");

        /// <summary> clear current console. </summary>
        void Clear();

#endregion
#region AlternativeOptions
        /// <summary> is current alternative panel showing now. </summary>
        bool IsAlternativeOptionsActive { get; set; }

        /// <summary> render current alternative options. </summary>
        List<string> AlternativeOptions { set; }

        /// <summary> highlight this alternative options index. </summary>
        int AlternativeOptionsIndex { set; }
#endregion
    }
}
