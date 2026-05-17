using UnityEngine;

namespace Sanmon.Editor
{
    public class DevConfigException : UnityException
    {
        public DevConfigException(string message) : base(message) { }
    }
    
    public class CodeGenException : UnityException
    {
        public CodeGenException(string message) : base(message) { }
    }
}