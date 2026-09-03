using System;
using Sanmon.Battle;
using Sanmon.Core;
using UnityEngine;

namespace Sanmon.GameEntity
{
    public class ModelBind: MonoBehaviour
        , IGetEntity
    {
        public CmModel ModelCmp { get; private set; }
        
        internal void Bind(CmModel modelCmp)
        {
            ModelCmp = modelCmp;
        }
    }
}