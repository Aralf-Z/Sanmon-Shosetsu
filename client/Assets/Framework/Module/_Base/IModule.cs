namespace Sanmon.Module
{
    internal interface IModule
    {
        int InitOrder { get; }
        //bool Inited { get; }
        void Init();
        void Deinit();
        void OnLogicUpdate(float dt);
    }
}