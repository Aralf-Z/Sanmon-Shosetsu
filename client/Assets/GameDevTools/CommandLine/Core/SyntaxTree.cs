using System;
using System.Linq;

namespace RedSaw.CommandLineInterface
{
    /// <summary> syntax tree, tree type, and represent the whole execution logic. </summary>
    public class SyntaxTree
    {
        /// <summary> Data. </summary>
        public readonly string data;

        /// <summary> Opcode. </summary>
        public readonly SyntaxTreeCode opcode;

        /// <summary> Children. </summary>
        public readonly SyntaxTree[] children;

        /// <summary> L. </summary>
        public SyntaxTree L => children[0];

        /// <summary> R. </summary>
        public SyntaxTree R => children[1];

        /// <summary> Is leaf. </summary>
        public bool IsLeaf => children.Length == 0;

        /// <summary> provide debug info about current node. </summary>
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

        /// <summary> Create a SyntaxTree instance. </summary>
        public SyntaxTree(SyntaxTreeCode opcode, string data)
        {
            this.opcode = opcode;
            this.data = data;
            this.children = Array.Empty<SyntaxTree>();
        }

        /// <summary> Create a SyntaxTree instance. </summary>
        public SyntaxTree(SyntaxTreeCode opcode, string data, SyntaxTree l)
        {
            this.opcode = opcode;
            this.data = data;
            this.children = new SyntaxTree[]
            {
                l
            };
        }

        /// <summary> Create a SyntaxTree instance. </summary>
        public SyntaxTree(SyntaxTreeCode opcode, string data, SyntaxTree l, SyntaxTree r)
        {
            this.opcode = opcode;
            this.data = data;
            this.children = new SyntaxTree[]
            {
                l,
                r
            };
        }

        /// <summary> Create a SyntaxTree instance. </summary>
        public SyntaxTree(SyntaxTreeCode opcode, string data, SyntaxTree[] children)
        {
            this.opcode = opcode;
            this.data = data;
            this.children = children;
        }

        /// <summary> To string. </summary>
        public override string ToString()
        {
            if (data != null || data.Length > 0)
                return data;
            return opcode.ToString();
        }
    }
}
