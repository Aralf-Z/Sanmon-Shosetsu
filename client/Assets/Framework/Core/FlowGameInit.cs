using System;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Core
{
    /// <summary>
    /// 初始化管理器用
    /// </summary>
    public class FlowGameInit: FlowBase
    {
        private static GameApplication App => GameApplication.instance;

        private DateTime _timer;
        
        protected internal override void Init() { }

        protected internal override void Enter()
        {
            _timer = DateTime.Now;
            
            Logger.LogInfo("初始化游戏", "初始化");
            
            App.gameModule.Init();
        }

        protected internal override void Check(float dt)
        {
            if(!App.gameModule.IsInit) return;
            
            Logger.LogInfo("'Module'初始化完成", "初始化");
            App.gameEntity.Init();
            
            if(!App.gameEntity.IsInit) return;
            
            Logger.LogInfo("'Entity'初始化完成", "初始化");
            App.gameNote.Init();
            
            if(!App.gameNote.IsInit) return;
            
            Logger.LogInfo("'Note'初始化完成", "初始化");
            App.gameSystem.Init();
            
            if(!App.gameSystem.IsInit) return;
            
            Logger.LogInfo("'System'初始化完成", "初始化");
            
            NextFlow();
        }

        protected override void Exit()
        {
            Logger.LogInfo($"初始化游戏模块结束, 耗时 [{(DateTime.Now - _timer).TotalMilliseconds / 1000:F5}s]", "初始化");
        }
    }
}