import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

export const logMessageColumns: readonly TrivyTableColumn[] = [
  {
    field: 'timestamp',
    header: 'Time',
    isFilterable: false,
    isSortable: true,
    style: 'width: 155px; max-width: 155px;',
    renderType: 'dateTime',
  },
  {
    field: 'level',
    header: 'Level',
    isFilterable: true,
    isSortable: true,
    tagSet: 'logLevel',
    style: 'width: 110px; max-width: 110px;',
    renderType: 'tag',
  },
  {
    field: 'source',
    header: 'Source',
    isFilterable: true,
    isSortable: true,
    style: 'width: 220px; max-width: 220px;',
    renderType: 'standard',
  },
  {
    field: 'code',
    header: 'Code',
    isFilterable: true,
    isSortable: true,
    style: 'width: 150px; max-width: 150px;',
    renderType: 'standard',
  },
  {
    field: 'message',
    header: 'Message',
    isFilterable: true,
    isSortable: true,
    style: 'width: 500px; max-width: 500px; white-space: normal;',
    renderType: 'standard',
  },
];
