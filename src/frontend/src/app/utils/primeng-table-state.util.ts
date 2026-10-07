export class PrimengTableStateUtil {
  public static clearTableFilters(tableState: any) {
    this.clearFilters(tableState.filters);
  }

  public static clearFilters(tableFilters: any): any {
    for (const filter in tableFilters) {
      if (tableFilters[filter] && tableFilters[filter].length > 0) {
        tableFilters[filter] = [tableFilters[filter][0]];
        tableFilters[filter].forEach((item: any) => {
          if (item.matchMode === 'in') {
            item.value = [];
          } else {
            item.value = '';
          }
        });
      }
    }
  }

  public static clearTableMultiSort(tableState: any) {
    tableState.multiSortMeta = undefined;
  }

  public static clearTableColumnWidths(tableState: any) {
    tableState.columnWidths = '';
  }

  public static clearTableColumnOrder(tableState: any) {
    tableState.columnOrder = undefined;
  }

  public static clearTableSelection(tableState: any) {
    if (tableState.hasOwnProperty('selection')) {
      tableState.selection = undefined;
    }
  }

  public static clearTableExpandedRows(tableState: any) {
    if (tableState.hasOwnProperty('expandedRowKeys')) {
      tableState.expandedRowKeys = undefined;
    }
  }
}
