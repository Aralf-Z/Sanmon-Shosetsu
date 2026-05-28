namespace Sanmon.Utility.Set
{
    public interface IBufferItem
    {
        EmBufferStatus Status { get;}
        int Order { get; }
        void OnAdd();
        void OnUpdate(float dt);
        void OnRemove();
        void SetStatus(EmBufferStatus status);
    }
}