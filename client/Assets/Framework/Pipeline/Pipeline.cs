using System.Collections.Generic;

namespace Framework.Pipeline
{
    public abstract class Pipeline<TContext>
    {
        private Handler<TContext> _header;

        public Handler<TContext> SetHeader(Handler<TContext> handler)
        {
            _header = handler;
            return _header;
        }
        
        public void Do(TContext context)
        {
            _header.Do(context);
        }
    }
}