namespace RedSaw.CommandLineInterface
{
    /// <summary> used to save suggestion information. </summary>
    public readonly struct Suggestion
    {
        public readonly string primary;

        public readonly string description;

        public Suggestion(string primary, string description)
        {
            this.primary = primary;
            this.description = description;
        }

        public Suggestion(string primary)
        {
            this.primary = primary;
            this.description = string.Empty;
        }

        public override string ToString()
        {
            if (this.description.Length > 0) return $"{primary} :{description}";

            return primary;
        }
    }
}
