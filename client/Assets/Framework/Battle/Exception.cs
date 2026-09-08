using System;

namespace Sanmon.Battle
{
    public class EffectException : Exception
    {
        public EffectException(string message) : base(message) { }
        public EffectException(string message, Exception inner) : base(message, inner) { }
    }
    
    public class UnitException : Exception
    {
        public UnitException(string message) : base(message) { }
        public UnitException(string message, Exception inner) : base(message, inner) { }
    }
}