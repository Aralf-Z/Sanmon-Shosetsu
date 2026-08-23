using System;
using System.Collections;
using UnityEngine;
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
        
        private bool _done = false;
        
        protected internal override void Init() { }

        protected internal override void Enter()
        {
            _timer = DateTime.Now;
            
            Logger.LogInfo("初始化游戏", "初始化");
            Logger.LogInfo($"unity version: {Application.unityVersion}", "初始化");
            
            StartCoroutine(InitGame());
        }

        protected internal override void LogicUpdate(float dt)
        {
            if(!_done) return;
            
            NextFlow();
        }

        protected override void Exit()
        {
            Logger.LogInfo($"初始化游戏模块结束, 耗时 [{(DateTime.Now - _timer).TotalMilliseconds / 1000:F5}s]", "初始化");
        }

        private IEnumerator InitGame()
        {
            App.gameModule.Init();
            
            yield return new WaitUntil(() => App.gameModule.IsInit);
            
            Logger.LogInfo("'Module'初始化完成", "初始化");
            App.gameEntity.Init();
            
            yield return new WaitUntil(() => App.gameEntity.IsInit);
            
            Logger.LogInfo("'Entity'初始化完成", "初始化");
            App.gameNote.Init();
            
            yield return new WaitUntil(() => App.gameNote.IsInit);
            
            Logger.LogInfo("'Note'初始化完成", "初始化");
            App.gameSystem.Init();
            
            yield return new WaitUntil(() => App.gameSystem.IsInit);
            
            Logger.LogInfo("'System'初始化完成", "初始化");
            
            _done = true;
        }
    }
}