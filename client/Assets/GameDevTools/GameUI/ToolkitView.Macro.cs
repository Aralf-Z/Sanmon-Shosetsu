using System;
using GameConsole.Extension;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public sealed partial class ToolkitView
    {
        private MacroSystem _macroSystem; // 显式传入的服务, 不依赖项目总控.
        private bool _macroAttempted; // 一次进入页面只尝试一次, 不在 Tick 重试刷日志.
        private Label _macroFailure; // 错误仅显示在 Macro 页, 不阻塞其他页签.
        private string _macroFailureReason; // 完整异常交给日志, 面板仅显示简短原因.

        // 原生编辑器仅在首次进入 Macro 时建立, 失败的构造由面板自身清理.
        private void InitializeMacroPanel()
        {
            _macroAttempted = true;
            var page = _pages["macro"];
            var content = page.Q<VisualElement>("macroContent");
            ToolkitMacroPanel candidate = null;
            try
            {
                candidate = new ToolkitMacroPanel(page);
                candidate.Bind(_macroSystem);
                _macros = candidate;
                _macroFailureReason = null;
                _macroFailure?.AddToClassList("is-hidden");
                content?.RemoveFromClassList("is-hidden");
                ToolkitTextOpacity.Track(page);
            }
            catch (Exception exception)
            {
                try { candidate?.Dispose(); }
                catch (Exception cleanup) { Debug.Log($"[GameDevTools] Macro 初始化清理失败.\n{cleanup}"); }
                _macros = null;
                _macroFailureReason = exception.Message;
                content?.AddToClassList("is-hidden");
                if (_macroFailure == null)
                {
                    _macroFailure = new Label { enableRichText = false };
                    _macroFailure.AddToClassList("macro-initialization-error");
                    _macroFailure.AddToClassList("macro-error");
                    page.Add(_macroFailure);
                    ToolkitTextOpacity.Track(_macroFailure);
                }
                RefreshMacroFailure();
                _macroFailure.RemoveFromClassList("is-hidden");
                Debug.Log($"[GameDevTools] Macro 页面初始化失败, 阶段: 编辑器控件与服务绑定. 其他页面继续可用, 再次进入 Macro 将重试.\n{exception}");
            }
        }

        // 语言变化不触碰输入值或未保存草稿.
        private void RefreshMacroFailure()
        {
            if (_macroFailureReason != null && _macroFailure != null)
                _macroFailure.text = ToolkitLocalization.Format("macro.editor.initializationFailed", ("reason", (object)_macroFailureReason));
        }
    }
}
