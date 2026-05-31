using System;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Core
{
    /// <summary>
    /// 初始化管理器用
    /// </summary>
    public class FlowGameInit: FlowBase
    {
        private static GameApplication App => GameApplication.Instance;

        private DateTime mTimer;
        
        protected internal override void Init() { }

        protected internal override void Enter()
        {
            mTimer = DateTime.Now;
            Logger.LogInfo("初始化游戏模块", "流程");
            App.gameModule.Init();
            App.gameEntity.Init();
            App.gameNote.Init();
            App.gameSystem.Init();
        }

        protected internal override void Check(float dt)
        {
            NextFlow();
        }

        protected override void Exit()
        {
            Logger.LogInfo($"初始化游戏模块结束, 耗时 [{(DateTime.Now - mTimer).TotalMilliseconds / 1000:F5}s]", "流程");
        }
    }
}