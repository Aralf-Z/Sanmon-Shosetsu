using Sanmon.Core;
using Sanmon.Module;

namespace Framework.Module
{
    public static class LuaUtils
    {
        private class Getter : IGetModule
        {
            
        }

        private static readonly Getter _getter = new Getter();
        
        public static ConfigModule config => _getter.Module().Config;
    }
}