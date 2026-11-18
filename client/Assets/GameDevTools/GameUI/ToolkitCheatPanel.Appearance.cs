using UnityEngine;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public sealed partial class ToolkitCheatPanel
    {
        private int _listAppearanceVersion = -1;

        private int _listReattachFrame = -1;

        private int _listRestoreFrame = -1;

        private int _listIndex;

        private VisualElement _listParent;

        private Vector2 _listScrollOffset;

        // Unity 2022 的 ListView 回收行可能在外观切换中保留未初始化的裁剪状态.
        // 等外观稳定后重新挂载列表, 命令顺序, 筛选及选中数据均保留在原实例中.
        private void RefreshCommandAppearance()
        {
            var version = ToolkitFactory.AppearanceVersion;

            if (_listAppearanceVersion != version)
            {
                var initialized = _listAppearanceVersion >= 0;

                _listAppearanceVersion = version;
                if (initialized)
                {
                    DetachCommandList();
                }
            }

            if (_listReattachFrame >= 0 && Time.frameCount >= _listReattachFrame)
            {
                ReattachCommandList();
            }

            if (_listRestoreFrame < 0 || Time.frameCount < _listRestoreFrame) return;

            _listRestoreFrame = -1;
            _commandsView.Q<ScrollView>().scrollOffset = _listScrollOffset;
        }

        private void DetachCommandList()
        {
            if (_commandsView.parent != null)
            {
                _listScrollOffset = _commandsView.Q<ScrollView>().scrollOffset;
                _listParent = _commandsView.parent;
                _listIndex = _listParent.IndexOf(_commandsView);
                _commandsView.RemoveFromHierarchy();
            }

            _listReattachFrame = Time.frameCount + 2;
            _listRestoreFrame = -1;
        }

        private void ReattachCommandList()
        {
            _listReattachFrame = -1;
            if (_listParent == null) return;

            _commandsView.Rebuild();
            _listParent.Insert(Mathf.Min(_listIndex, _listParent.childCount), _commandsView);
            _listParent = null;
            _listRestoreFrame = Time.frameCount + 1;
        }
    }
}
