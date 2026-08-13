import type { StepResult } from "../types";

export function ResultsPanel({ results, running }: { results: StepResult[]; running: boolean }) {
  const passed = results.filter((r) => r.status === "passed").length;
  const failed = results.filter((r) => r.status === "failed").length;
  const skipped = results.filter((r) => r.status === "skipped").length;

  return (
    <div className="results-panel">
      <h2>Results</h2>
      {running && <p className="results-running">Running…</p>}
      {!running && results.length === 0 && <p className="results-empty">No run yet.</p>}
      {results.length > 0 && (
        <p className="results-summary">
          {passed} passed, {failed} failed, {skipped} skipped
        </p>
      )}
      <ol className="results-list">
        {results.map((step, index) => (
          <li key={index} className={`step step-${step.status}`}>
            <div className="step-header">
              <span className="step-status">{statusIcon(step.status)}</span>
              <span className="step-description">{step.description}</span>
            </div>
            <div className="step-detail">
              {step.mechanism && <span>{step.mechanism}</span>}
              {step.confidence !== null && <span>confidence {step.confidence}</span>}
              <span>{Math.round(step.durationMs)} ms</span>
            </div>
            {step.message && <div className="step-message">{step.message}</div>}
            {step.status === "failed" && step.failureCause && (
              <div className="step-message">{describeFailureCause(step.failureCause)}</div>
            )}
          </li>
        ))}
      </ol>
    </div>
  );
}

function statusIcon(status: StepResult["status"]): string {
  switch (status) {
    case "passed":
      return "✓";
    case "failed":
      return "✗";
    case "skipped":
      return "–";
  }
}

// Puppet.Core.FailureCause enum member names, in plain language.
const FAILURE_CAUSE_LABELS: Record<string, string> = {
  NotFound: "control not found",
  FoundButDisabled: "control found but disabled",
  NoMechanismSucceeded: "control found but could not be driven",
};

function describeFailureCause(failureCause: string): string {
  return FAILURE_CAUSE_LABELS[failureCause] ?? failureCause;
}
