import { Pipe, PipeTransform } from '@angular/core';
import { TrivyExpandTableOptions } from '../ui-elements/trivy-table/trivy-table.types';

@Pipe({
  name: 'cellRowArray',
  standalone: true,
})
export class CellRowArrayPipe<TData> implements PipeTransform {
  transform(rowData: TData, trivyExpandTableOptions: TrivyExpandTableOptions<TData>): number[] {
    return trivyExpandTableOptions.getRowsArray(rowData);
  }
}
