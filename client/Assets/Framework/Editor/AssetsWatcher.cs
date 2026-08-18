using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Sanmon.Editor
{
    public class AssetsWatcher : AssetPostprocessor
    {
        private static Dictionary<string, Action<string[], string[], string[], string[]>> _extensionCallbacks = new ();
        
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            CheckAndCallback(importedAssets);
            CheckAndCallback(deletedAssets);
            CheckAndCallback(movedAssets);
            return;

            void CheckAndCallback(string[] assets)
            {
                foreach (string path in assets)
                {
                    if(_extensionCallbacks.TryGetValue(Path.GetExtension(path), out var callback))
                        callback?.Invoke(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
                }
            }
        }

        public static void AddCheckExtension(Action<string[], string[], string[], string[]> callback, params string[] extensions)
        {
            foreach (var ext in extensions)
            {
                _extensionCallbacks.Add(ext, callback);
            }
        }
    }
}