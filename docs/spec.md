# Puppet

**A model-driven no-code test automation tool for Windows desktop applications**

Specification v2.1, one-day build scope

---

## 1. Interpretation of the brief

The assignment asks for two things: a no-code automation tool for .NET Framework desktop UI applications, and a way to identify and interact with legacy Win32 controls without image-based recognition.

These are treated here as one system rather than two deliverables. The second item is the interaction layer that sits underneath the first. A no-code tool is only as good as its ability to find and drive controls, and a control-interaction library with no authoring surface is not a tool. Building them separately would duplicate the discovery mechanism.

**Reference reading.** Tricentis Tosca separates application knowledge from test logic: an application is scanned once into reusable Modules describing its steerable controls, and tests are then assembled from those Modules rather than from raw selectors. Puppet adopts that separation as its core idea, with the scanned model driving a Scratch-style block palette instead of Tosca's grid-based authoring.

---

## 2. Thesis

The application model is the product.

Puppet inspects a running application, produces a structural model of it, and generates everything else from that model: the available blocks, the values those blocks accept, and how they execute. Nothing in the block palette is written by hand for a specific application.

**Falsifiable form:** pointing Puppet at an application not used during development must produce a working palette with no code changes.

---

## 3. Scope

### 3.1 In scope

- Windows desktop applications on .NET Framework 4.8, WPF and WinForms
- Runtime inspection producing a serialised application model
- Automatic block palette generation from that model
- Browser-based block editor
- Sequential execution against the live application with per-step results
- Three-tier interaction strategy covering modern and legacy controls

### 3.2 Out of scope

Listed with reasons in section 9. That section is written deliberately rather than as an afterthought.

---

## 4. Architecture

```
              Target application (.NET Framework)
                          |
                 [ UI Automation API ]
                          |
        +-----------------+-----------------+
        |                                   |
   Model Builder                    Interaction Layer
        |                                   |
        +----------> model.json <-----------+
                         |
              +----------+----------+
              |                     |
       Block Generator         Test Runner
              |                     |
        palette.json          result stream
              |                     |
              +----------+----------+
                         |
                  Editor (browser)
```

### 4.1 Repository layout

```
/
  CLAUDE.md
  README.md
  docs/spec.md
  src/
    Puppet.sln
    Puppet.Core/        model builder, interaction layer, block generator, waits
    Puppet.Cli/         console entry point: dump, model, palette
    Puppet.Host/        ASP.NET Core host, test runner, WebSocket
  editor/               React + Blockly
  trees/                real UIA tree dumps of the target applications
  test-apps/            five .NET Framework 4.8 target applications
  submission/           prompt log and packaged deliverables
```

### 4.2 Process model

A single .NET 8 host process serves the editor as static files over HTTP and streams run events over WebSocket. The runner sits behind an `ITestRunner` interface, and everything crossing that interface is serialisable, so it could later move to a separate process or machine. That relocation is not performed here.

### 4.3 Technology

| Layer | Choice | Reason |
|---|---|---|
| Automation | FlaUI (UIA3) | Managed wrapper over UI Automation with full pattern coverage. UIA2 omits several patterns |
| Host | C#, .NET 8, ASP.NET Core minimal API | Targets run on .NET Framework; Puppet itself has no such constraint |
| Editor | React, Blockly (version pinned) | Blockly provides custom block definition from JSON and workspace serialisation, which is precisely the generated-palette requirement |
| Persistence | JSON files | No database is warranted at this scale |

`Puppet.Cli` and `Puppet.Host` build as x64 so they can automate 64-bit target processes.

### 4.4 Threading

All UI Automation calls execute on a dedicated STA thread owned by the automation layer. Callers reach it through an async queue. Calling UIA from a request handler thread deadlocks.

---

## 5. Data model

`model.json` is the interface between every component and is frozen before implementation begins.

```json
{
  "schemaVersion": 1,
  "appId": "menudemo",
  "appTitle": "MenuDemo",
  "processName": "MenuDemo",
  "builtAt": "2026-08-13T10:00:00Z",

  "elements": [
    {
      "id": "el_003",
      "automationId": "optionsBtn",
      "name": "options",
      "controlType": "Button",
      "path": ["Window", "Grid", "StackPanel", "Button[1]"],
      "nativeHandle": 657234,
      "patterns": ["Invoke"],
      "defaultAction": null,
      "mechanism": "UiaPattern",
      "confidence": 3,
      "constraints": null,
      "isEnabled": true
    },
    {
      "id": "el_022",
      "automationId": null,
      "name": "Save Draft",
      "controlType": "Pane",
      "path": ["Window", "Panel", "Pane[2]"],
      "nativeHandle": 331902,
      "patterns": ["LegacyIAccessible"],
      "defaultAction": "Press",
      "mechanism": "LegacyIAccessible",
      "confidence": 2,
      "constraints": null,
      "isEnabled": true
    }
  ]
}
```

Field notes:

- `mechanism` and `confidence` are resolved at build time and recorded. They are advisory. The runner re-resolves at execution time and reports what it actually used, so a mismatch is visible rather than silent.
- `constraints` carries minimum, maximum, and step where the control pattern exposes them. This is what allows a generated block to reject an out-of-range value at authoring time.
- `path` is a fallback locator, used only when `automationId` is absent or fails to resolve.
- `nativeHandle` is captured from `NativeWindowHandle` so the Win32 tier needs no separate window enumeration.

---

## 6. Components

### 6.1 Model Builder

Produces `model.json` from a running target.

| ID | Requirement |
|---|---|
| MB-1 | Attach to a target by process name or PID |
| MB-2 | Walk the UI Automation tree across all descendants of the target's main window |
| MB-3 | Batch property reads with a `CacheRequest` rather than reading per property |
| MB-4 | Capture per element: AutomationId, Name, ControlType, native handle, enabled state, structural path |
| MB-5 | Detect supported patterns: Invoke, Value, Toggle, RangeValue, SelectionItem, ExpandCollapse, LegacyIAccessible |
| MB-6 | Extract constraints from RangeValue where present |
| MB-7 | Read the LegacyIAccessible `DefaultAction` string where no richer pattern exists |
| MB-8 | Resolve mechanism and confidence per section 6.2 |
| MB-9 | Assign each element a deterministic id, hashed from AutomationId, control type, and structural path |
| MB-10 | Serialise to human-readable, diffable JSON |

Performance target: a walk of roughly 200 elements completes in under 3 seconds.

### 6.2 Interaction Layer

Performs an action on an element and reports which mechanism succeeded.

**Discovery is always UI Automation. Only execution falls back.** This is the central technical decision of the project. Win32 message handling and in-process injection can drive a control you already know about, but neither can answer "what can this control do?" in a general way. Since the palette is generated from that question, no fallback mechanism may participate in model building.

| Tier | Confidence | Mechanism | Applies when |
|---|---|---|---|
| 3 | 3 | UIA control patterns: Invoke, Value, Toggle, RangeValue, SelectionItem, ExpandCollapse | Element exposes a usable pattern. Covers WPF fully and most Win32 common controls via the built-in HWND provider |
| 2 | 2 | `LegacyIAccessible.DoDefaultAction()`, with `DefaultAction` read for capability inference | Element exposes no usable pattern but is reachable through the MSAA bridge. This is the primary answer to legacy Win32 |
| 1 | 1 | Win32 window messages against the captured HWND | Both tiers above fail |

Tier 1 message set is deliberately minimal: `BM_CLICK` and `BM_SETCHECK` for buttons, `WM_SETTEXT` and `WM_GETTEXT` for edits, `CB_SETCURSEL` for combo boxes. Messages requiring a struct marshalled into the target's address space are excluded.

| ID | Requirement |
|---|---|
| IL-1 | A single `IInteractionStrategy` interface, implemented once per tier |
| IL-2 | Resolve strategies in descending confidence order, first success wins |
| IL-3 | Report mechanism and confidence with every performed action |
| IL-4 | Prefer pattern invocation over synthetic input, so execution requires neither foreground focus nor exclusive control of the cursor |
| IL-5 | Distinguish three failure causes in reporting: element not found, element found but disabled, element found but no mechanism succeeded |

IL-4 has a consequence worth stating: because Puppet never seizes the mouse or keyboard, tests run while the machine is in use, and an isolated execution environment is not required for this version.

### 6.3 Block Generator

A pure function of the model. No running application required.

| Detected capability | Blocks emitted |
|---|---|
| Invoke | `click X` |
| Value | `type T into X`, `clear X`, `expect X has text T` |
| Toggle | `set X checked`, `set X unchecked`, `expect X is checked` |
| RangeValue | `set X to N`, bounded by that element's own min and max |
| SelectionItem | `select X`, `expect X is selected` |
| ExpandCollapse | `expand X`, `collapse X` |
| LegacyIAccessible, DefaultAction "Press" | `activate X`, marked low confidence |
| LegacyIAccessible, DefaultAction "Check" | `toggle X`, marked low confidence |

| ID | Requirement |
|---|---|
| BG-1 | Emit blocks strictly per the rule table, with no application-specific special cases |
| BG-2 | Bound block inputs using each element's own constraints |
| BG-3 | Mark blocks derived from confidence 1 or 2 elements with a visible reliability indicator |

BG-1 is the falsifiable form of the thesis in section 2. It is verified by acceptance criterion 2.

### 6.4 Editor

| ID | Requirement |
|---|---|
| ED-1 | Blockly workspace, vertical block stacking |
| ED-2 | Palette loaded from the generated toolbox |
| ED-3 | Save and load flows as JSON |
| ED-4 | Run, with live per-step status |

Deliberately minimal. Palette search, grouping polish, and composite blocks are excluded.

### 6.5 Test Runner

| ID | Requirement |
|---|---|
| TR-1 | Execute blocks sequentially, delegating interaction to the Interaction Layer |
| TR-2 | Stream a result per step as it completes |
| TR-3 | Apply an implicit wait before every interaction: wait until the element exists and is enabled, bounded, default 5 seconds |
| TR-4 | Implement waits as a bounded polling loop |
| TR-5 | Stop on first failure |
| TR-6 | Record per step: description, status, duration, mechanism used, confidence |

**Note on TR-4.** UIA event subscriptions would be the more elegant implementation, but they are cross-process COM callbacks that are delayed or dropped under load, so a correct implementation races events against a polling fallback. At this scope the polling loop alone is used, and the event-based upgrade is deferred.

### 6.6 Assertions

| ID | Assertion |
|---|---|
| AS-1 | Element exists / does not exist |
| AS-2 | Text equals / contains |
| AS-3 | Element is enabled / is disabled |
| AS-4 | Element is checked / is unchecked |

### 6.7 Run view

| ID | Requirement |
|---|---|
| RV-1 | Highlight the executing block, coloured green or red on completion |
| RV-2 | Display per step the mechanism used and its confidence |
| RV-3 | On failure, display the failure cause per IL-5 |
| RV-4 | Show a run summary with pass and fail counts |

RV-2 exists so that the tiered interaction design is visible in output rather than merely claimed.

---

## 7. Target applications

Five applications were built as test targets, all on .NET Framework 4.8. One is complex; four are deliberately small. Each exists to stress a specific claim.

| Application | Framework | Purpose |
|---|---|---|
| **LegacyForms** | WinForms | The complex target and the primary evidence for the second half of the brief. A deliberately dated line-of-business dialog: designer-default control names, no accessibility metadata, a classic MenuStrip, a ToolStrip, a modal About dialog, a TabControl, a ListView, and two owner-drawn panels that expose nothing useful to UI Automation. A working palette must still be generated |
| **MenuDemo** | WPF | The happy path. Nested menus three levels deep, every control carrying an explicit AutomationId. Any failure here is a defect in Puppet, not in the target |
| **ControlZoo** | WPF | Pattern coverage breadth. One of every common control type on a single scrollable panel, including two sliders with deliberately different value ranges, to verify per-element constraint extraction |
| **SimpleCalc** | WinForms | A clean WinForms application with proper accessible names, in contrast to LegacyForms. Demonstrates that the framework is not the variable; the accessibility metadata is |
| **TaskList** | WPF | Withheld during development. Used only at demonstration, as live evidence for BG-1 |

Real UIA tree dumps of the first four are committed under `trees/` and are read before any element-handling code is written. TaskList is deliberately not dumped.

---

## 8. Build order

| Stage | Deliverable | Hours |
|---|---|---|
| 0 | Five target applications | 2 |
| 1 | `puppet dump`: console tree walk to JSON | 1 |
| 2 | Model Builder producing `model.json` | 1.5 |
| 3 | Interaction Layer, three tiers | 1.5 |
| 4 | Block Generator producing a palette | 1 |
| 5 | Editor, minimal | 1.5 |
| 6 | Runner, waits, assertions | 1.5 |
| 7 | Run view and mechanism reporting | 1 |

Stage 1 exists to surface tree structure surprises before any design depends on them, and to give the AI coding tool a real tree to read rather than an imagined one. It is not optional.

---

## 9. Deliberately excluded

Each item below was specified, considered, and cut. The reason matters more than the omission.

| Excluded | Reason |
|---|---|
| **Navigation graph and screen model** | The largest cut. Screen fingerprinting plus guided exploration would enable intent-level navigation (`go to "options / audio"` resolving to a click path at execution time) and context-sensitive palette filtering. This is the strongest remaining idea and the first thing to build next, but it is a day of work by itself and Puppet is coherent without it |
| Assisted crawling | Depends on the navigation graph. Also needs cycle detection, a back-edge strategy, and a destructive-action blacklist |
| Composite reusable blocks | Editor work with no bearing on the thesis |
| Record-by-observation authoring | UIA event subscription and noise filtering are substantial, and the value is usability rather than architecture |
| Self-healing selectors | Genuinely valuable and orthogonal. Would resolve elements by AutomationId, then name plus type, then path, with a confidence score and a warning when a fallback wins |
| Event-based waits | A correct implementation races UIA events against polling. Polling alone is sufficient at this scope |
| Isolated or virtualised execution | Unnecessary because IL-4 removes the need for exclusive desktop control. Would become necessary for clean-state reset and parallel runs |
| Parallel execution, CI integration, scheduling | Product concerns rather than design concerns |
| Visual regression comparison | Image-based verification is explicitly rejected by the brief and by this design |
| In-process injection for custom-drawn controls | Highest capability against owner-drawn controls, and the approach commercial tools use. Also the most invasive: antivirus interference, crash risk, and version coupling. Tier 1 covers the demonstrable subset |
| Failure screenshots and UI tree capture | Diagnostic polish. Cheap to add, no architectural content |

---

## 10. Assumptions

1. The brief specifies .NET Framework, so all five targets are built on 4.8. Puppet itself runs on .NET 8, since UI Automation is an OS-level API and the tool's runtime is unconstrained by the target's.
2. The two numbered items in the brief describe one system, with item 2 as the interaction layer beneath item 1.
3. Applications under test run unelevated. Automating an elevated process requires a matching integrity level, which is noted but not handled.
4. Targets are single-window during a test run. A modal dialog exists in LegacyForms but is not required to be driven.
5. "No-code" is taken to mean visual composition with no text syntax, hence a block editor rather than a keyword-driven grid.

---

## 11. Acceptance criteria

1. A model is built for the four development targets with no manual editing of `model.json`.
2. A palette is generated for TaskList, which was not used during development, producing correct usable blocks with no code changes.
3. For LegacyForms, a working palette is produced despite elements resolving at confidence 1 and 2, and the run view reports the mechanism used per step.
4. The owner-drawn panels in LegacyForms are driven successfully via Tier 2 or Tier 1.
5. The two sliders in ControlZoo produce blocks carrying their own distinct ranges, not a hardcoded one.
6. A flow authored against MenuDemo executes end to end with per-step pass and fail reporting.
7. An interaction with a disabled control fails with a message distinguishing disabled from not found.
8. No step in any passing flow relies on a fixed-duration sleep.
9. Building the model twice against the same application produces identical element ids.

---

## 12. Next steps with more time

In priority order:

1. **Screen model and navigation graph.** Fingerprint screens by their visible interactive elements, deliberately excluding values so that changing content does not register as a new screen. Discover transitions by guided exploration. This unlocks the next two items and is the single highest-value addition.
2. **Intent-level navigation.** `go to <screen>` resolved by breadth-first search at execution time, so a flow survives restructuring of the target's menus. Report the resolved path in the run output.
3. **Context-sensitive palette.** Track the simulated current screen while authoring and dim blocks unreachable from it.
4. **Self-healing element resolution** with confidence-scored fallbacks and a warning when a fallback wins over the primary locator.
5. **Event-based synchronisation** racing UIA events against the existing polling loop.
6. **Runner relocation** to a separate process, which the existing interface already permits.
