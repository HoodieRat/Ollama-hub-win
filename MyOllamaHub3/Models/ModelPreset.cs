using System;

namespace MyOllamaHub3.Models
{
    public sealed class ModelPreset
    {
        public ModelPreset(string name, string modelId, string? description = null)
        {
            Name = name;
            ModelId = modelId;
            Description = description ?? string.Empty;
        }

        public string Name { get; }
        public string ModelId { get; }
        public string Description { get; }

        public bool Matches(string? candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                return false;

            return string.Equals(candidate.Trim(), ModelId, StringComparison.OrdinalIgnoreCase);
        }

        public bool Matches(ModelProfile profile)
            => profile != null && (Matches(profile.ModelId) || Matches(profile.DisplayName));
    }
}
