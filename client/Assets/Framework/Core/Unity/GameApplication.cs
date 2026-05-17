using System;
using UnityEngine;
namespace Sanmon.Core
{
    /// <summary>
    /// 程序接口，传递初始化、销毁、更新、后更新、物理更新
    /// </summary>
    public class GameApplication: MonoBehaviour
    {
        public static GameApplication Instance;
        
        [SerializeField] internal GameNote gameNote;
        [SerializeField] internal GameEntity gameEntity;
        [SerializeField] internal GameModule gameModule;
        [SerializeField] internal GameSystem gameSystem;
        [SerializeField] internal GameFlow   gameFlow;

        public void StartGame()
        {
            Instance = this;
            gameFlow.Init();
        }
        
        private void Update()
        {
            if(!Instance) return;
            
            var dt = Time.deltaTime;
            
            
            gameEntity.OnUpdate(dt);
            gameModule.OnUpdate(dt);
        }
        
        private void LateUpdate()
        {
            if(!Instance) return;
            
            var dt = Time.deltaTime;
            
            gameEntity.OnLateUpdate(dt);
            gameModule.OnLateUpdate(dt);
            gameFlow.OnUpdate(dt);
        }
        
        private void FixedUpdate()
        {
            if(!Instance) return;
            
            var dt = Time.fixedDeltaTime;
            
            gameEntity.OnFixedUpdate(dt);
            gameModule.OnFixedUpdate(dt);
        }

        public void ShutDown()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}