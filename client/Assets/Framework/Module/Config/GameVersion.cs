using Sanmon.Core;

namespace Sanmon.Module
{
    internal class GameVersion: AppConfig<GameVersion>
    {
        /// <summary>
        /// 游戏版本码,00.00.000.000（主版本号.次版本号.补丁号.内部版本号）
        /// </summary>
        public uint versionCode;
    }
}