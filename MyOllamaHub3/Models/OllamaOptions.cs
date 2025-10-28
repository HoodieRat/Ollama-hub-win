using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.AI;

namespace MyOllamaHub3.Models
{
    public sealed class OllamaOptions
    {
        private const int MaxStopSequencesSupported = 4;
        private const int MaxStopSequenceLength = 120;
        private const int MinSupportedNumPredict = 32;
        private const int MaxSupportedNumPredict = 2048;
        private const int MinSupportedNumCtx = 512;
        private const int MaxSupportedNumCtx = 32768;

        // Core sampling
        public double Temperature { get; set; } = 0.7;
        public double TopP { get; set; } = 0.9;
        public int? TopK { get; set; } = 40;
        public double? RepeatPenalty { get; set; } = 1.1;

        // Context/length
        public int? NumCtx { get; set; } = 4096;
        public int NumPredict { get; set; } = 512;

        // Determinism
        public int? Seed { get; set; }

        // Mirostat
        public int? Mirostat { get; set; } = 0;
        public double? MirostatTau { get; set; } = 5.0;
        public double? MirostatEta { get; set; } = 0.1;

        // Prompting extras (applies to ALL models; ignored if unsupported)
        public string? SystemPrompt { get; set; }
        public List<string> StopSequences { get; set; } = new List<string>();

        public static OllamaOptions Default() => new OllamaOptions();

        public OllamaOptions Clone() => new OllamaOptions
        {
            Temperature = Temperature,
            TopP = TopP,
            TopK = TopK,
            RepeatPenalty = RepeatPenalty,
            NumCtx = NumCtx,
            NumPredict = NumPredict,
            Seed = Seed,
            Mirostat = Mirostat,
            MirostatTau = MirostatTau,
            MirostatEta = MirostatEta,
            SystemPrompt = SystemPrompt,
            StopSequences = new List<string>(StopSequences ?? new List<string>())
        };

        public ChatOptions ToChatOptions() => ToChatOptions(out _);

        public ChatOptions ToChatOptions(out string? warning)
        {
            var warnings = new List<string>();

            int? resolvedNumPredict = null;
            if (NumPredict > 0)
            {
                resolvedNumPredict = ClampWithNotice(NumPredict, MinSupportedNumPredict, MaxSupportedNumPredict, "num_predict", warnings);
            }
            else if (NumPredict < 0)
            {
                warnings.Add($"num_predict {NumPredict} is below the supported minimum ({MinSupportedNumPredict}); using {MinSupportedNumPredict}.");
                resolvedNumPredict = MinSupportedNumPredict;
            }

            var options = new ChatOptions
            {
                Temperature = (float?)Temperature,
                TopP = (float?)TopP,
                MaxOutputTokens = resolvedNumPredict
            };

            options.AdditionalProperties ??= new AdditionalPropertiesDictionary();

            void Add(string key, object? value)
            {
                if (value is null) return;
                options.AdditionalProperties[key] = value;
            }

            Add("top_k", TopK);
            Add("repeat_penalty", RepeatPenalty);

            int? resolvedNumCtx = null;
            if (NumCtx.HasValue)
            {
                var rawNumCtx = NumCtx.Value;
                if (rawNumCtx <= 0)
                {
                    warnings.Add($"num_ctx {rawNumCtx} is below the supported minimum ({MinSupportedNumCtx}); using {MinSupportedNumCtx}.");
                    resolvedNumCtx = MinSupportedNumCtx;
                }
                else
                {
                    resolvedNumCtx = ClampWithNotice(rawNumCtx, MinSupportedNumCtx, MaxSupportedNumCtx, "num_ctx", warnings);
                }
            }

            Add("num_ctx", resolvedNumCtx);
            if (Seed.HasValue)
                options.AdditionalProperties["seed"] = Seed.Value;
            Add("mirostat", Mirostat);
            Add("mirostat_tau", MirostatTau);
            Add("mirostat_eta", MirostatEta);

            var normalizedStops = NormalizeStopSequences(StopSequences, out var stopSequenceWarning);
            if (!string.IsNullOrWhiteSpace(stopSequenceWarning))
                warnings.Add(stopSequenceWarning);

            if (normalizedStops.Count > 0)
                options.AdditionalProperties["stop"] = normalizedStops.ToArray();

            warning = warnings.Count == 0 ? null : string.Join(" ", warnings);
            return options;
        }

        private static List<string> NormalizeStopSequences(List<string>? source, out string? warning)
        {
            warning = null;
            if (source == null || source.Count == 0)
                return new List<string>();

            var warnings = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var cleaned = new List<string>();

            foreach (var entry in source)
            {
                var trimmed = entry?.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                var materialized = trimmed.Length > MaxStopSequenceLength
                    ? TrimWithNotice(trimmed, warnings)
                    : trimmed;

                if (seen.Add(materialized))
                    cleaned.Add(materialized);
            }

            if (cleaned.Count > MaxStopSequencesSupported)
            {
                warnings.Add($"Only the first {MaxStopSequencesSupported} stop sequences are supported; ignoring {cleaned.Count - MaxStopSequencesSupported} additional value(s).");
                cleaned = cleaned.Take(MaxStopSequencesSupported).ToList();
            }

            warning = warnings.Count == 0 ? null : string.Join(" ", warnings);
            return cleaned;

            static string TrimWithNotice(string value, List<string> warnings)
            {
                var truncated = value.Substring(0, MaxStopSequenceLength);
                var preview = value.Length > 30 ? value.Substring(0, 30) + "…" : value;
                warnings.Add($"Stop sequence '{preview}' exceeded {MaxStopSequenceLength} characters and was truncated.");
                return truncated;
            }
        }

        private static int ClampWithNotice(int value, int min, int max, string name, List<string> warnings)
        {
            if (value < min)
            {
                warnings.Add($"{name} {value} is below the supported minimum ({min}); using {min}.");
                return min;
            }

            if (value > max)
            {
                warnings.Add($"{name} {value} exceeds the supported maximum ({max}); using {max}.");
                return max;
            }

            return value;
        }
    }
}
