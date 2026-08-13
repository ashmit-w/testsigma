import { forwardRef, useEffect, useImperativeHandle, useRef } from "react";
import * as Blockly from "blockly";
import type { PaletteDocument, PuppetBlockMeta, FlowStepRequest, StepResult } from "../types";
import { registerPaletteBlocks, toBlocklyToolbox } from "../blockly/registerBlocks";
import { serializeFlow, walkExecutableBlocks } from "../blockly/serializeFlow";

const PASSED_COLOUR = "#2e7d32";
const FAILED_COLOUR = "#c62828";
const STEP_ANIMATION_MS = 200;

export interface WorkspaceHandle {
  serializeFlow: () => FlowStepRequest[];
  /** RV-1: highlight the block about to run, before its result is known. */
  highlightRunStart: () => void;
  /** RV-1: walk stepResults against the same block order, colouring each green/red as its result "arrives". */
  playResults: (results: StepResult[]) => Promise<void>;
}

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

/**
 * Hosts the single Blockly instance: an always-open flyout (the palette)
 * docked to the left, and the workspace canvas filling the rest. Blockly
 * owns both areas internally, which is what gives us "palette left,
 * workspace centre" without a separate palette component.
 *
 * The workspace itself is never cleared or recreated across palette
 * refreshes (checkpoint semantics: the authored flow persists even as
 * the live palette changes) - only the block definitions and the
 * toolbox/flyout contents are swapped in place.
 */
export const Workspace = forwardRef<WorkspaceHandle, { palette: PaletteDocument | null }>(
  function Workspace({ palette }, ref) {
    const containerRef = useRef<HTMLDivElement | null>(null);
    const workspaceRef = useRef<Blockly.WorkspaceSvg | null>(null);
    const metaByTypeRef = useRef<Map<string, PuppetBlockMeta>>(new Map());

    useEffect(() => {
      if (!containerRef.current) {
        return;
      }

      const workspace = Blockly.inject(containerRef.current, {
        toolbox: { kind: "flyoutToolbox", contents: [] },
        horizontalLayout: false,
        toolboxPosition: "start",
        trashcan: true,
        renderer: "zelos",
      });
      workspaceRef.current = workspace;

      return () => {
        workspace.dispose();
        workspaceRef.current = null;
      };
    }, []);

    useEffect(() => {
      const workspace = workspaceRef.current;
      if (!workspace || !palette) {
        return;
      }

      metaByTypeRef.current = registerPaletteBlocks(palette);
      workspace.updateToolbox(toBlocklyToolbox(palette));
    }, [palette]);

    useImperativeHandle(ref, () => ({
      serializeFlow: () => {
        const workspace = workspaceRef.current;
        if (!workspace) {
          return [];
        }

        return serializeFlow(workspace, metaByTypeRef.current);
      },

      highlightRunStart: () => {
        const workspace = workspaceRef.current;
        if (!workspace) {
          return;
        }

        const [first] = walkExecutableBlocks(workspace, metaByTypeRef.current);
        workspace.highlightBlock(first?.id ?? null);
      },

      playResults: async (results) => {
        const workspace = workspaceRef.current;
        if (!workspace) {
          return;
        }

        const blocks = walkExecutableBlocks(workspace, metaByTypeRef.current);
        for (let i = 0; i < blocks.length && i < results.length; i++) {
          const block = blocks[i];
          workspace.highlightBlock(block.id);
          await delay(STEP_ANIMATION_MS);

          if (results[i].status === "passed") {
            block.setColour(PASSED_COLOUR);
          } else if (results[i].status === "failed") {
            block.setColour(FAILED_COLOUR);
          }
        }

        workspace.highlightBlock(null);
      },
    }));

    return <div className="workspace" ref={containerRef} />;
  },
);
