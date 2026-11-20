using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using GameConsole.UI;

namespace GameConsole
{
    /// <summary> 工具独立的语言服务, 与游戏语言, 命令解析和日志存储无关. </summary>
    internal static partial class ToolkitLocalization
    {
        private static readonly string[] _modules =
        { "ui", "help", "macro" };

        private static readonly Dictionary<string, ToolkitLanguagePack> _cache = new(StringComparer.Ordinal);

        private static readonly HashSet<string> _reported = new(StringComparer.Ordinal);

        private static ToolkitLanguageEntry[] _languages = Array.Empty<ToolkitLanguageEntry>(); // 默认入口仅由 Schema 的排列决定.

        private static ToolkitLanguagePack _active;

        private static ToolkitLanguagePack _fallback;

        private static Dictionary<string, string> _resolved; // 当前语言及回退词条的单层替换结果, 查词时直接读取.

        internal static event Action e_onChanged;

        internal static int Version { get; private set; }

        internal static string ActiveLanguage => _active?.code ?? DefaultLanguage;

        /// <summary> 首个可加载的配置语言, 同时用于缺失翻译回退. </summary>
        internal static string DefaultLanguage => _fallback?.code ?? _languages.FirstOrDefault()?.code ?? string.Empty;

        internal static IReadOnlyList<ToolkitLanguageEntry> Languages => _languages;

        internal static void Initialize(IEnumerable<ToolkitLanguageEntry> entries, string selected)
        {
            var languages = BuildLanguages(entries);

            if (!_languages.Select(entry => (entry.code, entry.path))
                .SequenceEqual(languages.Select(entry => (entry.code, entry.path))))
            {
                _cache.Clear();
                _reported.Clear();
            }

            _languages = languages;
            _fallback = null;
            foreach (var entry in _languages)
            {
                try
                {
                    if (!_cache.TryGetValue(entry.code, out var pack))
                    {
                        pack = Load(entry);
                    }

                    _cache[entry.code] = pack;
                    _fallback = pack;
                    break;
                }
                catch (Exception exception)
                {
                    Report("bootstrap:" + entry.code, exception.Message);
                }
            }

            if (_fallback == null)
            {
                Report("bootstrap", "Schema.languages has no loadable language package.");
                _fallback = new ToolkitLanguagePack(string.Empty, new());
            }

            selected = Resolve(selected);
            try
            {
                var pack = _fallback;

                if (selected != _fallback.code && !_cache.TryGetValue(selected, out pack))
                {
                    pack = Load(_languages.First(entry => entry.code == selected));
                }

                ToolkitLocalizationData.Validate(pack.values, _fallback.values);
                if (!ReferenceEquals(_active, pack))
                {
                    Commit(pack);
                }
            }
            catch (Exception exception)
            {
                Report("startup:" + selected, exception.Message);
                if (!ReferenceEquals(_active, _fallback))
                {
                    Commit(_fallback);
                }
            }
        }

        /// <summary> 只保留配置中的有效路径和首个同代码定义, 不补入任何语言. </summary>
        internal static ToolkitLanguageEntry[] BuildLanguages(IEnumerable<ToolkitLanguageEntry> entries)
        {
            return (entries ?? Array.Empty<ToolkitLanguageEntry>()).Where(entry => entry != null && ValidPath(entry.path) && !string.IsNullOrWhiteSpace(entry.code))
                .GroupBy(entry => entry.code.Trim(), StringComparer.Ordinal)
                .Select(group => new ToolkitLanguageEntry { code = group.Key, name = string.IsNullOrWhiteSpace(group.First().name) ? group.Key : group.First().name.Trim(), path = group.First().path.Trim().Replace('\\', '/').Trim('/') })
                .ToArray();
        }

        internal static string Resolve(string code) => _languages.Any(entry => entry.code == code)
            ? code
            : DefaultLanguage;

        /// <summary> 缺失翻译回退默认包, 默认包缺失时显示键. </summary>
        internal static string Get(string key, string fallback = null)
        {
            EnsureInitialized();
            if (_resolved.TryGetValue(key, out var value)) return ToolkitIcons.Resolve(FormatConfig(value));

            Report("key:" + key, "Missing localization key: " + key);
            return fallback ?? key;
        }

        internal static string Format(string key, params (string name, object value)[] arguments) => ToolkitLocalizationData.Format(Get(key), arguments);

        /// <summary> 保留原插值表达式的数字, 时间格式与字符对齐. </summary>
        internal static string Value(object value, string format = null, int alignment = 0)
        {
            var text = value is IFormattable formattable
                ? formattable.ToString(format, CultureInfo.InvariantCulture)
                : value?.ToString() ?? string.Empty;

            return alignment > 0
                ? text.PadLeft(alignment)
                : alignment < 0
                    ? text.PadRight(-alignment)
                    : text;
        }

        internal static IEnumerator PrepareAsync(string code, Action<ToolkitLanguagePack> ready)
        {
            if (ready == null)
            {
                throw new ArgumentNullException(nameof(ready));
            }

            EnsureInitialized();
            Exception failure = null;

            foreach (var candidate in new[] { Resolve(code) }.Concat(_languages.Select(entry => entry.code))
                .Where(value => !string.IsNullOrEmpty(value))
                .Distinct(StringComparer.Ordinal))
            {
                ToolkitLanguagePack prepared = null;
                var work = PrepareCandidateAsync(candidate, pack => prepared = pack);
                using var lifetime = work as IDisposable;

                while (true)
                {
                    bool moved;
                    object current = null;

                    try
                    {
                        moved = work.MoveNext();
                        if (moved)
                        {
                            current = work.Current;
                        }
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                        break;
                    }

                    if (!moved)
                    {
                        break;
                    }

                    yield return current;
                }

                if (prepared == null)
                {
                    continue;
                }

                ready(prepared);
                yield break;
            }

            throw new InvalidOperationException("Schema.languages has no loadable language package.", failure);
        }

        // 单包完成解析与校验后才返回, 调用者负责按列表顺序回退.
        private static IEnumerator PrepareCandidateAsync(string code, Action<ToolkitLanguagePack> ready)
        {
            EnsureInitialized();
            code = Resolve(code);
            if (string.IsNullOrEmpty(code))
            {
                throw new InvalidOperationException("Schema.languages has no loadable language package.");
            }

            yield return null;
            if (_cache.TryGetValue(code, out var cached))
            {
                ready(cached);
                yield break;
            }

            var entry = _languages.First(language => language.code == code);
            var requests = _modules.Select(module => Resources.LoadAsync<TextAsset>(entry.path + "/" + module))
                .ToArray();
            var values = new Dictionary<string, string>(StringComparer.Ordinal);

            for (var index = 0; index < requests.Length; index++)
            {
                while (!requests[index].isDone)
                {
                    yield return requests[index];
                }

                var source = entry.path + "/" + _modules[index];

                if (requests[index].asset is not TextAsset asset)
                {
                    throw new InvalidOperationException("Missing localization module: " + source);
                }

                ToolkitLocalizationData.AddModule(values, ToolkitLocalizationData.Read(asset.text, source), source);
                yield return null;
            }

            ToolkitLocalizationData.Validate(values, _fallback.values);
            ready(new ToolkitLanguagePack(code, values));
        }

        /// <summary> 已验证数据一次提交, 个别显示订阅异常不破坏其他页面的更新. </summary>
        internal static void Commit(ToolkitLanguagePack pack)
        {
            if (pack == null)
            {
                throw new ArgumentNullException(nameof(pack));
            }

            var resolved = ToolkitLocalizationData.ResolveReferences(pack.values, _fallback?.values);

            _active = pack;
            _resolved = resolved;
            _cache[pack.code] = pack;
            RefreshPresentation();
        }

        internal static void RefreshPresentation()
        {
            Version++;
            ToolkitLocalizedBindings.Refresh();
            ToolkitMixedText.RefreshAll();
            if (e_onChanged == null) return;

            foreach (Action callback in e_onChanged.GetInvocationList())
            {
                try
                {
                    callback();
                }
                catch (Exception exception)
                {
                    Report("subscriber:" + callback.Method, exception.Message);
                }
            }
        }

        // 关闭 Domain Reload 时也清理上一次 Play 的语言缓存和显示订阅.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ReleaseCache()
        {
            _cache.Clear();
            _reported.Clear();
            _active = null;
            _fallback = null;
            _resolved = null;
            _languages = Array.Empty<ToolkitLanguageEntry>();
            e_onChanged = null;
            Version++;
            ToolkitLocalizedBindings.Reset();
        }

        private static void EnsureInitialized()
        {
            if (_active != null) return;

            Initialize(Resources.Load<Schema>("Schema")?.languages, Config.UILanguage);
        }

        // 同步读取仅用于工具首次初始化, 后续切换全部走异步准备.
        private static ToolkitLanguagePack Load(ToolkitLanguageEntry entry)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var module in _modules)
            {
                var source = entry.path + "/" + module;
                var asset = Resources.Load<TextAsset>(source);

                if (asset == null)
                {
                    throw new InvalidOperationException("Missing localization module: " + source);
                }

                ToolkitLocalizationData.AddModule(values, ToolkitLocalizationData.Read(asset.text, source), source);
            }

            ToolkitLocalizationData.Validate(values, _fallback?.values ?? values);
            return new ToolkitLanguagePack(entry.code, values);
        }

        // 语言必须位于专用目录, 不接受文件系统路径或上级路径.
        private static bool ValidPath(string path) => !string.IsNullOrWhiteSpace(path) && path.Trim()
            .Replace('\\', '/')
            .Trim('/')
            .StartsWith("localization/", StringComparison.OrdinalIgnoreCase) && !path.Split('/', '\\')
            .Any(part => part == ".." || part == "." || part.Contains(':'));

        // 引导失败不能再调用本地化, 避免递归报告.
        private static void Report(string key, string message)
        {
            if (_reported.Add(key))
            {
                Debug.LogWarning("[GameDevTools/localization] " + message);
            }
        }
    }

    /// <summary> 可独立校验的语言资源快照. </summary>
    internal sealed class ToolkitLanguagePack
    {
        internal readonly string code;

        internal readonly Dictionary<string, string> values;

        internal ToolkitLanguagePack(string code, Dictionary<string, string> values)
        {
            this.code = code;
            this.values = values;
        }
    }
}
