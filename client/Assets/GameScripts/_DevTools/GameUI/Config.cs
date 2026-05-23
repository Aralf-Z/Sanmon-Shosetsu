using System.IO;
using UnityEngine;

namespace GameConsole.GameUI
{
    public static class Config
    {
        private class SaveFile
        {
            public const string FILE_NAME = "development_config.json";

            public int inputHistoryCapacity = 20;
            public int commandQueryCacheCapacity = 20;
            public int alternativeCommandCount = 8;
            public bool outputWithTime = true;
            public bool recordFailedCommand = true;
            public bool receiveEngineMessage = true;
            public bool outputVmExceptionStack = false;

            public float infoShowDelay = .5f;
            public bool showInfo = true;
        }

        static Config()
        {
            var filePath = Path.Combine(Application.persistentDataPath, SaveFile.FILE_NAME);
            if (File.Exists(filePath))
                sFile = JsonUtility.FromJson<SaveFile>(File.ReadAllText(filePath));
            else
            {
                sFile = new SaveFile();
                File.WriteAllText(filePath, JsonUtility.ToJson(sFile));
            }
        }

        private static SaveFile sFile;

        public static string MainTagger
        {
            get => PlayerPrefs.GetString("dev_main_tagger");
            set => PlayerPrefs.SetString("dev_main_tagger", value);
        }

        public static int InputHistoryCapacity
        {
            get => sFile.inputHistoryCapacity;
            set => sFile.inputHistoryCapacity = value;
        }

        public static int CommandQueryCacheCapacity
        {
            get => sFile.commandQueryCacheCapacity;
            set => sFile.commandQueryCacheCapacity = value;
        }

        public static int AlternativeCommandCount
        {
            get => sFile.alternativeCommandCount;
            set => sFile.alternativeCommandCount = value;
        }

        public static bool OutputWithTime
        {
            get => sFile.outputWithTime;
            set => sFile.outputWithTime = value;
        }

        public static bool RecordFailedCommand
        {
            get => sFile.recordFailedCommand;
            set => sFile.recordFailedCommand = value;
        }

        public static bool ReceiveEngineMessage
        {
            get => sFile.receiveEngineMessage;
            set => sFile.receiveEngineMessage = value;
        }

        public static bool OutputVmExceptionStack
        {
            get => sFile.outputVmExceptionStack;
            set => sFile.outputVmExceptionStack = value;
        }

        public static bool ShowInfo
        {
            get => sFile.showInfo;
            set => sFile.showInfo = value;
        }

        public static float InfoShowDelay
        {
            get => sFile.infoShowDelay;
            set => sFile.infoShowDelay = value;
        }
        
        public static void Reset()
        {
            sFile =  new SaveFile();
            Save();
        }
        
        public static void Save()
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, SaveFile.FILE_NAME), JsonUtility.ToJson(sFile));
        }
    }
}