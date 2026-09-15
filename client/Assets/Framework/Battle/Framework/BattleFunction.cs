using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    public class BattleFunction: FunctionBase
    {
        private CmBuff _cmBuff;
        
        public override void OnAdded()
        {
            _cmBuff = Host.GetOrAddComponent<CmBuff>();
        }

        public override void OnLogicUpdate(float dt)
        {
           _cmBuff.Buffs.Update(dt);
        }

        public override void OnRemoved()
        {
            
        }
    }
}