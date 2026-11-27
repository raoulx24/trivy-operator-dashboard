export interface NodeDataDto {
  id?: string;
  name?: string | null;
  dependsOn?: Array<string> | null;
  groupName?: string;
  isMain?: boolean;
  colorClass?: string;
  // overrides "no dependsOn means a leaf": a graph that shows only part of the dependencies (e.g. the direct ones of
  // one node) still knows which nodes have children of their own, so they stay divable
  isLeaf?: boolean;
}

// fcose: the free force layout; layered: parents above the main node, children below it (see buildLayeredPositions)
export type GraphLayout = 'fcose' | 'layered';

export interface DeletedNodes {
  deleteType: 'node' | 'nodeAndChildren';
  mainNodeIds: string[];
  nodeIds: string[];
}
