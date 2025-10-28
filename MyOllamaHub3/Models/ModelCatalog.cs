using System.Collections.Generic;

namespace MyOllamaHub3.Models
{
    internal static class ModelCatalog
    {
        public static IReadOnlyList<ModelPreset> ModelPresets { get; } = new[]
        {
            new ModelPreset("Balanced (llama3.1:8b)", "llama3.1:8b", "Balanced for everyday prompts."),
            new ModelPreset("Fast (mistral:7b)", "mistral:7b", "Lower latency model for quick replies."),
            new ModelPreset("Deep (llama3.1:70b)", "llama3.1:70b", "Heavier model for deep reasoning."),
            new ModelPreset("Creative (llama3.1:8b-chat)", "llama3.1:8b-chat", "Warmer defaults for ideation."),
            new ModelPreset("Coding (codellama:7b)", "codellama:7b", "Prefers structured, code-first responses."),
        };

        public static IReadOnlyList<GenerationPreset> GenerationPresets { get; } = new[]
        {
            GenerationPreset.Create("Balanced", "Default temperature and context window.", options =>
            {
                options.Temperature = 0.7;
                options.TopP = 0.9;
                options.TopK = 40;
                options.NumPredict = 512;
                options.RepeatPenalty = 1.1;
            }, options =>
                GenerationPreset.NearlyEqual(options.Temperature, 0.7) &&
                GenerationPreset.NearlyEqual(options.TopP, 0.9) &&
                options.TopK == 40 &&
                options.NumPredict == 512 &&
                GenerationPreset.NearlyEqual(options.RepeatPenalty, 1.1)),

            GenerationPreset.Create("Fast", "Short replies with lower sampling for latency.", options =>
            {
                options.Temperature = 0.5;
                options.TopP = 0.85;
                options.TopK = 25;
                options.NumPredict = 256;
                options.RepeatPenalty = 1.2;
            }, options =>
                GenerationPreset.NearlyEqual(options.Temperature, 0.5) &&
                GenerationPreset.NearlyEqual(options.TopP, 0.85) &&
                options.TopK == 25 &&
                options.NumPredict == 256 &&
                GenerationPreset.NearlyEqual(options.RepeatPenalty, 1.2)),

            GenerationPreset.Create("Creative", "Higher temperature for expansive writing.", options =>
            {
                options.Temperature = 1.0;
                options.TopP = 0.95;
                options.TopK = 60;
                options.NumPredict = 768;
                options.RepeatPenalty = 0.95;
            }, options =>
                GenerationPreset.NearlyEqual(options.Temperature, 1.0) &&
                GenerationPreset.NearlyEqual(options.TopP, 0.95) &&
                options.TopK == 60 &&
                options.NumPredict == 768),

            GenerationPreset.Create("Deep", "More tokens and context for longer answers.", options =>
            {
                options.Temperature = 0.85;
                options.TopP = 0.92;
                options.TopK = 60;
                options.NumPredict = 1024;
                options.NumCtx = 8192;
                options.RepeatPenalty = 1.05;
            }, options =>
                GenerationPreset.NearlyEqual(options.Temperature, 0.85) &&
                GenerationPreset.NearlyEqual(options.TopP, 0.92) &&
                options.TopK == 60 &&
                options.NumPredict == 1024 &&
                GenerationPreset.NearlyEqual(options.RepeatPenalty, 1.05) &&
                options.NumCtx == 8192),

            GenerationPreset.Create("Deterministic", "Lower temperature for concise answers.", options =>
            {
                options.Temperature = 0.3;
                options.TopP = 0.8;
                options.TopK = 30;
                options.NumPredict = 400;
                options.RepeatPenalty = 1.15;
            }, options =>
                GenerationPreset.NearlyEqual(options.Temperature, 0.3) &&
                GenerationPreset.NearlyEqual(options.TopP, 0.8) &&
                options.TopK == 30 &&
                options.NumPredict == 400),

            GenerationPreset.Custom("Advanced")
        };

        public static IReadOnlyList<ModelProfile> BuiltInModelProfiles { get; } = new[]
        {
            new ModelProfile("llama3.1:8b", "Llama 3.1 8B", "General", "Conversation, analysis", outputLengthScore:4, outputLengthLabel:"~3K tokens", speedScore:3, speedLabel:"Medium", analyticalScore:4, creativityScore:3, accuracyScore:4, notes:"Great default. Pull with `ollama pull llama3.1:8b`.", baseScore:3.5),
            new ModelProfile("codellama:7b", "Code Llama 7B", "Coding", "Code generation & explanation", outputLengthScore:3, outputLengthLabel:"~2K tokens", speedScore:3, speedLabel:"Medium", analyticalScore:4, creativityScore:2, accuracyScore:4, notes:"Best with structured prompts.", baseScore:3.2),
            new ModelProfile("mistral:7b", "Mistral 7B", "General", "Fast drafts", outputLengthScore:3, outputLengthLabel:"~2K tokens", speedScore:4, speedLabel:"Fast", analyticalScore:3, creativityScore:3, accuracyScore:3, notes:"Good speed on consumer GPUs.", baseScore:3.0),
            new ModelProfile("llama3.1:70b", "Llama 3.1 70B", "General", "Deep reasoning & detailed analysis", outputLengthScore:5, outputLengthLabel:"~8K tokens", speedScore:1, speedLabel:"Slow", analyticalScore:5, creativityScore:4, accuracyScore:5, notes:"Requires substantial VRAM; excellent for depth.", baseScore:4.6)
        };

        public static IReadOnlyList<ModelSummaryEntry> ModelSummaryEntries { get; } = new[]
        {
            new ModelSummaryEntry("General purpose", "llama3.1:8b, mistral:7b, llama3.1:70b", "Balanced reasoning and creativity."),
            new ModelSummaryEntry("Coding", "codellama:7b, mistral:7b-code", "Prefers structured output for engineering work."),
            new ModelSummaryEntry("Creative", "llama3.1:8b", "Looser guardrails for ideation and story work."),
            new ModelSummaryEntry("Deep reasoning", "llama3.1:70b", "Longest context and highest accuracy."),
        };

        public static string ModelLegendText { get; } =
"• Type – Model family or specialization.\r\n" +
"• Ideal use – Recommended workloads.\r\n" +
"• Output – Approximate output length envelope.\r\n" +
"• Speed / Analytical / Creativity / Accuracy – Relative 0-5 scores.\r\n" +
"• Notes – Caveats or extra installation instructions.";
    }
}
