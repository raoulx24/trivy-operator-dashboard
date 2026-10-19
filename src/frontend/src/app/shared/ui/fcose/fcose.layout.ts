import { EdgeSingular } from 'cytoscape';
import { FcoseLayoutOptions } from 'cytoscape-fcose';

export function buildLayoutOptions(getIdealEdgeLength: () => number): FcoseLayoutOptions {
  return {
    name: 'fcose',
    nodeRepulsion: () => 20000,
    numIter: 2500,
    animate: false,
    fit: true,
    padding: 10,
    sampleSize: 50,
    nodeSeparation: 50,
    tilingPaddingHorizontal: 100,
    tilingPaddingVertical: 100,
    // pull group members toward their group's center so group boxes stay compact (defaults: 1 / 1.5)
    gravityCompound: 5,
    gravityRangeCompound: 1,
    idealEdgeLength: (edge: EdgeSingular) => {
      let denominator = 1;
      if (edge.source().parent().nonempty()) denominator = 8;
      if (edge.target().parent().nonempty()) denominator = 4;

      return getIdealEdgeLength() / denominator;
    },
    edgeElasticity: () => 0.15,
  };
}
