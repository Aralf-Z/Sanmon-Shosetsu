using System.Collections.Generic;
using RedSaw.CommandLineInterface;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole.GameUI
{

    /// <summary>the final wrapper of CommandConsoleSystem build in Unity</summary>
    public class GameDevTools : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        [SerializeField] private ConsolePanel consolePanel;
        [SerializeField] private CheatPanel cheatPanel;
        [SerializeField] private OptionsPanel optionsPanel;
        [SerializeField] private Tagger consoleTagger;
        [SerializeField] private Tagger cheatTagger;
        [SerializeField] private Tagger optionTagger;
        
        private Tagger mCurrentTagger;
        private Dictionary<string, Transform> mToolMap;
        
        private ConsoleController<LogType> mConsole;
        
        private void Awake()
        {
            gameObject.SetActive(true);
            root.gameObject.SetActive(false);
            mConsole = new ConsoleController<LogType>(
                consolePanel,
                new ConsolePanelInput(),

                inputHistoryCapacity: Config.InputHistoryCapacity,
                commandQueryCacheCapacity: Config.CommandQueryCacheCapacity,
                alternativeCommandCount: Config.AlternativeCommandCount,
                shouldRecordFailedCommand: Config.RecordFailedCommand,
                outputWithTime: Config.OutputWithTime,
                outputStackTraceOfCommandExecution: Config.OutputVmExceptionStack
            );
            if (Config.ReceiveEngineMessage) Application.logMessageReceived += UnityConsoleLog;
        }

        private void Start()
        {
            var tools = new []{consolePanel.transform, cheatPanel.transform, optionsPanel.transform};
            var taggers = new []{consoleTagger, cheatTagger, optionTagger};
            
            cheatPanel.SetConsole(mConsole);
            optionsPanel.Init();
            
            mToolMap = new();
            
            for (var i = 0; i < tools.Length; i++)
            {
                var tool = tools[i];
                var tagger = taggers[i];
                var tagName = tagger.GetComponentInChildren<Text>().text;
                
                mToolMap.Add(tagName, tool);
                tagger.Init(ChangeTagger);
                tagger.SetTagName(tagName);
                tool.gameObject.SetActive(false);
            }

            foreach (var tagger in taggers)
            {
                if (tagger.TagName == Config.MainTagger)
                {
                    ChangeTagger(tagger);
                    break;
                }
            }
            
            if(mCurrentTagger == null) ChangeTagger(consoleTagger);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                root.SetActive(!root.activeSelf);
            }
            
            mConsole.Update();
        }
        
        private void OnDestroy()
        {
            if (Config.ReceiveEngineMessage)
                Application.logMessageReceived -= UnityConsoleLog;
        }

        private void ChangeTagger(Tagger newTagger)
        {
            if (mCurrentTagger)
            {
                mToolMap[mCurrentTagger.TagName].gameObject.SetActive(false);
                mCurrentTagger.Deselect();
            }
            mCurrentTagger = newTagger;
            mCurrentTagger.Select();
            mToolMap[mCurrentTagger.TagName].gameObject.SetActive(true);
            Config.MainTagger = mCurrentTagger.TagName;
        }
        
        private void UnityConsoleLog(string msg, string stack, LogType type)
        {
            mConsole.Output(msg, GetHexColor(type));
        }
        
        private string GetHexColor(LogType type)
        {
            return type switch
            {
                LogType.Error or LogType.Exception or LogType.Assert => "#b13c45",
                LogType.Warning => "yellow",
                _ => "#fffde3",
            };
        }

        public void Print(string msg, LogType type)
        {
            mConsole.Output(msg, GetHexColor(type));
        }
        
        public void ClearOutput()
        {
            mConsole.ClearOutputPanel();
        } 
    }
}