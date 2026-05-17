using Sanmon.Helper;

namespace Sanmon.Core
{
    /// <summary>
    /// 最后一个流程，关闭游戏用
    /// </summary>
    public class FlowGameClose: FlowBase
    {
        private static GameApplication App => GameApplication.Instance;
        
        protected internal override void Init() { }

        protected internal override void Enter()
        {
            Logger.LogInfo("关闭游戏管理器", "流程");
            App.gameModule.Destroy();
            App.gameNote.Destroy();
            App.gameEntity.Destroy();
            App.gameSystem.Destroy();
            
            App.ShutDown();
        }

        protected internal override void Check(float dt) { }

        protected override void Exit()
        {
            Logger.LogInfo("关闭游戏管理器结束", "流程");
        }
    }
}