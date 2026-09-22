using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace Sanmon.Module
{
    public class AssetModule: MonoBehaviour,
        IModule
    {
        int IModule.InitOrder => InitOrderDefine.ASSET;
        bool IModule.IsInit => _isInit;

        void IModule.Init()
        {
            _logger = new AssetLogger();
            
            YooAssets.Initialize(_logger);
            
            if (!YooAssets.TryGetPackage(DEFAULT_PACKAGE, out var package))
                package = YooAssets.CreatePackage(DEFAULT_PACKAGE);
            _package = package;
            
            StartCoroutine(InitPackage());
        }

        void IModule.Deinit()
        {
            _package = null;
            YooAssets.Destroy();
        }
        
        void IModule.OnLogicUpdate(float dt)
        {
            CheckPrefabInfo(dt);
        }
        
        public const string DEFAULT_PACKAGE = "DefaultPackage";
        internal const string YOO_ASSET_VERSION = "3.0.5";
        
        public EPlayMode playMode = EPlayMode.EditorSimulateMode;
        
        private ResourcePackage _package;
        private AssetLogger _logger;
        private bool _isInit;
        
        private readonly Dictionary<string, PrefabInfo> _prefabInfos = new ();
        private readonly List<string> _prefabInfoPendingRemove = new();
        private float _disposeTimer;
        
        /// <summary>
        /// 同步加载预制体并且实例化
        /// </summary>
        /// <param name="location">加载路径</param>
        /// <param name="options">实例化选项</param>
        public GameObject LoadSyncGo(string location, InstantiateOptions? options = null)
        {
            if (!_prefabInfos.TryGetValue(location, out var prefabInfo))
            {
                prefabInfo = new PrefabInfo
                {
                    assetLocation = location,
                    handle = _package.LoadAssetSync<GameObject>(location),
                };
                _prefabInfos[location] = prefabInfo;
            }

#if UNITY_EDITOR
            if (!prefabInfo.handle.IsDone)
            {
                _logger.LogError($"'{location}' 出现了混用异步和同步的情况.");
            }
#endif
            
            return prefabInfo.NewOne(options);
        }
        
        /// <summary>
        /// 异步加载预制体并且实例化
        /// </summary>
        /// <param name="location">加载路径</param>
        /// <param name="options">实例化选项</param>
        public GameObjectAsyncHandle LoadAsyncGo(string location, InstantiateOptions? options = null)
        {
            if (!_prefabInfos.TryGetValue(location, out var prefabInfo))
            {
                prefabInfo = new PrefabInfo
                {
                    assetLocation = location,
                    handle = _package.LoadAssetAsync<GameObject>(location),
                };
                _prefabInfos[location] = prefabInfo;
            }
            
            return new GameObjectAsyncHandle(prefabInfo, options);
        }
        
        /// <summary>
        /// 同步加载, 需要手动释放: AssetHandle.Release();
        /// </summary>
        /// <param name="location">加载路径</param>
        public AssetHandle LoadSync<T>(string location) where T : Object
        {
            GameObjectCheck<T>();
            return _package.LoadAssetSync<T>(location);
        }

        /// <summary>
        /// 异步加载, 需要手动释放: AssetHandle.Release();
        /// </summary>
        /// <param name="location">加载路径</param>
        public AssetHandle LoadAsync<T>(string location) where T: Object
        {
            GameObjectCheck<T>();
            return _package.LoadAssetAsync<T>(location);
        }
        
        /// <summary>
        /// 同步卸载引用计数为0的资源
        /// </summary>
        /// <returns></returns>
        public void UnloadSyncUnusedAssets()
        {
            var operation = _package.UnloadUnusedAssetsAsync();
            operation.WaitForCompletion();
        }
        
        /// <summary>
        /// 异步卸载引用计数为0的资源
        /// </summary>
        /// <returns></returns>
        public UnloadUnusedAssetsOperation UnloadAsyncUnusedAssets()
        {
            return _package.UnloadUnusedAssetsAsync();
        }
        
        /// <summary>
        /// 异步卸载所有资源
        /// </summary>
        /// <para> 注意：ResourcePackage在销毁的时候也会自动调用该方法。</para>
        /// <para> 备注：不支持同步操作。</para>
        /// <returns></returns>
        public UnloadAllAssetsOperation ForceUnloadAsyncAllAssets()
        {
            return _package.UnloadAllAssetsAsync();
        }
        
        /// <summary>
        /// 尝试卸载指定的资源对象
        /// <para> 注意：如果该资源还在被使用，该方法会无效。 </para>
        /// </summary>
        public void TryUnloadUnusedAsset(string location)
        {
            _package.TryUnloadUnusedAsset(location);
        }
        
        //初始化包
        private IEnumerator InitPackage()
        {  
            InitializePackageOperation initOperation = null;
            
            // ReSharper disable once RedundantAssignment
            var mode = playMode;

#if UNITY_EDITOR
            mode = (EPlayMode)UnityEditor.EditorPrefs.GetInt("EditorAssetMode", (int)EPlayMode.EditorSimulateMode);
#else
            if(mode is EPlayMode.EditorSimulateMode) 
            {
                _logger.LogError($"资源加载模式'{mode}'在非unity编辑器环境下不支持.");
                yield break;
            }
#endif
            _logger.Log($"资源加载模式：{mode}.");
            
            if (mode is EPlayMode.EditorSimulateMode)//模拟编辑器模式
            {
                var buildResult = EditorSimulateBuildInvoker.Build(DEFAULT_PACKAGE, (int)EBundleType.VirtualAssetBundle);
                var packageRoot = buildResult.PackageRootDirectory;
                var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

                var createParameters = new EditorSimulateModeOptions();
                createParameters.EditorFileSystemParameters = fileSystemParams;
                
                initOperation = _package.InitializePackageAsync(createParameters);
            
                yield return initOperation;
            }
            else if(mode is EPlayMode.OfflinePlayMode)//单机模式
            {
                var fileSystemParams = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();
                var createParameters = new OfflinePlayModeOptions
                {
                    BuiltinFileSystemParameters = fileSystemParams
                };
                initOperation = _package.InitializePackageAsync(createParameters);
                
                yield return initOperation;
            }
            else
            {
                var msg = $"错误加载模式：'{mode}'尚未支持.";
                _logger.LogError(msg);
                throw new SystemException(msg);
            }

            if (initOperation.Status == EOperationStatus.Succeeded)
            {
                _logger.Log("资源包初始化成功");
                yield return LoadPackageVersion();
            }
            else 
                _logger.LogError($"资源包初始化失败：'{initOperation.Error}'");
        }

        //加载version
        private IEnumerator LoadPackageVersion()
        {
            var loadVersionOperation = _package.RequestPackageVersionAsync();
            yield return loadVersionOperation;

            if (loadVersionOperation.Status == EOperationStatus.Succeeded)
            {
                var packageVersion = loadVersionOperation.PackageVersion;
                _logger.Log($"资源包版本获取成功: '{packageVersion}'");
                yield return LoadPackageManifest(packageVersion);
            }
            else
            {
                _logger.LogError($"资源包版本获取失败：'{loadVersionOperation.Error}'");
            }
        }

        //加载manifest
        private IEnumerator LoadPackageManifest(string packageVersion)
        {
            LoadPackageManifestOperation loadManifestOperation = null;
            
            var options = new LoadPackageManifestOptions(packageVersion, 60);
            loadManifestOperation = _package.LoadPackageManifestAsync(options);
                
            yield return loadManifestOperation;

            if (loadManifestOperation.Status is EOperationStatus.Succeeded)
            {
                _isInit = true;
                _logger.Log($"资源包清单加载成功");
            }
            else
                _logger.LogError($"资源包清单加载失败：'{loadManifestOperation.Error}'");
        }
        
        [Conditional("DEBUG_MODE")]
        private void GameObjectCheck<T>() where T : Object
        {
            if(typeof(T) == typeof(GameObject))
                _logger.LogWarning($"不建议使用该方法加载GameObject, 推荐使用'{nameof(LoadSyncGo)}'和'{nameof(LoadAsyncGo)}'");
        }

        private void CheckPrefabInfo(float dt)
        {
            _disposeTimer -= dt;
            if(_disposeTimer > 0) return;
            
            _disposeTimer = 30f;
            
            var timeOut = Time.time - 120f;
            
            foreach (var (key, info) in _prefabInfos)
            {
                if (info.lastUseTimestamp < timeOut)
                    _prefabInfoPendingRemove.Add(key);
            }
            foreach (var key in _prefabInfoPendingRemove)
            {
                _prefabInfos[key].handle.Dispose();
                _prefabInfos.Remove(key);
            }
            _prefabInfoPendingRemove.Clear();
        }
        
        //todo 图集等
        //https://www.yooasset.com/docs/guide-runtime/ResourceLoad
    }
    
    internal class PrefabInfo
    {
        public string assetLocation;
        public AssetHandle handle;

        public float lastUseTimestamp;
            
        public GameObject NewOne(InstantiateOptions? options)
        {
            var go = handle.InstantiateSync(options ?? new InstantiateOptions(true));
            lastUseTimestamp = Time.time;
            go.AddComponent<AssetReference>();
            return go;
        }
    }
}