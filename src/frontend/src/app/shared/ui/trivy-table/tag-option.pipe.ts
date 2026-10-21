import { Pipe, PipeTransform } from '@angular/core';
import { TagOption, TagSetName, TrivyTableTagSets } from './trivy-table-tag-sets';

@Pipe({
  name: 'tagOption',
  standalone: true,
})
export class TagOptionPipe implements PipeTransform {
  transform(value: unknown, tagSet: TagSetName | undefined): TagOption {
    return TrivyTableTagSets.resolve(tagSet ?? 'semaphore', value);
  }
}
