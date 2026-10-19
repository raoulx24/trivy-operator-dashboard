import { Injectable } from '@angular/core';

export type Side = 'first' | 'second';

export interface SideStore {
  [fieldKey: string]: string; // joined values: "a__b__c"
}

export interface CompareArgs<
  TDto extends TrivyReportComparable<TDetail>,
  TDetail extends TrivyReportComparableDetail
> {
  isDependantOnExternalData: boolean;
  dataDtos?: TDto[];
  firstSelectedTrivyReportId?: string;
  secondSelectedTrivyReportId?: string;
  firstSelectedDto?: TDto;
  secondSelectedDto?: TDto;
  groupedFields: (keyof TDetail)[];
}

export type ComparableWorkingItem<TDetail> =
  TDetail & {
  first?: boolean;
  second?: boolean;
  firstCount?: number;
  secondCount?: number;
  modified?: boolean;
  __first?: SideStore;
  __second?: SideStore;
};

export interface TrivyReportComparable<TDetail extends TrivyReportComparableDetail> {
  uid: string;
  details: Array<TDetail>;
}

export interface TrivyReportComparableDetail {
  matchKey: string;
}

// TODO:
// - a fully typed version with no any
// - a version using mapped types instead of intersections
// - a version with stricter generics and compile‑time field validation
// - consts 'N/A', '__', '|'

@Injectable({ providedIn: 'root' })
export class TrivyReportCompareService {

  compareReports<
    TDto extends TrivyReportComparable<TDetail>,
    TDetail extends TrivyReportComparableDetail
  >(args: CompareArgs<TDto, TDetail>): ComparableWorkingItem<TDetail>[] {

    const {
      isDependantOnExternalData,
      dataDtos,
      firstSelectedTrivyReportId,
      secondSelectedTrivyReportId,
      firstSelectedDto,
      secondSelectedDto,
      groupedFields
    } = args;

    if (
      (!dataDtos && !isDependantOnExternalData) ||
      (!firstSelectedTrivyReportId && !secondSelectedTrivyReportId)
    ) {
      return [];
    }

    const firstDto = isDependantOnExternalData
      ? firstSelectedDto
      : dataDtos?.find(tr => tr.uid === firstSelectedTrivyReportId);

    const secondDto = isDependantOnExternalData
      ? secondSelectedDto
      : dataDtos?.find(tr => tr.uid === secondSelectedTrivyReportId);

    const detailSet = this.buildDetailSet(firstDto, secondDto, groupedFields);
    return this.finalizeComparison(detailSet, groupedFields);
  }

  // ---------------------------------------------------------
  // BUILD DETAIL SET
  // ---------------------------------------------------------

  private buildDetailSet<
    TDto extends TrivyReportComparable<TDetail>,
    TDetail extends TrivyReportComparableDetail
  >(
    firstDto: TDto | undefined,
    secondDto: TDto | undefined,
    groupedFields: (keyof TDetail)[]
  ): Map<string, ComparableWorkingItem<TDetail>> {

    const detailSet = new Map<string, ComparableWorkingItem<TDetail>>();

    this.applyDtoDetails(detailSet, firstDto, 'first', groupedFields);
    this.applyDtoDetails(detailSet, secondDto, 'second', groupedFields);

    return detailSet;
  }

  private applyDtoDetails<
    TDto extends TrivyReportComparable<TDetail>,
    TDetail extends TrivyReportComparableDetail
  >(
    detailSet: Map<string, ComparableWorkingItem<TDetail>>,
    dto: TDto | undefined,
    side: Side,
    groupedFields: (keyof TDetail)[]
  ): void {

    if (!dto?.details) return;

    dto.details.forEach(detail => {
      const existing = detailSet.get(detail.matchKey);

      if (existing) {
        existing[side] = true;
        if (side === 'first') {
          existing.firstCount = (existing.firstCount ?? 0) + 1;
        } else {
          existing.secondCount = (existing.secondCount ?? 0) + 1;
        }
        this.mergeValues(existing, detail, side === 'first', groupedFields);
      } else {
        const clone: ComparableWorkingItem<TDetail> = {
          ...(detail as TDetail),
          [side]: true,
          firstCount: side === 'first' ? 1 : 0,
          secondCount: side === 'second' ? 1 : 0,
        };

        groupedFields.forEach((field) => {
          const raw = (detail as any)[String(field)];
          this.mergeSideValue(clone, field, raw, side === 'first');
        });

        detailSet.set(detail.matchKey, clone);
      }
    });
  }

  // ---------------------------------------------------------
  // FINALIZE COMPARISON
  // ---------------------------------------------------------

  private finalizeComparison<TDetail extends TrivyReportComparableDetail>(
    detailSet: Map<string, ComparableWorkingItem<TDetail>>,
    groupedFields: (keyof TDetail)[]
  ): ComparableWorkingItem<TDetail>[] {

    detailSet.forEach(item => {
      const firstStore = item.__first;
      const secondStore = item.__second;

      groupedFields.forEach(field => {
        const key = String(field);

        const left = item.first ? firstStore?.[key] : undefined;
        const right = item.second ? secondStore?.[key] : undefined;

        (item as any)[key] = this.compareField(left, right);

        if (left !== undefined && right !== undefined && left !== right) {
          item.modified = true;
        }
      });

      delete item.__first;
      delete item.__second;

      item.first = item.first === true;
      item.second = item.second === true;
    });

    const compared = Array.from(detailSet.values());

    compared.forEach(x => {
      x.modified = x.modified || x.first !== x.second;
    });

    return compared;
  }

  // ---------------------------------------------------------
  // HELPERS
  // ---------------------------------------------------------

  private compareField(left: any, right: any): string {
    const normalize = (v: any) =>
      v === undefined || v === null ? 'N/A' : String(v);

    const L = normalize(left);
    const R = normalize(right);

    return L === R ? L : `${L}|${R}`;
  }

  private normalizeRawValue(value: any): string {
    if (value === null || value === undefined || value === '') {
      return 'N/A';
    }
    return String(value).replace(/[|_]/g, ' ');
  }

  private mergeSideValue<TDetail>(
    existing: ComparableWorkingItem<TDetail>,
    field: keyof TDetail,
    raw: any,
    isFirst: boolean
  ): void {
    const normalized = this.normalizeRawValue(raw);
    const key = isFirst ? '__first' : '__second';

    const store: SideStore = (existing[key] ??= {});
    const fieldKey = String(field);

    const current = store[fieldKey];
    const values = current ? current.split('__') : [];

    if (!values.includes(normalized)) {
      values.push(normalized);
      values.sort();
    }

    store[fieldKey] = values.join('__');
  }

  private mergeValues<TDetail>(
    existing: ComparableWorkingItem<TDetail>,
    incoming: TDetail,
    isFirst: boolean,
    groupedFields: (keyof TDetail)[]
  ): void {
    let modified = false;

    groupedFields.forEach(field => {
      const fieldKey = String(field);
      const raw = (incoming as any)[fieldKey];

      const key = isFirst ? '__first' : '__second';
      const store: SideStore = (existing[key] ??= {});

      const before = store[fieldKey];
      this.mergeSideValue(existing, field, raw, isFirst);
      const after = store[fieldKey];

      if (before !== undefined && before !== after) {
        modified = true;
      }
    });

    existing.modified = existing.modified || modified;
  }
}
