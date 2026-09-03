using Sanmon.Helper;

namespace Framework.Pipeline
{
    public abstract class Handler<TContext>
    {
        private Handler<TContext> _next;

        public Handler<TContext> SetNext(Handler<TContext> next)
        {
            _next = next;
            return next;
        }
        public void Do(TContext context)
        {
            Logger.LogTime($"Do {GetType().Name}");
            var go = CanHandle(context) && Process(context);
            Logger.LogTime($"DoEnd {GetType().Name}");
            if (go)
                _next?.Do(context);
        }

        /// <returns> 处理条件检测，默认true </returns>
        protected virtual bool CanHandle(TContext context) => true;
        
        /// <returns> 是否进入下一个Handler </returns>
        protected abstract bool Process(TContext context);
    }
}