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
        private readonly Dictionary<Type, FlowBase> _flowMap = new ();

        public FlowBase curFlow;
        
        internal void Init()
        {
            var flows = GetComponentsInChildren<FlowBase>();

            foreach (var flow in flows)
            {
                flow.Init();
                _flowMap.Add(flow.GetType(), flow);
            }
            
            curFlow = _flowMap[typeof(FlowGameInit)];
            curFlow.Enter();
        }
        
        internal void OnLogicUpdate(float dt)
        {
            curFlow.LogicUpdate(dt);
        }
    }
}