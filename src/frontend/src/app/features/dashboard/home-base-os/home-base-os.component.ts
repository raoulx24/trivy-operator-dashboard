import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';

import { HomeBaseOsItemDto } from '@api/models/home-base-os-item-dto';
import { HomeService } from '@api/services/home.service';
import { EolRunwayComponent } from '@shared/ui/eol-runway/eol-runway.component';
import { eolRelative, eolStateOf, parseEolDay } from '@shared/ui/eol-runway/eol-runway.state';
import { EolRunwayItem } from '@shared/ui/eol-runway/eol-runway.types';
import { TitledBoxComponent } from '@shared/ui/titled-box/titled-box.component';

import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';

// Base OS status: the EOL runway (one bar per OS cycle); while expanded, also a box per OS version (family + version as
// Trivy reports it) with its images and namespaces
// it loads by itself (a change of a filter, or of refresh, reloads it): the lists of images and namespaces are asked
// from the server only while expanded (the runway needs the counts only)
// expanded layout: three boxes per row; the runway takes two thirds of the last row; when one box is left over after
// the full rows (or there is only one), it sits beside the runway, else the runway has its row alone
@Component({
  selector: 'app-home-base-os',
  standalone: true,
  imports: [HomeWidgetFrameComponent, EolRunwayComponent, TitledBoxComponent, NgTemplateOutlet, ButtonModule],
  templateUrl: './home-base-os.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must scroll inside, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeBaseOsComponent extends HomeWidgetBase {
  // the server allows 500
  private static readonly collapsedLimit = 1;
  private static readonly expandedLimit = 10;
  private static readonly columns = 3;

  private readonly homeService = inject(HomeService);
  private readonly router = inject(Router);

  hint = input<string>('');
  namespaces = input.required<string[]>();
  // "yyyy-MM-dd"
  today = input.required<string>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  // a new object on any change, so the resources reload; empty namespaces are not sent (the backend default applies)
  private readonly scope = computed(() => ({
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
  }));

  private readonly collapsedData = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeBaseOs({
        namespaces: params.namespaces,
        limit: HomeBaseOsComponent.collapsedLimit,
      }),
  });

  // idle (and empty) while collapsed
  private readonly expandedData = rxResource({
    params: () => (this.isExpanded() ? this.scope() : undefined),
    stream: ({ params }) =>
      this.homeService.getHomeBaseOs({
        namespaces: params.namespaces,
        limit: HomeBaseOsComponent.expandedLimit,
      }),
  });

  // set when the lists of this expand were revealed; reset on collapse (so a reload by a filter does not scroll)
  private revealed = false;

  constructor() {
    super();

    // the lists arrived: after they are rendered the page may scroll to show the whole widget
    effect(() => {
      if (!this.isExpanded()) {
        this.revealed = false;
      } else if (this.expandedData.hasValue() && !this.revealed) {
        this.revealed = true;
        this.revealAfterRender();
      }
    });
  }

  // what is shown: the lists when they are there, else the counts only
  private readonly shown = computed(() =>
    this.isExpanded() && this.expandedData.hasValue() ? this.expandedData : this.collapsedData,
  );

  protected readonly cycles = computed(() => this.toEolRunwayItems(this.shown().value()?.items ?? []));

  // the boxes exist only with the lists, so only from the expanded data
  // only the cycles at risk: end of life passed, or in less than EOL_RISK_DAYS days (on the "today" of the runway)
  private readonly allBoxes = computed<HomeBaseOsItemDto[]>(() =>
    this.isExpanded() && this.expandedData.hasValue()
      ? (this.expandedData.value()?.items ?? [])
          .filter((item) => this.isAtRisk(item))
          .sort((a, b) => this.title(a).localeCompare(this.title(b), undefined, { numeric: true, sensitivity: 'base' }))
      : [],
  );
  protected readonly hasBoxes = computed(() => this.allBoxes().length > 0);
  private readonly leftOver = computed(() => this.allBoxes().length % HomeBaseOsComponent.columns === 1);

  protected readonly boxes = computed(() => (this.leftOver() ? this.allBoxes().slice(0, -1) : this.allBoxes()));
  // the box beside the runway
  protected readonly besideBox = computed(() => (this.leftOver() ? this.allBoxes().at(-1) : undefined));

  protected readonly state = computed<HomeWidgetState>(() => {
    const resource = this.shown();
    if (resource.error()) {
      return { error: (resource.error() as Error)?.message ?? 'request failed' };
    }

    const dto = resource.value();
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary:
        `${dto.items.length} OS versions; ${dto.items.filter((i) => i.isEol).length} end of life, ` +
        `${dto.items.filter((i) => i.status === 'Pending').length} pending` +
        (this.isExpanded() && this.expandedData.isLoading() ? ' (loading more...)' : ''),
    };
  });

  // the same states as the runway bars; a version without dates (pending, not applicable, error) is not at risk
  private isAtRisk(item: HomeBaseOsItemDto): boolean {
    const eol = parseEolDay(item.eolFrom);
    const today = parseEolDay(this.today());
    if (item.status !== 'Ok' || eol === undefined || today === undefined) return false;

    const state = eolStateOf(eol, parseEolDay(item.eoasFrom), today);
    return state === 'eol' || state === 'risky';
  }

  // the vulnerability report of the image, in a new tab (the same way the image dependency tree opens its reports)
  protected openReport(digest: string): void {
    const url = this.router.serializeUrl(
      this.router.createUrlTree(['/open-trivy-rep'], { queryParams: { page: 'vulnerability-reports', digest } }),
    );
    window.open(url, '_blank');
  }

  protected title(item: HomeBaseOsItemDto): string {
    return `${item.productLabel ?? item.family} ${item.version}`;
  }

  // the lines before the images: end of life and end of active support (with how far they are from "today", as in the
  // runway popover), the latest version of the cycle, what runs on it (the counts, as the lists below are capped)
  // the same lines in every box
  protected info(item: HomeBaseOsItemDto): string[] {
    const today = parseEolDay(this.today());
    // every line is always there; "n/a" when the cycle has no such date (e.g. no active support phase)
    const dated = (label: string, day?: string | null): string => {
      const ms = parseEolDay(day);
      return ms === undefined || today === undefined ? `${label}: n/a` : `${label}: ${day} (${eolRelative(ms, today)})`;
    };

    return [
      dated('End of life', item.eolFrom),
      dated('End of active support', item.eoasFrom),
      `Latest version: ${item.latestVersion ?? 'n/a'}`,
      `${item.images} images, ${item.namespaces} namespaces, ${item.workloads} workloads`,
    ];
  }

  // the lists are capped by the server; the counts are not
  protected moreImages(item: HomeBaseOsItemDto): number {
    return Math.max(0, item.images - item.imageDetails.length);
  }

  protected moreNamespaces(item: HomeBaseOsItemDto): number {
    return Math.max(0, item.namespaces - item.namespaceNames.length);
  }

  // the endpoint has a row per OS cycle, and so has the runway; rows without dates (pending, not applicable, error)
  // are left out
  private toEolRunwayItems(items: HomeBaseOsItemDto[]): EolRunwayItem[] {
    return items
      .filter((item) => item.status === 'Ok' && !!item.eolFrom)
      .map((item) => ({
        label: `${item.productLabel ?? item.family} ${item.version}`,
        eolFrom: item.eolFrom!,
        eoasFrom: item.eoasFrom,
        images: item.images,
      }));
  }
}
