using System;
using Sanmon.Core;
using Sanmon.Helper;

namespace GameScripts
{
    public class FlowGameRuntime: FlowBase
    {
        private float mTimer;
        
        protected override void Init()
        {
            
        }

        protected override void Enter()
        {
            mTimer = 5f;
            Logger.LogInfo($"Enter game at {DateTime.Now:HH:mm:ss}", "flow");
        }

        protected override void Check(float dt)
        {
            mTimer -= dt;
            if (mTimer <= 0f)
            {
                Logger.LogInfo($"Game is running. Ticked at {DateTime.Now:HH:mm:ss}", "flow");
                mTimer = 5f;
            }
        }

        protected override void Exit()
        {
            Logger.LogInfo($"Exit game at {DateTime.Now:HH:mm:ss}", "flow");
        }
    }
}