import { ChartOptions } from 'chart.js';
import { ColorHelper } from './color.utils';
import { SeverityUtils } from './severity.utils';

export type PrimeNgPieChartData = {
  labels: string[];
  datasets: Array<{
    data: number[];
    backgroundColor: string[];
    hoverBackgroundColor: string[];
  }>;
  title: string;
};

export type PrimeNgHorizontalBarChartData = {
  labels: string[];
  datasets: Array<{
    label: string | null;
    data: number[];
    backgroundColor: string | null;
    hoverBackgroundColor: string | null;
    hidden: boolean;
  }>;
  title: string;
};

export interface SeveritiesSummary {
  namespaceName?: string | null;
  details?: Array<{
    id?: number;
    totalCount?: number;
    distinctCount?: number;
  }>;
  uid?: string;
  isTotal?: boolean;
}

export class PrimeNgChartUtils {
  public static getDataForHorizontalBarChartByNamespace(
    severitiesSummary: SeveritiesSummary[],
    distinct: boolean,
  ): PrimeNgHorizontalBarChartData {
    // TODO make required everything in severitiesDto and severitiesSummaryDto
    const chartData: PrimeNgHorizontalBarChartData = {
      datasets: [],
      labels: [],
      title: 'a title',
    };
    if (severitiesSummary.length === 0) {
      return chartData;
    }
    severitiesSummary
      .filter((x) => !x.isTotal)
      .forEach((x) => {
        chartData.labels.push(x.namespaceName!);
      });
    const severities = severitiesSummary[0].details!.map((x) => x.id).sort((a, b) => a! - b!);
    severities.forEach((severity) => {
      const totalVulnerabilities: number[] = [];
      severitiesSummary
        .filter((x) => !x.isTotal)
        .forEach((severitySummary) => {
          const severityDetail = severitySummary.details!.find((x) => x.id! == severity);
          totalVulnerabilities.push(
            distinct ? (severityDetail?.distinctCount ?? 0) : (severityDetail?.totalCount ?? 0),
          );
        });
      chartData.datasets.push({
        label: SeverityUtils.getCapitalizedName(severity!),
        data: totalVulnerabilities,
        backgroundColor: SeverityUtils.getCssColor(severity!),
        hoverBackgroundColor: SeverityUtils.getCssColorHover(severity!),
        hidden: false,
      });
    });

    return chartData;
  }

  public static getDataForHorizontalBarChartBySeverity(
    severitiesSummary: SeveritiesSummary[],
    distinct: boolean,
  ): PrimeNgHorizontalBarChartData {
    // TODO make required everything in severitiesDto and severitiesSummaryDto
    const chartData: PrimeNgHorizontalBarChartData = {
      datasets: [],
      labels: [],
      title: 'a title',
    };
    if (severitiesSummary.length === 0) {
      return chartData;
    }
    const severities = severitiesSummary[0].details!.map((x) => x.id).sort((a, b) => a! - b!);
    let namespacesCounter: number = 0;
    severities.forEach((x) => {
      chartData.labels.push(SeverityUtils.getCapitalizedName(x!));
    });
    severitiesSummary
      .filter((x) => !x.isTotal)
      .forEach((severitySummary) => {
        const totalVulnerabilities: number[] = [];
        severities.forEach((severity) => {
          const severityDetail = severitySummary.details!.find((x) => x.id == severity);
          totalVulnerabilities.push(
            distinct ? (severityDetail?.distinctCount ?? 0) : (severityDetail?.totalCount ?? 0),
          );
        });
        const color: string = ColorHelper.rainbow(severitiesSummary.length, namespacesCounter);
        chartData.datasets.push({
          label: severitySummary.namespaceName!,
          data: totalVulnerabilities,
          backgroundColor: color,
          hoverBackgroundColor: color,
          hidden: false,
        });
        namespacesCounter++;
      });

    return chartData;
  }

  public static getHorizontalBarChartOption(): ChartOptions {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--p-text-color');
    const textColorSecondary = documentStyle.getPropertyValue('--p-text-muted-color');
    const surfaceBorder = documentStyle.getPropertyValue('--p-surface-700');
    return {
      indexAxis: 'y',
      responsive: true,
      maintainAspectRatio: false,
      aspectRatio: 0.6,
      plugins: {
        legend: {
          labels: {
            color: textColor,
          },
          position: 'bottom',
        },
      },
      scales: {
        x: {
          ticks: {
            color: textColorSecondary,
            font: {
              weight: 500,
            },
          },
          grid: {
            color: surfaceBorder,
            //drawBorder: false,
          },
        },
        y: {
          ticks: {
            color: textColorSecondary,
          },
          grid: {
            color: surfaceBorder,
            //drawBorder: false,
          },
        },
      },
    };
  }

  public getDataForPieChart(severitiesSummary: SeveritiesSummary[], title: string = 'generic'): PrimeNgPieChartData[] {
    const pieChartData: PrimeNgPieChartData[] = [];
    const severityDtos = SeverityUtils.severityDtos;
    const severityLabels: string[] = [];
    const cssColors: string[] = [];
    const cssColorHovers: string[] = [];
    severityDtos.forEach((x) => {
      severityLabels.push(x.name);
      cssColors.push(SeverityUtils.getCssColor(x.id));
      cssColorHovers.push(SeverityUtils.getCssColorHover(x.id));
    });
    severitiesSummary
      .filter((x) => !x.isTotal)
      .forEach((severitySummary) => {
        const values: number[] = [];
        if (severitySummary.details != null) {
          severitySummary.details
            .sort((a, b) => a.id! - b.id!)
            .forEach((x) => {
              values.push(x.distinctCount!);
            });
        }
        const chartData: PrimeNgPieChartData = {
          labels: severityLabels,
          datasets: [
            {
              data: values,
              backgroundColor: cssColors,
              hoverBackgroundColor: cssColorHovers,
            },
          ],
          title: severitySummary.namespaceName ? severitySummary.namespaceName : title,
        };
        pieChartData.push(chartData);
      });

    return pieChartData;
  }
}
