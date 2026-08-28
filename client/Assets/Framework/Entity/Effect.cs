using Game.Config.Battle;
using Sanmon.Core;

namespace Sanmon.GameEntity
{
    public class Effect: 
        IGetModule
    {
        //public const string ""
        
        public EffectData Config { get; }
        
        internal Effect(int configId)
        {
            Config = this.Module().Config.Tables.TbEffectData[configId];
        }
    }
}