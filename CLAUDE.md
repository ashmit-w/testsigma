# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Puppet

A model-driven no-code UI test automation tool for Windows desktop
applications. Full spec in docs/spec.md. Read it before proposing any
architecture change.

## Thesis
The application model is the product. The block palette, block inputs,
and execution all derive from model.json. Nothing in the palette is
hand-written for a specific application.

## Solutions
- src/Puppet.sln is the tool. Build this after every change.
- test-apps/TestTargets.sln is five .NET Framework 4.8 applications used
  as test targets. Do not modify them unless I explicitly ask.
  LegacyForms deliberately has NO accessibility metadata and uses
  designer-default control names (textBox1, button3). That is
  intentional and is the entire point of that app. Never "fix" it.

## Test target apps (test-apps/)
Nothing under src/ exists yet — test-apps/ is the only buildable code in
the repo right now. Build/verify with:
  dotnet build test-apps/TestTargets.sln -c Release
Requires the .NET Framework 4.8 Developer Pack (targets net48, no
Visual Studio needed). test-apps/build.ps1 builds all five and copies
the executables to test-apps/dist/ (gitignored). To build one project:
  dotnet build test-apps/LegacyForms/LegacyForms.csproj -c Release

| App | Window title | Process | Exercises |
|---|---|---|---|
| LegacyForms | LegacyForms | LegacyForms.exe | Hostile-to-accessibility WinForms app - designer-default names only, no AccessibleName/AccessibleDescription anywhere, two custom-painted (OnPaint) controls, a modal About dialog. |
| MenuDemo | MenuDemo | MenuDemo.exe | WPF, single-Grid visibility-toggled screen navigation with a nav-stack-driven breadcrumb and back buttons; two different paths reach the same screen. |
| ControlZoo | ControlZoo | ControlZoo.exe | WPF, one instance of every common control type in a scrollable panel, each with an AutomationId, wired to a shared status text. |
| SimpleCalc | SimpleCalc | SimpleCalc.exe | Clean WinForms calculator - descriptive names and AccessibleName on every button, the deliberate contrast case to LegacyForms. |
| TaskList | TaskList | TaskList.exe | Minimal WPF to-do list, code-behind only, no data binding. |

All five target net48, use no NuGet packages, and hold all data
hardcoded in memory (no file I/O, network, or database).

## Stack (do not change without asking)
- C#, .NET 8 for the tool. Targets under test are .NET Framework 4.8.
- FlaUI (UIA3) for automation. Never UIA2 / System.Windows.Automation.
- ASP.NET Core minimal API host, static-file editor, WebSocket for runs.
- React + Blockly for the editor. Blockly version pinned in package.json.
- JSON files on disk. No database.
- Puppet.Cli and Puppet.Host build as x64 so they can automate 64-bit
  targets.

## Hard rules
- All UI Automation calls run on ONE dedicated STA thread owned by the
  automation layer. Never call UIA from a request handler thread.
- Discovery is ALWAYS UIA. Only execution falls back to LegacyIAccessible
  or Win32 messages. No fallback mechanism may participate in building
  model.json.
- Never use Thread.Sleep or Task.Delay as a wait. All waiting goes through
  the bounded polling helper in Puppet.Core/Waits.cs.
- No image recognition, ever. No OCR, no pixel matching, no screen scraping.
- The block generator is a pure function of model.json. It must contain no
  application-specific special cases. This is the falsifiable claim of the
  whole project.

## Real tree dumps
trees/*.json are real UIA dumps of the target applications. Read the
relevant one before writing any element-handling code. Do not guess at
tree structure.

## model.json
The schema in docs/spec.md section 5 is frozen. If you think it needs to
change, stop and ask me first.

## Style
- Small files, one class per file.
- Explicit over clever. This code will be explained live in an interview.
- Comment only non-obvious decisions, never restate the code.

## Verification
After any change under src/, run:
  dotnet build src/Puppet.sln
After any change under test-apps/, run:
  dotnet build test-apps/TestTargets.sln -c Release
Fix all errors before reporting done. Never report success on a
non-compiling change.