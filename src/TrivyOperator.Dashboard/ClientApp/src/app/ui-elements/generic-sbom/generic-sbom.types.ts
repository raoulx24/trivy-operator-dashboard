import { SbomReportImageMinimalDto } from '../../../api/models/sbom-report-image-minimal-dto';
import { SbomReportImageDto } from '../../../api/models/sbom-report-image-dto';
import { SbomReportDetailDto } from '../../../api/models/sbom-report-detail-dto';

export interface GenericSbomReportDto extends Omit<SbomReportImageDto, 'details'> {
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
