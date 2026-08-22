using Game.Config.Logic;
using Sanmon.Core;

namespace Sanmon.GameEntity
{
    public class Effect: 
        IGetModule
    {
        public Entity Host { get; internal set; }
        public EffectParam Config { get; }

        protected Effect(int configId)
        {
            Config = this.Module().Config.Tables.TbEffectParam[configId];
        }
    }
}