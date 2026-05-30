import { Component, computed, effect, inject, input, model, output, signal } from '@angular/core';

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
import { ComparableWorkingItem, TrivyReportCompareService } from '../../services/trivy-report-compare.service';

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

  private trivyReportCompareService = inject(TrivyReportCompareService);

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

      let compared = this.trivyReportCompareService.compareReports({
        isDependantOnExternalData: isDep,
        dataDtos: data,
        firstSelectedTrivyReportId: firstId,
        secondSelectedTrivyReportId: secondId,
        firstSelectedDto: firstDto,
        secondSelectedDto: secondDto,
        groupedFields: this._groupedFields,
      });

      compared = this.postProcessComparedData(compared);

      this.fullTrivyReportDetailsCompared.set(showOnlyModified ? compared.filter((x) => x.modified) : compared);
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

  // -------------------------
  // walk methods
  // -------------------------

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

  swapFirstAndSecond() {
    const first = this.firstSelectedTrivyReportId();
    const second = this.secondSelectedTrivyReportId();
    if (!first || !second) return;

    this.firstSelectedTrivyReportId.set(second);
    this.secondSelectedTrivyReportId.set(first);
  }

  private postProcessComparedData<
    TDetail extends TrivyReportComparableDetail
  >(
    compared: ComparableWorkingItem<TDetail>[]
  ): ComparableWorkingItem<TDetail>[] {

    const severityFields = this.comparedTableColumns()
      .filter(col => col.renderType === 'severityStackedBadge')
      .map(col => col.field as string);

    if (severityFields.length === 0) return compared;

    return compared.map(row => {
      const clone = { ...row };
      const rowAny = clone as Record<string, any>;

      for (const field of severityFields) {
        const raw = rowAny[field];
        if (typeof raw !== 'string') continue;

        const parts = raw.split('|');

        if (parts.length === 1) {
          rowAny[field] = `${parts[0]}|9`;
        }
        if (parts.length === 2 && parts[0] === 'N/A') {
          rowAny[field] = `8|${parts[1]}`;
        }
        if (parts.length === 2 && parts[1] === 'N/A') {
          rowAny[field] = `${parts[0]}|8`;
        }
      }

      return clone;
    });
  }

}
