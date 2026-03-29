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
        { label: '03-02', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-03', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-04', critical: 1, high: 0, medium: 0, low: 0, unknown: 0, total: 1 }, // small
        { label: '03-05', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-06', critical: 0, high: 2, medium: 1, low: 0, unknown: 0, total: 3 }, // small
        { label: '03-07', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-08', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-09', critical: 1, high: 1, medium: 1, low: 1, unknown: 1, total: 5 }, // full
        { label: '03-10', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-11', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-12', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-13', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-14', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-15', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-16', critical: 0, high: 1, medium: 0, low: 0, unknown: 0, total: 1 }, // small
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
        { label: '03-02', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-03', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-04', critical: 1, high: 0, medium: 0, low: 0, unknown: 0, total: 1 }, // small
        { label: '03-05', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-06', critical: 0, high: 2, medium: 1, low: 0, unknown: 0, total: 3 }, // small
        { label: '03-07', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-08', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-09', critical: 1, high: 1, medium: 1, low: -2, unknown: 1, total: 6 }, // full
        { label: '03-10', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-11', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-12', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-13', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-14', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-15', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-16', critical: 0, high: 1, medium: 0, low: 0, unknown: 0, total: 1 }, // small
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
        { label: '03-02', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-03', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-04', critical: 1, high: 0, medium: 0, low: 0, unknown: 0, total: 1 }, // small
        { label: '03-05', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-06', critical: 0, high: 2, medium: 1, low: 0, unknown: 0, total: 3 }, // small
        { label: '03-07', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-08', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-09', critical: 1, high: 1, medium: 1, low: 1, unknown: 1, total: 5 }, // full
        { label: '03-10', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-11', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-12', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-13', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-14', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-15', critical: 0, high: 0, medium: 0, low: 0, unknown: 0, total: 0 },
        { label: '03-16', critical: 0, high: 1, medium: 0, low: 0, unknown: 0, total: 1 }, // small
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
      field: 'criticalCount',
      header: 'C',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'severityValue',
      extraFields: ['0'],
    },
    {
      field: 'highCount',
      header: 'H',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px;',
      renderType: 'severityValue',
      extraFields: ['1'],
    },
    {
      field: 'mediumCount',
      header: 'M',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'severityValue',
      extraFields: ['2'],
    },
    {
      field: 'lowCount',
      header: 'L',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'severityValue',
      extraFields: ['3'],
    },
    {
      field: 'unknownCount',
      header: 'U',
      isFilterable: false,
      isSortable: true,
      multiSelectType: 'none',
      style: 'width: 50px; max-width: 50px;',
      renderType: 'severityValue',
      extraFields: ['4'],
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
  ];

  ngOnInit(): void {

  }

}
