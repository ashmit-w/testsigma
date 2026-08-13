import { forwardRef, useEffect, useImperativeHandle, useRef } from "react";
import * as Blockly from "blockly";
import type { PaletteDocument, PuppetBlockMeta, FlowStepRequest } from "../types";
import { registerPaletteBlocks, toBlocklyToolbox } from "../blockly/registerBlocks";
import { serializeFlow } from "../blockly/serializeFlow";

export interface WorkspaceHandle {
  serializeFlow: () => FlowStepRequest[];
}

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
    }));

    return <div className="workspace" ref={containerRef} />;
  },
);
