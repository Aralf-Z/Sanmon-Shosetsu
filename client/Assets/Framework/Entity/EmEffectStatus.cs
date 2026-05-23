namespace Sanmon.Entity
{
    public enum EmEffectStatus
    {
        None = 0,
        /// <summary>
        /// 运行中
        /// </summary>
        Running = 1,
        /// <summary>
        /// 失活，暂时不运行
        /// </summary>
        Disabled = 2,
        /// <summary>
        /// 脏，即需要被移出
        /// </summary>
        Dirty = 3,
    }
}