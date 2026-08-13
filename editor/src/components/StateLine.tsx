import type { CoverageReport } from "../types";

// Matches the Reason strings ModelBuilder/CoverageDetector emit
// (see src/Puppet.Core/ModelBuilder.cs, CoverageDetector.cs).
const REASON_LABELS: Record<string, string> = {
  UnopenedMenu: "unopened menus",
  CollapsedNode: "collapsed nodes",
  UnselectedTabPage: "unselected tabs",
};

export function StateLine({ coverage }: { coverage: CoverageReport }) {
  const unexploredCount = coverage.unexplored.length;
  const reasons = [...new Set(coverage.unexplored.map((u) => REASON_LABELS[u.reason] ?? u.reason))];

  return (
    <div className="state-line">
      <span>{coverage.elementCount} elements</span>
      {unexploredCount > 0 && (
        <span className="state-line-warning">
          {unexploredCount} container{unexploredCount === 1 ? "" : "s"} not yet opened ({reasons.join(", ")})
        </span>
      )}
    </div>
  );
}
