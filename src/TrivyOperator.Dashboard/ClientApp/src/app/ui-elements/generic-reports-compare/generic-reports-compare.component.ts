import { Component, effect, input, model, OnInit, output } from '@angular/core';

import { TrivyReportComparable, TrivyReportComparableDetail } from '../../trivy-reports/abstracts/trivy-report';
import { NamespaceImageSelectorComponent } from '../namespace-image-selector/namespace-image-selector.component';
import { NamespacedImageDto } from '../namespace-image-selector/namespace-image-selector.types';
import { TrivyTableComponent } from '../trivy-table/trivy-table.component';
import { TrivyTableColumn } from '../trivy-table/trivy-table.types';

type TrivyReportDetailComparedDto = TrivyReportComparableDetail & { first?: boolean; second?: boolean };

@Component({
  selector: 'app-generic-reports-compare',
  imports: [NamespaceImageSelectorComponent, TrivyTableComponent],
  templateUrl: './generic-reports-compare.component.html',
  styleUrl: './generic-reports-compare.component.scss',
})
export class GenericReportsCompareComponent<
  TTrivyReportComparableDto extends TrivyReportComparable<TTrivyReportDetailComparableDto>,
  TTrivyReportDetailComparableDto extends TrivyReportComparableDetail,
> implements OnInit {
  dataDtos = input.required<TTrivyReportComparableDto[] | undefined>();
  comparedTableColumns = input.required<TrivyTableColumn[]>();
  namespacedImageDtos = input.required<NamespacedImageDto[] | undefined>();

  firstSelectedTrivyReportId = model<string | undefined>();
  secondSelectedTrivyReportId = model<string | undefined>();

  trivyReportDetailsCompared?: TrivyReportDetailComparedDto[];

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

  firstDtoRequested = output<string>();
  secondDtoRequested = output<string>();

  private _dataDtos?: TTrivyReportComparableDto[];
  private _groupedFields: string[] = [];
  private _firstSelectedTrivyReportId?: string;
  private _secondSelectedTrivyReportId?: string;

  private _isDependantOnExternalData?: boolean;
  private _firstSelectedDto?: TTrivyReportComparableDto;
  private _secondSelectedDto?: TTrivyReportComparableDto;

  constructor() {
    effect(() => {
      this._isDependantOnExternalData = this.isDependantOnExternalData();
    });
    effect(() => {
      this._firstSelectedTrivyReportId = this.firstSelectedTrivyReportId();
      if (this._isDependantOnExternalData) {
        this._firstSelectedDto = undefined;
        if (this._firstSelectedTrivyReportId) {
          this.firstDtoRequested.emit(this._firstSelectedTrivyReportId);
        }
      }
      this.compareSelectedTrivyReports();
    });
    effect(() => {
      this._secondSelectedTrivyReportId = this.secondSelectedTrivyReportId();
      if (this._isDependantOnExternalData) {
        this._secondSelectedDto = undefined;
        if (this._secondSelectedTrivyReportId) {
          this.secondDtoRequested.emit(this._secondSelectedTrivyReportId);
        }
      }
      this.compareSelectedTrivyReports();
    });
    effect(() => {
      this._dataDtos = this.dataDtos();
      this.compareSelectedTrivyReports();
    });
    effect(() => {
      this._firstSelectedDto = this.firstSelectedDto();
      this.compareSelectedTrivyReports();
    });
    effect(() => {
      this._secondSelectedDto = this.secondSelectedDto();
      this.compareSelectedTrivyReports();
    });
  }

  ngOnInit() {
    this._groupedFields = this.comparedTableColumns()
      .filter((col) => col.renderType == 'compareStacked')
      .map((col) => col.field);
  }

  compareSelectedTrivyReports() {
    if (
      (!this._dataDtos && !this._isDependantOnExternalData) ||
      (!this._firstSelectedTrivyReportId && !this._secondSelectedTrivyReportId)
    ) {
      if (this.trivyReportDetailsCompared) {
        this.trivyReportDetailsCompared = undefined;
      }
      return;
    }

    const detailSet = new Map<string, TrivyReportDetailComparedDto>();

    const firstDto = this._isDependantOnExternalData
      ? this._firstSelectedDto
      : this._dataDtos?.find((tr) => tr.uid === this._firstSelectedTrivyReportId);

    if (firstDto) {
      firstDto.details?.forEach((detail) => {
        const existing = detailSet.get(detail.matchKey);
        if (existing) {
          this.mergeValues(existing, detail, true);
        } else {
          const clone = { ...detail, first: true };
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

    const secondDto = this._isDependantOnExternalData
      ? this._secondSelectedDto
      : this._dataDtos?.find((tr) => tr.uid === this._secondSelectedTrivyReportId);

    if (secondDto) {
      secondDto.details?.forEach((detail) => {
        const existing = detailSet.get(detail.matchKey);
        if (existing) {
          existing.second = true;
          this.mergeValues(existing, detail, false);
        } else {
          const clone = { ...detail, second: true };
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
      if (this._firstSelectedTrivyReportId) {
        item.first = item.first ?? false;
      }
      if (this._secondSelectedTrivyReportId) {
        item.second = item.second ?? false;
      }
    });

    this.trivyReportDetailsCompared = Array.from(detailSet.values());
  }

  private mergeValues(
    existing: TrivyReportComparableDetail,
    incoming: TrivyReportComparableDetail,
    isFirst: boolean,
  ): void {
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
    });
  }

  private getPropertyAsString(dto: TrivyReportComparableDetail, key: string): string | undefined {
    const value = (dto as any)[key];
    return value != null ? value.toString() : undefined;
  }
}
