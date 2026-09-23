using System;
using Game.Config.Module;
using Sanmon.Core;
using Sanmon.Helper;
using UnityEngine;
using YooAsset;

namespace Sanmon.Module
{
    public sealed class UIHandle: IGetModule
    {
        public bool IsLoaded { get; private set; }
        public bool IsVisible { get; private set; }
        public bool IsDestroy { get; private set; }
        public UIWindow Window { get; private set; }
        public UIData Config { get; private set; }

        public UIContext context;
        internal UIMetaDataAttribute metaData;
        
        private GameObject _go;
        private Canvas _mainCanvas;
        private AssetHandle _assetHandle;
        
        internal UIHandle() { }

        public void Destroy()
        {
            if (IsLoaded)
            {
                Window.Close();
            }
            
            IsDestroy = true;
            _assetHandle.Dispose();
        }
        
        public void Show()
        {
            IsVisible = true;
            SetVisible();
        }

        public void Hide()
        {
            IsVisible = false;
            SetVisible();
        }
        
        internal void TryLoad()
        {
            if(IsLoaded) return;
            if(metaData == null) return;

            Config = this.Module().Config.Tables.TbUIData.GetOrDefault(metaData.id);

            if (Config == null)
            {
                SanmonLogger.LogWarning($"未找到配置数据 -> '{metaData.id}'", UIModule.TITLE);
            }
            else
            {
                _assetHandle = this.Module().Asset.LoadAsync<GameObject>(Config.Asset);
                _assetHandle.Completed += OnLoaded;
            }
        }

        private void SetVisible()
        {
            if(!_assetHandle.IsDone) return;
            
            if (IsVisible)
            {
                Window.Show();
            }
            else
            {
                Window.Hide();
            }
        }
        
        private void OnLoaded(AssetHandle handle)
        {
            _assetHandle = handle;
            _go = handle.GetAssetObject<GameObject>();
            _mainCanvas = _go.GetComponent<Canvas>();
            Window = _go.GetComponent<UIWindow>();

            _go.name = Config.Name;
            _go.transform.SetParent(this.Module().UI.root);
            _mainCanvas.sortingLayerName = Config.SortingLayer;
            _mainCanvas.sortingOrder = Config.OrderInLayer;
            Window.OnCreate();
            
            IsLoaded = true;
            SetVisible();
        }
    }
}