using System;
using Sanmon.Core;
using UnityEngine;

namespace Sanmon.Entities
{
    public class ModelBind: MonoBehaviour
        , IGetEntity
    {
        public CmWorldModel ModelCmp { get; private set; }
        
        internal void Bind(CmWorldModel modelCmp)
        {
            ModelCmp = modelCmp;
        }

        private void OnDestroy()
        {
            this.Entity().Recycle(ModelCmp.Host);
        }
    }
}