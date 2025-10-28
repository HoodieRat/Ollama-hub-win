using System;

namespace MyOllamaHub3.Models
{
    public sealed class ModelProfile
    {
        public ModelProfile(
            string modelId,
            string displayName,
            string type,
            string idealUse,
            int outputLengthScore,
            string outputLengthLabel,
            int speedScore,
            string speedLabel,
            int analyticalScore,
            int creativityScore,
            int accuracyScore,
            string notes,
            double baseScore,
            bool isUncensored = false)
        {
            ModelId = modelId;
            DisplayName = displayName;
            Type = type;
            IdealUse = idealUse;
            OutputLengthScore = outputLengthScore;
            OutputLengthLabel = outputLengthLabel;
            SpeedScore = speedScore;
            SpeedLabel = speedLabel;
            AnalyticalScore = analyticalScore;
            CreativityScore = creativityScore;
            AccuracyScore = accuracyScore;
            Notes = notes;
            BaseScore = baseScore;
            IsUncensored = isUncensored;
        }

        public string ModelId { get; }
        public string DisplayName { get; }
        public string Type { get; }
        public string IdealUse { get; }
        public int OutputLengthScore { get; }
        public string OutputLengthLabel { get; }
        public int SpeedScore { get; }
        public string SpeedLabel { get; }
        public int AnalyticalScore { get; }
        public int CreativityScore { get; }
        public int AccuracyScore { get; }
        public string Notes { get; }
        public double BaseScore { get; }
        public bool IsUncensored { get; }

        public bool Matches(string? candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                return false;

            var trimmed = candidate.Trim();
            return string.Equals(trimmed, ModelId, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(trimmed, DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        public bool Matches(ModelProfile? other)
        {
            if (other == null)
                return false;

            return Matches(other.ModelId) || Matches(other.DisplayName);
        }

        public ModelProfile WithUncensoredFlag(bool flag)
            => new ModelProfile(ModelId, DisplayName, Type, IdealUse, OutputLengthScore, OutputLengthLabel, SpeedScore, SpeedLabel, AnalyticalScore, CreativityScore, AccuracyScore, Notes, BaseScore, flag);

        public ModelProfile WithOverrides(
            string? type = null,
            string? idealUse = null,
            int? outputLengthScore = null,
            string? outputLengthLabel = null,
            int? speedScore = null,
            string? speedLabel = null,
            int? analyticalScore = null,
            int? creativityScore = null,
            int? accuracyScore = null,
            string? notes = null,
            bool? isUncensored = null)
            => new ModelProfile(
                ModelId,
                DisplayName,
                type ?? Type,
                idealUse ?? IdealUse,
                outputLengthScore ?? OutputLengthScore,
                outputLengthLabel ?? OutputLengthLabel,
                speedScore ?? SpeedScore,
                speedLabel ?? SpeedLabel,
                analyticalScore ?? AnalyticalScore,
                creativityScore ?? CreativityScore,
                accuracyScore ?? AccuracyScore,
                notes ?? Notes,
                BaseScore,
                isUncensored ?? IsUncensored);

        public static ModelProfile CreateFallback(string modelName)
        {
            var classification = ModelClassifier.Classify(modelName);

            var display = string.IsNullOrWhiteSpace(classification.DisplayName)
                ? (string.IsNullOrWhiteSpace(modelName) ? "Unknown Model" : modelName)
                : classification.DisplayName;

            var notes = string.IsNullOrWhiteSpace(classification.Notes)
                ? "No profile available."
                : classification.Notes;

            return new ModelProfile(
                string.IsNullOrWhiteSpace(modelName) ? display : modelName,
                display,
                classification.TypeLabel,
                classification.IdealUse,
                classification.OutputScore,
                classification.OutputLabel,
                classification.SpeedScore,
                classification.SpeedLabel,
                classification.AnalyticalScore,
                classification.CreativityScore,
                classification.AccuracyScore,
                notes,
                baseScore: 2.5,
                isUncensored: classification.IsUncensored);
        }
    }
}
