using Alchemy.Inspector;
using Sanmon.Module;
using UnityEngine;

namespace Framework.Module
{
    public class CameraModule: MonoBehaviour
        ,IModule
    {
        [LabelText("相机控制器")][SerializeField] internal CameraController controller;
        
        private bool _isInit;

        public void SetMainFollower(Transform follower)
        {
            controller.normalCamera.Priority = 100;
            controller.normalCamera.LookAt = follower;
            controller.normalCamera.Follow = follower;
        }
        
        int IModule.InitOrder => InitOrderDefine.CAMERA;
        bool IModule.IsInit => _isInit;

        void IModule.Init()
        {
            DontDestroyOnLoad(controller.gameObject);
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