import { useEffect, useRef, useState } from "react";
import { api } from "./api";
import type { PaletteDocument, StepResult } from "./types";
import { StateLine } from "./components/StateLine";
import { ResultsPanel } from "./components/ResultsPanel";
import { Workspace, type WorkspaceHandle } from "./components/Workspace";

export function App() {
  const [palette, setPalette] = useState<PaletteDocument | null>(null);
  const [processId, setProcessId] = useState<number | null>(null);
  const [stepResults, setStepResults] = useState<StepResult[]>([]);
  const [busy, setBusy] = useState<"starting" | "running" | "resetting" | null>("starting");
  const [error, setError] = useState<string | null>(null);
  const workspaceRef = useRef<WorkspaceHandle | null>(null);

  useEffect(() => {
    const exePath = import.meta.env.VITE_EXE_PATH;
    if (!exePath) {
      setError("No target exe configured. Set VITE_EXE_PATH and restart the dev server (or rebuild).");
      setBusy(null);
      return;
    }

    api
      .start(exePath)
      .then((response) => {
        setPalette(response.palette);
        setProcessId(response.processId);
      })
      .catch((err) => setError(String(err)))
      .finally(() => setBusy(null));
  }, []);

  async function handleRun() {
    if (!workspaceRef.current) {
      return;
    }

    const workspace = workspaceRef.current;
    const steps = workspace.serializeFlow();
    setBusy("running");
    setError(null);
    workspace.highlightRunStart();
    try {
      const response = await api.run({ steps });
      setStepResults(response.stepResults);
      setPalette(response.palette);
      setProcessId(response.processId);
      await workspace.playResults(response.stepResults);
    } catch (err) {
      setError(String(err));
    } finally {
      setBusy(null);
    }
  }

  async function handleReset() {
    setBusy("resetting");
    setError(null);
    try {
      const response = await api.reset();
      setPalette(response.palette);
      setProcessId(response.processId);
      // The authored flow on the workspace is intentionally left alone.
    } catch (err) {
      setError(String(err));
    } finally {
      setBusy(null);
    }
  }

  return (
    <div className="app">
      <header className="toolbar">
        <h1>
          {palette?.appTitle ?? "Puppet"}
          {processId !== null && <span className="toolbar-pid">pid {processId}</span>}
        </h1>
        <div className="toolbar-actions">
          <button onClick={handleReset} disabled={busy !== null}>
            Reset
          </button>
          <button onClick={handleRun} disabled={busy !== null || !palette}>
            {busy === "running" ? "Running…" : "Run"}
          </button>
        </div>
      </header>

      {error && <div className="error-banner">{error}</div>}
      {palette && <StateLine coverage={palette.coverage} />}

      <div className="main">
        <Workspace ref={workspaceRef} palette={palette} />
        <ResultsPanel results={stepResults} running={busy === "running"} />
      </div>
    </div>
  );
}
