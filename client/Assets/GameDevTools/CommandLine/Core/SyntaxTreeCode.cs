namespace RedSaw.CommandLineInterface
{
    /// <summary> syntax tree code, tell VirualMachine how to execute given node. </summary>
    public enum SyntaxTreeCode
    {
        FactorString,
        FactorFloat,
        FactorTrue,
        FactorFalse,
        FactorNull,
        FactorInt,
        FactorId,
        FactorInput,
        FactorParams,
        SetElement,
        SetField,
        Loadvar,
        Assign,
        Index,
        Call,
        Cvt,
        Dot,
    }
}
