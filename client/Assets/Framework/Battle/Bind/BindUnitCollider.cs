using System;
using Sanmon.GameEntity;
using UnityEngine;

namespace Sanmon.Battle
{
    /// <summary>
    /// 单位的受击碰撞盒绑定
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BindUnitCollider : MonoBehaviour
    {
        public Collider bindCollider;
        public Unit unit;
        
        private void Awake()
        {
            bindCollider ??= GetComponent<Collider>();
        }
    }
}