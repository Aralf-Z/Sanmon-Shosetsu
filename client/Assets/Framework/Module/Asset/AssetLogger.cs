using System;
using Sanmon.Helper;
using UnityEngine;
using ILogger = YooAsset.ILogger;

namespace Sanmon.Module
{
    internal sealed class AssetLogger: ILogger
    {
        public void Log(string message)
        {
            SanmonLogger.LogInfo(message, "ASSET", Color.green);
        }

        public void LogWarning(string message)
        {
            SanmonLogger.LogWarning(message, "ASSET");
        }

        public void LogError(string message)
        {
            SanmonLogger.LogError(message, "ASSET");
        }

        public void LogException(Exception exception)
        {
            SanmonLogger.LogError(exception.Message, "ASSET");
        }
    }
}