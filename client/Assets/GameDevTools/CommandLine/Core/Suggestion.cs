namespace RedSaw.CommandLineInterface
{
    /// <summary> used to save suggestion information. </summary>
    public readonly struct Suggestion
    {
        /// <summary> Primary. </summary>
        public readonly string primary;

        /// <summary> Description. </summary>
        public readonly string description;

        /// <summary> Create a Suggestion instance. </summary>
        public Suggestion(string primary, string description)
        {
            this.primary = primary;
            this.description = description;
        }

        /// <summary> Create a Suggestion instance. </summary>
        public Suggestion(string primary)
        {
            this.primary = primary;
            this.description = string.Empty;
        }

        /// <summary> To string. </summary>
        public override string ToString()
        {
            if (this.description.Length > 0)
            {
                return $"{primary} :{description}";
            }

            return primary;
        }
    }
}
