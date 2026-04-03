import { MiniBarChartDataDto } from '../ui-elements/mini-bar-chart/mini-bar-chart.types';

export interface TestDto {
  id: string;
  imageName: string;
  imageTag: string;
  imageDigest: string;
  imageRepository: string;
  criticalCount: number;
  highCount: number;
  mediumCount: number;
  lowCount: number;
  unknownCount: number;
  criticalNew: number;
  highNew: number;
  mediumNew: number;
  lowNew: number;
  unknownNew: number;
  criticalRemoved: number;
  highRemoved: number;
  mediumRemoved: number;
  lowRemoved: number;
  unknownRemoved: number;
  history: MiniBarChartDataDto[];
}
