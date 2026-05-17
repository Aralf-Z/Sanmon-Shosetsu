using System.IO;
using Sanmon.Helper;
using Sanmon.Module;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Sanmon.Editor
{
    internal class AssetEditorProcessor : AssetPostprocessor
    {
        private const string kResourcesFileHeader = "Assets/Resources/";
        
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var filePath = AssetModule.MAP_FILE_NAME;
            var assetMap = File.Exists(filePath) ? JsonHelper.DeserializeObject<AssetMap>(File.ReadAllText(filePath)) : new AssetMap();

            if (importedAssets.Length > 0)
                ProcessNewResourcesAssetsImport(importedAssets, assetMap);

            if (deletedAssets.Length > 0)
                ProcessResourcesAssetsDelete(deletedAssets, assetMap);

            if (movedFromAssetPaths.Length > 0)
                ProcessResourcesAssetsDelete(movedFromAssetPaths, assetMap);

            if (movedAssets.Length > 0)
                ProcessNewResourcesAssetsImport(movedAssets, assetMap);

            var directoryName = Directory.GetParent(filePath)!.FullName;
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
            File.WriteAllText(filePath, JsonHelper.SerializeObject(assetMap));
            AssetDatabase.Refresh();
        }

        private static void ProcessNewResourcesAssetsImport(string[] importedAssets, AssetMap assetMap)
        {
            foreach (var t in importedAssets)
            {
                if (!t.StartsWith(kResourcesFileHeader)) continue;
                var subPath = t[kResourcesFileHeader.Length..];
                var subS = subPath.Split('.');
                if (subS.Length == 1) continue;
                var assetName = AssetDatabase.LoadAssetAtPath<Object>(t).name;
                
                assetMap.Add(assetName,subS[0]);
            }
        }

        private static void ProcessResourcesAssetsDelete(string[] deletedAssets, AssetMap assetMap)
        {
            foreach (var t in deletedAssets)
            {
                if (!t.StartsWith(kResourcesFileHeader)) continue;
                var subPath = t[kResourcesFileHeader.Length..];
                var subS = subPath.Split('.');
                if (subS.Length == 1) continue;
                var subSplitStr = subS[0].Split('/');
                var assetName = subSplitStr[^1];

                assetMap.Remove(assetName, subS[0]);
            }
        }
    }
}
