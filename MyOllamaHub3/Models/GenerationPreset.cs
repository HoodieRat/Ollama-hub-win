using System;

namespace MyOllamaHub3.Models
{
    public sealed class GenerationPreset
    {
        private const double DoubleComparisonTolerance = 0.005;
        private readonly Action<OllamaOptions> _apply;
        private readonly Func<OllamaOptions, bool> _matches;

        private GenerationPreset(string name, string description, Action<OllamaOptions> apply, Func<OllamaOptions, bool> matches, bool isCustom)
        {
            Name = name;
            Description = description;
            _apply = apply ?? (_ => { });
            _matches = matches ?? (_ => false);
            IsCustom = isCustom;
        }

        public string Name { get; }
        public string Description { get; }
        public bool IsCustom { get; }

        public static GenerationPreset Create(string name, string description, Action<OllamaOptions> apply, Func<OllamaOptions, bool> matches)
            => new GenerationPreset(name, description, apply, matches, isCustom: false);

        public static GenerationPreset Custom(string name)
            => new GenerationPreset(name, "User-defined generation settings.", _ => { }, _ => false, isCustom: true);

        public void ApplyTo(OllamaOptions target) => _apply(target);

        public bool Matches(OllamaOptions candidate) => _matches(candidate);

    internal static bool NearlyEqual(double? left, double? right)
        {
            if (!left.HasValue || !right.HasValue)
                return false;

            return Math.Abs(left.Value - right.Value) <= DoubleComparisonTolerance;
        }
    }
}
