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

export interface ModelElement {
  id: string;
  automationId: string | null;
  name: string | null;
  controlType: string;
  path: string[];
  nativeHandle: number;
  patterns: string[];
  defaultAction: string | null;
  mechanism: string | null;
  confidence: number | null;
  constraints: { minimum: number; maximum: number; step: number } | null;
  isEnabled: boolean;
}

export interface ModelDocument {
  schemaVersion: number;
  appId: string;
  appTitle: string;
  processName: string;
  builtAt: string;
  elements: ModelElement[];
  coverage: CoverageReport;
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

export type StepStatus = "Passed" | "Failed" | "Skipped";

export interface StepResult {
  description: string;
  status: StepStatus;
  duration: string;
  mechanism: string | null;
  confidence: number | null;
  failureCause: string | null;
  message: string | null;
}

export interface StartResponse {
  palette: PaletteDocument;
  model: ModelDocument;
}

export interface ResetResponse {
  palette: PaletteDocument;
  model: ModelDocument;
}

export interface RunResponse {
  stepResults: StepResult[];
  palette: PaletteDocument;
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
