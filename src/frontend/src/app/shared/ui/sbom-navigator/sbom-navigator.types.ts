import { GenericSbomReportDetailDto } from '../generic-sbom/generic-sbom.types';

/** The level of a package in the dive set (generic-sbom's list). */
export type SbomNeighbourLevel = NonNullable<GenericSbomReportDetailDto['level']>;

/** One row of the navigator's neighbours table: a package that uses the shown one, or one it depends on. */
export interface SbomNeighbourRow extends Omit<GenericSbomReportDetailDto, 'level'> {
  // direction + bomRef: a package can be both a parent and a child (a cycle), so bomRef alone would clash as dataKey
  rowKey: string;
  // also the sort key of the dive column: 'Depends on' < 'Used by'
  direction: 'Used by' | 'Depends on';
  // the dive button of the row: up (used by, a dive out) or down (depends on, a dive in)
  directionIcon: string;
  // not in the list on the left (outside the dive), so the Select button of the row is disabled
  isSelectDisabled: boolean;
}
