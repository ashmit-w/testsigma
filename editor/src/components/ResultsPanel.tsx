import type { StepResult } from "../types";

export function ResultsPanel({ results, running }: { results: StepResult[]; running: boolean }) {
  return (
    <div className="results-panel">
      <h2>Results</h2>
      {running && <p className="results-running">Running…</p>}
      {!running && results.length === 0 && <p className="results-empty">No run yet.</p>}
      <ol className="results-list">
        {results.map((step, index) => (
          <li key={index} className={`step step-${step.status.toLowerCase()}`}>
            <div className="step-header">
              <span className="step-status">{statusIcon(step.status)}</span>
              <span className="step-description">{step.description}</span>
            </div>
            <div className="step-detail">
              {step.mechanism && <span>{step.mechanism}</span>}
              {step.confidence !== null && <span>confidence {step.confidence}</span>}
              {step.duration && <span>{step.duration}</span>}
            </div>
            {step.message && <div className="step-message">{step.message}</div>}
            {step.failureCause && <div className="step-message">{step.failureCause}</div>}
          </li>
        ))}
      </ol>
    </div>
  );
}

function statusIcon(status: StepResult["status"]): string {
  switch (status) {
    case "Passed":
      return "✓";
    case "Failed":
      return "✗";
    case "Skipped":
      return "–";
  }
}
