// Mirrors Puppet.Core's camelCase-serialised JSON shapes
// (ModelDocument, PaletteDocument, StepResult - see src/Puppet.Core).

export interface UnexploredContainer {
  containerId: string;
  containerType: string;
  reason: string;
}

export interface CoverageReport {
  elementCount: number;
  unexplored: UnexploredContainer[];
}

export interface PuppetBlockMeta {
  elementId: string;
  automationId: string | null;
  controlType: string;
  path: string[];
  action: string;
  assertKind: string | null;
  targetState: boolean | null;
  clear: boolean | null;
  mechanism: string | null;
  confidence: number | null;
  lowConfidence: boolean;
}

// A Blockly JSON block definition (message0/args0/colour/...), plus the
// puppet-specific metadata BlockGenerator attaches to it.
export interface PaletteBlock {
  type: string;
  message0: string;
  args0: Record<string, unknown>[];
  previousStatement: unknown;
  nextStatement: unknown;
  colour: number;
  tooltip: string | null;
  puppet: PuppetBlockMeta;
}

export interface PaletteToolboxEntry {
  kind: string;
  type: string;
}

export interface PaletteToolbox {
  kind: string;
  contents: PaletteToolboxEntry[];
}

export interface PaletteDocument {
  appId: string;
  appTitle: string;
  modelBuiltAt: string;
  coverage: CoverageReport;
  blocks: PaletteBlock[];
  toolbox: PaletteToolbox;
}

// Puppet.Host serialises StepStatus with a camelCase string enum
// converter, so these come over the wire lowercase.
export type StepStatus = "passed" | "failed" | "skipped";

export interface StepResult {
  description: string;
  status: StepStatus;
  durationMs: number;
  mechanism: string | null;
  confidence: number | null;
  failureCause: string | null;
  message: string | null;
}

export interface StartResponse {
  palette: PaletteDocument;
  coverage: CoverageReport;
  processId: number | null;
}

export interface ResetResponse {
  palette: PaletteDocument;
  coverage: CoverageReport;
  processId: number | null;
}

export interface RunResponse {
  stepResults: StepResult[];
  palette: PaletteDocument;
  coverage: CoverageReport;
  processId: number | null;
}

// What the client sends to POST /session/run: the workspace flow,
// serialised as one entry per statement block, in execution order. Each
// step carries its own locator (automationId, falling back to path) taken
// straight from the block's puppet metadata, since resolution happens
// against the live tree at execution time - never through a model lookup.
export interface FlowStepRequest {
  description: string;
  elementId: string;
  automationId: string | null;
  controlType: string;
  path: string[];
  action: string;
  args: {
    text?: string;
    targetState?: boolean;
    value?: number;
    index?: number;
  };
}

export interface RunRequest {
  steps: FlowStepRequest[];
}
