namespace Sanmon.Utility.Set
{
    public enum EmBufferStatus
    {
        /// <summary>
        /// 无状态，处于该状态才能被添加
        /// </summary>
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
        /// 脏，处于该状态才能被移出
        /// </summary>
        Dirty = 3,
        /// <summary>
        /// 待加入
        /// </summary>
        PendingAdd = 4,
        /// <summary>
        /// 待移出
        /// </summary>
        PendingRemove = 5,
    }
}