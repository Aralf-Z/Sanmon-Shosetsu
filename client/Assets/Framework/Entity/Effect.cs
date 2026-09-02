using Game.Config.Battle;
using Sanmon.Core;

namespace Sanmon.GameEntity
{
    public class Effect: 
        IGetModule
    {
        public EffectData Config { get; }
        
        internal Effect(int id)
        {
            Config = this.Module().Config.Tables.TbEffectData[id];
        }
    }
}