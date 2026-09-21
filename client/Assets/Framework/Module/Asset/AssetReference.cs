using System;
using Sanmon.Core;
using UnityEngine;
using YooAsset;

namespace Sanmon.Module
{
    [DisallowMultipleComponent]
    public class AssetReference: MonoBehaviour
        , IGetModule
    {
        // internal AssetInfo info;
        //
        // private void OnDestroy()
        // {
        //     this.Module().Asset.AssetCountSubOne(info);
        // }
    }
}