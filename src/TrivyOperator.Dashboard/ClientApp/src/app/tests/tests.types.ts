import { MiniBarChartDataDto } from '../ui-elements/mini-bar-chart/mini-bar-chart.types';

export interface TestDto {
  imageName: string;
  imageTag: string;
  imageDigest: string;
  imageRepository: string;
  criticalCount: number;
  highCount: number;
  mediumCount: number;
  lowCount: number;
  unknownCount: number;
  history: MiniBarChartDataDto[];
}
