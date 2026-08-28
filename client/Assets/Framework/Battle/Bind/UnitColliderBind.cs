using System;
using Sanmon.GameEntity;
using UnityEngine;

namespace Sanmon.Battle
{
    /// <summary>
    /// 单位的受击碰撞盒绑定
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ColliderBind : MonoBehaviour
    {
        [SerializeField]private Collider Collider;
        
        public Entity host;
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
    }
}