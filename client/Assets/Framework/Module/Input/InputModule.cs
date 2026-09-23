using UnityEngine;

namespace Sanmon.Module
{
    public class InputModule: MonoBehaviour, 
        IModule
    {
        private bool _isInit;
        
        int IModule.InitOrder => InitOrderDefine.INPUT;
        bool IModule.IsInit => _isInit;

        void IModule.Init()
        {
            _isInit = true;
        }

        void IModule.Deinit()
        {
            _isInit = false;
        }
        
        void IModule.OnLogicUpdate(float dt)
        {
            
        }
    }
}