namespace Sanmon.Utility.Singleton
{
    /// <summary>
    /// 普通单例类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : new()
    {
        private static T _instance;
        public static T Ins => _instance ??= new T();
        protected Singleton() { }
    }
}