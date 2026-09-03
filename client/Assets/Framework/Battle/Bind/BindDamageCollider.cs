using System;
using Sanmon.GameEntity;
using UnityEngine;

namespace Sanmon.Battle
{
    /// <summary>
    /// 伤害碰撞绑定
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BindDamageCollider: MonoBehaviour
    {
        [SerializeField] private Collider Collider;
        
        public ColliderType type { get; private set; }
        public BoxCollider Box { get; private set; }
        public SphereCollider Sphere { get; private set; }
        public CapsuleCollider Capsule { get; private set; }
        
        private void Awake()
        {
            switch (Collider)
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
                    throw new Exception("不支持的碰撞体类型");
            }
        }

        public Action<Unit> onEntityEnter;
        public Action<Unit> onEntityStay;
        
        private void OnTriggerEnter(Collider other)
        {
            if(onEntityEnter == null) return;
            
            if (other.TryGetComponent<BindUnitCollider>(out var bind))
            {
                onEntityEnter.Invoke(bind.unit);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if(onEntityStay == null) return;
            
            if (other.TryGetComponent<BindUnitCollider>(out var bind))
            {
                onEntityStay.Invoke(bind.unit);
            }
        }
    }
}