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
            _logger.Log("YooAsset version: 3.0.5");
            
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
        
        private IEnumerator InitPackage()
        {  
            //先创建包
            //再加载version
            //最后加载manifest
            
            InitializePackageOperation initializationOperation = null;
            LoadPackageManifestOperation loadPackageManifestOperation = null;
            
            var mode = playMode;//todo 编辑器

            _logger.Log($"资源加载模式：{mode}");
            
            if (mode is EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateBuildInvoker.Build(DEFAULT_PACKAGE, (int)EBundleType.VirtualAssetBundle);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeOptions
                {
                    EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot)
                };
                initializationOperation = _package.InitializePackageAsync(createParameters);
            
                yield return initializationOperation;

                var options = new LoadPackageManifestOptions("Simulate", 60);
                loadPackageManifestOperation = _package.LoadPackageManifestAsync(options);
                
                yield return loadPackageManifestOperation;
            }
            else if(mode is EPlayMode.OfflinePlayMode)
            {
                var fileSystemParams = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();
                var createParameters = new OfflinePlayModeOptions
                {
                    BuiltinFileSystemParameters = fileSystemParams
                };
                initializationOperation = _package.InitializePackageAsync(createParameters);
                
                yield return initializationOperation;
                
                var options = new LoadPackageManifestOptions("2026-08-20-696", 60);
                loadPackageManifestOperation = _package.LoadPackageManifestAsync(options);
                
                yield return loadPackageManifestOperation;
            }
            else
            {
                throw new SystemException($"错误加载模式：'{mode}'尚未支持.");
            }
            
            if (initializationOperation?.Status == EOperationStatus.Succeeded)
                _logger.Log("资源包初始化成功！");
            else 
                _logger.LogError($"资源包初始化失败：{initializationOperation?.Error}");

            if(loadPackageManifestOperation?.Status is EOperationStatus.Succeeded)
                _logger.Log($"Manifest  加载成功");
            else
                _logger.LogError($"Manifest 加载失败： {loadPackageManifestOperation?.Error}");
            
            _isInit = loadPackageManifestOperation?.Status is EOperationStatus.Succeeded && initializationOperation?.Status == EOperationStatus.Succeeded;

            if (_isInit)
            {
                _logger.Log($"Package Version: {_package.GetPackageVersion()}");
                _logger.Log($"Package Note: {_package.GetPackageNote()}");
            }
            else
            {
                _logger.LogError("资源模块加载失败");
            }
        }
        
        //todo 图集等
        //https://www.yooasset.com/docs/guide-runtime/ResourceLoad
    }
}