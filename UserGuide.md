# MyOllamaHub3 User Guide

This guide walks through every major feature in MyOllamaHub3 so you can move from install to productive use quickly. It pairs with the README (project overview) but dives much deeper into day-to-day operations, UI controls, and troubleshooting.

---

## 1. Installation & Requirements

1. **Prerequisites**
   - Windows 10 (1903+) or Windows 11
   - .NET 8.0 SDK (for build) and runtime (`dotnet --list-sdks`)
   - Ollama installed and running locally (default http://localhost:11434/)
   - Microsoft Edge WebView2 Runtime
2. **Clone & restore**
   ```powershell
   git clone https://github.com/<your-account>/MyOllamaHub3.git
   cd MyOllamaHub3
   dotnet restore
   ```
3. **Run the app**
   ```powershell
   dotnet run --project MyOllamaHub3/MyOllamaHub3.csproj
   ```
4. **Optional adjustments**
   - If your Ollama host is not `http://localhost:11434/`, update `OllamaBase` in `Form1.cs`.
   - Install WebView2 if the chat pane renders blank.

---

## 2. First Run Experience

- On first launch, the application creates `%LOCALAPPDATA%\MyOllamaHub3` to store `settings.json` and `sessions.json`.
- A default session called *Session YYYY-MM-DD HH:MM* is created. It remains only if you interact (empty sessions are pruned automatically).
- The main window opens with two tabs: **Prompt** (chat surface) and **Sessions**.

---

## 3. Main Window Tour

### 3.1 Menu Bar

| Menu | Commands |
| --- | --- |
| **File** | New Session, Export Session(s), Rename Session, Delete Session(s), Exit |
| **Tools** | Launch Model Tuner, Warmup controls, Diagnostics toggles (if enabled) |
| **Help** | Link to documentation and about dialog (future expansion) |

### 3.2 Prompt Tab Layout

| Area | Explanation |
| --- | --- |
| **Prompt Editor** | Multi-line text box at the bottom-left. Enter your request here. Supports standard clipboard shortcuts. |
| **Send / Cancel Buttons** | Located alongside the prompt box. `Send` dispatches a request to Ollama; `Cancel` aborts a streaming response in progress. |
| **Diagnostics rail** | Right-hand column listing background events (session persistence, tool calls, warmups, and errors). Entries highlight warnings/errors for quick scanning. |
| **Markdown conversation view** | Center pane rendered via WebView2. Displays chat in discrete blocks: user (blue), assistant (gray), system/tool (accent). |
| **Status labels** | Along the top of the prompt tab, showing active model and warmup progress. |

#### Prompt Toolbar (if enabled)
- Quick toggles for personalities (auto model selection), streaming indicators, and warmup status.
- Hover the warmup icon to see which models have been preloaded.

### 3.3 Sessions Tab

- **ListView**: Shows persisted sessions sorted by `Last Updated`. The list supports:
  - Single-click to select (loads the session into the Prompt tab).
  - Ctrl/Shift-click to multi-select for bulk export or delete.
  - Columns: *Session* (title) and *Last Updated* (local time `g` format).
- **Context actions**:
  - `File → Rename Session`: enabled when exactly one session is selected (falls back to current session if none selected).
  - `File → Delete Session(s)`: enabled when sessions are selected or a current session exists. Multi-select deletion triggers a confirmation dialog.
  - `File → Export Session(s)`: exports the selected set. Single selection asks for a file path; multi-selection prompts for a folder and writes Markdown transcripts with unique file names.
- **Auto selection**: When a streaming response is in progress, selecting another session reverts focus to the current one with an explanatory diagnostic message.

### 3.4 Diagnostics Buffer

- Captures up to 200 entries (configurable via constants in `Form1`).
- Typical messages include session save success/failure, warmup outcomes, and tool invocations.
- Use it to quickly diagnose connection or persistence issues without inspecting logs.

---

## 4. Model Tuner Dialog

Open from **Tools → Model Tuner**. The window contains **Simple** and **Advanced** tabs with a persistent command bar at the bottom.

### 4.1 Simple Tab

| Control | Purpose |
| --- | --- |
| **Preset selector** | Drop-down of curated personas (Speed, Balanced, Depth, Creative, Deterministic). Changes update the numeric model options automatically. |
| **Depth slider + numeric** | Controls conversation richness; higher values allow longer, more detailed responses. The numeric up/down grants precise edits. |
| **Speed slider + numeric** | Balances latency vs. reasoning steps. Lower speed = more deliberate reasoning. |
| **Quick sanity test panel** | Contains:
  - Prompt text area (`txtSamplePrompt`) with a default sample.
  - `Test` button to send the prompt using current settings and display results in the read-only rich text box.
  - `Progress` bar indicating test status.
|

### 4.2 Advanced Tab – Section-by-Section

#### Sampling Group
- **Temperature slider/numeric** – tunes creativity vs. determinism.
- **Top-p (nucleus) slider/numeric** – narrows candidate token distribution.
- **Top-k slider/numeric** – restricts the candidate shortlist length.

#### Length & Context Group
- **num_predict slider/numeric** – maximum tokens generated in a single reply.
- **num_ctx slider/numeric** – context window size; ensure compatibility with selected model.

#### Anti-repeat Group
- **Repeat penalty slider/numeric** – discourages repetitive loops; values > 1.0 penalize repeats, < 1.0 encourages reuse.

#### Mirostat Group
- **Mode combo box** – Off, V1, or V2.
- **Tau slider/numeric** – target entropy.
- **Eta slider/numeric** – learning rate.

#### System & Stops Group
- **System prompt text box** – persistent instructions given to the model.
- **Stop sequences list** – strings that end the generation when encountered.
- **Add/Remove buttons** – manage the stop list; duplicates are ignored.

#### Determinism Group
- **Seed numeric** – fixed random seed (negative values trigger randomization per run if `Randomize each run` checked).
- **Randomize each run** – when checked and seed < 0, each run uses a different seed.

### 4.3 Command Bar (bottom of Model Tuner)

| Button | Action |
| --- | --- |
| **Reset to defaults** | Restores recommended baseline values across both tabs. |
| **Import…** | Load settings from a JSON file. |
| **Export…** | Save current settings to JSON. |
| **OK** | Apply changes and close the tuner. |
| **Cancel** | Discard changes. |
| **Apply** | Apply changes without closing (useful for iterative experimentation). |

### 4.4 Tooltips & Validation

- Every slider and numeric up/down shows a tooltip describing its effect when hovered.
- ErrorProvider surfaces validation issues (e.g., out-of-range values) with inline visual cues.

---

## 5. Session Persistence & Storage

- **Files**: Saved to `%LOCALAPPDATA%\MyOllamaHub3`.
- **Debounce**: Saves are debounced (600ms) to avoid excessive disk writes during rapid edits.
- **Empty session skipping**: Sessions with no user, assistant, or tool messages are excluded from the persisted JSON.
- **Manual backup**: Copy `sessions.json` to keep an archive. When re-imported, the app normalizes IDs and timestamps as needed.

---

## 6. Warmup Manager

- Located in the background service layer; surfaces status via the main window.
- **Purpose**: Preload selected models to reduce first-response latency.
- **Controls**: Auto warmup toggles can be exposed via Tools menu (future configuration). Warmup progress appears next to the model selector.

---

## 7. Tool Registry & Streaming Updates

Although primarily a user-facing product, developers may integrate tools:
- `ToolRegistry` registers plugins that the assistant can call.
- `StreamingUpdateHelper` manages incremental updates to the UI while streaming responses.

For users, the key takeaway is that tool outputs appear as dedicated blocks in the transcript (with success or error state).

---

## 8. Keyboard & Mouse Shortcuts

| Action | Shortcut |
| --- | --- |
| Send prompt | `Ctrl+Enter` (when focus in prompt box) |
| Cancel streaming | `Esc` |
| Multi-select sessions | `Shift+Click`, `Ctrl+Click` |
| Open Model Tuner | `Alt+T` (if menu accelerators configured) |
| Navigate tabs | `Ctrl+Tab` / `Ctrl+Shift+Tab` |

---

## 9. Exporting & Sharing Conversations

1. Select the desired session(s) from the Sessions tab.
2. Choose `File → Export Session(s)`.
3. For a single session, provide the file name.
4. For multiple sessions, choose a destination folder; the app creates Markdown files (unique names enforced).
5. Each transcript includes user, assistant, system, and tool messages with headings and markdown-friendly formatting.

Transcripts are clean for sharing—no proprietary metadata is included.

---

## 10. Troubleshooting & Diagnostics

| Symptom | Resolution |
| --- | --- |
| Chat view blank | Install/repair WebView2 runtime. |
| Ollama connection error | Verify Ollama service is listening on the configured base URL. |
| Sessions disappeared | Ensure a message was sent; empty sessions are not saved. |
| Build failures due to locked files | Close running `MyOllamaHub3.exe` before rebuilding. |
| Slow first response | Use Warmup Manager or pre-pull the Ollama model. |

**Diagnostics panel tips**: Red entries indicate failures (e.g., session save). Click the panel to scroll through recent events.

---

## 11. Advanced Configuration (Power Users)

- Edit `settings.json` manually for global tweaks. After editing, restart the application.
- The `AppSettings` model supports:
  - Personality preset selection
  - Auto model selection toggle
  - History turn limits
  - Last advanced options snapshot
- Adjust constants (e.g., debounce intervals, default trackbar values) in `Form1.cs` and `ModelTuner.Designer.cs` if recompiling.

---

## 12. Testing & Validation

Run all automated tests:
```powershell
dotnet test
```

Key suites:
- `SessionRecordTests` – ensures transcript formatting and persistence rules.
- `ToolRegistryTests` – validates tool registration behaviors.
- `StreamingUpdateHelperTests` – confirms UI updates remain responsive during streaming.

---

## 13. Support & Feedback

- **Issues**: Submit via GitHub with logs from the Diagnostics panel (if possible).
- **Feature requests**: Include context (workflow, expected outcome). Screenshots of the relevant UI help triage.
- **Contributions**: Fork, branch, and open a PR. Follow project conventions for code style and UI design.

---

## 14. Glossary

| Term | Meaning |
| --- | --- |
| **Ollama** | Local LLM server providing HTTP APIs. |
| **System prompt** | Instruction prepended to every prompt guiding assistant behavior. |
| **Stop sequence** | String that halts generation when encountered. |
| **Warmup** | Preload models to reduce first-response latency. |
| **Session** | Saved conversation (messages + metadata). |

---

Have an idea to improve this guide? Open an issue and tag it “documentation.” MyOllamaHub3 evolves quickly, and detailed feedback keeps the docs relevant.
