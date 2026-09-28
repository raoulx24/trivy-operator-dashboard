import { TrivyTableColumn } from '../../ui-elements/trivy-table/trivy-table.types';

export const namespacedColumns: readonly TrivyTableColumn[] = [
  {
    field: 'resourceNamespace',
    header: 'NS',
    isFilterable: true,
    isSortable: true,
    multiSelectType: 'namespaces',
    style: 'width: 130px; max-width: 130px;',
    renderType: 'standard',
  },
];

export const namespacedArrayColumns: readonly TrivyTableColumn[] = [
  {
    field: '__namespaceNamesSort',
    header: 'NS',
    isFilterable: true,
    isSortable: true,
    multiSelectType: 'namespacesArrays',
    style: 'width: 130px; max-width: 130px;',
    renderType: 'array',
    extraFields: ['namespaceNames'],
  },
];

