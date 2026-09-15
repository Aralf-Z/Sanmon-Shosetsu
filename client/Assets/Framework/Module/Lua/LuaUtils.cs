using System;
using System.Collections.Generic;
using System.Text;

namespace Sanmon.Module
{
    public static partial class LuaUtils
    {
        public static string[] AnalyzeKey(string lua)
        {
            return LuaKeyAnalyzer.Analyze(lua);
        }
    }
}