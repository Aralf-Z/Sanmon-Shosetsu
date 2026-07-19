namespace Sanmon.Entities
{
    /// <summary>
    /// 这是实体运行时的数据类，Cm前缀
    /// </summary>
    public abstract class ComponentBase
    {
        public Entity Host { get; internal set; }
        
        /// <summary>
        /// 被添加时
        /// </summary>
        protected internal virtual void OnAdded()
        {
            
        }
        
        /// <summary>
        /// 被移出时
        /// </summary>
        protected internal virtual void OnRemoved()
        {
            
        }
        
        public T GetSibling<T>() where T : ComponentBase => Host.GetComponent<T>();
    }
}