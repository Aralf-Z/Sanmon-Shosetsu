using Game.Config.Battle;
using Sanmon.Core;

namespace Sanmon.GameEntity
{
    public class Effect: 
        IGetModule
    {
        public Entity Host { get; internal set; }
        public EffectData Config { get; }

        protected Effect(int configId)
        {
            Config = this.Module().Config.Tables.TbEffectData[configId];
        }
    }
}