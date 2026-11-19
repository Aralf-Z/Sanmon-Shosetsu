using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> use to record command input history. </summary>
    class InputHistory
    {
        public string Last
        {
            get
            {
                if (_history.Count == 0) return string.Empty;

                if (_lastIndex < 0 || _lastIndex >= _history.Count)
                {
                    _lastIndex = _history.Count - 1;
                }

                return _history[_lastIndex--];
            }
        }

        public string Next
        {
            get
            {
                if (_history.Count == 0) return string.Empty;

                if (_lastIndex >= _history.Count || _lastIndex < 0)
                {
                    _lastIndex = 0;
                }

                return _history[_lastIndex++];
            }
        }

        private readonly List<string> _history;

        private readonly int _capacity; // Capacity.

        private int _lastIndex;

        internal event Action e_onChanged;

        internal IReadOnlyList<string> Entries => _history;

        internal int Capacity => _capacity;

        public InputHistory(int capacity)
        {
            this._lastIndex = 0;
            this._capacity = capacity;
            this._history = new List<string>();
        }

        public void Record(string input)
        {
            Append(input);
            e_onChanged?.Invoke();
        }

        internal bool Clear()
        {
            if (_history.Count == 0) return false;

            _history.Clear();
            _lastIndex = 0;
            e_onChanged?.Invoke();
            return true;
        }

        internal bool RemoveAt(int index)
        {
            if (index < 0 || index >= _history.Count) return false;

            _history.RemoveAt(index);
            _lastIndex = _history.Count - 1;
            e_onChanged?.Invoke();
            return true;
        }

        internal void Restore(IEnumerable<string> entries)
        {
            _history.Clear();
            _lastIndex = 0;
            foreach (var input in entries)
            {
                if (!string.IsNullOrEmpty(input))
                {
                    Append(input);
                }
            }
        }

        private void Append(string input)
        {
            if (_history.Count == _capacity)
            {
                _history.RemoveAt(0);
            }

            _history.Add(input);
            _lastIndex = _history.Count - 1;
        }
    }
}
