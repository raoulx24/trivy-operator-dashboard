import { MiniBarChartDataDto } from '../ui-elements/mini-bar-chart/mini-bar-chart.types';

export interface VrHistoryDto {
  id: string;
  imageRepository: string;
  imageName: string;
  digests: {
    imageTag: string;
    imageDigest: string;
    firstCriticalCount: number;
    firstHighCount: number;
    firstMediumCount: number;
    firstLowCount: number;
    firstUnknownCount: number;
    history: MiniBarChartDataDto[];
  }[];
}

export interface VrHistoryDenormalizedDto {
  id: string;
  imageRepository: string;
  imageName: string;
  imageTag: string;
  imageDigest: string;
  firstCriticalCount: number;
  firstHighCount: number;
  firstMediumCount: number;
  firstLowCount: number;
  firstUnknownCount: number;
  lastCriticalCount: number;
  lastHighCount: number;
  lastMediumCount: number;
  lastLowCount: number;
  lastUnknownCount: number;
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
