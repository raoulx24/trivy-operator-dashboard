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

// the navigator's neighbours table (used by and depends on, in one): static, except the dive button of each row, whose
// arrow is the direction (up: used by, a dive out; down: depends on, a dive in); sorting on it groups the directions
export const sbomNavigatorDiveField = 'direction';
// next to the dive button: selects the row's package in the list on the left
export const sbomNavigatorSelectField = 'select';
export const sbomNavigatorColumns: readonly TrivyTableColumn[] = [
  {
    field: sbomNavigatorDiveField,
    // also the aria label of the icon-only button
    header: 'Dive',
    isFilterable: false,
    isSortable: true,
    multiSelectType: 'none',
    actionIconField: 'directionIcon',
    style: 'width: 5.5rem; max-width: 5.5rem; text-align: center;',
    renderType: 'action',
  },
  {
    field: sbomNavigatorSelectField,
    // also the aria label of the icon-only button
    header: 'Select',
    isFilterable: false,
    isSortable: false,
    multiSelectType: 'none',
    actionIcon: 'pi pi-arrow-left',
    // a package that is not in the list on the left (outside the dive) cannot be selected there
    actionDisabledField: 'isSelectDisabled',
    style: 'width: 5.5rem; max-width: 5.5rem; text-align: center;',
    renderType: 'action',
  },
  // no Level: it is the level in the list on the left (from the dive root), the dive arrow already tells the relation
  // to the shown package
  ...genericSbomReportDetailColumns
    .filter((x) => x.field in slimDetailColumnStyles && x.field !== 'level')
    .map((x) => ({ ...x, isFilterable: false, style: slimDetailColumnStyles[x.field] })),
];

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
