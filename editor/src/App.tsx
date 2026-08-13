import { useEffect, useRef, useState } from "react";
import { api } from "./api";
import type { PaletteDocument, StepResult } from "./types";
import { StateLine } from "./components/StateLine";
import { ResultsPanel } from "./components/ResultsPanel";
import { Workspace, type WorkspaceHandle } from "./components/Workspace";

export function App() {
  const [palette, setPalette] = useState<PaletteDocument | null>(null);
  const [stepResults, setStepResults] = useState<StepResult[]>([]);
  const [busy, setBusy] = useState<"starting" | "running" | "resetting" | null>("starting");
  const [error, setError] = useState<string | null>(null);
  const workspaceRef = useRef<WorkspaceHandle | null>(null);

  useEffect(() => {
    api
      .start()
      .then((response) => setPalette(response.palette))
      .catch((err) => setError(String(err)))
      .finally(() => setBusy(null));
  }, []);

  async function handleRun() {
    if (!workspaceRef.current) {
      return;
    }

    const steps = workspaceRef.current.serializeFlow();
    setBusy("running");
    setError(null);
    try {
      const response = await api.run({ steps });
      setStepResults(response.stepResults);
      setPalette(response.palette);
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
        <h1>{palette?.appTitle ?? "Puppet"}</h1>
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
