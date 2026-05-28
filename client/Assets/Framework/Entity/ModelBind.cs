using System;
using Sanmon.Core;
using UnityEngine;

namespace Sanmon.Entities
{
    public class ModelBind: MonoBehaviour
        , IGetEntity
    {
        public WorldModel ModelCmp { get; private set; }
        
        internal void Bind(WorldModel modelCmp)
        {
            ModelCmp = modelCmp;
        }

        private void OnDestroy()
        {
            this.Entity().Recycle(ModelCmp.Host);
        }
    }
}