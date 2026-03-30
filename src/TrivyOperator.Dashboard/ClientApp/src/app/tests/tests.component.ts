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
      criticalCount: 0,
      highCount: 2,
      mediumCount: 10,
      lowCount: 6,
      unknownCount: 0,
      history: [
        { label: '2026-03-02', criticalCount: -2, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-03', criticalCount: 2, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-04', criticalCount: 1, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 }, // small
        { label: '2026-03-05', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-06', criticalCount: 0, highCount: 2, mediumCount: 1, lowCount: 0, unknownCount: 0 }, // small
        { label: '2026-03-07', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-08', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-09', criticalCount: 1, highCount: 1, mediumCount: 1, lowCount: 1, unknownCount: 1 }, // full
        { label: '2026-03-10', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-11', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-12', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-13', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-14', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-15', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-16', criticalCount: 0, highCount: 1, mediumCount: 0, lowCount: 0, unknownCount: 0 }, // small
      ],
    },
    {
      imageName: 'image name',
      imageTag: 'latest',
      imageDigest: 'sha256: xyz',
      imageRepository: 'myrepo.com',
      criticalCount: 2,
      highCount: -2,
      mediumCount: 6,
      lowCount: -2,
      unknownCount: 0,
      history: [
        { label: '2026-03-02', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-03', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-04', criticalCount: 1, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 }, // small
        { label: '2026-03-05', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-06', criticalCount: 0, highCount: 2, mediumCount: 1, lowCount: 0, unknownCount: 0 }, // small
        { label: '2026-03-07', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-08', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-09', criticalCount: 1, highCount: 1, mediumCount: 1, lowCount: -2, unknownCount: 1 }, // full
        { label: '2026-03-10', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-11', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-12', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-13', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-14', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-15', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-16', criticalCount: 0, highCount: 1, mediumCount: 0, lowCount: 0, unknownCount: 0 }, // small
      ],
    },
    {
      imageName: 'image name',
      imageTag: 'latest',
      imageDigest: 'sha256: xyz',
      imageRepository: 'myrepo.com',
      criticalCount: 0,
      highCount: 2,
      mediumCount: 10,
      lowCount: 6,
      unknownCount: 0,
      history: [
        { label: '2026-03-02', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-03', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-04', criticalCount: 1, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 }, // small
        { label: '2026-03-05', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-06', criticalCount: 0, highCount: 2, mediumCount: 1, lowCount: 0, unknownCount: 0 }, // small
        { label: '2026-03-07', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-08', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-09', criticalCount: 1, highCount: 1, mediumCount: 1, lowCount: 1, unknownCount: 1 }, // full
        { label: '2026-03-10', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-11', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-12', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-13', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-14', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-15', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-16', criticalCount: 0, highCount: 1, mediumCount: 0, lowCount: 0, unknownCount: 0 }, // small
      ],
    },
    {
      imageName: 'image name',
      imageTag: 'latest',
      imageDigest: 'sha256: xyz',
      imageRepository: 'myrepo.com',
      criticalCount: 0,
      highCount: 2,
      mediumCount: 10,
      lowCount: 6,
      unknownCount: 0,
      history: [
        { label: '2026-03-02', criticalCount: 6, highCount: 0, mediumCount: 1, lowCount: 0, unknownCount: -2 },
        { label: '2026-03-03', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-04', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 }, // small
        { label: '2026-03-05', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-06', criticalCount: 0, highCount: 2, mediumCount: 1, lowCount: 0, unknownCount: 0 }, // small
        { label: '2026-03-07', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-08', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-09', criticalCount: 1, highCount: 1, mediumCount: 1, lowCount: 1, unknownCount: 1 }, // full
        { label: '2026-03-10', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-11', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-12', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-13', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-14', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-15', criticalCount: 0, highCount: 0, mediumCount: 0, lowCount: 0, unknownCount: 0 },
        { label: '2026-03-16', criticalCount: 0, highCount: 1, mediumCount: 0, lowCount: 0, unknownCount: 0 }, // small
      ],
    }
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
      field: 'criticalCount',
      header: 'C',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'doubleSeverityDifValue',
      extraFields: ['0'],
    },
    {
      field: 'highCount',
      header: 'H',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px;',
      renderType: 'severityDifValue',
      extraFields: ['1'],
    },
    {
      field: 'mediumCount',
      header: 'M',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'severityDifValue',
      extraFields: ['2'],
    },
    {
      field: 'lowCount',
      header: 'L',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'severityDifValue',
      extraFields: ['3'],
    },
    {
      field: 'unknownCount',
      header: 'U',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'severityDifValue',
      extraFields: ['4'],
    },
  ];

  ngOnInit(): void {

  }

}
