using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyOllamaHub3
{
    internal interface IExternalTool
    {
        string Name { get; }
        IReadOnlyCollection<string> Triggers { get; }
        string Description { get; }
        Task<ToolInvocationResult> ExecuteAsync(string arguments, CancellationToken cancellationToken);
    }

    internal sealed class ToolInvocationResult
    {
        private ToolInvocationResult(bool success, string output, string? error)
        {
            Success = success;
            Output = output ?? string.Empty;
            ErrorMessage = error;
        }

        public bool Success { get; }
        public string Output { get; }
        public string? ErrorMessage { get; }

        public static ToolInvocationResult Successful(string output) => new ToolInvocationResult(true, output, null);
        public static ToolInvocationResult Failure(string? error) => new ToolInvocationResult(false, string.Empty, error);
    }

    internal sealed class ToolRegistry
    {
        private readonly Dictionary<string, IExternalTool> _tools = new Dictionary<string, IExternalTool>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _triggerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _enabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public void Register(IExternalTool tool)
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            if (_tools.ContainsKey(tool.Name)) throw new InvalidOperationException($"Tool '{tool.Name}' is already registered.");

            _tools[tool.Name] = tool;

            foreach (var trigger in tool.Triggers ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(trigger)) continue;
                _triggerMap[NormalizeTrigger(trigger)] = tool.Name;
            }
        }

        public IEnumerable<IExternalTool> AllTools => _tools.Values;

        public bool HasEnabledTools => _enabled.Count > 0;

        public bool TryGetTool(string name, out IExternalTool tool) => _tools.TryGetValue(name, out tool!);

        public bool TryGetToolByTrigger(string trigger, out IExternalTool tool)
        {
            tool = null!;
            if (string.IsNullOrWhiteSpace(trigger)) return false;
            if (!_triggerMap.TryGetValue(NormalizeTrigger(trigger), out var toolName)) return false;
            return _tools.TryGetValue(toolName, out tool!);
        }

        public void SetEnabled(string name, bool enabled)
        {
            if (!_tools.ContainsKey(name)) return;
            if (enabled) _enabled.Add(name);
            else _enabled.Remove(name);
        }

        public bool IsEnabled(string name) => _enabled.Contains(name);

        public IEnumerable<IExternalTool> EnabledTools => _enabled
            .Select(n => _tools.TryGetValue(n, out var tool) ? tool : null)
            .Where(t => t != null)!;

        private static string NormalizeTrigger(string trigger)
        {
            trigger = trigger.Trim();
            return trigger.StartsWith("/") ? trigger : "/" + trigger;
        }
    }

    internal sealed class GmailTool : IExternalTool
    {
        private static readonly IReadOnlyCollection<string> ToolTriggers = new[] { "/gmail" };

        public string Name => "Gmail";
        public IReadOnlyCollection<string> Triggers => ToolTriggers;
        public string Description => "Fetches summaries of Gmail conversations (stub).";

        public async Task<ToolInvocationResult> ExecuteAsync(string arguments, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);

            var query = string.IsNullOrWhiteSpace(arguments) ? "latest messages" : arguments.Trim();
            var sb = new StringBuilder();
            sb.AppendLine($"[Simulated Gmail search for '{query}']");
            sb.AppendLine("- TODO: Integrate Gmail API SDK and OAuth flow.");
            sb.AppendLine("- Sample message → Subject: Project kickoff, Snippet: Reminder that kickoff is Monday.");
            sb.AppendLine("- Sample message → Subject: Quarterly report, Snippet: Draft ready for review.");

            return ToolInvocationResult.Successful(sb.ToString());
        }
    }

    internal sealed class GoogleDocsTool : IExternalTool
    {
        private static readonly IReadOnlyCollection<string> ToolTriggers = new[] { "/docs", "/gdocs" };

        public string Name => "Google Docs";
        public IReadOnlyCollection<string> Triggers => ToolTriggers;
        public string Description => "Searches Google Docs for a summary (stub).";

        public async Task<ToolInvocationResult> ExecuteAsync(string arguments, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);

            var query = string.IsNullOrWhiteSpace(arguments) ? "recent documents" : arguments.Trim();
            var sb = new StringBuilder();
            sb.AppendLine($"[Simulated Google Docs lookup for '{query}']");
            sb.AppendLine("- TODO: Implement Docs API integration and OAuth.");
            sb.AppendLine("- Sample doc → 'Strategy Outline' last edited 2 days ago.");
            sb.AppendLine("- Sample doc → 'Budget FY2026' shared by finance.");

            return ToolInvocationResult.Successful(sb.ToString());
        }
    }
}
