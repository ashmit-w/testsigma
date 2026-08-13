import * as Blockly from "blockly";
import type { FlowStepRequest, PuppetBlockMeta } from "../types";

/**
 * Every statement block in the workspace, in the same top-to-bottom,
 * stack-by-stack order execution follows. Both serializeFlow and the run
 * animation (Workspace.tsx) walk in this order, so a step result at
 * index i always corresponds to the block at index i here.
 */
export function walkBlocksInOrder(workspace: Blockly.WorkspaceSvg): Blockly.Block[] {
  const blocks: Blockly.Block[] = [];

  const topBlocks = workspace.getTopBlocks(true);
  for (const topBlock of topBlocks) {
    let block: Blockly.Block | null = topBlock;
    while (block !== null) {
      blocks.push(block);
      block = block.getNextBlock();
    }
  }

  return blocks;
}

/**
 * The subset of walkBlocksInOrder that actually produces a step (i.e. has
 * registered puppet metadata). This is the list the run animation must
 * zip against stepResults, since serializeFlow silently skips anything
 * else - a raw walkBlocksInOrder() could drift out of index alignment.
 */
export function walkExecutableBlocks(
  workspace: Blockly.WorkspaceSvg,
  metaByType: Map<string, PuppetBlockMeta>,
): Blockly.Block[] {
  return walkBlocksInOrder(workspace).filter((block) => metaByType.has(block.type));
}

export function serializeFlow(
  workspace: Blockly.WorkspaceSvg,
  metaByType: Map<string, PuppetBlockMeta>,
): FlowStepRequest[] {
  return walkExecutableBlocks(workspace, metaByType).map((block) => toFlowStep(block, metaByType)!);
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
