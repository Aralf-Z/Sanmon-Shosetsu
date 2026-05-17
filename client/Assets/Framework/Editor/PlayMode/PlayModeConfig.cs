using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    [CreateAssetMenu]
    [FilePath("Editor/PlayMode.asset", FilePathAttribute.Location.ProjectFolder)]
    public class PlayModeConfig : ScriptableSingleton<PlayModeConfig>
    {
        [SerializeField] public bool isActive;
        [SerializeField] public SceneAsset entryScene;  // Play 时切换到的入口场景
    }
}