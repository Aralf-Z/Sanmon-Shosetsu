using System;
using System.Collections;
using System.IO;
using Sanmon.Helper;
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
            _logger.Log($"YooAsset version: {YOO_ASSET_VERSION}");
            
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
            
        }

        public const string YOO_ASSET_VERSION = "3.0.5";
        public const string DEFAULT_PACKAGE = "DefaultPackage";

        public EPlayMode playMode = EPlayMode.EditorSimulateMode;

        private ResourcePackage _package;
        private AssetLogger _logger;
        private bool _isInit;
        
        public T LoadSync<T>(string location) where T : Object
        {
            return _package.LoadAssetSync<T>(location).AssetObject as T;
        }
        
        public AssetHandle LoadAsync<T>(string location) where T: Object
        {
            return _package.LoadAssetAsync<T>(location);
        }
        
        //初始化包
        private IEnumerator InitPackage()
        {  
            InitializePackageOperation initOperation = null;
            
            var mode = playMode;//todo 编辑器

            _logger.Log($"资源加载模式：{mode}");
            
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
                _logger.Log("资源包初始化成功！");
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
                _logger.Log($"资源包版本获取成功: '{packageVersion}'!");
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
                _logger.Log($"资源包清单加载成功！");
            }
            else
                _logger.LogError($"资源包清单加载失败：'{loadManifestOperation.Error}'");
        }
        
        //todo 图集等
        //https://www.yooasset.com/docs/guide-runtime/ResourceLoad
    }
}