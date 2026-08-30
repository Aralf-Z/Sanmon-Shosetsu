using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Editor
{
    public sealed class SyncLuaScriptsToStreamingAssets : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            SyncLuaScripts();
        }

        public static void SyncLuaScripts()
        {
            var sourceDir = Path.Combine(PathHelper.PROJECT_PATH, "luaScripts");
            var targetDir = Path.Combine(Application.streamingAssetsPath, "luaScripts");

            if (!Directory.Exists(sourceDir))
            {
                Logger.LogWarning($"lua源文件路径错误: {sourceDir}", "lua");
                return;
            }

            Directory.CreateDirectory(targetDir);

            var expectedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 同步源文件
            foreach (var sourcePath in Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories))
            {
                // 获取相对路径
                var relativePath = Path.GetRelativePath(sourceDir, sourcePath);

                var targetPath = Path.Combine(targetDir, relativePath);

                expectedTargets.Add(targetPath);

                // 确保目标目录存在
                var targetDirectory = Path.GetDirectoryName(targetPath);

                if (!string.IsNullOrEmpty(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);

                // 覆盖复制
                File.Copy(sourcePath, targetPath, overwrite: true);
            }

            // 删除目标目录中的冗余 Lua
            foreach (var existingPath in Directory.GetFiles(targetDir, "*.lua", SearchOption.AllDirectories))
            {
                if (expectedTargets.Contains(existingPath))
                    continue;

                File.Delete(existingPath);

                var metaPath = existingPath + ".meta";

                if (File.Exists(metaPath))
                    File.Delete(metaPath);
            }

            AssetDatabase.Refresh();

            Logger.LogInfo($"已将lua脚本从'{sourceDir}'同步到'{targetDir}'", "lua");
        }
    }
}