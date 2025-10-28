# MyOllamaHub3

A Windows desktop companion for orchestrating Ollama models with a focus on approachable UI, rich conversation history, and fine-grained model tuning. MyOllamaHub3 wraps the Ollama HTTP API in a WinForms experience that keeps workflows fast while still exposing the depth of advanced generation controls.

## Key Capabilities

- **Guided chat surface:** Markdown-rendered assistant responses with inline system, user, and tool messages.
- **Session management:** Rename, export, and multi-select delete sessions; automatic persistence with debounce to avoid disk churn.
- **Model presets & tuning:** One-click persona presets plus a full advanced tuner covering sampling, context, Mirostat, determinism, and stop sequences.
- **Quick sanity tests:** Run sample prompts against current options without leaving the tuner.
- **Auto model selection (optional):** Automatically pick a suitable Ollama model based on prompt intent heuristics.
- **Diagnostics stream:** Visibility into background operations (persistence, warmup, tool execution) for easier troubleshooting.

## Architecture Overview

The solution contains two projects:

- `MyOllamaHub3` – WinForms application targeting .NET 8 (Windows) with WebView2 for markdown rendering and service classes for Ollama orchestration.
- `MyOllamaHub3.Tests` – xUnit test project validating session persistence, tool registry behaviors, and streaming update helpers.

Notable runtime components include:

| Component | Purpose |
| --- | --- |
| `Form1` | Primary shell managing chat UI, session lifecycle, and diagnostics.
| `SessionManager` / `SessionRecord` | Tracks conversation state, provides transcript export, and enforces persistence rules (skips empty sessions).
| `ModelTuner` | Dialog exposing simple and advanced configuration for Ollama options.
| `WarmupManager` | Warms models ahead of first use to reduce latency.
| `ThemedMarkdownView` | Custom WebView2 host for displaying chat history with theme support.

## Prerequisites

- **Operating System:** Windows 10 (1903+) or Windows 11.
- **.NET SDK:** .NET 8.0 SDK and runtime (`dotnet --list-sdks` to verify).
- **Ollama:** Installed locally with the HTTP API available at `http://localhost:11434/` (default). Ensure required models are pulled.
- **WebView2 Runtime:** Automatically installed on most Windows systems; otherwise download from [Microsoft](https://developer.microsoft.com/microsoft-edge/webview2/).

## Getting Started

1. **Clone the repository**
   ```powershell
   git clone https://github.com/<your-account>/MyOllamaHub3.git
   cd MyOllamaHub3
   ```

2. **Restore dependencies**
   ```powershell
   dotnet restore
   ```

3. **Run the application**
   ```powershell
   dotnet run --project MyOllamaHub3/MyOllamaHub3.csproj
   ```

4. **Point to an Ollama instance**
   - The app assumes Ollama is listening at `http://localhost:11434/`.
   - Adjust the base URL in `Form1.cs` (`OllamaBase`) if your host differs.

5. **Verify WebView2**
   - If the chat surface remains blank, install or repair the WebView2 runtime.

## Solution Structure

```
MyOllamaHub3.sln
├─ MyOllamaHub3/
│  ├─ Services/
│  ├─ Models/
│  ├─ ModelTuner.*
│  ├─ Form1.* (UI, persistence, diagnostics)
│  └─ ...
├─ MyOllamaHub3.Tests/
│  ├─ Services/
│  └─ Session/
└─ docs/
   └─ SprintPlan.md
```

## Application Walkthrough

### Chat & Diagnostics Pane

- **Prompt editor:** Enter instructions for the model; multi-line supported.
- **Send / Cancel buttons:** Kick off or abort streaming requests.
- **Diagnostics rail:** Shows background events (e.g., persistence failures, warmups). Errors appear in red for quick attention.
- **Markdown transcript:** Renders system, user, assistant, and tool outputs. Tool executions appear in a dedicated block with success/failure status.

### Sessions Tab

- **List View:** Displays stored sessions sorted by last activity. Multi-select is supported for bulk delete/export actions.
- **New Session:** `File → New Session` starts a fresh conversation. Unsaved empty sessions are pruned automatically.
- **Rename:** Select one session and choose `File → Rename Session`.
- **Export:**
  - Single selection: prompts for a file path.
  - Multi-selection: prompts for a folder and writes Markdown transcripts (unique filenames enforced).
- **Delete:** Supports single or multi-select deletion with confirmation. The app switches to the most recent remaining session if the current one is removed.

### Model Tuner Dialog

#### Simple Tab

- **Preset selector:** Switch between curated personas (Speed, Balanced, Depth, Creative, Deterministic). The combo box scales with window size.
- **Depth & Speed sliders:** Adjust response richness vs. latency. Numeric up/downs provide precise values.
- **Quick sanity test:** Run a prompt snippet against current options; progress feedback appears alongside.

#### Advanced Tab

- **Sampling:** Temperature, top-p, and top-k controls for creativity and determinism.
- **Length & context:** Configure `num_predict` (max new tokens) and `num_ctx` (context window). Values respect model limits.
- **Anti-repeat:** Tune repetition penalty to avoid loops.
- **Mirostat:** Toggle Mirostat modes with tau/eta fine-tuning.
- **System & stops:** Edit the system prompt and manage stop sequences (add/remove list).
- **Determinism:** Set a fixed seed or enable per-run randomization.
- **Command bar:** Reset to defaults, import/export option JSON, or apply without closing.

### Settings & Persistence

- Settings and sessions save to `%LOCALAPPDATA%\MyOllamaHub3`.
- Debounced savers prevent frequent disk writes.
- Sessions without user/assistant/tool messages are excluded from persistence to keep the history tidy.

## Running Tests

Execute the unit suite from the solution root:

```powershell
dotnet test
```

The tests emphasize persistence behaviors, session serialization, and helper services (streaming updates, tool registry).

## Troubleshooting

| Symptom | Resolution |
| --- | --- |
| Chat pane blank | Install WebView2 runtime; relaunch app. |
| `Send` is disabled | Ensure no request is already streaming and that prompt text is present. |
| Ollama connection errors | Confirm Ollama is running; adjust `OllamaBase` URL if remote. |
| Sessions missing after restart | Only sessions with activity are persisted. Begin interacting with a session before expecting it to save. |
| Locked build outputs | Close any running instance of `MyOllamaHub3.exe` before rebuilding. |

## Extending the App

- **Add new tools:** Register via `ToolRegistry` to expose additional capabilities to the assistant.
- **Integrate new presets:** Update `ModelCatalog.GenerationPresets` and ensure combo box data binding reflects the change.
- **UI theming:** `ThemeApplier` centralizes control styling; extend it to keep a consistent look.

## Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/my-improvement`).
3. Commit with clear messages and run `dotnet test`.
4. Submit a pull request detailing the change and any UI screenshots where helpful.

## License

Add your chosen license here (MIT, Apache 2.0, etc.).

---

Need help or have feature ideas? Open an issue or start a discussion—MyOllamaHub3 is evolving rapidly, and community input is welcome.
