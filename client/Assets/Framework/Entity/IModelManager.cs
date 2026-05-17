namespace Sanmon.Entity
{
    public abstract class ModelManager: ComponentBase
    {
        protected internal abstract void OnModelCreated();
        protected internal abstract void OnModelDestroyed();
    }
}