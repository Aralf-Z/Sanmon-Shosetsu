using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> define the interface of console renderer. </summary>
    public interface IConsoleRenderer
    {
        bool IsVisible { get; set; }

        #region InputField

        bool IsInputFieldFocus { get; }

        string InputText { get; set; }

        string InputTextToCursor { get; set; }

        void Focus();

        void ActivateInput();

        void QuitFocus();

        void BindOnTextChanged(Action<string> callback);

        void BindOnSubmit(Action<string> callback);

        void SetInputCursorPosition(int pos);

        void MoveScrollBarToEnd();

        void MoveCursorToEnd()
        {
            if (InputText != null)
            {
                SetInputCursorPosition(InputText.Length);
            }
        }

        #endregion
        #region OutputPanel

        void Output(string msg);

        void Output(string[] msg);

        void Output(string msg, string color = "#ffffff");

        void Output(string[] msg, string color = "#ffffff");

        void Clear();
        #endregion
        #region AlternativeOptions

        bool IsAlternativeOptionsActive { get; set; }

        List<string> AlternativeOptions { set; }

        int AlternativeOptionsIndex { set; }
        #endregion
    }
}
