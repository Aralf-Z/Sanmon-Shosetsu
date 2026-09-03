using System.Collections.Generic;

namespace Framework.Pipeline
{
    public abstract class Pipeline<TContext>
    {
        protected Handler<TContext> _header;
        
        protected Handler<TContext> SetHeader(Handler<TContext> handler)
        {
            _header = handler;
            return _header;
        }
        
        public virtual void Do(TContext context)
        {
            _header.Do(context);
        }
    }
}