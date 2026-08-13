import type { CoverageReport } from "../types";

export function StateLine({ coverage }: { coverage: CoverageReport }) {
  const unexploredCount = coverage.unexplored.length;

  return (
    <div className="state-line">
      <span>{coverage.elementCount} elements</span>
      {unexploredCount > 0 && (
        <span className="state-line-warning">
          {unexploredCount} container{unexploredCount === 1 ? "" : "s"} not yet opened
        </span>
      )}
    </div>
  );
}
