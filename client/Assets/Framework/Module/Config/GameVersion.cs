using Sanmon.Core;

namespace Sanmon.Module
{
    internal class GameVersion: AppConfig<GameVersion>
    {
        /// <summary>
        /// 游戏版本码,00.00.00.0000（主版本号.次版本号.补丁号.构建号）
        /// </summary>
        public uint versionCode;
    }
}