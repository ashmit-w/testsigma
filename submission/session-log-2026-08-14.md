# Session log — 2026-08-13 / 2026-08-14

This is a reconstructed summary of the Claude Code session that built the
test-target apps and the first slice of Puppet (`Puppet.Core`,
`Puppet.Cli`), written from memory at the user's request before clearing
context. It is not a verbatim transcript — it's a faithful, chronological
account of what was asked, what was built, what broke, and how it was
fixed, so a future session (or a human) can pick up the thread without
re-deriving any of it. Per `docs/spec.md` section 4.1, this belongs under
`submission/`.

---

## 1. Five WinForms/WPF test target apps (`test-apps/`)

Asked to build five .NET Framework 4.8 desktop apps as UI-automation test
targets, per a detailed spec in the prompt (later formalized into
`docs/spec.md` section 7):

- **LegacyForms** (WinForms) — deliberately hostile to accessibility:
  every control left at its designer-default `Name` (`textBox1`,
  `button3`, ...), no `AccessibleName`/`AccessibleDescription` anywhere.
  MenuStrip/ToolStrip/StatusStrip, an "Employee Details" form over 8
  hardcoded employees, a TabControl (Notes/History), and two
  custom-painted (`OnPaint`) controls (`PaintedButton`, `BarChartPanel`)
  that expose nothing useful to UI Automation beyond `LegacyIAccessible`.
- **MenuDemo** (WPF) — single-Grid, `Visibility`-toggled screen
  navigation with a navigation-stack-driven breadcrumb and per-panel back
  buttons; a root "video shortcut" button and `options → video` both
  reach the same screen via different paths (breadcrumbs differ
  correctly).
- **ControlZoo** (WPF) — one of every common WPF control type in a
  scrollable panel, each wired to a shared `statusText`; two Sliders with
  genuinely different ranges (0–100 vs 1–16).
- **SimpleCalc** (WinForms) — the deliberate contrast case: descriptive
  names + `AccessibleName` on every button, clean accessibility metadata.
- **TaskList** (WPF) — minimal to-do list, code-behind only, no data
  binding, ~85 lines.

Plus `TestTargets.sln`, `build.ps1` (builds all five in Release, copies
exes to `dist/`), and a `README.md`.

**Environment setup**: this sandbox had no .NET SDK at all. Installed
.NET SDK 8.0.424 and the .NET Framework 4.8 Developer Pack via `winget`
(user approved). Built everything — 0 warnings, 0 errors — and smoke-
tested by launching each exe and confirming it stayed alive.

**Bug found and fixed**: `ControlZoo.exe` crashed on startup with a
`NullReferenceException`. Root cause: the two `Slider`s have explicit
`Value="50"`/`Value="4"` in XAML, which fires `ValueChanged` synchronously
*during* `InitializeComponent()`, before the `statusText` field (declared
later in the XAML) had been wired up — so the handler dereferenced a
still-null `statusText`. Fixed by adding an `IsReady` guard
(`statusText != null`) to every handler in
`test-apps/ControlZoo/MainWindow.xaml.cs`.

**Later restructure**: originally scaffolded under
`test-apps/TestTargets/*`; per a follow-up instruction, flattened so the
five app folders + `TestTargets.sln` + `build.ps1` + `README.md` sit
directly under `test-apps/`. Rebuilt and re-verified after the move.

**`test-apps/.gitignore`** added: `dist/`, `bin/`, `obj/` — "only source
code is tracked." Verified clean.

---

## 2. `CLAUDE.md` (`/init`)

Repo already had a hand-written `CLAUDE.md` (project vision/hard rules
for "Puppet") — preserved it entirely rather than regenerating. Added the
required `# CLAUDE.md` preamble, a "Test target apps" section (build
command + the five-app reference table, since `src/` didn't exist yet at
that point), and extended the Verification section with the `test-apps/`
build command.

---

## 3. `Puppet.Cli` — `puppet dump` (Stage 1 of the build order)

Built `src/Puppet.sln` with two projects:

- **`Puppet.Core`** (net8.0-windows) — `FlaUI.Core` + `FlaUI.UIA3` 4.0.0.
  `UiaThread.cs` (dedicated STA thread + async `BlockingCollection`
  queue — "all UI Automation calls run on one dedicated STA thread"),
  `AutomationSession.cs` (owns the `UIA3Automation` instance, only ever
  touched from that thread), `ProcessAttacher.cs`, `TreeDumper.cs`,
  `ElementNode.cs`.
- **`Puppet.Cli`** (net8.0-windows, x64, `AssemblyName=puppet`) —
  `Program.cs`, `DumpCommand.cs`, `DumpWriter.cs`, `SummaryPrinter.cs`.

**Important process note**: FlaUI's exact API (`CacheRequest`,
`Properties`/`Patterns` accessors, `PropertyLibrary`/`PatternLibrary`
shapes) was verified by restoring the real NuGet package and reflecting
over the DLL (`Assembly.Load` + `GetMembers()` in a scratch console app),
rather than trusted from memory — this caught real details (e.g.
`CacheRequest.Add(PropertyId)` needs `automation.PropertyLibrary.Element.X`,
not a `Properties`-namespace type). This reflection-first approach was
repeated for every subsequent FlaUI-touching feature and is worth
continuing.

**Runtime bug found and fixed**: the convenience properties
(`element.ClassName`, `.Name`, etc.) throw when a property isn't
supported by a given element's provider — hit this on a real MenuDemo
element. Fixed by reading everything through
`element.Properties.X.ValueOrDefault` instead (fails gracefully), not
the throwing convenience accessors. This became a standing lesson applied
in every later file that reads element properties.

Command: `puppet dump --process <name> --out <path.json> [--summary]`.
Verified against live `MenuDemo` and `LegacyForms` — both JSON and
`--summary` table modes produce correct, full trees with real
`AutomationId`s and patterns.

### Cross-checked dumps against source, in depth

- `trees/legacyforms.json` vs `test-apps/LegacyForms/*.cs`: matched
  exactly (control names, order, live `IsEnabled` state, employee list).
  One real, expected gap: `tabPage2`/`listView1` (History tab) is
  entirely absent because WinForms doesn't keep an inactive `TabPage`'s
  window realized/exposed to UIA — not a tool bug.
- `trees/menudemo.json` vs `test-apps/MenuDemo/MainWindow.xaml`: matched
  exactly. Two WPF-specific findings worth remembering: (1) layout-only
  `Grid`/`StackPanel` nodes don't appear at all — WPF's default "control
  view" treats them as `IsControlElement=false`, so children of nested
  panels show up as flat siblings of the Window; (2) every button on a
  `Visibility="Collapsed"` panel that was never navigated to shows
  `IsOffscreen: true` and has no nested `Text` child (WPF only realizes a
  `Button.Content`'s auto-generated `TextBlock` peer after a layout pass,
  which never happens for a permanently-collapsed panel). Confirmed with
  the user: **a single dump is a snapshot of whatever screen the app is
  currently on, not a complete model of the app** — this is exactly what
  `docs/spec.md` section 12 names as the top "next step" (a screen model
  + guided exploration).

---

## 4. `ModelBuilder` — `puppet model` (MB-1 through MB-10 + coverage + merge)

Planned via `EnterPlanMode` first (multi-file, architectural). Implemented
in `Puppet.Core`:

- **DTOs** (records, camelCase-serialized to match `docs/spec.md` section
  5's frozen `model.json` schema exactly): `ModelElement`, `ModelDocument`,
  `CoverageReport`, `UnexploredContainer`, `ElementConstraints`.
- **`ElementIdHasher`** — SHA-256 of `automationId|controlType|path`,
  truncated to 8 bytes → `el_<hex>`. Verified deterministic by running
  the build twice and diffing every id (identical, until a process
  restart — see the id-stability note below).
- **`ModelBuilder`** — MB-1 (attach by name or PID) through MB-10. One
  `CacheRequest` batches element properties, the MB-5 pattern list
  (`Invoke, Value, Toggle, RangeValue, SelectionItem, ExpandCollapse,
  LegacyIAccessible`), plus `RangeValue.{Minimum,Maximum,SmallChange}`,
  `ExpandCollapse.ExpandCollapseState`, and
  `LegacyIAccessible.DefaultAction`. `path` built with `Type[n]` indices
  only when siblings share a `ControlType`. `constraints` from
  RangeValue. `defaultAction` only populated when `LegacyIAccessible` is
  the *only* pattern present.
- **Coverage** — `UnselectedTabPage` (TabItem count > realized Pane
  count among children) is detected *structurally* from `path` prefixes,
  so it's re-derivable after a merge with no live tree access.
  `CollapsedNode`/`UnopenedMenu` (TreeItem/MenuItem with
  `ExpandCollapseState == Collapsed`) are inherently live-only (not part
  of the persisted schema), so they're detected during the walk itself.
- **`ModelMerger`** — unions by element id: new → appended; existing →
  only `name`/`isEnabled`/`constraints` refreshed (per instruction);
  elements absent from the fresh walk are always kept ("absence is not
  evidence of non-existence"); coverage recomputed over the merged set.
- **CLI**: `puppet model --process X --out models/x.json [--merge] [--pid n]`.

Verified against live `LegacyForms`: 73 elements, 6 unexplored (1
`UnselectedTabPage` for `tabControl1`, 5 `UnopenedMenu` for
File/Edit/View/Help + the window's own "System" chrome menu — all
matching what was already known from the raw dump). Ran twice
(plain, then `--merge`) — identical element ids, no duplication on a
no-op merge.

---

## 5. Interaction Layer (IL-1 through IL-5) + MB-8

Also planned first. Implemented in `Puppet.Core` (13 flat files):

- **`IInteractionStrategy`** + three tiers:
  - `UiaPatternStrategy` (confidence 3) — UIA control patterns.
  - `LegacyAccessibleStrategy` (confidence 2) — `DoDefaultAction()` only,
    no parameters/return value, so it covers `Invoke`/`Toggle`/
    `SelectIndex` (all "perform this element's one default action") but
    never `SetValue`/`GetValue`.
  - `Win32MessageStrategy` (confidence 1) — exactly the five allowed
    messages (`BM_CLICK`, `BM_SETCHECK`, `WM_SETTEXT`, `WM_GETTEXT`,
    `CB_SETCURSEL`) via P/Invoke `SendMessage` in `NativeMethods.cs`.
    `Toggle` requires an explicit `ActionArgs.TargetState` — `BM_GETCHECK`
    isn't in the allowed set, so Tier 1 genuinely can't read-before-write
    and refuses rather than guessing.
- **`ActionKind`** (`Invoke/Toggle/SetValue/GetValue/SelectIndex`) — one
  per allowed Win32 message. All FlaUI method names (`Invoke()`,
  `Toggle()`, `SetValue(string)`, `Select()`, `DoDefaultAction()`)
  verified via reflection before writing `Execute` bodies.
- **`InteractionResolver`** — IL-2/IL-5: `null` element → `NotFound`;
  disabled → `FoundButDisabled`; strategies tried in descending
  confidence order, first success wins; otherwise `NoMechanismSucceeded`.
- **`MechanismResolver`** — MB-8's resolution is *static*: given an
  element's already-known `patterns` + `nativeHandle` (never invokes
  anything, per "no fallback mechanism may participate in model
  building"). Wired into `ModelBuilder.Walk`, replacing the `null`
  placeholders.

Verified against live `LegacyForms`: same 73 elements/6 unexplored;
`mechanism`/`confidence` now populated everywhere — 48 `UiaPattern`/3, 25
`LegacyIAccessible`/2, 0 `Win32Message`, 0 `null` (expected for WinForms,
where the MSAA bridge is nearly always present). Spot-checked `button1`
(has `Invoke`) → `UiaPattern`/3, and the custom-painted `panel1` (only
`LegacyIAccessible`) → `LegacyIAccessible`/2 — matches the tier table.

**Side-finding, not a bug**: 2 of 73 element ids changed between two runs
because the app process had been restarted in between. Both belong to
`numericUpDown1`'s internal spinner sub-control, whose `AutomationId` is
literally its own stringified `NativeWindowHandle` (a real WinForms
quirk) — HWNDs are reassigned fresh by the OS on every launch, so MB-9's
"same element, same id" only holds for elements with a real
`Control.Name`-based automation id, which this one doesn't have.

---

## 6. `src/.gitignore` fix

Asked to check whether `test-apps/.gitignore` had "the necessary
pieces" — it did. But checking around it surfaced that **`src/` had no
`.gitignore` at all**, and 102 build-artifact files (~3MB: DLLs, `.exe`,
`.pdb`, NuGet/MSBuild caches under `Puppet.Cli`/`Puppet.Core`'s
`bin`/`obj`) had been committed by accident. Fixed: added
`src/.gitignore` (same `bin/`/`obj/` pattern), `git rm --cached` the 102
paths (kept on disk, just untracked), committed
(`6bb300d`, "Stop tracking build output under src/"), and pushed to
`origin/main`. All commits are authored as the user
(`Ashmit Arya <ashmitsoftware@gmail.com>`), no Claude co-author trailer.

---

## 7. In progress / not yet done

**`Puppet.Core/AppSession.cs`** — was mid-plan when interrupted by the
gitignore question above, and has **not been implemented yet**. The ask:

> An `AppSession` owns one target application process and its current
> state — the basis of an interactive, checkpoint-based authoring loop
> (palette derived from whatever state the app is in *right now*, not an
> accumulated model).
>
> - `Start(exePath)`: launch, wait for main window, scan with
>   `ModelBuilder`, return the model.
> - `Reset()`: kill, relaunch, rescan. Returns the fresh model.
> - `Replay(flow)`: kill, relaunch, execute every block in the flow from
>   the top via the interaction layer, then rescan. Returns
>   `{ stepResults[], model }` (model = state *after* the last step).
> - `Current()`: the model from the most recent scan.
> - `Dispose()`: kill the process.
>
> Rules: one session, one process (kill on dispose, and on `Start` if one
> is already running); `Replay` is always from scratch, never continues
> from current state (determinism over speed); all UIA work on the
> existing dedicated STA thread; if the process exits mid-replay, fail
> remaining steps with a clear message rather than throwing; reuse
> `ModelBuilder` unchanged; do not use `ModelMerger` here — each scan
> replaces the session's current model.
>
> Also: `puppet session --exe path\to\MenuDemo.exe` — starts a session,
> prints current palette-relevant elements, and a REPL (type an element
> id to click + rescan, `reset` to restart, `quit` to exit).

Design work done so far (not yet written to any plan file that survived —
the plan file at `C:\Users\snr9r\.claude\plans\twinkly-meandering-wand.md`
was overwritten by the gitignore-fix plan afterward, so this needs
re-planning from scratch next time):

- Needs a minimal `Flow`/`FlowStep`/`StepResult`/`ReplayResult` concept —
  none exists yet (Block Generator / Runner haven't been built). Keep it
  minimal: `FlowStep { ElementId, Action: ActionKind, Text?, TargetState?,
  Index? }`, reusing the existing `ActionKind`/`ActionArgs` types from
  the Interaction Layer rather than inventing a parallel vocabulary.
- `Replay` needs to *re-locate* each step's element in the freshly
  relaunched process (model.json doesn't hold live `AutomationElement`
  references). Plan: an `ElementLocator` that tries `ByAutomationId`
  first, then falls back to walking the structural `path` segments
  (`Type[n]`) from the main window — this is the exact inverse of
  `ModelBuilder`'s child-segment computation, so that logic should be
  extracted into a small shared helper (e.g. `ChildSegmentBuilder`) used
  by both, to guarantee they can't drift apart.
- The REPL's "type an element id to click it and rescan" is a *different,
  lighter* operation than `Replay` — it should act on the *currently
  running* process in place (no kill/relaunch) and rescan, matching the
  "checkpoint-based authoring loop" framing in the ask. This isn't one of
  the five explicitly named lifecycle methods, so it needs an additional
  method (tentatively `Act(elementId, actionKind, args)`) — flag this
  explicitly as an addition beyond the given list when re-planning, so
  the user can confirm or redirect.
- `Application.Launch(...)` overload/parameter order should be verified
  by reflection before use (same reflection-first pattern as everywhere
  else) — only `Launch(string, string)` and `Launch(ProcessStartInfo)`
  were seen in earlier scans, no confirmed single-arg `Launch(string)`.
- Open question worth a quick check-in when resuming: whether `Replay`'s
  final rescan, if the process is dead/unreachable, should return the
  *previous* successfully-scanned model (fallback) rather than throwing —
  reasoned through but not confirmed with the user.

**Next step if resuming this thread**: re-enter plan mode, re-derive the
above (or read this log), and get the plan approved before writing any
`AppSession` code.

---

## Repo state at time of writing

- Branch `main`, pushed through commit `6bb300d` ("Stop tracking build
  output under src/"). Working tree clean.
- `test-apps/` — 5 target apps + `TestTargets.sln` + `build.ps1` +
  `README.md`, `.gitignore` in place.
- `src/` — `Puppet.sln` with `Puppet.Core` (UiaThread, AutomationSession,
  ProcessAttacher, TreeDumper, ElementNode, ModelBuilder + DTOs,
  CoverageDetector, ElementIdHasher, ModelMerger, MechanismResolver,
  MechanismNames, IInteractionStrategy + 3 tiers, ActionKind/ActionArgs/
  InteractionResult/FailureCause, InteractionResolver, NativeMethods) and
  `Puppet.Cli` (Program, DumpCommand, DumpWriter, SummaryPrinter,
  ModelCommand, ModelWriter), `.gitignore` in place.
- `trees/` — `menudemo.json`, `legacyforms.json` (raw dumps).
- `models/` — `legacyforms.json` (model.json with mechanism/confidence
  populated).
- `docs/spec.md` — the frozen spec, sections 1–12.
- `CLAUDE.md` — project rules + test-apps reference table.
