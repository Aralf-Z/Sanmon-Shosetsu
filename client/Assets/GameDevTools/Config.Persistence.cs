using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        internal static event Action e_onInputHistoryChanged;

        internal static IReadOnlyList<string> InputHistoryEntries => _inputHistory?.Entries ?? Array.Empty<string>();

        internal static int ActiveInputHistoryCapacity => _inputHistory?.Capacity ?? InputHistoryCapacity;

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
        public static GameDevToolsConfig Snapshot()
        {
            var json = JsonUtility.ToJson(Data);

            return JsonUtility.FromJson<GameDevToolsConfig>(json);
        }

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
            _inputHistory.e_onChanged += OnInputHistoryChanged;
            if (_inputHistory.Entries.Count != entries.Count)
            {
                QueueInputHistorySave();
            }
        }

        internal static void UnbindInputHistory()
        {
            if (_inputHistory == null) return;

            _inputHistory.e_onChanged -= OnInputHistoryChanged;
            if (_historySavePending)
            {
                _document.inputHistory = _inputHistory.Entries.ToList();
            }

            _inputHistory = null;
        }

        internal static void ReleaseSnapshot()
        {
            if (_historySavePending) return;

            _document = null;
            _backupUnreadableFile = false;
        }

        internal static void ClearInputHistory()
        {
            if (_inputHistory?.Clear() == true)
            {
                FlushInputHistory();
            }
        }

        internal static void RemoveInputHistory(int index)
        {
            if (_inputHistory?.RemoveAt(index) == true)
            {
                FlushInputHistory();
            }
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

            var loaded = new PersistentData();

            JsonUtility.FromJsonOverwrite(json, loaded);
            var fields = JsonShape.ReadFields(json);

            JsonShape.Require(fields, "settings", '{');
            var macros = JsonShape.Require(fields, "macros", '[');

            JsonShape.Require(fields, "inputHistory", '[');
            Normalize(loaded.settings);
            var macroItems = JsonShape.ReadItems(macros).ToArray();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var position = 0;

            if (macroItems.Length != loaded.macros.Count)
            {
                throw new FormatException("宏数据与存档条目数不一致.");
            }

            loaded.macros.RemoveAll(entry =>
            {
                var item = macroItems[position++];

                if (entry == null || string.IsNullOrEmpty(entry.id) || entry.source == null || !JsonShape.IsKind(item, '{')) return true;

                var macroFields = JsonShape.ReadFields(item);

                return !macroFields.TryGetValue("source", out var source) || source.Value == "null" || !ids.Add(entry.id);
            });
            return loaded;
        }

        private static void OnInputHistoryChanged()
        {
            QueueInputHistorySave();
            e_onInputHistoryChanged?.Invoke();
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
                File.WriteAllText(temporary, JsonUtility.ToJson(snapshot, true));
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

        [Serializable]
        private sealed class PersistentData
        {
            public GameDevToolsConfig settings = new();

            public List<MacroEntry> macros = new();

            public List<string> inputHistory = new();
        }

        [Serializable]
        private sealed class MacroEntry
        {
            public string id;

            public string source;
        }
    }
}
