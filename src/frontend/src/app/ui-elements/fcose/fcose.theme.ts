export type DarkLightMode = 'Dark' | 'Light';

/**
 * Colors used by the graph stylesheet. Node text is dark in both modes on purpose:
 * node fills (including custom `extraColorClasses`) are always mid/light tones.
 */
export interface FcoseTheme {
  nodeFill: string;
  nodeBorder: string;
  nodeText: string;
  edge: string;
  hover: string;
  selected: string;
  groupFill: string;
  groupBorder: string;
  groupText: string;
}

export const FCOSE_THEMES: Record<DarkLightMode, FcoseTheme> = {
  Light: {
    nodeFill: '#5cc4d6',
    nodeBorder: '#2b7a8c',
    nodeText: '#0f172a',
    edge: '#94a3b8',
    hover: '#2563eb',
    selected: '#dc2626',
    groupFill: '#64748b',
    groupBorder: '#64748b',
    groupText: '#334155',
  },
  Dark: {
    nodeFill: '#ffb5c8',
    nodeBorder: '#1e6f80',
    nodeText: '#0f172a',
    edge: '#8b9bb4',
    hover: '#60a5fa',
    selected: '#f87171',
    groupFill: '#94a3b8',
    groupBorder: '#94a3b8',
    groupText: '#cbd5e1',
  },
};
