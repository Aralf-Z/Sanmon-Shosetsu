namespace Sanmon.Module
{
    public interface IBrain
    {
        void Init();
        void Start();
        void Think(float dt);
        void Stop();
    }
}