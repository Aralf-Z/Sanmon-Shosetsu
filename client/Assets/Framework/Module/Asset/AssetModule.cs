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
            _logger.Log("YooAsset Version: 3.0.5");
            
            YooAssets.Initialize(_logger);
            
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
            if (!YooAssets.TryGetPackage(DEFAULT_PACKAGE, out var package))
                package = YooAssets.CreatePackage(DEFAULT_PACKAGE);
            _package = package;
            InitializePackageOperation initializationOperation = null;
            
            var buildResult = EditorSimulateBuildInvoker.Build(DEFAULT_PACKAGE, (int)EBundleType.VirtualAssetBundle);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeOptions();
            createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            initializationOperation = package.InitializePackageAsync(createParameters);
            
            yield return initializationOperation;

            var options = new LoadPackageManifestOptions("Simulate", 60);
            var operation = package.LoadPackageManifestAsync(options);
            yield return operation;
            
            if(operation.Status is EOperationStatus.Succeeded)
                _logger.Log("LoadPackageManifestAsync Done");
            else
                _logger.LogError($"LoadPackageManifestAsync Error: {operation.Error}");
            
            if (initializationOperation.Status == EOperationStatus.Succeeded)
            {
                _logger.Log("资源包初始化成功！");
                var gop1 = LoadSync<GameObject>("Assets/GameAsset/prefab/Capsule.prefab");
                var gop2 = LoadSync<GameObject>("Assets/GameAsset/prefab/Capsule");
                
                var go1 = Instantiate(gop1);
                go1.transform.localPosition = new Vector3(0, 0, 0);
            
                var go2 = Instantiate(gop2);
                go2.transform.localPosition = new Vector3(3, 3, 3);
            }
            else 
                _logger.LogError($"资源包初始化失败：{initializationOperation.Error}");

            _isInit = true;
        }
        
        //todo 图集等
        //https://www.yooasset.com/docs/guide-runtime/ResourceLoad
    }
}