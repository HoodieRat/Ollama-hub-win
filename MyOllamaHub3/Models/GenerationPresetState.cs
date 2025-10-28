using System.Collections.Generic;

namespace MyOllamaHub3.Models
{
    internal static class GenerationPresetState
    {
        public static GenerationPreset? FindMatch(IEnumerable<GenerationPreset>? presets, OllamaOptions? candidate)
        {
            if (presets == null || candidate == null)
                return null;

            foreach (var preset in presets)
            {
                if (preset == null || preset.IsCustom)
                    continue;

                if (preset.Matches(candidate))
                    return preset;
            }

            return null;
        }

        public static bool ShouldCaptureAdvancedSnapshot(IEnumerable<GenerationPreset>? presets, OllamaOptions? candidate, OllamaOptions? existingAdvanced)
        {
            if (candidate == null)
                return false;

            var match = FindMatch(presets, candidate);
            if (match == null)
                return true;

            return existingAdvanced == null;
        }
    }
}
