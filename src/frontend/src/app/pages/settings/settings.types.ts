export class ClearTablesOptions {
  dataKey: string = '';
  description: string = '';
  filters: boolean = false;
  sort: boolean = false;
  columnWidths: boolean = false;
  columnOrder: boolean = false;
  all: boolean = false;

  constructor(dataKey: string, description: string) {
    this.dataKey = dataKey;
    this.description = description;
  }
}

export interface SavedCsvFileName {
  dataKey: string;
  description: string;
  savedCsvName: string;
}

export interface TrivyReportConfig {
  id: string;
  name: string;
  backendEnabled: boolean;
  frontendEnabled: boolean;
}
