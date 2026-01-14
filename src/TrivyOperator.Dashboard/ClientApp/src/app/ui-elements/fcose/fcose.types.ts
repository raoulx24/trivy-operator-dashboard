export interface NodeDataDto {
  id?: string;
  name?: string | null;
  dependsOn?: Array<string> | null;
  groupName?: string;
  isMain?: boolean;
  colorClass?: string;
}

export interface DeletedNodes {
  deleteType: 'node' | 'nodeAndChildren';
  mainNodeIds: string[];
  nodeIds: string[];
}
