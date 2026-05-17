namespace Sanmon.Core
{
    public interface IGetSystem
    {
        
    }

    public static class GetSystemExtension
    {
        public static GameSystem System(this IGetSystem getSystem)
        {
            return GameApplication.Instance.gameSystem;
        }
    }
}