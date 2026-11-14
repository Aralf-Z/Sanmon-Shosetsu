/*
Name: VirtualMachine.cs
Time: @2024.01.26
Auth: Prince Biscuit/王子饼干
Desc: A simple virtual machine to execute syntax tree parsed by SyntaxAnalyzer.
      it use C#'s reflection to access properties and methods, no other dependencices.
      lightweight and easy to use. it highly decoupled with Console part, so it could be used in other projects singlely.
      also because using reflection, it's not fast enough, so it's usually used in console input to do 
      fast and simple operations, like set/get property, call method, etc.
      in this case, it's fast enough and very useful.
*/

namespace RedSaw.CommandLineInterface
{
    /// <summary> a source input without any decorator like &quot; &quot; or @ usually, system would treat it as a command call, but if it&apos;s not a command, then it would be parsed to a normal value accorrding to required type, if required type is null, then it would be treated as a string it string is not suitable for target type, then system would raise en error to tell user example1 : @a = xxx in example1, &apos;xxx&apos; is a source input without any decorators, then it would be treated as an command calling, but if vm cannot found an command named &apos;xxx&apos;, then try to find suitable value parser to parse it to Type &apos;a&apos;, if not found suitable ValueParser, then vm simplely check if &apos;a&apos; is string type, if failed all, vm would raise en CommandExecuteException. </summary>
    public class StackSourceInput : StackObject
    {
        /// <summary> the input string. </summary>
        public readonly string inputStr;

        /// <summary> Create a StackSourceInput instance. </summary>
        public StackSourceInput(string inputStr) : base(StackObjectType.Any)
        {
            this.inputStr = inputStr;
        }

        /// <summary> Get value. </summary>
        public object GetValue()
        {
            return inputStr;
        }
    }
}
