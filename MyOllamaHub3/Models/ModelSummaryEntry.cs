namespace MyOllamaHub3.Models
{
    internal readonly struct ModelSummaryEntry
    {
        public ModelSummaryEntry(string category, string bestModels, string notes)
        {
            Category = category;
            BestModels = bestModels;
            Notes = notes;
        }

        public string Category { get; }
        public string BestModels { get; }
        public string Notes { get; }
    }
}
