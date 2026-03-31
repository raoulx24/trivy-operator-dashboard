import { Component, inject, OnInit } from '@angular/core';

import { TableModule } from 'primeng/table';
import { TrivyTableComponent } from '../ui-elements/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../ui-elements/trivy-table/trivy-table.types';
import { TestDto } from './tests.types';

@Component({
  selector: 'app-tests',
  imports: [TableModule, TrivyTableComponent],
  templateUrl: './tests.component.html',
  styleUrl: './tests.component.scss',
})
export class TestsComponent implements OnInit {
  dataDtos?: TestDto[] = [
    {
      imageName: 'image name',
      imageTag: 'latest',
      imageDigest: 'sha256: xyz',
      imageRepository: 'myrepo.com',

      criticalNew: 0, highNew: 1, mediumNew: 0, lowNew: 0, unknownNew: 0,
      criticalRemoved: 0, highRemoved: 0, mediumRemoved: 0, lowRemoved: 0, unknownRemoved: 0,

      history: [
        { label: '2026-03-02', newCount: [0, 0, 0, 0, 0], removedCount: [2, 0, 0, 0, 0] },
        { label: '2026-03-03', newCount: [2, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-04', newCount: [1, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-05', newCount: [0, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-06', newCount: [0, 2, 1, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-07', newCount: [0, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-08', newCount: [0, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-09', newCount: [1, 1, 1, 1, 1], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-10', newCount: [0, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-11', newCount: [0, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-12', newCount: [0, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-13', newCount: [0, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-14', newCount: [0, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-15', newCount: [0, 0, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
        { label: '2026-03-16', newCount: [0, 1, 0, 0, 0], removedCount: [0, 0, 0, 0, 0] },
      ],
    },

    {
      imageName: 'backend-service',
      imageTag: 'latest',
      imageDigest: 'sha256:111aaa',
      imageRepository: 'repo.company.com/backend',

      // last non-zero newCount: [1,1,1,1,1]
      criticalNew: 1, highNew: 1, mediumNew: 1, lowNew: 1, unknownNew: 1,
      // last non-zero removedCount: [1,0,0,0,0]
      criticalRemoved: 1, highRemoved: 0, mediumRemoved: 0, lowRemoved: 0, unknownRemoved: 0,

      history: [
        { label: '2026-03-01', newCount: [0,0,0,0,0], removedCount: [1,0,0,0,0] },
        { label: '2026-03-02', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-03', newCount: [1,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-04', newCount: [0,1,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-05', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-06', newCount: [1,1,1,1,1], removedCount: [0,0,0,0,0] }, // last new
        { label: '2026-03-07', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-08', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-09', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-10', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-11', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-12', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-13', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-14', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-15', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
      ],
    },

    // 2. Frontend UI
    {
      imageName: 'frontend-ui',
      imageTag: '1.4.2',
      imageDigest: 'sha256:222bbb',
      imageRepository: 'repo.company.com/frontend',

      // last non-zero newCount: [0,2,0,0,0]
      criticalNew: 0, highNew: 2, mediumNew: 0, lowNew: 0, unknownNew: 0,
      // last non-zero removedCount: [0,0,1,0,0]
      criticalRemoved: 0, highRemoved: 0, mediumRemoved: 1, lowRemoved: 0, unknownRemoved: 0,

      history: [
        { label: '2026-03-01', newCount: [0,0,0,0,0], removedCount: [0,1,0,0,0] },
        { label: '2026-03-02', newCount: [0,2,0,0,0], removedCount: [0,0,0,0,0] }, // last new
        { label: '2026-03-03', newCount: [0,0,0,0,0], removedCount: [0,0,1,0,0] }, // last removed
        { label: '2026-03-04', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-05', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-06', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-07', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-08', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-09', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-10', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-11', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-12', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-13', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-14', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-15', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
      ],
    },

    // 3. Payment Gateway
    {
      imageName: 'payment-gateway',
      imageTag: 'stable',
      imageDigest: 'sha256:333ccc',
      imageRepository: 'repo.company.com/payments',

      // last non-zero newCount: [0,0,2,0,0]
      criticalNew: 0, highNew: 0, mediumNew: 2, lowNew: 0, unknownNew: 0,
      // last non-zero removedCount: [1,0,0,0,0]
      criticalRemoved: 1, highRemoved: 0, mediumRemoved: 0, lowRemoved: 0, unknownRemoved: 0,

      history: [
        { label: '2026-03-01', newCount: [0,0,1,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-02', newCount: [0,0,2,0,0], removedCount: [0,0,0,0,0] }, // last new
        { label: '2026-03-03', newCount: [0,0,0,0,0], removedCount: [1,0,0,0,0] }, // last removed
        { label: '2026-03-04', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-05', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-06', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-07', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-08', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-09', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-10', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-11', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-12', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-13', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-14', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-15', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
      ],
    },

    // 4. Analytics Engine
    {
      imageName: 'analytics-engine',
      imageTag: '2.0.1',
      imageDigest: 'sha256:444ddd',
      imageRepository: 'repo.company.com/analytics',

      // last non-zero newCount: [0,0,0,4,0]
      criticalNew: 0, highNew: 0, mediumNew: 0, lowNew: 4, unknownNew: 0,
      // last non-zero removedCount: [0,0,0,0,2]
      criticalRemoved: 0, highRemoved: 0, mediumRemoved: 0, lowRemoved: 0, unknownRemoved: 2,

      history: [
        { label: '2026-03-01', newCount: [0,0,0,2,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-02', newCount: [0,0,0,4,0], removedCount: [0,0,0,0,0] }, // last new
        { label: '2026-03-03', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,2] }, // last removed
        { label: '2026-03-04', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-05', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-06', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-07', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-08', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-09', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-10', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-11', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-12', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-13', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-14', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-15', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
      ],
    },

    // 5. Notification Service
    {
      imageName: 'notification-service',
      imageTag: 'beta',
      imageDigest: 'sha256:555eee',
      imageRepository: 'repo.company.com/notify',

      // last non-zero newCount: [0,1,0,0,0]
      criticalNew: 0, highNew: 1, mediumNew: 0, lowNew: 0, unknownNew: 0,
      // last non-zero removedCount: [0,0,0,1,0]
      criticalRemoved: 0, highRemoved: 0, mediumRemoved: 0, lowRemoved: 1, unknownRemoved: 0,

      history: [
        { label: '2026-03-01', newCount: [0,1,0,0,0], removedCount: [0,0,0,0,0] }, // last new
        { label: '2026-03-02', newCount: [0,0,0,0,0], removedCount: [0,0,0,1,0] }, // last removed
        { label: '2026-03-03', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-04', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-05', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-06', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-07', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-08', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-09', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-10', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-11', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-12', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-13', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-14', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
        { label: '2026-03-15', newCount: [0,0,0,0,0], removedCount: [0,0,0,0,0] },
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

  ngOnInit(): void {

  }

}
