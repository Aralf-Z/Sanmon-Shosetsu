using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Query variable type. </summary>
    public delegate bool QueryVariableType(string name, out Type type);
}
