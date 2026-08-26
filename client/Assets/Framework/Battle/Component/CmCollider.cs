using Sanmon.GameEntity;

namespace Sanmon.Battle
{
    public class CmCollider: ComponentBase
    {
        public ColliderBind Bind
        {
            get => _bind;
            set
            {
                _bind = value;
                _bind.host = Host;
            }
        }

        private ColliderBind _bind;
    }
}