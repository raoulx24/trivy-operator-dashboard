import { SbomReportImageMinimalDto } from '../../../api/models/sbom-report-image-minimal-dto';
import { ClusterSbomReportDto } from '../../../api/models/cluster-sbom-report-dto';
import { SbomReportDetailDto } from '../../../api/models/sbom-report-detail-dto';

export interface GenericSbomReportDto {
  uid: string;
  rootNodeBomRef: string;
  details: Array<GenericSbomReportDetailDto>;
}

export interface GenericSbomReportDetailDto extends SbomReportDetailDto {
  level?: 'Ancestor' | 'Base' | 'Child' | 'Descendant';
  group?: string;
}

// export interface GenericSbomDetailExtendedDto extends GenericSbomReportDetailDto {
//
// }

export interface GenericSbomReportMinimalDto extends SbomReportImageMinimalDto { }
