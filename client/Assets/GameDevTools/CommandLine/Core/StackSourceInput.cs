/* Auth: Prince Biscuit/王子饼干 */
namespace RedSaw.CommandLineInterface
{
    /// <summary> 无引号或 @ 修饰的输入. 优先尝试命令, 再按目标类型解析; 无目标类型时视为字符串, 无法转换时抛出异常. </summary>
    public class StackSourceInput : StackObject
    {
        public readonly string inputStr;

        public StackSourceInput(string inputStr) : base(StackObjectType.Any)
        {
            this.inputStr = inputStr;
        }

        public object GetValue()
        {
            return inputStr;
        }
    }
}
