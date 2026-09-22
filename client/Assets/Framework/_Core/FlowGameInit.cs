using System;
using System.Collections;
using Sanmon.Helper;
using UnityEngine;

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
            
            SanmonLogger.LogInfo("初始化游戏", "初始化");
            SanmonLogger.LogInfo($"unity版本: {Module.Version.UnityVersion}", "初始化");
            SanmonLogger.LogInfo($"yooAsset版本: {Module.Version.YooAssetVersion}", "初始化");
            SanmonLogger.LogInfo($"游戏版本：{Module.Version.GameVersion}",  "初始化");
            SanmonLogger.LogInfo($"游戏构建版本：{Module.Version.GameVersionWithBuild}",  "初始化");
            
            StartCoroutine(InitGame());
        }

        protected internal override void LogicUpdate(float dt)
        {
            if(!_done) return;
            
            NextFlow();
        }

        protected override void Exit()
        {
            SanmonLogger.LogInfo($"游戏模块初始化结束, 耗时 [{(DateTime.Now - _timer).TotalMilliseconds / 1000:F5}s]", "初始化");
        }

        private IEnumerator InitGame()
        {
            App.gameModule.Init();
            
            yield return new WaitUntil(() => App.gameModule.IsInit);
            
            SanmonLogger.LogInfo("'Module'初始化完成", "初始化");
            App.gameEntity.Init();
            
            yield return new WaitUntil(() => App.gameEntity.IsInit);
            
            SanmonLogger.LogInfo("'Entity'初始化完成", "初始化");
            App.gameNote.Init();
            
            yield return new WaitUntil(() => App.gameNote.IsInit);
            
            SanmonLogger.LogInfo("'Note'初始化完成", "初始化");
            App.gameSystem.Init();
            
            yield return new WaitUntil(() => App.gameSystem.IsInit);
            
            SanmonLogger.LogInfo("'System'初始化完成", "初始化");
            
            _done = true;
        }
    }
}