using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    public class CmCollider: ComponentBase
    {
        public UnitColliderBind Bind
        {
            get => _bind;
            set
            {
                _bind = value;
                _bind.host = Host;
            }
        }

        private UnitColliderBind _bind;
    }
}