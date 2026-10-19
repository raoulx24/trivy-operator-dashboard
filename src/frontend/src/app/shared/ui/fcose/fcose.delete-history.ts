import { Core, NodeSingular } from 'cytoscape';

import { DeletedNodes, NodeDataDto } from './fcose.types';

export interface FcoseDeleteHistoryHost {
  getSelectedNodes: () => NodeSingular[];
  getNodeDataDtos: () => NodeDataDto[];
  onDeleted: (nodeIds: string[]) => void;
  onUndeleted: (nodeIds: string[]) => void;
}

/**
 * "Deleting" only hides nodes (class `deleted`); this class tracks those steps for undo / redo.
 */
export class FcoseDeleteHistory {
  entries: DeletedNodes[] = [];
  currentIndex: number = -1;

  constructor(
    private readonly cy: Core,
    private readonly host: FcoseDeleteHistoryHost,
  ) {}

  reset() {
    this.entries = [];
    this.currentIndex = -1;
  }

  deleteNodes(areChildrenIncluded: boolean, mainNodes: NodeSingular[] = []) {
    const isRedo = mainNodes.length > 0;

    if (!isRedo) {
      mainNodes = [...this.host.getSelectedNodes()];
    }
    this.host.getSelectedNodes().forEach((node) => node.unselect());
    const mainNodeIds: string[] = [];
    const deletedNodes: string[] = [];
    mainNodes.forEach((node) => {
      mainNodeIds.push(node.id());
      areChildrenIncluded
        ? this.deleteNodeChildrenAndOrphans(node, deletedNodes)
        : this.deleteNodeAndOrphans(node, deletedNodes);
    });
    this.cleanupParentsAndOrphans(deletedNodes);
    this.processDeletedNodeIds(mainNodeIds, deletedNodes, areChildrenIncluded, isRedo);
  }

  undo() {
    if (this.currentIndex == -1 || this.entries.length == 0) {
      return;
    }
    this.host.onUndeleted(this.entries[this.currentIndex].nodeIds);
    this.recreateNodes(this.entries[this.currentIndex].nodeIds);
    const lastNodeIds: string[] = [...this.entries[this.currentIndex].mainNodeIds];
    setTimeout(() => {
      this.host.getSelectedNodes().forEach((node) => {
        node.unselect();
      });
      lastNodeIds.forEach((nodeId) => {
        const node = this.cy.getElementById(nodeId) as NodeSingular;
        node?.select();
      });
    }, 0);
    this.currentIndex--;
  }

  redo() {
    if (this.currentIndex >= this.entries.length - 1) {
      return;
    }
    const entry = this.entries[this.currentIndex + 1];
    const nodes = entry.mainNodeIds
      .map((id) => this.cy.getElementById(id) as NodeSingular)
      .filter((node) => node !== undefined);
    switch (entry.deleteType) {
      case 'node':
        this.deleteNodes(false, nodes);
        break;
      case 'nodeAndChildren':
        this.deleteNodes(true, nodes);
        break;
    }
  }

  showAll() {
    if (this.entries.length == 0 || this.currentIndex == -1) {
      return;
    }
    for (let i = this.currentIndex; i >= 0; i--) {
      this.host.onUndeleted(this.entries[i].nodeIds);
      this.recreateNodes(this.entries[i].nodeIds);
    }
    this.reset();
  }

  private deleteNodeAndOrphans(node: NodeSingular, deletedNodes: string[]) {
    deletedNodes.push(node.id());
    node.addClass('deleted');
    node.connectedEdges().addClass('deleted');
    node
      .outgoers('node')
      .filter((x: NodeSingular) => !x.hasClass('deleted'))
      .filter((x: NodeSingular) => x.connectedEdges().filter((x) => !x.hasClass('deleted')).length === 0)
      .forEach((x: NodeSingular) => {
        deletedNodes.push(x.id());
        x.addClass('deleted');
      });
  }

  private deleteNodeChildrenAndOrphans(node: NodeSingular, deletedNodes: string[]) {
    this.deleteNodeAndOrphans(node, deletedNodes);
    node
      .outgoers('node')
      .filter((node: NodeSingular) => !node.hasClass('deleted'))
      .forEach((x: NodeSingular) => this.deleteNodeAndOrphans(x, deletedNodes));
  }

  private cleanupParentsAndOrphans(deletedNodeIds: string[]) {
    this.cy
      .nodes()
      .filter((node) => node.isParent())
      .filter((parentNode: NodeSingular) => parentNode.children().filter((x) => !x.hasClass('deleted')).length < 2)
      .forEach((parent) => {
        parent.children().forEach((node: NodeSingular) => {
          node.move({ parent: null });
        });
        parent.addClass('deleted');
        deletedNodeIds.push(parent.id());
      });
    this.cy
      .nodes()
      .filter(
        (x) =>
          !x.isParent() &&
          x.connectedEdges().filter((y) => !y.hasClass('deleted')).length === 0 &&
          !x.hasClass('deleted'),
      )
      .forEach((x) => {
        deletedNodeIds.push(x.id());
        x.addClass('deleted');
      });
  }

  private recreateNodes(deletedNodes: string[]) {
    deletedNodes.forEach((nodeId) => {
      const node = this.cy.getElementById(nodeId);
      if (node) {
        node.removeClass('deleted');
        node
          .connectedEdges()
          .filter((edge) => !edge.source().hasClass('deleted') && !edge.target().hasClass('deleted'))
          .forEach((edge) => {
            edge.removeClass('deleted');
          });
      }
    });
    const emptyParents = this.cy.nodes('.parentNode').filter((x) => x.children().length === 0 && !x.hasClass('deleted'));
    emptyParents.forEach((parent) => {
      this.host
        .getNodeDataDtos()
        .filter((dto) => dto.groupName == parent.id())
        .forEach((dto) => {
          this.cy.nodes(`#${dto.id}`).move({ parent: parent.id() });
        });
    });
  }

  private processDeletedNodeIds(
    mainNodeIds: string[],
    deletedNodes: string[],
    areChildrenIncluded: boolean,
    isRedo: boolean,
  ) {
    if (deletedNodes.length > 0) {
      this.host.onDeleted(deletedNodes);
      const deleteType = areChildrenIncluded ? 'nodeAndChildren' : 'node';
      if (!isRedo) {
        this.entries = this.entries.slice(0, this.currentIndex + 1);
        this.entries.push({ deleteType: deleteType, mainNodeIds: mainNodeIds, nodeIds: deletedNodes });
      }
      this.currentIndex++;
    }
  }
}
