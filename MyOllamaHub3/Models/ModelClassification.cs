using System;
using System.Globalization;

namespace MyOllamaHub3.Models
{
    public enum ModelSuggestedUse
    {
        General = 0,
        Coding,
        Vision,
        Reasoning,
        Creative,
        Tooling
    }

    public sealed class ModelClassification
    {
        public string ModelId { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public ModelSuggestedUse SuggestedUse { get; init; } = ModelSuggestedUse.General;
        public string SuggestedUseLabel { get; init; } = "General";
        public string TypeLabel { get; init; } = "General";
        public string IdealUse { get; init; } = "General prompting";
        public int OutputScore { get; init; } = 3;
        public string OutputLabel { get; init; } = "~2K tokens";
        public int SpeedScore { get; init; } = 3;
        public string SpeedLabel { get; init; } = "Medium";
        public double TokensPerSecond { get; init; }
            = 0;
        public int AnalyticalScore { get; init; } = 3;
        public int CreativityScore { get; init; } = 3;
        public int AccuracyScore { get; init; } = 3;
        public string Notes { get; init; } = string.Empty;
        public string SizeLabel { get; init; } = string.Empty;
        public string Quantization { get; init; } = string.Empty;
        public bool IsUncensored { get; init; }
            = false;
        public bool IsHeuristic { get; init; }
            = false;
        public bool HasMeasuredSpeed { get; init; }
            = false;

        public string FormatSpeedDisplay()
        {
            var score = Math.Max(0, SpeedScore);
            var label = string.IsNullOrWhiteSpace(SpeedLabel) ? "Unknown" : SpeedLabel;
            return string.Create(CultureInfo.InvariantCulture, $"{score} – {label}");
        }

        public string FormatOutputDisplay()
        {
            var score = Math.Max(0, OutputScore);
            var label = string.IsNullOrWhiteSpace(OutputLabel) ? "Unknown" : OutputLabel;
            return string.Create(CultureInfo.InvariantCulture, $"{score} – {label}");
        }
    }
}
