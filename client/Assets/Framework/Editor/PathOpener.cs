using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sanmon.Editor
{
    internal class PathOpener
    {
        private static readonly string PROJECT_PATH = Directory.GetParent(Directory.GetParent(Application.dataPath)!.FullName)!.FullName;
        
        [MenuItem("Path/项目根目录")]
        public static void OpenProjectPath()
        {
            Application.OpenURL("file://" + PROJECT_PATH);
        }
        
        [MenuItem("Path/表格工作区")]
        public static void OpenTableConfigPath()
        {
            Application.OpenURL("file://" + Path.Combine(PROJECT_PATH, "table_config"));
        }
        
        [MenuItem("Path/表格")]
        public static void OpenTablePath()
        {
            Application.OpenURL("file://" + Path.Combine(PROJECT_PATH, "table_config/_tables"));
        }
        
        [MenuItem("Path/存档")]
        public static void OpenSavePath()
        {
            Application.OpenURL("file://" + Application.persistentDataPath);
        }
    }
}