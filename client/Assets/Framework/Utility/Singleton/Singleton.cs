namespace Sanmon.Utility.Singleton
{
    /// <summary>
    /// 普通单例类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : new()
    {
        private static T sInstance;
        public static T Ins => sInstance ??= new T();
        protected Singleton() { }
    }
}