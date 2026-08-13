# Puppet

A model-driven no-code UI test automation tool for Windows desktop applications, targeting .NET Framework apps that resist the usual accessibility shortcuts.

Full design: [docs/spec.md](docs/spec.md). Journey documentation (how this was actually built, session by session): [Notion — TestSigma Backend Assignment Journey](https://app.notion.com/p/TestSigma-Backend-Assignment-Journey-Documentation-3ba18c660fd880afbcb2f57c78859eff?source=copy_link).

## 1. What I understood the problem to be

The brief reads as two asks — a no-code automation tool, and a way to identify and drive legacy Win32 controls without image recognition — but I treated them as one system. A no-code tool is only as good as its ability to find and operate controls; a control-interaction layer with no authoring surface isn't a tool. Building them as separate deliverables would mean building discovery twice.

The reference point is Tricentis Tosca's Module concept: scan an application once into a reusable description of its steerable controls, then assemble tests from that description instead of from raw selectors. Puppet adopts the same separation of application knowledge from test logic — a scanned model drives a generated, Scratch-style block palette in place of Tosca's grid.

## 2. What I built, and what I deliberately left out

**Built:** runtime inspection of a live process into `model.json`; a three-tier interaction layer (UIA patterns, then the MSAA/LegacyIAccessible bridge, then raw Win32 messages) that reports which tier actually fired and at what confidence; a block generator that turns that model into a Blockly palette with no per-app logic; a checkpoint-based editor where the palette always reflects the target's current live state; and a replay runner with pass/fail/skip reporting and AS-1–AS-4 assertions.

**Left out, on purpose:**
- **Navigation graph and screen model** — the largest cut; would enable intent-level navigation and a context-aware palette, but is a day of work by itself.
- **Assisted crawling** — depends on the navigation graph above.
- **Composite reusable blocks** — editor convenience, no bearing on the core thesis.
- **Record-by-observation authoring** — UIA event capture and noise filtering are substantial for a usability win, not an architectural one.
- **Self-healing selectors** — valuable and orthogonal, but out of scope for a one-day build.
- **Event-based waits** — a correct version races UIA events against polling; polling alone is sufficient here.
- **Isolated/parallel execution** — unnecessary since the interaction layer never seizes the desktop, so runs don't need exclusive control of the machine.
- **Visual regression** — image-based verification is exactly what the brief rules out.
- **In-process injection** — the highest-capability answer for owner-drawn controls, but also the most invasive (AV interference, crash risk); Tier 1 covers the demonstrable subset.

## 3. Assumptions

- Targets are built on .NET Framework 4.8 per the brief; Puppet itself runs on .NET 8, since UI Automation is an OS API and the tool isn't bound by the target's runtime.
- Puppet **launches** the target process rather than attaching to one already running, because checkpoint reset requires owning the process lifecycle.
- Replay is always from a clean launch — a flow never continues from wherever the app happens to be, so determinism wins over speed.
- Targets run unelevated; automating an elevated process needs a matching integrity level, which is noted but not handled.
- "No-code" means visual composition with no text syntax, hence a block editor rather than a keyword-driven grid.

## 4. What I would do next

In order, each unlocking the next: derive a **navigation graph** from the checkpoints already produced during authoring; use it for **intent-level navigation** (`go to <screen>`, resolved at execution time so a flow survives menu restructuring); use that to build a **context-sensitive palette** that dims blocks unreachable from the current screen; add **self-healing selectors** (AutomationId, then name+type, then path, with a confidence score and a warning when a fallback wins); and finally **event-based waits** racing UIA events against the existing polling loop.

## Running it

```powershell
# 1. Build the five target applications
dotnet build test-apps/TestTargets.sln -c Release

# 2. Build the tool (Puppet.Core, Puppet.Cli, Puppet.Host)
dotnet build src/Puppet.sln

# 3. Build the editor (VITE_EXE_PATH is baked in at build time - the exe
#    the editor starts a session against on load)
cd editor
npm install
$env:VITE_EXE_PATH = "C:\testsigma\test-apps\dist\MenuDemo.exe"
npm run build
cd ..

# 4. Start the host (serves the editor and the /session/* API on one origin)
dotnet run --project src/Puppet.Host --urls http://localhost:5100
```

5. Open `http://localhost:5100` in a browser.
