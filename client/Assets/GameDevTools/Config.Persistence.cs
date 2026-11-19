using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using GameConsole.Extension;
using RedSaw.CommandLineInterface;

namespace GameConsole
{
    public static partial class Config
    {
        public static string FilePath => Path.Combine(Application.persistentDataPath, "development_data.json");

        private static PersistentData _document;
        private static InputHistory _inputHistory;
        private static bool _historySavePending;
        private static float _historySaveAfter;
        private static bool _backupUnreadableFile;

        private static PersistentData Document
        {
            get
            {
                if (_document == null)
                {
                    Reload();
                }

                return _document;
            }
        }

        private static GameDevToolsConfig Data => Document.settings;

        /// <summary> 启动时载入全部数据, 已绑定控制台时仅重读设置. </summary>
        public static void Reload()
        {
            PersistentData loaded;
            try
            {
                loaded = Read();
            }
            catch (Exception exception)
            {
                _backupUnreadableFile = true;
                Debug.LogWarning($"[GameDevTools] 读取数据失败. {exception.Message}");
                if (_inputHistory != null) return;

                _document = new PersistentData();
                _historySavePending = false;
                NotifySettingsChanged();
                return;
            }

            _backupUnreadableFile = false;
            if (_inputHistory != null)
            {
                _document.settings = loaded?.settings ?? new GameDevToolsConfig();
            }
            else
            {
                _document = loaded ?? new PersistentData();
                _historySavePending = false;
                if (loaded == null)
                {
                    WriteCurrent();
                }
            }

            NotifySettingsChanged();
        }

        /// <summary> 为 Inspector 提供独立的设置副本. </summary>
        public static GameDevToolsConfig Snapshot() => JsonConvert.DeserializeObject<GameDevToolsConfig>(JsonConvert.SerializeObject(Data));

        public static void Reset()
        {
            Document.settings = new GameDevToolsConfig();
            Save();
        }

        public static bool Save()
        {
            Normalize(Data);
            if (!WriteCurrent()) return false;

            NotifySettingsChanged();
            return true;
        }

        internal static void NotifySettingsChanged() => e_onChanged?.Invoke();

        internal static void BindInputHistory(InputHistory history)
        {
            var entries = Document.inputHistory;

            _inputHistory = history;
            _inputHistory.Restore(entries);
            _inputHistory.e_onRecorded += QueueInputHistorySave;
            if (_inputHistory.Entries.Count != entries.Count)
            {
                QueueInputHistorySave();
            }
        }

        internal static void UnbindInputHistory()
        {
            if (_inputHistory == null) return;

            _inputHistory.e_onRecorded -= QueueInputHistorySave;
            if (_historySavePending)
            {
                _document.inputHistory = _inputHistory.Entries.ToList();
            }

            _inputHistory = null;
        }

        internal static void TickInputHistory()
        {
            if (_historySavePending && Time.realtimeSinceStartup >= _historySaveAfter)
            {
                FlushInputHistory();
            }
        }

        internal static void FlushInputHistory()
        {
            if (_historySavePending)
            {
                WriteCurrent();
            }
        }

        internal static IEnumerable<Macro> LoadMacros() => Document.macros.Select(entry => new Macro(entry.source) { Id = entry.id });

        internal static bool TrySaveMacros(IEnumerable<Macro> macros, string deletedId, out bool pinsChanged, out string error)
        {
            var entries = macros.Select(macro => new MacroEntry { id = macro.Id, source = macro.ToSource() })
                .ToList();
            var settings = Data;

            pinsChanged = deletedId != null && settings.cheatPinnedMacros.Contains(deletedId);
            if (pinsChanged)
            {
                settings = Snapshot();
                settings.cheatPinnedMacros.Remove(deletedId);
            }

            if (!TryWrite(settings, entries, out error)) return false;

            _document.macros = entries;
            if (pinsChanged)
            {
                _document.settings.cheatPinnedMacros = settings.cheatPinnedMacros;
            }

            return true;
        }

        private static PersistentData Read()
        {
            string json;
            try
            {
                json = File.ReadAllText(FilePath);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }

            var loaded = JsonConvert.DeserializeObject<PersistentData>(json) ?? throw new JsonSerializationException("数据根节点不能为空.");
            var ids = new HashSet<string>(StringComparer.Ordinal);

            Normalize(loaded.settings);
            loaded.macros.RemoveAll(entry => entry == null || string.IsNullOrEmpty(entry.id) || entry.source == null || !ids.Add(entry.id));
            return loaded;
        }

        private static void QueueInputHistorySave()
        {
            if (_historySavePending) return;

            _historySavePending = true;
            _historySaveAfter = Time.realtimeSinceStartup + CONFIG_SAVE_DELAY;
        }

        private static bool WriteCurrent()
        {
            if (TryWrite(_document.settings, _document.macros, out var error)) return true;

            Debug.LogWarning($"[GameDevTools] 保存数据失败. {error}");
            return false;
        }

        private static bool TryWrite(GameDevToolsConfig settings, List<MacroEntry> macros, out string error)
        {
            try
            {
                var snapshot = new PersistentData
                {
                    settings = settings,
                    macros = macros,
                    inputHistory = _inputHistory?.Entries.ToList() ?? _document.inputHistory
                };
                var path = FilePath;
                var temporary = path + ".tmp";

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(temporary, JsonConvert.SerializeObject(snapshot, Formatting.Indented));
                if (File.Exists(path))
                {
                    var backup = _backupUnreadableFile ? $"{path}.{Guid.NewGuid():N}.corrupt" : null;

                    File.Replace(temporary, path, backup);
                }
                else
                {
                    File.Move(temporary, path);
                }

                _document.inputHistory = snapshot.inputHistory;
                _historySavePending = false;
                _backupUnreadableFile = false;
                error = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                if (_historySavePending)
                {
                    _historySaveAfter = Time.realtimeSinceStartup + CONFIG_SAVE_RETRY_DELAY;
                }

                error = exception.Message;
                return false;
            }
        }

        private sealed class PersistentData
        {
            [JsonProperty(Required = Required.Always)]
            public GameDevToolsConfig settings = new();

            [JsonProperty(Required = Required.Always)]
            public List<MacroEntry> macros = new();

            [JsonProperty(Required = Required.Always)]
            public List<string> inputHistory = new();
        }

        private sealed class MacroEntry
        {
            public string id;

            public string source;
        }
    }
}
