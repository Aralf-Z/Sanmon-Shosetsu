using System.IO;

namespace Sanmon.Helper
{
    public partial class Extension
    {
        public static string PathFormat(this string path)
        {
            return Path.GetFullPath(path).Replace('\\', '/');
        }
    }
}