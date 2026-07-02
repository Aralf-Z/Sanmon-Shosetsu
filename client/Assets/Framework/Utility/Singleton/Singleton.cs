namespace Sanmon.Utility.Singleton
{
    /// <summary>
    /// 普通单例类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : new()
    {
        private static T instance;
        public static T Ins => instance ??= new T();
        protected Singleton() { }
    }
}