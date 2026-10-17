import { Core, EdgeSingular, NodeSingular } from 'cytoscape';

type StylePrefix = 'hovered' | 'selected';

export interface FcoseHighlighterHost {
  /** True while the graph is being redrawn; hover un-highlighting is suspended then. */
  isRedrawing: () => boolean;
  onHoveredNodeIdChange: (nodeId: string | undefined) => void;
  onSelectedNodeIdChange: (nodeId: string | undefined) => void;
}

/**
 * Owns hover / selection / filter-by-name state and the cytoscape classes that visualize it.
 */
export class FcoseHighlighter {
  private _selectedNodes: NodeSingular[] = [];
  private _hoveredNode?: NodeSingular;
  private highlightedHoverNode?: NodeSingular;

  constructor(
    private readonly cy: Core,
    private readonly host: FcoseHighlighterHost,
  ) {}

  get selectedNodes(): NodeSingular[] {
    return this._selectedNodes;
  }

  private get hoveredNode(): NodeSingular | undefined {
    return this._hoveredNode;
  }
  private set hoveredNode(node: NodeSingular | undefined) {
    if ((node && node.isParent()) || node === this._hoveredNode) {
      return;
    }
    this._hoveredNode = node;
    this.host.onHoveredNodeIdChange(node?.id());
  }

  reset() {
    this.hoveredNode = undefined;
    this.highlightedHoverNode = undefined;
    this._selectedNodes = [];
  }

  // #region Hover
  onMouseOver(node: NodeSingular) {
    this.hoveredNode = node;
    this.highlightHoveredNode(node);
  }

  onMouseOut(node: NodeSingular) {
    this.hoveredNode = undefined;
    this.unhighlightHoveredNode(node);
  }

  highlightHoveredNode(node: NodeSingular) {
    if (this.highlightedHoverNode == node || node.isParent()) {
      return;
    }
    if (this.highlightedHoverNode) {
      this.unhighlightHoveredNode(this.highlightedHoverNode);
    }
    this.highlightNode(node, 'hovered');
    this.highlightedHoverNode = node;
  }

  private unhighlightHoveredNode(node: NodeSingular) {
    if (node.isParent() || this.host.isRedrawing()) {
      return;
    }
    this.unhighlightNode(node, 'hovered');
    this.highlightedHoverNode = undefined;
  }
  // #endregion

  // #region Select
  highlightSelectedNode(node: NodeSingular) {
    if (node.isParent()) {
      node.unselect();
      return;
    }
    if (this._selectedNodes.some((x) => x === node)) {
      return;
    }
    this._selectedNodes.push(node);
    if (this._selectedNodes.length == 2) {
      this.unhighlightNode(this._selectedNodes[0], 'selected');
      this.highlightNode(this._selectedNodes[0], 'selected', true);
    }
    if (this.highlightedHoverNode) {
      this.unhighlightHoveredNode(this.highlightedHoverNode);
    }
    this.highlightNode(node, 'selected', this._selectedNodes.length > 1);
    this.host.onSelectedNodeIdChange(node.id());
  }

  unhighlightSelectedNode(node: NodeSingular) {
    this._selectedNodes = this._selectedNodes.filter((x) => x != node);
    this.unhighlightNode(node, 'selected', this._selectedNodes.length > 1);
    if (this.highlightedHoverNode) {
      this.unhighlightHoveredNode(this.highlightedHoverNode);
    }
    if (this._selectedNodes.length == 1) {
      this.highlightNode(this._selectedNodes[0], 'selected');
    }
    if (this.hoveredNode === node) {
      this.highlightHoveredNode(node);
    }
    this.host.onSelectedNodeIdChange(undefined);
  }
  // #endregion

  // #region Filter by name
  filterByName(value: string) {
    value = value.toLowerCase();
    this.cy.batch(() => {
      if (value) {
        this.cy.nodes().forEach((node: NodeSingular) => {
          if (!node.isParent()) {
            const label = node.data('label').toLowerCase();
            if (label.includes(value)) {
              node.removeClass('filtered-unhighlighted');
              node.addClass('filtered-highlighted');
            } else {
              node.removeClass('filtered-highlighted');
              node.addClass('filtered-unhighlighted');
            }
          }
        });
      } else {
        this.cy.nodes().forEach((node: NodeSingular) => {
          node.removeClass('filtered-highlighted');
          node.removeClass('filtered-unhighlighted');
        });
      }
    });
  }

  private swapClassNodesHighlightedByName(node: NodeSingular, action: 'highlight' | 'unhighlight') {
    if (node.hasClass('filtered-unhighlighted') && action == 'highlight') {
      node.removeClass('filtered-unhighlighted');
      node.addClass('filtered-semihighlighted');
      return;
    }
    if (node.hasClass('filtered-semihighlighted') && action == 'unhighlight') {
      node.removeClass('filtered-semihighlighted');
      node.addClass('filtered-unhighlighted');
    }
  }
  // #endregion

  // #region Node highlight
  private highlightNode(node: NodeSingular, stylePrefix: StylePrefix, justNode: boolean = false) {
    node.addClass(`${stylePrefix}Common ${stylePrefix}`);
    this.swapClassNodesHighlightedByName(node, 'highlight');
    if (justNode) {
      return;
    }
    node.incomers('node').forEach((depNode: NodeSingular) => {
      if (!depNode.hasClass('selectedIncomers') && !depNode.hasClass('selectedCommon')) {
        depNode.addClass(`${stylePrefix}Common`);
        if (node.outgoers('node').has(depNode)) {
          depNode.addClass(`${stylePrefix}Highlight`);
        } else {
          depNode.addClass(`${stylePrefix}Incomers`);
        }
      }
      this.swapClassNodesHighlightedByName(depNode, 'highlight');
    });
    node.outgoers('node').forEach((depNode: NodeSingular) => {
      if (!depNode.hasClass('selectedOutgoers') && !depNode.hasClass('selectedCommon')) {
        depNode.addClass(`${stylePrefix}Common ${stylePrefix}Outgoers`);
      }
      this.swapClassNodesHighlightedByName(depNode, 'highlight');
    });

    node.connectedEdges().forEach((edge: EdgeSingular) => {
      edge.addClass(`${stylePrefix}Edge`);
    });
  }

  private unhighlightNode(node: NodeSingular, stylePrefix: StylePrefix, justNode: boolean = false) {
    node.removeClass(`${stylePrefix}Common ${stylePrefix}`);
    this.swapClassNodesHighlightedByName(node, 'unhighlight');
    if (justNode) {
      return;
    }
    node.outgoers('node').forEach((depNode: NodeSingular) => {
      depNode.removeClass(`${stylePrefix}Common ${stylePrefix}Outgoers ${stylePrefix}Highlight`);
      this.swapClassNodesHighlightedByName(depNode, 'unhighlight');
    });
    node.incomers('node').forEach((depNode: NodeSingular) => {
      depNode.removeClass(`${stylePrefix}Common ${stylePrefix}Incomers`);
      this.swapClassNodesHighlightedByName(depNode, 'unhighlight');
    });

    node.connectedEdges().forEach((edge: EdgeSingular) => {
      edge.removeClass(`${stylePrefix}Edge`);
    });
  }
  // #endregion
}
