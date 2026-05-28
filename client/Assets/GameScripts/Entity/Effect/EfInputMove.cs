using Sanmon.Entities;
using UnityEngine;

namespace GameScripts
{
    public class EfInputMove: EffectBase
    {
        public override int ConfigId => 100000003;

        private InputDetector mInput;

        private float mSpeed = 3f;

        private Transform mTrans;
        
        public override void OnAdd()
        {
            mInput = new GameObject("Input").AddComponent<InputDetector>();
            mTrans = Host.GetComponent<WorldModel>().Transform;
            mInput.transform.SetParent(mTrans);
        }

        public override void OnUpdate(float dt)
        {
            mTrans.position += mInput.Move * dt * mSpeed;
        }

        public override void OnRemove()
        {
            
        }
    }
}