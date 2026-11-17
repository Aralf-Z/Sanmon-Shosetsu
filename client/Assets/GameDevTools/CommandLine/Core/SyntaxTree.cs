using System;
using System.Linq;

namespace RedSaw.CommandLineInterface
{
    /// <summary> syntax tree, tree type, and represent the whole execution logic. </summary>
    public class SyntaxTree
    {
        public readonly string data;

        public readonly SyntaxTreeCode opcode;

        public readonly SyntaxTree[] children;

        public SyntaxTree L => children[0];

        public SyntaxTree R => children[1];

        public bool IsLeaf => children.Length == 0;

        public string DebugInfo
        {
            get
            {
                return opcode switch
                {
                    SyntaxTreeCode.Dot => $"{L?.DebugInfo}.{R?.DebugInfo}",
                    SyntaxTreeCode.Call => $"{L?.DebugInfo}{R?.DebugInfo}",
                    SyntaxTreeCode.Index => $"{L?.DebugInfo}[{R?.DebugInfo}]",
                    SyntaxTreeCode.Assign => $"{L?.DebugInfo} = {R?.DebugInfo}",
                    SyntaxTreeCode.SetField => $"{L?.DebugInfo}.{R?.DebugInfo} = {children[2]?.DebugInfo}",
                    SyntaxTreeCode.Cvt => $"{L?.DebugInfo}:{R?.DebugInfo}",
                    SyntaxTreeCode.FactorParams => $"({string.Join(", ", children.Select(x => x.DebugInfo))})",
                    SyntaxTreeCode.FactorFloat => $"{data}f",
                    SyntaxTreeCode.FactorString => data,
                    SyntaxTreeCode.FactorTrue => data,
                    SyntaxTreeCode.FactorFalse => data,
                    SyntaxTreeCode.FactorNull => data,
                    SyntaxTreeCode.FactorInt => data,
                    SyntaxTreeCode.FactorId => "@" + data,
                    SyntaxTreeCode.FactorInput => "?" + data,
                    _ => ToString(),
                };
            }
        }

        public SyntaxTree(SyntaxTreeCode opcode, string data)
        {
            this.opcode = opcode;
            this.data = data;
            this.children = Array.Empty<SyntaxTree>();
        }

        public SyntaxTree(SyntaxTreeCode opcode, string data, SyntaxTree l)
        {
            this.opcode = opcode;
            this.data = data;
            this.children = new SyntaxTree[]
            { l };
        }

        public SyntaxTree(SyntaxTreeCode opcode, string data, SyntaxTree l, SyntaxTree r)
        {
            this.opcode = opcode;
            this.data = data;
            this.children = new SyntaxTree[]
            { l, r };
        }

        public SyntaxTree(SyntaxTreeCode opcode, string data, SyntaxTree[] children)
        {
            this.opcode = opcode;
            this.data = data;
            this.children = children;
        }

        public override string ToString()
        {
            if (data != null || data.Length > 0) return data;

            return opcode.ToString();
        }
    }
}
