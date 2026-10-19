import { StylesheetJson, StylesheetJsonBlock } from 'cytoscape';

import { FcoseTheme } from './fcose.theme';

const FONT_FAMILY = 'system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif';
const TRANSITION_NODE = 'width height background-color font-size border-color border-width opacity';

/**
 * Builds the cytoscape stylesheet. The per-instance color classes come first,
 * so the base styles take precedence over them.
 */
export function buildStyles(extraColorClasses: { name: string; code: string }[], theme: FcoseTheme): StylesheetJson {
  const extraStyles: StylesheetJsonBlock[] = extraColorClasses.map((c) => ({
    selector: `.${c.name}`,
    style: {
      'background-color': c.code,
    },
  }));

  return [...extraStyles, ...buildBaseStyles(theme)];
}

function buildBaseStyles(theme: FcoseTheme): StylesheetJson {
  return [
    // groups
    {
      selector: ':parent',
      style: {
        'background-color': theme.groupFill,
        'background-opacity': 0.08,
        'border-width': 1,
        'border-style': 'dashed',
        'border-color': theme.groupBorder,
        'border-opacity': 0.6,
        shape: 'round-rectangle',
        label: 'data(label)',
        padding: 20,
      },
    },
    {
      selector: '.parentNode',
      style: {
        color: theme.groupText,
        'font-family': FONT_FAMILY,
        'text-valign': 'top',
        'text-halign': 'center',
        'text-margin-y': 18,
        'font-size': '12px',
        'font-weight': 'bold',
      },
    },

    // nodes
    {
      selector: '.nodeCommon',
      style: {
        opacity: 1,
        'transition-property': 'opacity',
        'transition-duration': 300,
        'border-width': 1,
      },
    },
    {
      selector: '.mainBackgroundColor',
      style: {
        'background-color': theme.nodeFill,
      },
    },
    {
      selector: '.nodePackage',
      style: {
        label: 'data(label)',
        color: theme.nodeText,
        width: 'mapData(label.length, 1, 30, 24, 220)',
        height: '20px',
        'text-valign': 'center',
        'text-halign': 'center',
        'text-wrap': 'ellipsis',
        'text-max-width': '220px',
        'font-family': FONT_FAMILY,
        'font-size': '11px',
        'border-color': theme.nodeBorder,
        'transition-property': TRANSITION_NODE,
        'transition-duration': 300,
      },
    },
    {
      selector: '.nodeBranch',
      style: {
        shape: 'round-rectangle',
        'corner-radius': '4px',
        'border-width': 2,
      },
    },
    {
      selector: '.nodeLeaf',
      style: {
        shape: 'round-rectangle',
        'corner-radius': '12px',
      },
    },

    // edges
    {
      selector: '.edgeCommon',
      style: {
        'curve-style': 'bezier',
        'control-point-step-size': 24,
        width: 1.25,
        'line-color': theme.edge,
        'target-arrow-color': theme.edge,
        'target-arrow-shape': 'triangle',
        'arrow-scale': 1,
        'target-distance-from-node': 1,
        opacity: 0.9,
        'transition-property': 'width line-color target-arrow-color opacity',
        'transition-duration': 300,
      },
    },

    // hover / select: common size bump
    {
      selector: '.hoveredCommon, .selectedCommon',
      style: {
        width: 'mapData(label.length, 1, 30, 24, 250)',
        height: '24px',
        'font-size': '12px',
        'transition-property': TRANSITION_NODE,
        'transition-duration': 300,
      },
    },

    // hover: ring on the node, border style on neighbours
    {
      selector: '.hovered',
      style: {
        'border-width': 3,
        'border-style': 'solid',
        'border-color': theme.hover,
        'underlay-color': theme.hover,
        'underlay-opacity': 0.2,
        'underlay-padding': 4,
        'underlay-shape': 'round-rectangle',
      },
    },
    {
      selector: '.hoveredIncomers',
      style: {
        'background-color': theme.hoverIncomers,
        'border-width': 2,
        'border-style': 'solid',
        'border-color': theme.hover,
      },
    },
    {
      selector: '.hoveredOutgoers',
      style: {
        'background-color': theme.hoverOutgoers,
        'border-width': 2,
        'border-style': 'dashed',
        'border-color': theme.hover,
      },
    },
    {
      selector: '.hoveredHighlight',
      style: {
        'border-width': 2,
        'border-style': 'solid',
        'border-color': theme.hover,
        'overlay-opacity': 0.15,
        'overlay-color': theme.hover,
        'font-style': 'italic',
      },
    },

    // select: same language as hover, defined later so it wins
    {
      selector: '.selected',
      style: {
        'border-width': 3,
        'border-style': 'solid',
        'border-color': theme.selected,
        'underlay-color': theme.selected,
        'underlay-opacity': 0.2,
        'underlay-padding': 4,
        'underlay-shape': 'round-rectangle',
      },
    },
    {
      selector: '.selectedIncomers',
      style: {
        'background-color': theme.selectedIncomers,
        'border-width': 2,
        'border-style': 'solid',
        'border-color': theme.selected,
      },
    },
    {
      selector: '.selectedOutgoers',
      style: {
        'background-color': theme.selectedOutgoers,
        'border-width': 2,
        'border-style': 'dashed',
        'border-color': theme.selected,
      },
    },
    {
      selector: '.selectedHighlight',
      style: {
        'border-width': 2,
        'border-style': 'solid',
        'border-color': theme.selected,
        'overlay-opacity': 0.15,
        'overlay-color': theme.selected,
        'font-style': 'italic',
      },
    },

    // highlighted edges
    {
      selector: '.hoveredEdge',
      style: {
        width: 2.5,
        'line-color': theme.hover,
        'target-arrow-color': theme.hover,
        opacity: 1,
        'z-index': 10,
      },
    },
    {
      selector: '.selectedEdge',
      style: {
        width: 2.5,
        'line-color': theme.selected,
        'target-arrow-color': theme.selected,
        opacity: 1,
        'z-index': 11,
      },
    },

    // state
    {
      selector: '.hidden',
      style: {
        opacity: 0,
      },
    },
    {
      selector: '.deleted',
      style: {
        visibility: 'hidden',
      },
    },

    // filter by name
    {
      selector: '.filtered-highlighted',
      style: {
        width: 'mapData(label.length, 1, 30, 24, 250)',
        height: '24px',
      },
    },
    {
      selector: '.filtered-unhighlighted',
      style: {
        opacity: 0.4,
      },
    },
    {
      selector: '.filtered-semihighlighted',
      style: {
        opacity: 0.8,
      },
    },
  ] as StylesheetJson;
}
