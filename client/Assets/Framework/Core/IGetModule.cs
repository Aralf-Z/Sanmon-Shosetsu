using Sanmon.Core;
using Sanmon.Module;

namespace Sanmon.Core
{
    public interface IGetModule
    {
        
    }

    public static class GetModuleExtenstion
    {
        public static GameModule Module(this IGetModule getModule)
        {
            return GameApplication.Instance.gameModule;
        }
    }
}