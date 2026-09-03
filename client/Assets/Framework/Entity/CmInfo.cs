namespace Sanmon.GameEntity
{
    public class CmInfo : ComponentBase
    {
        public string Name { get; internal set; }
        public int InstanceId { get; internal set; }

        public override string ToString()
        {
            return $"name:{Name}, instanceId:{InstanceId}";
        }
    }
}