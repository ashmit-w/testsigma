import * as Blockly from "blockly";
import type { FlowStepRequest, PuppetBlockMeta } from "../types";

/**
 * Walks every stack of statement blocks in the workspace, top to bottom
 * and left to right, and turns each block into a FlowStepRequest using
 * the metadata registerPaletteBlocks recorded for its type. Blocks whose
 * type isn't in metaByType (shouldn't happen - every block on the
 * workspace came from a registered palette block) are skipped rather
 * than crashing the run.
 */
export function serializeFlow(
  workspace: Blockly.WorkspaceSvg,
  metaByType: Map<string, PuppetBlockMeta>,
): FlowStepRequest[] {
  const steps: FlowStepRequest[] = [];

  const topBlocks = workspace.getTopBlocks(true);
  for (const topBlock of topBlocks) {
    let block: Blockly.Block | null = topBlock;
    while (block !== null) {
      const step = toFlowStep(block, metaByType);
      if (step !== null) {
        steps.push(step);
      }

      block = block.getNextBlock();
    }
  }

  return steps;
}

function toFlowStep(block: Blockly.Block, metaByType: Map<string, PuppetBlockMeta>): FlowStepRequest | null {
  const meta = metaByType.get(block.type);
  if (!meta) {
    return null;
  }

  const args: FlowStepRequest["args"] = {};

  const text = fieldValue(block, "TEXT");
  if (text !== null) {
    args.text = text;
  }

  const value = fieldValue(block, "VALUE");
  if (value !== null) {
    args.value = Number(value);
  }

  if (meta.targetState !== null) {
    args.targetState = meta.targetState;
  }

  if (meta.clear) {
    args.text = "";
  }

  return {
    description: describe(block),
    elementId: meta.elementId,
    automationId: meta.automationId,
    controlType: meta.controlType,
    path: meta.path,
    action: meta.action,
    args,
  };
}

function fieldValue(block: Blockly.Block, name: string): string | null {
  const field = block.getField(name);
  return field ? field.getValue() : null;
}

function describe(block: Blockly.Block): string {
  return block.toString();
}
