export interface MultiHeaderAction {
  label: string;
  enabledIfRowSelected?: boolean;
  enabledIfDataLoaded?: boolean;
  icon?: string;
  specialAction?: 'Go to Detailed \u29C9' | 'Clear Selection' | 'Clear Sort/Filters' | 'Collapse All';
}

export interface Column {
  field: string;
  header: string;
  customExportHeader?: string;
}

export interface ExportColumn {
  title: string;
  dataKey: string;
}

export interface TrivyTableColumn extends Column {
  isSortable: boolean;
  isSortIconVisible?: boolean;
  isFilterable: boolean;
  isCounter?: boolean;
  style: string;
  multiSelectType: 'none' | 'namespaces' | 'severities' | 'booleans';
  renderType:
    | 'standard'
    | 'severityBadge'
    | 'severityMultiTags'
    | 'severityValue'
    | 'imageNameTag'
    | 'link'
    | 'date'
    | 'dateTime'
    | 'eosl'
    | 'semaphore'
    | 'multiline'
    | 'action'
    | 'boolean'
    | 'unPascalCase'
    | 'compareStacked'
    | 'counter'
    | 'miniChart'
    | 'severityDifValue'
    | 'doubleStackedSpans'
    | 'doubleSeverityDifValue'
    | 'imageFullAndDigest'
    | 'severityStackedBadge'
    | 'dateTimeStacked'
    | 'compareDateStacked';
  extraFields?: string[];
}

export interface TrivyFilterData {
  namespaceName?: string | null;
  selectedSeverityIds: number[];
}

export interface TrivyTableExpandRowData<TData> {
  rowKey: TData;
  colStyles: { [klass: string]: any }[];
  headerDef?: {
    label: string;
    class?: string;
  }[];
  details: {
    label: string;
    class?: string;
    buttonLink?: string;
    badge?: string;
    localTime?: string;
    cron?: string;
    url?: {
      text: string;
      link: string;
    };
  }[][];
}

export interface SelectedDtosEvent<TData> {
  selectedDtos: TData[];
  source: 'user' | 'programmatic';
}
