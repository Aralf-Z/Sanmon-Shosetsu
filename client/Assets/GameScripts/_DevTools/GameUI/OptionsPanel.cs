using System.Collections.Generic;
using UnityEngine;

namespace GameConsole.GameUI
{
    public class OptionsPanel: MonoBehaviour
    {
        [SerializeField] private OptionsPanelOption option;
        
        private SimplePool<OptionsPanelOption> mOptionPool;

        public void Init()
        {
            mOptionPool = new(option);

            var o1 = mOptionPool.Require();
            o1.Init("[控制台] 输入历史记录容量", Config.InputHistoryCapacity, n => Config.InputHistoryCapacity = n);
            var o2 = mOptionPool.Require();
            o2.Init("[控制台] 命令查询缓存容量", Config.CommandQueryCacheCapacity, n => Config.CommandQueryCacheCapacity = n);
            var o3 = mOptionPool.Require();
            o3.Init("[控制台] 命令查询上限", Config.AlternativeCommandCount, n => Config.AlternativeCommandCount = n);
            var o4 = mOptionPool.Require();
            o4.Init("[控制台] 输出时间戳", Config.OutputWithTime, b => Config.OutputWithTime = b); //todo 后面改成 dropdown
            var o5 = mOptionPool.Require();
            o5.Init("[控制台] 记录执行失败的命令", Config.RecordFailedCommand, b => Config.RecordFailedCommand = b);
            var o6 = mOptionPool.Require();
            o6.Init("[控制台] 接受Unity日志", Config.ReceiveEngineMessage, b => Config.ReceiveEngineMessage = b);
            var o7 = mOptionPool.Require();
            o7.Init("[控制台] 输出命令解析错误调用栈", Config.OutputVmExceptionStack, b => Config.OutputVmExceptionStack = b);
        }
    }
}