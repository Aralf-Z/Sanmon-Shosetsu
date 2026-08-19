using Sanmon.Core;
using Sanmon.GameEntity;
using UnityEngine;

namespace GameScripts
{
    public class EfInputMove: EffectBase
    {
        public override int ConfigId => 100000003;

        private Vector3 mMove;
        private float mSpeed = 3f;

        private Transform mTrans;
        
        public override void OnAdd()
        {
            FrameUpdater.Ins.Add(Update);
        }

        public override void OnUpdate(float dt)
        {
            // mTrans.position += mMove * dt * mSpeed;
            // mMove = Vector3.zero;
        }

        public override void OnRemove()
        {
            FrameUpdater.Ins.Remove(Update);
        }
        
        private void Update(float dt)
        {
            if (mMove == Vector3.zero)
            {
                mMove = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            }
        }
    }
}