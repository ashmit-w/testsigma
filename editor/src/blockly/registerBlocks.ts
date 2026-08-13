import * as Blockly from "blockly";
import type { PaletteBlock, PaletteDocument, PuppetBlockMeta } from "../types";

/**
 * Defines every block in a freshly-fetched palette with Blockly, and
 * returns a type -> metadata lookup so the workspace can later be
 * serialised back into a Flow. Re-registering a block type Blockly
 * already knows about just overwrites its definition, which is what we
 * want across repeated /session/run calls.
 */
export function registerPaletteBlocks(palette: PaletteDocument): Map<string, PuppetBlockMeta> {
  const definitions = palette.blocks.map(toBlocklyDefinition);
  Blockly.common.defineBlocksWithJsonArray(definitions);

  const metaByType = new Map<string, PuppetBlockMeta>();
  for (const block of palette.blocks) {
    metaByType.set(block.type, block.puppet);
  }

  return metaByType;
}

function toBlocklyDefinition(block: PaletteBlock): object {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const { puppet, ...blocklyJson } = block;
  return blocklyJson;
}

export function toBlocklyToolbox(palette: PaletteDocument): Blockly.utils.toolbox.ToolboxDefinition {
  return {
    kind: palette.toolbox.kind,
    contents: palette.toolbox.contents.map((entry) => ({ kind: entry.kind, type: entry.type })),
  };
}
