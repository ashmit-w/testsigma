# TestTargets

Five small .NET Framework 4.8 desktop apps used as UI automation test targets.
All data is hardcoded in memory; there is no file I/O, network, or database
access anywhere in this solution.

| App | Window Title | Process Name | Exercises |
|---|---|---|---|
| LegacyForms | LegacyForms | LegacyForms.exe | A WinForms app with every control left at its designer-default `Name` and no `AccessibleName`/`AccessibleDescription` set anywhere — a worst-case target for UI Automation ID/name resolution, including two custom-painted (`OnPaint`) controls and a modal dialog. |
| MenuDemo | MenuDemo | MenuDemo.exe | A WPF app with single-Grid, visibility-toggled screen navigation, a navigation stack driving back buttons and a breadcrumb, and two different navigation paths reaching the same screen. |
| ControlZoo | ControlZoo | ControlZoo.exe | A WPF app exposing one instance of every common WPF control type (including two sliders with different ranges) inside a single scrollable panel, each wired to a shared status text. |
| SimpleCalc | SimpleCalc | SimpleCalc.exe | A clean WinForms app (descriptive control names and `AccessibleName` on every button) exercising basic arithmetic, chained operations, and a divide-by-zero error state. |
| TaskList | TaskList | TaskList.exe | A minimal WPF CRUD-style list: add/delete/check items and a live "N of M complete" summary, built entirely with code-behind (no data binding). |

## Solution layout

```
test-apps/
  TestTargets.sln
  build.ps1
  README.md
  LegacyForms/
  MenuDemo/
  ControlZoo/
  SimpleCalc/
  TaskList/
  dist/            (created by build.ps1)
```

## Building from scratch

Requires the .NET Framework 4.8 Developer Pack and the `dotnet` CLI
(no Visual Studio needed).

```powershell
# From the repository root:
cd test-apps

# Restore + build every project in Release and collect the executables:
.\build.ps1

# The executables are written to:
#   test-apps\dist\LegacyForms.exe
#   test-apps\dist\MenuDemo.exe
#   test-apps\dist\ControlZoo.exe
#   test-apps\dist\SimpleCalc.exe
#   test-apps\dist\TaskList.exe
```

To build a single project instead of the whole solution:

```powershell
dotnet build LegacyForms\LegacyForms.csproj -c Release
```
