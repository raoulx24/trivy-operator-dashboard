export interface CarSeveritySummary {
  severityName: string;
  count: number;
}

export interface CarDetailsDto {
  namespaceName: string;
  values: { severityId: number; count: number }[];
  isTotal: boolean;
}
