namespace Sanmon.Battle
{
    public enum BuffCasterType
    {
        Unit = 1,
    }
    
    public class BuffCaster
    {
        public BuffCasterType casterType;

        public Unit unitCaster;
    }
}