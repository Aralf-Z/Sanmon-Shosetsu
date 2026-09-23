using System;
using Alchemy.Inspector;
using UnityEngine;

namespace Sanmon.Core
{
    /// <summary>
    /// 程序接口，传递初始化、销毁、更新、后更新、物理更新
    /// </summary>
    public class GameApplication: MonoBehaviour
    {
        public static GameApplication instance;
        
        [LabelText("游戏记录")][SerializeField] internal GameNote gameNote;
        [LabelText("游戏实体")][SerializeField] internal GameEntity gameEntity;
        [LabelText("游戏模块")][SerializeField] internal GameModule gameModule;
        [LabelText("游戏系统")][SerializeField] internal GameSystem gameSystem;
        [LabelText("游戏流程")][SerializeField] internal GameFlow gameFlow;

        internal bool isRunning;
        
        public void StartGame()
        {
            instance = this;
            isRunning = true;
            gameFlow.Init();
        }
        
        private void Update()
        {
            if(!instance) return;
            
            var dt = Time.deltaTime;
            
            FrameUpdater.Ins.FrameUpdate(dt);
        }
        
        private void FixedUpdate()
        {
            if(!instance) return;
            
            var dt = Time.fixedDeltaTime;
            
            gameSystem.OnLogicUpdate(dt);
            gameEntity.OnLogicUpdate(dt);
            gameModule.OnLogicUpdate(dt);
            gameFlow.OnLogicUpdate(dt);
        }

        public void ShutDown()
        {
            isRunning = false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}