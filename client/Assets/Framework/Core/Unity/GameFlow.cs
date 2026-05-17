using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sanmon.Core
{
    /// <summary>
    /// 流程管理器
    /// </summary>
    public class GameFlow: MonoBehaviour
    {
        private readonly Dictionary<Type, FlowBase> mFlowMap = new ();

        public FlowBase curFlow;
        
        internal void Init()
        {
            var flows = GetComponentsInChildren<FlowBase>();

            foreach (var flow in flows)
            {
                flow.Init();
                mFlowMap.Add(flow.GetType(), flow);
            }
            
            curFlow = mFlowMap[typeof(FlowGameInit)];
            curFlow.Enter();
        }
        
        internal void OnUpdate(float dt)
        {
            curFlow.Check(dt);
        }
    }
}