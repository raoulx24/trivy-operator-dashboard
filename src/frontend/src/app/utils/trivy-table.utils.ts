import { ExportColumn, TrivyTableColumn } from '../ui-elements/trivy-table/trivy-table.types';

export class TrivyTableUtils {
  public static convertFromTableColumnToExportColumn(columns: TrivyTableColumn[]): ExportColumn[] {
    if (!columns) {
      return [];
    }
    return columns.map((x) => {
      return { dataKey: x.field, title: x.header };
    });
  }
}
