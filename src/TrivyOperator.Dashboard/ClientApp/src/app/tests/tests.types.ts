import { MiniBarChartDataDto } from '../ui-elements/mini-bar-chart/mini-bar-chart.types';

export interface VrHistoryDto {
  id: string;
  imageRepository: string;
  imageName: string;
  resourceNamespace: string;
  digests: {
    imageTag: string;
    imageDigest: string;
    firstCriticalCount: number;
    firstHighCount: number;
    firstMediumCount: number;
    firstLowCount: number;
    firstUnknownCount: number;
    history: VrHistoryDetailDto[];
  }[];
}

export interface VrHistoryDetailDto {
  moment: string;
  addedCount: number[];
  droppedCount: number[];
}

export interface VrHistoryDenormalizedDto {
  uid: string;
  resourceNamespace: string;
  imageFull: string;
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
  lastChangeMoment: string;
  criticalAdded: number;
  highAdded: number;
  mediumAdded: number;
  lowAdded: number;
  unknownAdded: number;
  criticalDropped: number;
  highDropped: number;
  mediumDropped: number;
  lowDropped: number;
  unknownDropped: number;
  details: VrHistoryDenormalizedDetailDto[];
  history: MiniBarChartDataDto[];
}

export interface VrHistoryDenormalizedDetailDto {
  moment: string;

  criticalAdded: number;
  highAdded: number;
  mediumAdded: number;
  lowAdded: number;
  unknownAdded: number;
  criticalDropped: number;
  highDropped: number;
  mediumDropped: number;
  lowDropped: number;
  unknownDropped: number;
}
