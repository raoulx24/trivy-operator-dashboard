import { ElementDefinition } from 'cytoscape';

import { NodeDataDto } from './fcose.types';

export function buildElements(nodeDataDtos: NodeDataDto[]): ElementDefinition[] {
  const elements: ElementDefinition[] = [];
  const groupMap = new Map<string, number>();
  nodeDataDtos
    .filter((x) => x.groupName)
    .forEach((x) => {
      const currentCount = (groupMap.get(x.groupName ?? '') || 0) + 1;
      groupMap.set(x.groupName ?? '', currentCount);
    });
  groupMap.forEach((value, key) => {
    if (value > 1) {
      elements.push({ data: { id: key, label: key }, classes: 'parentNode' });
    }
  });

  nodeDataDtos.forEach((nodeData) => {
    const parentId = (groupMap.get(nodeData.groupName ?? '') || 0) > 1 ? nodeData.groupName : undefined;
    elements.push({
      data: {
        id: nodeData.id,
        label: nodeData.name ? nodeData.name : ' ', // so called 'em space' - just a 'long' space,
        parent: parentId,
      },
      classes: `nodeCommon nodePackage ${nodeData.colorClass ?? 'mainBackgroundColor'} ${nodeData.dependsOn?.length ? 'nodeBranch' : 'nodeLeaf'}`,
    });
    nodeData.dependsOn?.forEach((depends) => {
      elements.push({
        data: {
          source: nodeData.id,
          target: depends,
        },
        classes: 'edgeCommon',
      });
    });
  });

  return elements;
}
