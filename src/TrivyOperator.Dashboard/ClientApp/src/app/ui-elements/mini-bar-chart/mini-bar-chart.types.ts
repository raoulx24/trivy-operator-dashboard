export type SeveritiesCount = [number, number, number, number, number];

export interface MiniBarChartDataDto {
  label: string;
  newCount: SeveritiesCount;
  removedCount: SeveritiesCount;
}
