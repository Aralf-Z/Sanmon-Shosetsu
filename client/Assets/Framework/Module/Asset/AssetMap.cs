using System.Collections.Generic;

namespace Sanmon.Module
{
    public class AssetMap
    {
        public readonly Dictionary<string, string> mNamePathDic = new();

        public string this[string key] => mNamePathDic[key];
        
        public void Add(string name, string path)
        {
            mNamePathDic[name] = path;
        }

        public void Remove(string name, string path)
        {
            mNamePathDic.Remove(name);
        }

        public bool Contains(string name)
        {
            return mNamePathDic.ContainsKey(name);
        }

        public bool Try(string name, out string path)
        {
            return mNamePathDic.TryGetValue(name, out path);
        }
    }
}