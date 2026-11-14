using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> command selector. </summary>
    class LinearSelector
    {
        /// <summary> On selection changed. </summary>
        public event Action<int> e_onSelectionChanged;

        /// <summary> the current selection of the alternative options if value is -1, it means there&apos;s no alternative options choosed. </summary>
        public int SelectionIndex => _currentIndex;

        private readonly List<string> _optionsBuffer; // Options buffer.

        private int _currentIndex; // Current index.

        private int TotalCount
        {
            get
            {
                if (_optionsBuffer == null)
                    return 0;
                return _optionsBuffer.Count;
            }
        } // Total count.

        /// <summary> Create a LinearSelector instance. </summary>
        public LinearSelector()
        {
            this._optionsBuffer = new();
            this._currentIndex = -1;
        }

        /// <summary> Load options. </summary>
        public void LoadOptions(List<string> options)
        {
            this._optionsBuffer.Clear();
            this._optionsBuffer.AddRange(options);
            this._currentIndex = -1;
        }

        /// <summary> Get current selection. </summary>
        public bool GetCurrentSelection(out string selection)
        {
            selection = string.Empty;
            if (_currentIndex == -1)
                return false;
            selection = _optionsBuffer[_currentIndex];
            return true;
        }

        /// <summary> move to next alternative option. </summary>
        public void MoveNext()
        {
            if (TotalCount == 0)
                return;
            if (_currentIndex == -1)
            {
                _currentIndex = 0;
                e_onSelectionChanged?.Invoke(_currentIndex);
                return;
            }

            _currentIndex = _currentIndex < TotalCount - 1 ? _currentIndex + 1 : 0;
            e_onSelectionChanged?.Invoke(_currentIndex);
        }

        /// <summary> move to last alternative option. </summary>
        public void MoveLast()
        {
            if (TotalCount == 0)
                return;
            if (_currentIndex == -1)
            {
                _currentIndex = TotalCount - 1;
                e_onSelectionChanged?.Invoke(_currentIndex);
                return;
            }

            _currentIndex = _currentIndex > 0 ? _currentIndex - 1 : TotalCount - 1;
            e_onSelectionChanged?.Invoke(_currentIndex);
        }
    }
}
