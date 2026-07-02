namespace Sanmon.Utility.Set
{
    public interface IBufferItem
    {
        BufferStatus Status { get;}
        int Order { get; }
        void OnAdd();
        void OnUpdate(float dt);
        void OnRemove();
        void SetStatus(BufferStatus status);
    }
}