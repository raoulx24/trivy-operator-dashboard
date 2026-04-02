import { Component, effect, inject, OnInit } from '@angular/core';

import { TableModule } from 'primeng/table';
import { TrivyTableComponent } from '../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../ui-elements/trivy-table/trivy-table.types';
import { TestDto } from './tests.types';
import {MiniBarChartDataDto} from "../ui-elements/mini-bar-chart/mini-bar-chart.types";

@Component({
  selector: 'app-tests',
  imports: [TableModule, TrivyTableComponent],
  templateUrl: './tests.component.html',
  styleUrl: './tests.component.scss',
})
export class TestsComponent implements OnInit {
  readonly minHistoryDays = 14;

  dataDtos?: TestDto[] = [
    {
      id: '01',
      imageName: 'image name',
      imageTag: 'latest',
      imageDigest: 'sha256: xyz',
      imageRepository: 'myrepo.com',

      criticalNew: 0, highNew: 1, mediumNew: 0, lowNew: 0, unknownNew: 0,
      criticalRemoved: 0, highRemoved: 0, mediumRemoved: 0, lowRemoved: 0, unknownRemoved: 0,

      history: [
        { moment: '2026-03-20', newCount: [0, 1, 0, 2, 0], removedCount: [2, 1, 0, 3, 0] },
        { moment: '2026-03-24', newCount: [2, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-25', newCount: [1, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-27', newCount: [0, 2, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-02', newCount: [1, 1, 1, 1, 1], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-04-03', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
      ],
    },
    {
      id: '02',
      imageName: 'backend-service',
      imageTag: 'latest',
      imageDigest: 'sha256:111aaa',
      imageRepository: 'repo.company.com/backend',

      criticalNew: 1, highNew: 1, mediumNew: 1, lowNew: 1, unknownNew: 1,
      criticalRemoved: 1, highRemoved: 0, mediumRemoved: 0, lowRemoved: 0, unknownRemoved: 0,

      history: [
        { moment: '2026-03-21', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 1, 0] },
        { moment: '2026-03-23', newCount: [1, 0, 0, 0, 0], removedCount: [0, 0, 1, 0, 0] },
        { moment: '2026-03-26', newCount: [0, 0, 2, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-30', newCount: [0, 1, 1, 0, 0], removedCount: [1, 0, 0, 0, 0] },
        { moment: '2026-04-02', newCount: [0, 0, 0, 1, 0], removedCount: [0, 0, 0, 0, 0] }
      ],
    },
    {
      id: '03',
      imageName: 'frontend-ui',
      imageTag: '1.4.2',
      imageDigest: 'sha256:222bbb',
      imageRepository: 'repo.company.com/frontend',

      // last non-zero newCount: [0,2,0,0,0]
      criticalNew: 0, highNew: 2, mediumNew: 0, lowNew: 0, unknownNew: 0,
      // last non-zero removedCount: [0,0,1,0,0]
      criticalRemoved: 0, highRemoved: 0, mediumRemoved: 1, lowRemoved: 0, unknownRemoved: 0,

      history: [
        { moment: '2026-03-22', newCount: [1, 0, 0, 0, 0], removedCount: [0, 1, 0, 0, 0] },
        { moment: '2026-03-25', newCount: [0, 0, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-29', newCount: [0, 2, 0, 0, 0], removedCount: [0, 0, 0, 1, 0] },
        { moment: '2026-04-01', newCount: [1, 0, 0, 1, 0], removedCount: [0, 0, 0, 0, 0] }
      ],
    },
    {
      id: '04',
      imageName: 'payment-gateway',
      imageTag: 'stable',
      imageDigest: 'sha256:333ccc',
      imageRepository: 'repo.company.com/payments',

      // last non-zero newCount: [0,0,2,0,0]
      criticalNew: 0, highNew: 0, mediumNew: 2, lowNew: 0, unknownNew: 0,
      // last non-zero removedCount: [1,0,0,0,0]
      criticalRemoved: 1, highRemoved: 0, mediumRemoved: 0, lowRemoved: 0, unknownRemoved: 0,

      history: [
        { moment: '2026-03-20', newCount: [0, 0, 0, 1, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-22', newCount: [1, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 1] },
        { moment: '2026-03-24', newCount: [0, 0, 0, 0, 1], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-27', newCount: [2, 0, 0, 0, 0], removedCount: [1, 0, 0, 0, 0] },
        { moment: '2026-03-31', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-04-03', newCount: [0, 0, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] }
      ],
    },
    {
      id: '05',
      imageName: 'analytics-engine',
      imageTag: '2.0.1',
      imageDigest: 'sha256:444ddd',
      imageRepository: 'repo.company.com/analytics',

      // last non-zero newCount: [0,0,0,4,0]
      criticalNew: 0, highNew: 0, mediumNew: 0, lowNew: 4, unknownNew: 0,
      // last non-zero removedCount: [0,0,0,0,2]
      criticalRemoved: 0, highRemoved: 0, mediumRemoved: 0, lowRemoved: 0, unknownRemoved: 2,

      history: [
        { moment: '2026-03-28', newCount: [0, 0, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-30', newCount: [1, 0, 0, 0, 0], removedCount: [0, 1, 0, 0, 0] },
        { moment: '2026-04-02', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] }
      ],
    },
    {
      id: '06',
      imageName: 'notification-service',
      imageTag: 'beta',
      imageDigest: 'sha256:555eee',
      imageRepository: 'repo.company.com/notify',

      // last non-zero newCount: [0,1,0,0,0]
      criticalNew: 0, highNew: 1, mediumNew: 0, lowNew: 0, unknownNew: 0,
      // last non-zero removedCount: [0,0,0,1,0]
      criticalRemoved: 0, highRemoved: 0, mediumRemoved: 0, lowRemoved: 1, unknownRemoved: 0,

      history: [
        { moment: '2026-03-21', newCount: [0, 0, 0, 0, 1], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-23', newCount: [1, 0, 0, 0, 0], removedCount: [0, 0, 1, 0, 0] },
        { moment: '2026-03-26', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-28', newCount: [0, 0, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { moment: '2026-03-31', newCount: [1, 0, 0, 0, 0], removedCount: [0, 0, 0, 1, 0] },
        { moment: '2026-04-03', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] }
      ],
    },
  ];

  isLoading: boolean = false;

  trivyTableColumns: TrivyTableColumn[] = [
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
      style: 'width: 550px; max-width: 550px;',
      renderType: 'standard',
    },
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
      field: 'history',
      header: 'History',
      isFilterable: false,
      isSortable: false,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'miniChart',
    },
    {
      field: 'criticalNew',
      header: 'C',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['0', 'criticalRemoved'],
    },
    {
      field: 'highNew',
      header: 'H',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['1', 'highRemoved'],
    },
    {
      field: 'mediumNew',
      header: 'M',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['2', 'mediumRemoved'],
    },
    {
      field: 'lowNew',
      header: 'L',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['3', 'lowRemoved'],
    },
    {
      field: 'unknownNew',
      header: 'U',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['4', 'unknownRemoved'],
    },
  ];

  constructor() {
    //effect(() => {
    //   const dtos = this.dataDtos();
      const dtos = this.dataDtos;

      if (!dtos) return;

      for (const dto of dtos) {
        if (!dto.history || dto.history.length === 0) continue;

        // 1. Find the last history entry that has data
        let lastWithData: MiniBarChartDataDto | null = null;

        for (let i = dto.history.length - 1; i >= 0; i--) {
          const h = dto.history[i];
          const hasData =
              h.newCount.some(v => v !== 0) ||
              h.removedCount.some(v => v !== 0);

          if (hasData) {
            lastWithData = h;
            break;
          }
        }

        // If none have data, treat as all zeros
        const src = lastWithData ?? {
          newCount: [],
          removedCount: []
        };

        // 2. Mutate dto.* fields
        dto.criticalNew   = src.newCount[0] ?? 0;
        dto.highNew       = src.newCount[1] ?? 0;
        dto.mediumNew     = src.newCount[2] ?? 0;
        dto.lowNew        = src.newCount[3] ?? 0;
        dto.unknownNew    = src.newCount[4] ?? 0;

        dto.criticalRemoved = src.removedCount[0] ?? 0;
        dto.highRemoved     = src.removedCount[1] ?? 0;
        dto.mediumRemoved   = src.removedCount[2] ?? 0;
        dto.lowRemoved      = src.removedCount[3] ?? 0;
        dto.unknownRemoved  = src.removedCount[4] ?? 0;
      }
    // });

  }

  ngOnInit(): void {

  }

}
