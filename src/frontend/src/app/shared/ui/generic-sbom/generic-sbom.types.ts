import { SbomReportImageMinimalDto } from '@api/models/sbom-report-image-minimal-dto';
import { SbomReportDetailDto } from '@api/models/sbom-report-detail-dto';

export interface GenericSbomReportDto {
  uid: string;
  rootNodeBomRef: string;
  details: Array<GenericSbomReportDetailDto>;
}

export interface GenericSbomReportDetailDto extends SbomReportDetailDto {
  level?: 'Ancestor' | 'Base' | 'Child' | 'Descendant';
  group?: string;
}

export interface GenericSbomReportMinimalDto extends SbomReportImageMinimalDto { }

/** Lookups over the details of the current report, built once per report instead of scanning per node. */
export interface SbomIndex {
  byBomRef: Map<string, GenericSbomReportDetailDto>;
  /** position of a detail in the report; keeps children in report order */
  order: Map<string, number>;
  /** bomRef -> details that depend on it (report order) */
  parentsByBomRef: Map<string, GenericSbomReportDetailDto[]>;
}

/** what the right side of generic-sbom shows */
export type GenericSbomViewMode = 'navigator' | 'graph';
