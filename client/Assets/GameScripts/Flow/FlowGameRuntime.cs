using System;
using Sanmon.Core;
using Sanmon.Helper;
using UnityEngine.SceneManagement;

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
            SceneManager.LoadScene("GameRuntime");
        }

        protected override void Check(float dt)
        {
            
        }

        protected override void Exit()
        {
            
        }
    }
}