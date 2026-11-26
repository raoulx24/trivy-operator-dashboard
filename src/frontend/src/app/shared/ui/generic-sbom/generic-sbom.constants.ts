import { TrivyTableColumn } from '../trivy-table/trivy-table.types';

export const genericSbomReportDetailColumns: readonly TrivyTableColumn[] = [
  {
    field: 'name',
    header: 'Name',
    isFilterable: true,
    isSortable: true,
    multiSelectType: 'none',
    style: 'white-space: nowrap; text-overflow: ellipsis; overflow: hidden; width: 290px;',
    renderType: 'standard',
  },
  {
    field: 'version',
    header: 'Version',
    isFilterable: true,
    isSortable: true,
    multiSelectType: 'none',
    style: 'width: 130px; max-width: 130px;',
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
    field: 'level',
    header: 'Level',
    isFilterable: true,
    isSortable: true,
    multiSelectType: 'none',
    style: 'width: 130px; max-width: 130px;',
    renderType: 'standard',
  },
];

// the package list next to the navigator / graph: narrower than the full detail columns, all severity counts kept
const slimDetailColumnStyles: Record<string, string> = {
  name: 'white-space: nowrap; text-overflow: ellipsis; overflow: hidden; min-width: 10rem;',
  version: 'white-space: nowrap; text-overflow: ellipsis; overflow: hidden; width: 8rem; max-width: 8rem;',
  criticalCount: 'width: 3.5rem; max-width: 3.5rem;',
  highCount: 'width: 3.5rem; max-width: 3.5rem;',
  mediumCount: 'width: 3.5rem; max-width: 3.5rem;',
  lowCount: 'width: 3.5rem; max-width: 3.5rem;',
  unknownCount: 'width: 3.5rem; max-width: 3.5rem;',
  level: 'width: 7rem; max-width: 7rem;',
};

export const genericSbomReportSlimDetailColumns: readonly TrivyTableColumn[] = genericSbomReportDetailColumns
  .filter((x) => x.field in slimDetailColumnStyles)
  .map((x) => ({ ...x, style: slimDetailColumnStyles[x.field] }));

// the navigator's "used by" / "depends on" tables: static, except the dive button of each row
const navigatorColumns = (diveField: string, diveIcon: string, diveLabel: string): readonly TrivyTableColumn[] => [
  ...genericSbomReportDetailColumns
    .filter((x) => x.field in slimDetailColumnStyles && x.field !== 'level')
    .map((x) => ({ ...x, isFilterable: false, style: slimDetailColumnStyles[x.field] })),
  {
    field: diveField,
    // also the aria label of the icon-only button
    header: diveLabel,
    isFilterable: false,
    isSortable: false,
    actionIcon: diveIcon,
    style: 'width: 5.5rem; max-width: 5.5rem; text-align: center;',
    renderType: 'action',
  },
];

export const sbomNavigatorDiveOutField = 'diveOut';
export const sbomNavigatorDiveInField = 'diveIn';
export const sbomNavigatorUsedByColumns = navigatorColumns(sbomNavigatorDiveOutField, 'pi pi-arrow-up', 'Dive out');
export const sbomNavigatorDependsOnColumns = navigatorColumns(sbomNavigatorDiveInField, 'pi pi-arrow-down', 'Dive in');

export const genericSbomReportComparedColumns: readonly TrivyTableColumn[] = [
  {
    field: 'first',
    header: '1st',
    isFilterable: false,
    isSortable: true,
    multiSelectType: 'none',
    style: 'width: 90px; max-width: 90px;',
    renderType: 'boolean',
    extraFields: ['firstCount'],
  },
  {
    field: 'second',
    header: '2nd',
    isFilterable: false,
    isSortable: true,
    multiSelectType: 'none',
    style: 'width: 90px; max-width: 90px;',
    renderType: 'boolean',
    extraFields: ['secondCount'],
  },
  {
    field: 'name',
    header: 'Name',
    isFilterable: true,
    isSortable: true,
    multiSelectType: 'none',
    style: 'white-space: nowrap; text-overflow: ellipsis; overflow: hidden; width: 290px;',
    renderType: 'compareStacked',
  },
  {
    field: 'version',
    header: 'Version',
    isFilterable: true,
    isSortable: true,
    multiSelectType: 'none',
    style: 'width: 130px; max-width: 130px;',
    renderType: 'compareStacked',
  },
];
