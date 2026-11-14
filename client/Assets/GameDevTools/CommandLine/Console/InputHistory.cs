using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> use to record command input history. </summary>
    class InputHistory
    {
        /// <summary> get last input string. </summary>
        public string Last
        {
            get
            {
                if (_history.Count == 0)
                    return string.Empty;
                if (_lastIndex < 0 || _lastIndex >= _history.Count)
                    _lastIndex = _history.Count - 1;
                return _history[_lastIndex--];
            }
        }

        /// <summary> get next input string. </summary>
        public string Next
        {
            get
            {
                if (_history.Count == 0)
                    return string.Empty;
                if (_lastIndex >= _history.Count || _lastIndex < 0)
                    _lastIndex = 0;
                return _history[_lastIndex++];
            }
        }

        private readonly List<string> _history; // History.

        private readonly int _capacity; // Capacity.

        private int _lastIndex; // Last index.

        /// <summary> Create a InputHistory instance. </summary>
        public InputHistory(int capacity)
        {
            this._lastIndex = 0;
            this._capacity = capacity;
            this._history = new List<string>();
        }

        /// <summary> record input string. </summary>
        public void Record(string input)
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
