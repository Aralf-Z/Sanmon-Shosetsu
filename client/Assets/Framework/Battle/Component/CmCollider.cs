using System;
using Sanmon.Core;
using Sanmon.GameEntity;
using UnityEngine;

namespace Sanmon.Battle
{
    public class CmCollider: ComponentBase
    , IGetNote
    {
        private BindUnitCollider _bind;
        
        public ColliderType type { get; private set; }
        public BoxCollider Box { get; private set; }
        public SphereCollider Sphere { get; private set; }
        public CapsuleCollider Capsule { get; private set; }
        
        public void SetBind(Transform transform)
        {
            _bind = transform.GetComponent<BindUnitCollider>() ?? transform.gameObject.AddComponent<BindUnitCollider>();
            _bind.unit = this.Note().Get<BattleNote>().allUnits[Owner];
            switch (_bind.bindCollider)
            {
                case BoxCollider boxCollider:
                    Box = boxCollider;
                    type = ColliderType.Box;
                    break;
                case SphereCollider sphereCollider:
                    Sphere = sphereCollider;
                    type = ColliderType.Sphere;
                    break;
                case CapsuleCollider capsuleCollider:
                    Capsule = capsuleCollider;
                    type = ColliderType.Capsule;
                    break;
                default:
                    if(_bind.bindCollider)
                        throw new UnitException($"不支持的碰撞体类型 [{_bind.bindCollider} - {_bind.bindCollider.GetType().FullName}].");
                    throw new UnitException($"碰撞体组件为空.");
            }
        }
    }
}