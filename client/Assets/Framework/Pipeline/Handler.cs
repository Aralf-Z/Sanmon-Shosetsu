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

        public void Do(TContext request)
        {
            if (CanHandle(request))
            {
                Process(request);
                return;
            }

            _next?.Do(request);
        }

        protected abstract bool CanHandle(TContext request);

        protected abstract void Process(TContext request);
    }
}