using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> command selector. </summary>
    class LinearSelector
    {
        public event Action<int> e_onSelectionChanged;

        public int SelectionIndex => _currentIndex;

        private readonly List<string> _optionsBuffer;

        private int _currentIndex;

        private int TotalCount
        {
            get
            {
                if (_optionsBuffer == null) return 0;

                return _optionsBuffer.Count;
            }
        }

        public LinearSelector()
        {
            this._optionsBuffer = new();
            this._currentIndex = -1;
        }

        public void LoadOptions(List<string> options)
        {
            this._optionsBuffer.Clear();
            this._optionsBuffer.AddRange(options);
            this._currentIndex = -1;
        }

        public bool GetCurrentSelection(out string selection)
        {
            selection = string.Empty;
            if (_currentIndex == -1) return false;

            selection = _optionsBuffer[_currentIndex];
            return true;
        }

        public void MoveNext()
        {
            if (TotalCount == 0) return;

            if (_currentIndex == -1)
            {
                _currentIndex = 0;
                e_onSelectionChanged?.Invoke(_currentIndex);
                return;
            }

            _currentIndex = _currentIndex < TotalCount - 1
                ? _currentIndex + 1
                : 0;
            e_onSelectionChanged?.Invoke(_currentIndex);
        }

        public void MoveLast()
        {
            if (TotalCount == 0) return;

            if (_currentIndex == -1)
            {
                _currentIndex = TotalCount - 1;
                e_onSelectionChanged?.Invoke(_currentIndex);
                return;
            }

            _currentIndex = _currentIndex > 0
                ? _currentIndex - 1
                : TotalCount - 1;
            e_onSelectionChanged?.Invoke(_currentIndex);
        }
    }
}
