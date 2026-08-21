using System;
using UnityEngine;
using ILogger = YooAsset.ILogger;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Module
{
    public sealed class AssetLogger: ILogger
    {
        public void Log(string message)
        {
            Logger.LogInfo(message, "ASSET", Color.green);
        }

        public void LogWarning(string message)
        {
            Logger.LogWarning(message, "ASSET");
        }

        public void LogError(string message)
        {
            Logger.LogError(message, "ASSET");
        }

        public void LogException(Exception exception)
        {
            Logger.LogError(exception.Message, "ASSET");
        }
    }
}