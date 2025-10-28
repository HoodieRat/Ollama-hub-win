using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyOllamaHub3.Models
{
    internal static class ModelClassifier
    {
        private sealed record HeuristicInfo(
            string BaseName,
            int ParametersB,
            string SizeLabel,
            string Quantization,
            ModelSuggestedUse SuggestedUse,
            bool IsUncensored,
            string DisplayName);

        private static readonly ConcurrentDictionary<string, double> MeasuredTokensPerSecond = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Regex SizeRegex = new(@"(?:(?<num>\d+)(?:\.(?<frac>\d+))?)\s*(?<unit>[bm])(?=[^\w]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex QuantRegex = new(@"-q(?<bits>\d+)(?:_[a-z_]+)?(?=[^\w]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void UpdateMeasurement(string modelId, double tokensPerSecond)
        {
            if (string.IsNullOrWhiteSpace(modelId) || tokensPerSecond <= 0)
                return;

            MeasuredTokensPerSecond.AddOrUpdate(
                modelId,
                tokensPerSecond,
                (_, existing) => existing <= 0
                    ? tokensPerSecond
                    : (existing * 0.7) + (tokensPerSecond * 0.3));
        }

        public static ModelClassification Classify(string modelId, ModelProfile? baseline = null)
        {
            var normalizedId = (modelId ?? string.Empty).Trim();
            var measured = TryGetMeasuredTokens(normalizedId);
            var heuristics = AnalyzeName(normalizedId);

            if (baseline != null)
                return BuildFromBaseline(normalizedId, baseline, heuristics, measured);

            return BuildFromHeuristics(normalizedId, heuristics, measured);
        }

        private static ModelClassification BuildFromBaseline(string modelId, ModelProfile baseline, HeuristicInfo heuristics, double measuredTps)
        {
            var suggestedUse = GuessSuggestedUseFromBaseline(baseline, heuristics.SuggestedUse);
            var (speedScore, speedLabel, hasMeasured) = ComposeSpeed(baseline.SpeedScore, baseline.SpeedLabel, measuredTps, heuristics);
            var notes = AppendMeasurementToNotes(baseline.Notes, measuredTps, heuristics);

            return new ModelClassification
            {
                ModelId = modelId,
                DisplayName = string.IsNullOrWhiteSpace(baseline.DisplayName) ? heuristics.DisplayName : baseline.DisplayName,
                SuggestedUse = suggestedUse,
                SuggestedUseLabel = ToFriendlyUseLabel(suggestedUse),
                TypeLabel = string.IsNullOrWhiteSpace(baseline.Type) ? ToFriendlyUseLabel(suggestedUse) : baseline.Type,
                IdealUse = string.IsNullOrWhiteSpace(baseline.IdealUse) ? DefaultIdealUse(suggestedUse) : baseline.IdealUse,
                OutputScore = Math.Max(0, baseline.OutputLengthScore),
                OutputLabel = string.IsNullOrWhiteSpace(baseline.OutputLengthLabel) ? heuristics.SizeLabel : baseline.OutputLengthLabel,
                SpeedScore = speedScore,
                SpeedLabel = speedLabel,
                TokensPerSecond = hasMeasured ? measuredTps : EstimateTokensPerSecond(heuristics),
                AnalyticalScore = Math.Max(0, baseline.AnalyticalScore),
                CreativityScore = Math.Max(0, baseline.CreativityScore),
                AccuracyScore = Math.Max(0, baseline.AccuracyScore),
                Notes = notes,
                SizeLabel = heuristics.SizeLabel,
                Quantization = heuristics.Quantization,
                IsUncensored = baseline.IsUncensored || heuristics.IsUncensored,
                IsHeuristic = false,
                HasMeasuredSpeed = hasMeasured
            };
        }

        private static ModelClassification BuildFromHeuristics(string modelId, HeuristicInfo heuristics, double measuredTps)
        {
            var suggestedUse = heuristics.SuggestedUse;
            var baseQuality = QualityFromSize(heuristics.ParametersB);
            var outputScore = OutputFromSize(heuristics.ParametersB);
            var estimated = measuredTps > 0 ? measuredTps : EstimateTokensPerSecond(heuristics);
            var speedScore = SpeedScoreFromTokens(estimated);
            var speedLabel = ComposeSpeedLabel(speedScore, estimated, measuredTps > 0);

            var analytical = AdjustForUse(baseQuality, suggestedUse, boostFor: ModelSuggestedUse.Reasoning, clampMax: 5);
            analytical = AdjustForUse(analytical, suggestedUse, boostFor: ModelSuggestedUse.Coding, clampMax: 5);

            var creativity = AdjustForUse(baseQuality, suggestedUse, boostFor: ModelSuggestedUse.Creative, clampMax: 5);
            creativity = AdjustForUse(creativity, suggestedUse, reduceFor: ModelSuggestedUse.Reasoning, clampMin: 1);

            var accuracy = AdjustForUse(baseQuality, suggestedUse, boostFor: ModelSuggestedUse.Reasoning, clampMax: 5);
            accuracy = AdjustForUse(accuracy, suggestedUse, reduceFor: ModelSuggestedUse.Creative, clampMin: 1);

            if (speedScore >= 4)
                accuracy = Math.Max(accuracy - 1, 1);

            var notes = BuildHeuristicNotes(heuristics, measuredTps);

            return new ModelClassification
            {
                ModelId = modelId,
                DisplayName = heuristics.DisplayName,
                SuggestedUse = suggestedUse,
                SuggestedUseLabel = ToFriendlyUseLabel(suggestedUse),
                TypeLabel = ToFriendlyUseLabel(suggestedUse),
                IdealUse = DefaultIdealUse(suggestedUse),
                OutputScore = outputScore,
                OutputLabel = OutputLabelFromSize(heuristics.ParametersB),
                SpeedScore = speedScore,
                SpeedLabel = speedLabel,
                TokensPerSecond = estimated,
                AnalyticalScore = analytical,
                CreativityScore = creativity,
                AccuracyScore = accuracy,
                Notes = notes,
                SizeLabel = heuristics.SizeLabel,
                Quantization = heuristics.Quantization,
                IsUncensored = heuristics.IsUncensored,
                IsHeuristic = true,
                HasMeasuredSpeed = measuredTps > 0
            };
        }

        private static (int Score, string Label, bool HasMeasured) ComposeSpeed(int baselineScore, string baselineLabel, double measuredTps, HeuristicInfo heuristics)
        {
            if (measuredTps <= 0)
                return (baselineScore, string.IsNullOrWhiteSpace(baselineLabel) ? ComposeSpeedLabel(baselineScore, EstimateTokensPerSecond(heuristics), false) : baselineLabel, false);

            var score = SpeedScoreFromTokens(measuredTps);
            var label = ComposeSpeedLabel(score, measuredTps, true);
            return (score, label, true);
        }

        private static string AppendMeasurementToNotes(string notes, double measuredTps, HeuristicInfo heuristics)
        {
            var trimmed = (notes ?? string.Empty).Trim();
            var quant = heuristics.Quantization;
            var size = heuristics.SizeLabel;
            var measurement = measuredTps > 0
                ? string.Create(CultureInfo.InvariantCulture, $"~{measuredTps:0.#} tok/s observed")
                : string.Empty;

            var qualifier = string.Join(", ", new[] { size, quant, measurement }.Where(s => !string.IsNullOrWhiteSpace(s)));

            if (string.IsNullOrWhiteSpace(qualifier))
                return trimmed;

            if (string.IsNullOrWhiteSpace(trimmed))
                return qualifier;

            if (trimmed.IndexOf(qualifier, StringComparison.OrdinalIgnoreCase) >= 0)
                return trimmed;

            return string.Create(CultureInfo.InvariantCulture, $"{trimmed} | {qualifier}");
        }

        private static string BuildHeuristicNotes(HeuristicInfo heuristics, double measuredTps)
        {
            var measurement = measuredTps > 0
                ? string.Create(CultureInfo.InvariantCulture, $"~{measuredTps:0.#} tok/s observed")
                : string.Empty;

            var parts = new[]
            {
                heuristics.SizeLabel,
                heuristics.Quantization,
                measurement
            };

            return string.Join(" | ", parts.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private static ModelSuggestedUse GuessSuggestedUseFromBaseline(ModelProfile baseline, ModelSuggestedUse defaultUse)
        {
            if (!string.IsNullOrWhiteSpace(baseline.Type))
            {
                var typeLower = baseline.Type.ToLowerInvariant();
                if (typeLower.Contains("code")) return ModelSuggestedUse.Coding;
                if (typeLower.Contains("vision")) return ModelSuggestedUse.Vision;
                if (typeLower.Contains("reason") || typeLower.Contains("math") || typeLower.Contains("logic")) return ModelSuggestedUse.Reasoning;
                if (typeLower.Contains("creative") || typeLower.Contains("story")) return ModelSuggestedUse.Creative;
                if (typeLower.Contains("tool")) return ModelSuggestedUse.Tooling;
            }

            if (!string.IsNullOrWhiteSpace(baseline.IdealUse))
            {
                var idealLower = baseline.IdealUse.ToLowerInvariant();
                if (idealLower.Contains("code")) return ModelSuggestedUse.Coding;
                if (idealLower.Contains("vision")) return ModelSuggestedUse.Vision;
                if (idealLower.Contains("reason") || idealLower.Contains("math") || idealLower.Contains("logic")) return ModelSuggestedUse.Reasoning;
                if (idealLower.Contains("story") || idealLower.Contains("creative") || idealLower.Contains("writing")) return ModelSuggestedUse.Creative;
                if (idealLower.Contains("tool")) return ModelSuggestedUse.Tooling;
            }

            return defaultUse;
        }

        private static string ToFriendlyUseLabel(ModelSuggestedUse use)
            => use switch
            {
                ModelSuggestedUse.Coding => "Coding",
                ModelSuggestedUse.Vision => "Vision",
                ModelSuggestedUse.Reasoning => "Reasoning",
                ModelSuggestedUse.Creative => "Creative",
                ModelSuggestedUse.Tooling => "Tools",
                _ => "General"
            };

        private static string DefaultIdealUse(ModelSuggestedUse use)
            => use switch
            {
                ModelSuggestedUse.Coding => "Code generation & explanation",
                ModelSuggestedUse.Vision => "Multimodal or image-grounded replies",
                ModelSuggestedUse.Reasoning => "Analytical and math-heavy tasks",
                ModelSuggestedUse.Creative => "Storytelling and ideation",
                ModelSuggestedUse.Tooling => "Function calling & planning",
                _ => "Conversation and research"
            };

        private static double TryGetMeasuredTokens(string modelId)
        {
            if (MeasuredTokensPerSecond.TryGetValue(modelId, out var rate) && rate > 0)
                return rate;

            return 0;
        }

        private static HeuristicInfo AnalyzeName(string modelId)
        {
            var lower = modelId.ToLowerInvariant();
            var baseName = modelId;
            var colonIndex = modelId.IndexOf(':');
            if (colonIndex >= 0)
                baseName = modelId.Substring(0, colonIndex);

            var sizeLabel = "";
            var parametersB = 8;

            var sizeMatch = SizeRegex.Matches(lower);
            if (sizeMatch.Count > 0)
            {
                var last = sizeMatch[^1];
                if (double.TryParse(last.Groups["num"].Value + (last.Groups["frac"].Success ? "." + last.Groups["frac"].Value : string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    var unit = last.Groups["unit"].Value.ToLowerInvariant();
                    if (unit == "m")
                        value = value / 1000.0;

                    parametersB = Math.Max(1, (int)Math.Round(value));
                    sizeLabel = unit == "m"
                        ? string.Create(CultureInfo.InvariantCulture, $"{value * 1000:0.#}M params")
                        : string.Create(CultureInfo.InvariantCulture, $"{value:0.#}B params");
                }
            }

            var quantization = string.Empty;
            var quantMatch = QuantRegex.Match(lower);
            if (quantMatch.Success)
                quantization = "q" + quantMatch.Groups["bits"].Value;
            else if (lower.Contains("-q8"))
                quantization = "q8";
            else if (lower.Contains("-q6"))
                quantization = "q6";
            else if (lower.Contains("-q5"))
                quantization = "q5";
            else if (lower.Contains("-q4"))
                quantization = "q4";
            else if (lower.Contains("-q3"))
                quantization = "q3";

            var suggestedUse = GuessUseFromName(lower);
            var isUncensored = lower.Contains("uncensored") || lower.Contains("nsfw");
            var display = BuildDisplayName(modelId);

            return new HeuristicInfo(baseName, parametersB, sizeLabel, quantization, suggestedUse, isUncensored, display);
        }

        private static string BuildDisplayName(string modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId))
                return "Unknown Model";

            var cleaned = modelId.Replace('_', ' ').Replace('-', ' ').Trim();
            if (cleaned.Contains(':'))
            {
                var parts = cleaned.Split(':');
                cleaned = string.Join(" ", parts);
            }

            var info = CultureInfo.InvariantCulture.TextInfo;
            return info.ToTitleCase(cleaned.ToLowerInvariant());
        }

        private static ModelSuggestedUse GuessUseFromName(string lower)
        {
            if (lower.Contains("coder") || lower.Contains("code") || lower.Contains("codellama") || lower.Contains("starcoder"))
                return ModelSuggestedUse.Coding;

            if (lower.Contains("vision") || lower.Contains("vl") || lower.Contains("llava") || lower.Contains("clip"))
                return ModelSuggestedUse.Vision;

            if (lower.Contains("math") || lower.Contains("deepseek-r") || lower.Contains("reason") || lower.Contains("logic") || lower.Contains("think"))
                return ModelSuggestedUse.Reasoning;

            if (lower.Contains("creative") || lower.Contains("story") || lower.Contains("write") || lower.Contains("artist"))
                return ModelSuggestedUse.Creative;

            if (lower.Contains("tool") || lower.Contains("function") || lower.Contains("planner") || lower.Contains("agent"))
                return ModelSuggestedUse.Tooling;

            return ModelSuggestedUse.General;
        }

        private static double EstimateTokensPerSecond(HeuristicInfo info)
        {
            var baseEstimate = info.ParametersB switch
            {
                <= 3 => 55,
                <= 7 => 35,
                <= 13 => 22,
                <= 30 => 12,
                _ => 7
            };

            var quantFactor = info.Quantization switch
            {
                "q3" => 1.35,
                "q4" => 1.2,
                "q5" => 1.05,
                "q6" => 0.92,
                "q8" => 0.82,
                _ => 1.0
            };

            return Math.Max(4, baseEstimate * quantFactor);
        }

        private static int QualityFromSize(int paramsB)
        {
            return paramsB switch
            {
                >= 70 => 5,
                >= 30 => 4,
                >= 13 => 4,
                >= 7 => 3,
                >= 3 => 2,
                _ => 1
            };
        }

        private static int OutputFromSize(int paramsB)
        {
            return paramsB switch
            {
                >= 70 => 5,
                >= 30 => 4,
                >= 13 => 4,
                >= 7 => 3,
                >= 3 => 2,
                _ => 2
            };
        }

        private static string OutputLabelFromSize(int paramsB)
            => paramsB switch
            {
                >= 70 => "~12K tokens",
                >= 30 => "~10K tokens",
                >= 13 => "~6K tokens",
                >= 7 => "~4K tokens",
                >= 3 => "~2K tokens",
                _ => "~1K tokens"
            };

        private static int SpeedScoreFromTokens(double tokensPerSecond)
        {
            if (tokensPerSecond >= 60) return 5;
            if (tokensPerSecond >= 35) return 4;
            if (tokensPerSecond >= 20) return 3;
            if (tokensPerSecond >= 10) return 2;
            return 1;
        }

        private static string ComposeSpeedLabel(int score, double tokensPerSecond, bool measured)
        {
            var descriptor = score switch
            {
                >= 5 => "Very fast",
                4 => "Fast",
                3 => "Medium",
                2 => "Steady",
                _ => "Deliberate"
            };

            if (tokensPerSecond <= 0)
                return descriptor;

            var suffix = measured ? "observed" : "est.";
            return string.Create(CultureInfo.InvariantCulture, $"{descriptor} (~{tokensPerSecond:0.#} tok/s {suffix})");
        }

        private static int AdjustForUse(int baseScore, ModelSuggestedUse use, ModelSuggestedUse? boostFor = null, ModelSuggestedUse? reduceFor = null, int clampMax = 5, int clampMin = 1)
        {
            var score = baseScore;
            if (boostFor.HasValue && use == boostFor.Value)
                score = Math.Min(clampMax, score + 1);
            if (reduceFor.HasValue && use == reduceFor.Value)
                score = Math.Max(clampMin, score - 1);
            return Math.Max(clampMin, Math.Min(clampMax, score));
        }
    }
}
