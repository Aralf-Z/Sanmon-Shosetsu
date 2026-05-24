using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    internal class PathOpener
    {
        private static readonly string projectPath = Directory.GetParent(Directory.GetParent(Application.dataPath)!.FullName)!.FullName;
        
        [MenuItem("Path/项目根目录")]
        public static void OpenProjectPath()
        {
            Application.OpenURL("file://" + projectPath);
        }
        
        [MenuItem("Path/表格配置")]
        public static void OpenTableConfigPath()
        {
            Application.OpenURL("file://" + Path.Combine(projectPath, "config"));
        }
        
        [MenuItem("Path/表格")]
        public static void OpenTablePath()
        {
            Application.OpenURL("file://" + Path.Combine(projectPath, "config/_tables"));
        }
        
        [MenuItem("Path/存档")]
        public static void OpenSavePath()
        {
            Application.OpenURL("file://" + Application.persistentDataPath);
        }
    }
}