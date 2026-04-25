import { Component, computed, effect, input, model, output, signal } from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { TrivyReportComparable, TrivyReportComparableDetail } from '../../trivy-reports/abstracts/trivy-report';
import { NamespaceImageSelectorComponent } from '../namespace-image-selector/namespace-image-selector.component';
import { NamespacedImageDto } from '../namespace-image-selector/namespace-image-selector.types';
import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../trivy-table/trivy-table.types';
import { FormsModule } from '@angular/forms';
import { NgClass } from '@angular/common';
import { LocalStorageUtils } from '../../utils/local-storage.utils';

type TrivyReportDetailComparedDto = TrivyReportComparableDetail & {
  first?: boolean;
  second?: boolean;
  modified?: boolean;
};

@Component({
  selector: 'app-generic-reports-compare',
  imports: [NamespaceImageSelectorComponent, TrivyTableComponent, ButtonModule, ToggleSwitchModule, FormsModule, NgClass,],
  templateUrl: './generic-reports-compare.component.html',
  styleUrl: './generic-reports-compare.component.scss',
})
export class GenericReportsCompareComponent<
  TTrivyReportComparableDto extends TrivyReportComparable<TTrivyReportDetailComparableDto>,
  TTrivyReportDetailComparableDto extends TrivyReportComparableDetail,
> {
  dataDtos = input.required<TTrivyReportComparableDto[] | undefined>();
  comparedTableColumns = input.required<TrivyTableColumn[]>();
  namespacedImageDtos = input.required<NamespacedImageDto[] | undefined>();
  walkingIsEnabled = input<boolean>(false);

  firstSelectedTrivyReportId = model<string | undefined>();
  secondSelectedTrivyReportId = model<string | undefined>();

  protected showOnlyModified = signal<boolean>(false);

  protected fullTrivyReportDetailsCompared = signal<TrivyReportDetailComparedDto[]>([]);


  compareIsCollapseAllVisible = input<boolean | undefined>(false);
  compareIsResetFiltersVisible = input<boolean | undefined>(false);
  compareStateKey = input<string | undefined>(undefined);
  compareExtraClasses = input<string | undefined>(undefined);

  namespacePlaceholder = input<string>('Select namespace');
  imagePlaceholder = input<string>('Select image');

  // Indicates that dataDtos are just minimal for selection, but not for comparison
  isDependantOnExternalData = input<boolean>(false);
  firstSelectedDto = input<TTrivyReportComparableDto | undefined>();
  secondSelectedDto = input<TTrivyReportComparableDto | undefined>();

  firstInSelectorLonger = input<boolean>(false);

  firstDtoRequested = output<string>();
  secondDtoRequested = output<string>();

  protected isWalkingEnabledInternal = computed(() => {
    return this.walkingIsEnabled() && !this.isDependantOnExternalData();
  })

  protected canWalkLeft = computed(() => this.canWalk('left'));
  protected canWalkRight = computed(() => this.canWalk('right'))

  protected trivyReportDetailsCompared = computed(() =>
    this.showOnlyModified()
      ? this.fullTrivyReportDetailsCompared().filter((x) => x.modified)
      : this.fullTrivyReportDetailsCompared()
  );

  // now reactive, no mirrors
  private _groupedFields: (keyof TTrivyReportDetailComparableDto)[] = [];

  constructor() {
    // grouped fields react to column changes
    effect(() => {
      this._groupedFields = this.comparedTableColumns()
        .filter((col) => col.renderType.toLowerCase().includes('stacked'))
        .map((col) => col.field as keyof TTrivyReportDetailComparableDto);
    });

    // single effect orchestrating everything
    effect(() => {
      const isDep = this.isDependantOnExternalData();
      const data = this.dataDtos();
      const firstId = this.firstSelectedTrivyReportId();
      const secondId = this.secondSelectedTrivyReportId();
      const firstDto = this.firstSelectedDto();
      const secondDto = this.secondSelectedDto();
      const showOnlyModified = this.showOnlyModified();

      if (isDep) {
        if (firstId && !firstDto) {
          this.firstDtoRequested.emit(firstId);
        }
        if (secondId && !secondDto) {
          this.secondDtoRequested.emit(secondId);
        }
      }

      this.compareSelectedTrivyReports({
        isDependantOnExternalData: isDep,
        dataDtos: data,
        firstSelectedTrivyReportId: firstId,
        secondSelectedTrivyReportId: secondId,
        firstSelectedDto: firstDto,
        secondSelectedDto: secondDto,
      });
    });

    effect(() => {
      const showOnlyModified = this.showOnlyModified();
      const keyName = LocalStorageUtils.toCamelCase(`${this.compareStateKey()}.Show Only Modified`);
      localStorage.setItem(keyName, showOnlyModified.toString());
    });
  }

  ngOnInit() {
    if (this.compareStateKey()) {
      const keyName = LocalStorageUtils.toCamelCase(`${this.compareStateKey()}.Show Only Modified`);
      this.showOnlyModified.set(LocalStorageUtils.getBoolKeyValue(keyName) ?? true);
    }
  }

  private compareSelectedTrivyReports(args: {
    isDependantOnExternalData: boolean;
    dataDtos?: TTrivyReportComparableDto[];
    firstSelectedTrivyReportId?: string;
    secondSelectedTrivyReportId?: string;
    firstSelectedDto?: TTrivyReportComparableDto;
    secondSelectedDto?: TTrivyReportComparableDto;
  }) {
    const {
      isDependantOnExternalData,
      dataDtos,
      firstSelectedTrivyReportId,
      secondSelectedTrivyReportId,
      firstSelectedDto,
      secondSelectedDto,
    } = args;

    if ((!dataDtos && !isDependantOnExternalData) || (!firstSelectedTrivyReportId && !secondSelectedTrivyReportId)) {
      this.fullTrivyReportDetailsCompared.set([]);
      return;
    }

    const detailSet = new Map<string, TrivyReportDetailComparedDto>();

    const firstDto = isDependantOnExternalData
      ? firstSelectedDto
      : dataDtos?.find((tr) => tr.uid === firstSelectedTrivyReportId);

    if (firstDto) {
      firstDto.details?.forEach((detail) => {
        const existing = detailSet.get(detail.matchKey);
        if (existing) {
          this.mergeValues(existing, detail, true);
        } else {
          const clone: TrivyReportDetailComparedDto = { ...detail, first: true };
          this._groupedFields.forEach((field) => {
            const value = this.getPropertyAsString(clone, field);
            if (value) {
              (clone as any)[field] = value;
            }
          });
          detailSet.set(detail.matchKey, clone);
        }
      });
    }

    const secondDto = isDependantOnExternalData
      ? secondSelectedDto
      : dataDtos?.find((tr) => tr.uid === secondSelectedTrivyReportId);

    if (secondDto) {
      secondDto.details?.forEach((detail) => {
        const existing = detailSet.get(detail.matchKey);
        if (existing) {
          existing.second = true;
          this.mergeValues(existing, detail, false);
        } else {
          const clone: TrivyReportDetailComparedDto = { ...detail, second: true };
          this._groupedFields.forEach((field) => {
            const value = this.getPropertyAsString(clone, field);
            if (value) {
              (clone as any)[field] = value;
            }
          });
          detailSet.set(detail.matchKey, clone);
        }
      });
    }

    detailSet.forEach((item) => {
      if (firstSelectedTrivyReportId) {
        item.first = item.first ?? false;
      }
      if (secondSelectedTrivyReportId) {
        item.second = item.second ?? false;
      }
    });

    const compared = Array.from(detailSet.values());
    compared.forEach((x) => {
      x.modified = x.modified || x.first !== x.second;
    });

    this.fullTrivyReportDetailsCompared.set(compared);
  }

  swapFirstAndSecond() {
    const first = this.firstSelectedTrivyReportId();
    const second = this.secondSelectedTrivyReportId();
    if (!first || !second) return;

    this.firstSelectedTrivyReportId.set(second);
    this.secondSelectedTrivyReportId.set(first);
  }

  walk(direction: 'left' | 'right') {
    const list = this.namespacedImageDtos();
    if (!list) return;

    const firstId = this.firstSelectedTrivyReportId();
    const secondId = this.secondSelectedTrivyReportId();
    if (!firstId || !secondId) return;

    const firstIndex = list.findIndex(x => x.uid === firstId);
    const secondIndex = list.findIndex(x => x.uid === secondId);

    // Template guarantees adjacency, so we don't validate it here.

    let newFirstIndex: number;
    let newSecondIndex: number;

    if (direction === 'left') {
      newFirstIndex = firstIndex - 1;
      newSecondIndex = firstIndex;
    } else {
      newFirstIndex = secondIndex;
      newSecondIndex = secondIndex + 1;
    }

    this.firstSelectedTrivyReportId.set(list[newFirstIndex].uid);
    this.secondSelectedTrivyReportId.set(list[newSecondIndex].uid);
  }

    private mergeValues(
    existing: TrivyReportDetailComparedDto,
    incoming: TrivyReportDetailComparedDto,
    isFirst: boolean,
  ): void {
    let modified = false;

    this._groupedFields.forEach((field) => {
      const existingValue = this.getPropertyAsString(existing, field);
      const incomingValue = this.getPropertyAsString(incoming, field);

      if (existingValue === incomingValue) return;

      const [firstRaw = '', secondRaw = ''] = existingValue?.split('|') ?? [];
      const firstPart = firstRaw.split('__').filter(Boolean);
      const secondPart = secondRaw.split('__').filter(Boolean);
      const incomingParts = incomingValue?.split('__').filter(Boolean) ?? [];

      const mergedFirst = isFirst ? Array.from(new Set([...firstPart, ...incomingParts])).sort() : firstPart;
      const mergedSecond = !isFirst ? Array.from(new Set([...secondPart, ...incomingParts])).sort() : secondPart;

      (existing as any)[field] = [
        mergedFirst.length ? mergedFirst.join('__') : '',
        mergedSecond.length ? mergedSecond.join('__') : '',
      ]
        .filter(Boolean)
        .join('|');

      if (mergedFirst.join() !== mergedSecond.join()) {
        modified = true;
      }
    });

    existing.modified = modified;
  }

  private getPropertyAsString(dto: any, key: string | number | symbol): string | undefined {
    const value = dto[key];
    return value != null ? value.toString() : undefined;
  }

  private canWalk(direction: 'left' | 'right'): boolean {
    const list = this.namespacedImageDtos();
    if (!list) return false;

    const firstId = this.firstSelectedTrivyReportId();
    const secondId = this.secondSelectedTrivyReportId();
    if (!firstId || !secondId) return false;

    const firstIndex = list.findIndex(x => x.uid === firstId);
    const secondIndex = list.findIndex(x => x.uid === secondId);

    if (direction === 'left') {
      return firstIndex > 0;
    }

    return secondIndex < list.length - 1;
  }
}
