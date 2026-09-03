using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    public class CmCollider: ComponentBase
    {
        public BindUnitCollider Bind
        {
            get => _bind;
            set => _bind = value;
        }

        private BindUnitCollider _bind;
    }
}