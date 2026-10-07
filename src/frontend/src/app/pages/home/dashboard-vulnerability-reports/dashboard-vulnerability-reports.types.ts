export interface SeveritySummary {
  severityName: string;
  count: number;
  fixable: number;
}

export interface OtherSummaryMainStatistics {
  description: 'Images' | 'Images OSes' | 'End of Service Life';
  count: number;
}

export interface GenericSummaryDto {
  name: string;
  count: number;
}

export interface GenericByNsSummaryDto {
  namespaceName: string;
  totalCount: number;
  distinctCount: number;
  isTotal: boolean;
}

export interface GenericNsTotalSortable {
  namespaceName: string;
  isTotal: boolean;
}

export interface VrDetailsDto {
  namespaceName: string;
  values: { severityId: number; count: number }[];
  isTotal: boolean;
}
