using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameConsole.GameUI
{
    public class CheatPanel: MonoBehaviour
    {
        private const string kAllTag = "all";
        
        [SerializeField] private CheatPanelCheat cheatTemplate;
        [SerializeField] private Tagger singleTagger;
        [SerializeField] private CheatPanelSubmenuBox submenuBox;
        [SerializeField] private CheatPanelParamsBox paramsBox;
        [SerializeField] private Searcher searcher;
        
        
        private SimplePool<CheatPanelCheat> mCheatPool;
        private SimplePool<Tagger> mTaggerPool;
        
        private ConsoleController<LogType> mConsole;

        private List<CheatPanelCheat> mAllCheats = new();
        private Dictionary<string, CommandWrapper> mCommandMap;
        private Dictionary<string, List<CommandWrapper>> mCommandByTag;
        private List<CommandWrapper> mAllShowCommands = new ();
        
        private Tagger mCurrentTagger;
        private Tagger mAllTagger;
        
        private void OnEnable()
        {
            if (mCommandMap == null && mConsole != null)
            {
                var time = Time.realtimeSinceStartup;
                mCommandMap = mConsole.commandSystem.vm.AllCallables
                    .OfType<Command>()
                    .OrderBy(x => x.Name)
                    .ToDictionary(x => $"{x.tag}_{x.name}_{x.description}", c => new CommandWrapper(c));

                mCommandByTag = new Dictionary<string, List<CommandWrapper>>()
                {
                    [kAllTag] = new (),
                };
                foreach (var cw in mCommandMap.Values)
                {
                    var tagName = string.IsNullOrEmpty(cw.Command.tag) ? kAllTag : cw.Command.tag;
                    
                    if (mCommandByTag.TryGetValue(tagName, out var commandList))
                        commandList.Add(cw);
                    else
                        mCommandByTag.Add(tagName, new List<CommandWrapper>(){cw});
                }

                mCheatPool = new SimplePool<CheatPanelCheat>(cheatTemplate, x => x.Init(OnClickSubmit));
                mTaggerPool = new SimplePool<Tagger>(singleTagger, x => x.Init(OnClickTagger));

                submenuBox.Init(ExecuteCommand);
                submenuBox.Hide();
                paramsBox.Init(ExecuteCommand);
                paramsBox.Hide();
                searcher.Init(mCommandMap.Keys.ToList(), OnSearch, OnClearSearch);

                ShowAllTaggers();
                ShowAllCheats();

                if (Time.realtimeSinceStartup - time > .5f)
                    mConsole.Output($"cheat panel init cost '{Time.realtimeSinceStartup - time}s' too expensive.", "#b13c45");
            }
            
            var target = mCurrentTagger??mAllTagger;
            mCurrentTagger?.Deselect();
            mCurrentTagger = null;
            OnClickTagger(target);
        }

        private void OnDisable()
        {
            submenuBox.Hide();
            paramsBox.Hide();
        }
        
        public void SetConsole(ConsoleController<LogType> console)
        {
            mConsole = console;
        }
        
        private void ShowAllTaggers()
        {
            foreach (var key in mCommandByTag.Keys)
            {
                var tagger = mTaggerPool.Require();
                tagger.SetTagName(key);

                if (tagger.TagName == kAllTag) mAllTagger = tagger;
            }
        }

        private void ShowAllCheats()
        {
            foreach (var (_, wrapper) in mCommandMap)
            {
                if(wrapper.IsCheatIgnore) continue;
                var cheat0 = mCheatPool.Require();
                cheat0.SetCommand(wrapper);
                mAllCheats.Add(cheat0);
            }
        }
        
        private IEnumerator ShowFilteredCheat()
        {
            foreach (var cheat1 in mAllCheats)
            {
                cheat1.gameObject.SetActive(false);
            }
            
            foreach (var cheat2 in mAllCheats)
            {
                cheat2.gameObject.SetActive(mAllShowCommands.Contains(cheat2.CommandWrapper));
                yield return null;
            }
        }

        private void OnClickTagger(Tagger tagger)
        {
            if (tagger == mCurrentTagger) return;
            
            mCurrentTagger?.Deselect();
            StopCoroutine(ShowFilteredCheat());
            mCheatPool.RecycleAll();
            mAllShowCommands.Clear();
            mCurrentTagger = tagger;
            mCurrentTagger.Select();
            
            if(mCurrentTagger == mAllTagger)
            {
                foreach (var (_, command) in mCommandMap)
                {
                    mAllShowCommands.Add(command);
                }
            }
            else
            {
                mAllShowCommands.AddRange(mCommandByTag[mCurrentTagger.TagName]);
            }
            
            StartCoroutine(ShowFilteredCheat());
        }

        private void OnSearch(List<string> results)
        {
            StopCoroutine(ShowFilteredCheat());
            mCurrentTagger?.Deselect();
            mCurrentTagger = null;
            mCheatPool.RecycleAll();
            mAllShowCommands.Clear();
            
            foreach (var key in results)
            {
                if (mCommandMap.TryGetValue(key, out var command))
                {
                    mAllShowCommands.Add(command);
                }
            }
            
            StartCoroutine(ShowFilteredCheat());
        }

        private void OnClearSearch()
        {
            OnClickTagger(mAllTagger);
        }
        
        private void OnClickSubmit(CommandWrapper commandWrapper)
        {
            //todo 分析采用什么模式、记录历史等等
            
            if (commandWrapper.ParameterDefines.Count > 0)
            {
                submenuBox.Show(commandWrapper);
            }
            else if (commandWrapper.ParameterInfos.Count > 0)
            {
                paramsBox.Show(commandWrapper);
            }
            else
            {
                ExecuteCommand(commandWrapper.Command);
            }
        }

        private void ExecuteCommand(Command command, string param = null)
        {
            var commandStr = $"{command.name} {param}";
            mConsole.OnSubmit(commandStr);

            StartCoroutine(ClearSubmitFocus());
        }
        
        private IEnumerator ClearSubmitFocus()
        {
            yield return null;
            EventSystem.current.SetSelectedGameObject(null);//避免聚焦控制台文本输入框
        }
    }
}