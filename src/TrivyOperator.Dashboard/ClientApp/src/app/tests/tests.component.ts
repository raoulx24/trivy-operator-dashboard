import { Component, effect, inject, OnInit } from '@angular/core';

import { TableModule } from 'primeng/table';
import { TrivyTableComponent } from '../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../ui-elements/trivy-table/trivy-table.types';
import { VrHistoryDenormalizedDto, VrHistoryDto } from './tests.types';
import {MiniBarChartDataDto} from "../ui-elements/mini-bar-chart/mini-bar-chart.types";

@Component({
  selector: 'app-tests',
  imports: [TableModule, TrivyTableComponent],
  templateUrl: './tests.component.html',
  styleUrl: './tests.component.scss',
})
export class TestsComponent implements OnInit {
  readonly minHistoryDays = 14;

  dataDtos: VrHistoryDto[] = [
    {
      id: '01',
      imageRepository: 'myrepo.com',
      imageName: 'image name',
      digests: [
        {
          imageTag: 'latest',
          imageDigest: 'sha256: xyz',
          firstCriticalCount: 0, firstHighCount: 2, firstMediumCount: 6, firstLowCount: 24, firstUnknownCount: 0,
          history: [
            { moment: '2026-03-20', newCount: [0, 1, 0, 2, 0], removedCount: [2, 1, 0, 3, 0] },
            { moment: '2026-03-24T14:30:00', newCount: [2, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-24T15:30:00', newCount: [1, 0, 0, 0, 1], removedCount: [0, 0, 0, 0, 1] },
            { moment: '2026-03-25', newCount: [1, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-27', newCount: [0, 2, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-02', newCount: [1, 1, 1, 1, 1], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-04-03', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
          ],
        },
      ],
    },
    {
      id: '11',
      imageRepository: 'repo.company.com/backend',
      imageName: 'backend-service',
      digests: [
        {
          imageTag: 'latest',
          imageDigest: 'sha256:111aaa',
          firstCriticalCount: 3, firstHighCount: 6, firstMediumCount: 12, firstLowCount: 34, firstUnknownCount: 1,
          history: [
            { moment: '2026-03-21', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 1, 0] },
            { moment: '2026-03-23', newCount: [1, 0, 0, 0, 0], removedCount: [0, 0, 1, 0, 0] },
            { moment: '2026-03-26', newCount: [0, 0, 2, 0, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-30', newCount: [0, 1, 1, 0, 0], removedCount: [1, 0, 0, 0, 0] },
            { moment: '2026-04-02', newCount: [0, 0, 0, 1, 0], removedCount: [0, 0, 0, 0, 0] }
          ],
        },
      ],
    },
    {
      id: '21',
      imageRepository: 'repo.company.com/frontend',
      imageName: 'frontend-ui',
      digests: [
        {
          imageTag: '1.4.2',
          imageDigest: 'sha256:222bbb',
          firstCriticalCount: 3, firstHighCount: 6, firstMediumCount: 12, firstLowCount: 34, firstUnknownCount: 1,
          history: [
            { moment: '2026-03-22', newCount: [1, 0, 0, 0, 0], removedCount: [0, 1, 0, 0, 0] },
            { moment: '2026-03-25', newCount: [0, 0, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-29', newCount: [0, 2, 0, 0, 0], removedCount: [0, 0, 0, 1, 0] },
            { moment: '2026-04-01', newCount: [1, 0, 0, 1, 0], removedCount: [0, 0, 0, 0, 0] }
          ],
        },
      ],
    },
    {
      id: '31',
      imageRepository: 'repo.company.com/payments',
      imageName: 'payment-gateway',
      digests: [
        {
          imageTag: 'stable',
          imageDigest: 'sha256:333ccc',
          firstCriticalCount: 3, firstHighCount: 6, firstMediumCount: 12, firstLowCount: 34, firstUnknownCount: 1,
          history: [
            { moment: '2026-03-20', newCount: [0, 0, 0, 1, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-22', newCount: [1, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 1] },
            { moment: '2026-03-24', newCount: [0, 0, 0, 0, 1], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-27', newCount: [2, 0, 0, 0, 0], removedCount: [1, 0, 0, 0, 0] },
            { moment: '2026-03-31', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-04-03', newCount: [0, 0, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] }
          ],
        },
      ],
    },
    {
      id: '41',
      imageRepository: 'repo.company.com/analytics',
      imageName: 'analytics-engine',
      digests: [
        {
          imageTag: '2.0.1',
          imageDigest: 'sha256:444ddd',
          firstCriticalCount: 3, firstHighCount: 6, firstMediumCount: 12, firstLowCount: 34, firstUnknownCount: 1,
          history: [
            { moment: '2026-03-28', newCount: [0, 0, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-30', newCount: [1, 0, 0, 0, 0], removedCount: [0, 1, 0, 0, 0] },
            { moment: '2026-04-02', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] }
          ],
        },
      ],
    },
    {
      id: '51',
      imageRepository: 'repo.company.com/notify',
      imageName: 'notification-service',
      digests: [
        {
          imageTag: 'beta',
          imageDigest: 'sha256:555eee',
          firstCriticalCount: 3, firstHighCount: 6, firstMediumCount: 12, firstLowCount: 34, firstUnknownCount: 1,
          history: [
            { moment: '2026-03-21', newCount: [0, 0, 0, 0, 1], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-23', newCount: [1, 0, 0, 0, 0], removedCount: [0, 0, 1, 0, 0] },
            { moment: '2026-03-26', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-28', newCount: [0, 0, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] },
            { moment: '2026-03-31', newCount: [1, 0, 0, 0, 0], removedCount: [0, 0, 0, 1, 0] },
            { moment: '2026-04-03', newCount: [88, 88, 88, 88, 88], removedCount: [88, 88, 88, 88, 88] }
          ],
        },
      ],
    },
  ];

  dataExtendedDtos: VrHistoryDenormalizedDto[] = [];

  isLoading: boolean = false;

  trivyTableColumns: TrivyTableColumn[] = [
    {
      field: 'imageRepository',
      header: 'Repository',
      isFilterable: true,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 220px; max-width: 220px;',
      renderType: 'standard',
    },
    {
      field: 'imageName',
      header: 'Image',
      isFilterable: true,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 210px; max-width: 210px;',
      renderType: 'standard',
    },
    {
      field: 'imageTag',
      header: 'Tag',
      isFilterable: true,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 115px; max-width: 115px;',
      renderType: 'standard',
    },
    {
      field: 'imageDigest',
      header: 'Image Digest',
      isFilterable: true,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 250px; max-width: 250px;',
      renderType: 'standard',
    },
    {
      field: 'vrsLegend',
      header: 'VRs',
      isFilterable: false,
      isSortable: false,
      multiSelectType: 'none',
      style: 'width: 120px; max-width: 120px;',
      renderType: 'doubleStackedSpans',
      extraFields: ['Last', 'First'],
    },
    {
      field: 'lastCriticalCount',
      header: 'C',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['0', 'firstCriticalCount', 'false', 'false'],
    },
    {
      field: 'lastHighCount',
      header: 'H',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['1', 'firstHighCount', 'false', 'false'],
    },
    {
      field: 'lastMediumCount',
      header: 'M',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['2', 'firstMediumCount', 'false', 'false'],
    },
    {
      field: 'lastLowCount',
      header: 'L',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['3', 'firstLowCount', 'false', 'false'],
    },
    {
      field: 'lastUnknownCount',
      header: 'U',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['4', 'firstUnknownCount', 'false', 'false'],
    },
    {
      field: 'history',
      header: 'Delta History',
      isFilterable: false,
      isSortable: false,
      multiSelectType: 'none',
      style: 'width: 120px; max-width: 120px;',
      renderType: 'miniChart',
    },
    {
      field: 'deltaLegend',
      header: 'Last Delta',
      isFilterable: false,
      isSortable: false,
      multiSelectType: 'none',
      style: 'width: 120px; max-width: 120px;',
      renderType: 'doubleStackedSpans',
      extraFields: ['New', 'Removed'],
    },
    {
      field: 'criticalNew',
      header: 'C-Dif',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 68px; max-width: 68px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['0', 'criticalRemoved', 'true', 'false'],
    },
    {
      field: 'highNew',
      header: 'H-Dif',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 68px; max-width: 68px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['1', 'highRemoved', 'true'],
    },
    {
      field: 'mediumNew',
      header: 'M-Dif',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 68px; max-width: 68px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['2', 'mediumRemoved', 'true'],
    },
    {
      field: 'lowNew',
      header: 'L-Dif',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 68px; max-width: 68px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['3', 'lowRemoved', 'true'],
    },
    {
      field: 'unknownNew',
      header: 'U-Dif',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 68px; max-width: 68px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['4', 'unknownRemoved', 'true'],
    },
  ];

  constructor() {
    this.dataExtendedDtos = this.denormalizeVrHistory(this.dataDtos)
  }

  ngOnInit(): void {

  }

  private denormalizeVrHistory(dataDtos: VrHistoryDto[]): VrHistoryDenormalizedDto[] {
    const result: VrHistoryDenormalizedDto[] = [];

    for (const dto of dataDtos) {
      for (const digest of dto.digests) {
        const history = digest.history ?? [];

        // 1. Find last history entry with any data
        let lastWithData: MiniBarChartDataDto | null = null;

        for (let i = history.length - 1; i >= 0; i--) {
          const h = history[i];
          const hasData =
            h.newCount.some(v => v !== 0) ||
            h.removedCount.some(v => v !== 0);

          if (hasData) {
            lastWithData = h;
            break;
          }
        }

        const src = lastWithData ?? {
          newCount: [],
          removedCount: []
        };

        // Extract new/removed counts
        const criticalNew   = src.newCount[0] ?? 0;
        const highNew       = src.newCount[1] ?? 0;
        const mediumNew     = src.newCount[2] ?? 0;
        const lowNew        = src.newCount[3] ?? 0;
        const unknownNew    = src.newCount[4] ?? 0;

        const criticalRemoved = src.removedCount[0] ?? 0;
        const highRemoved     = src.removedCount[1] ?? 0;
        const mediumRemoved   = src.removedCount[2] ?? 0;
        const lowRemoved      = src.removedCount[3] ?? 0;
        const unknownRemoved  = src.removedCount[4] ?? 0;

        // 2. Compute lastXyzCount by accumulating all history entries
        const sumForIndex = (index: number) =>
          history.reduce((acc, h) => {
            const n = h.newCount[index] ?? 0;
            const r = h.removedCount[index] ?? 0;
            return acc + n - r;
          }, 0);

        const lastCriticalCount = digest.firstCriticalCount + sumForIndex(0);
        const lastHighCount     = digest.firstHighCount     + sumForIndex(1);
        const lastMediumCount   = digest.firstMediumCount   + sumForIndex(2);
        const lastLowCount      = digest.firstLowCount      + sumForIndex(3);
        const lastUnknownCount  = digest.firstUnknownCount  + sumForIndex(4);

        // 3. Push denormalized row
        result.push({
          id: dto.id,
          imageRepository: dto.imageRepository,
          imageName: dto.imageName,

          imageTag: digest.imageTag,
          imageDigest: digest.imageDigest,

          firstCriticalCount: digest.firstCriticalCount,
          firstHighCount: digest.firstHighCount,
          firstMediumCount: digest.firstMediumCount,
          firstLowCount: digest.firstLowCount,
          firstUnknownCount: digest.firstUnknownCount,

          lastCriticalCount,
          lastHighCount,
          lastMediumCount,
          lastLowCount,
          lastUnknownCount,

          criticalNew,
          highNew,
          mediumNew,
          lowNew,
          unknownNew,

          criticalRemoved,
          highRemoved,
          mediumRemoved,
          lowRemoved,
          unknownRemoved,

          history
        });
      }
    }

    return result;
  }
}
