import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { Observable } from 'rxjs';

import { HomeWhereIsPackageDetailDto } from '@api/models/home-where-is-package-detail-dto';
import { HomeWhereIsPackageDetailItemDto } from '@api/models/home-where-is-package-detail-item-dto';
import { HomeWhereIsPackageDto } from '@api/models/home-where-is-package-dto';
import { HomeWhereIsPackageItemDto } from '@api/models/home-where-is-package-item-dto';
import { HomeService } from '@api/services/home.service';
import { PurlPartsPipe } from '@shared/pipes/purl-parts.pipe';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { HomeWhereIsBase } from '../home-where-is-base/home-where-is.base';
import { HomeWhereIsShellComponent } from '../home-where-is-base/home-where-is-shell.component';

interface VulnerabilityRow {
  // the place of the vulnerability in the vulnerabilities of the detail
  index: number;
  vulnerabilityId: string;
  severityId: number;
  fixedVersion: string;
  images: number;
}

// Where is... package: a package (a name, or a purl prefix) and where each of its versions runs
// - one row per package version, i.e. per purl with its version
// - the search, the list, the expand and the filters of the namespaces and images are in HomeWhereIsBase and
//   HomeWhereIsShellComponent; here are the calls, the rows, and what is specific to a purl: its head (with the copy
//   button), its licenses and its vulnerabilities
@Component({
  selector: 'app-home-where-is-package',
  standalone: true,
  imports: [
    HomeWhereIsShellComponent,
    ButtonModule,
    NgTemplateOutlet,
    TagModule,
    TrivyBoxComponent,
    TrivyTableComponent,
    PurlPartsPipe,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
  ],
  templateUrl: './home-where-is-package.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeWhereIsPackageComponent extends HomeWhereIsBase<
  HomeWhereIsPackageItemDto,
  HomeWhereIsPackageDto,
  HomeWhereIsPackageDetailItemDto,
  HomeWhereIsPackageDetailDto
> {
  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help = `
    <p class="mb-2">Where a <strong>package</strong> runs now, one row per <strong>version</strong> (a purl with its version). Type a <strong>name</strong> (e.g. <em>openssl</em>, <em>libc</em>) or the <strong>beginning of a purl</strong> (<em>pkg:npm/axios</em>, with an <strong>@</strong> and the beginning of a version: <em>pkg:npm/axios@1.6</em>) and press <strong>Enter</strong> (at least 3 characters; case does not matter).</p>
    <p class="mb-2"><strong>Collapsed</strong>: the 5 purls that reach the most images. A click on a purl expands the widget on it.</p>
    <p class="mb-2"><strong>Expanded</strong>: on the left every purl found (up to 500; a click selects one), on the right the namespaces and the images where the selected purl runs, its licenses and its vulnerabilities.</p>
    <p class="mb-1"><strong>A row</strong></p>
    <p class="mb-2">The <strong>severity</strong> is the highest of its vulnerabilities (none when it has no active ones). Then the counts: <strong>images</strong>, <strong>namespaces</strong>, <strong>workloads</strong> and <strong>vulnerabilities</strong> (distinct ids).</p>
    <p class="mb-0 tod-text-muted-color">The packages come from the <strong>SBOM reports</strong>; without them only the vulnerable packages are found and there are no licenses. Only the <strong>namespaces filter</strong> applies; a change of it does not run the search again, the refresh button does.</p>
  `;

  // the height of a row of the list (a scroller needs the same for all): the name and version (two lines at most), the tags, the counts
  protected readonly rowHeight = 104;

  protected readonly noun = 'purls';

  private readonly homeService = inject(HomeService);

  protected readonly vulnerabilityColumns: TrivyTableColumn[] = [
    { field: 'vulnerabilityId', header: 'Vulnerability', isFilterable: false, isSortable: true, style: 'width: 38%', renderType: 'vulnerabilityId' },
    { field: 'severityId', header: 'Severity', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'severityBadge' },
    { field: 'fixedVersion', header: 'Fixed', isFilterable: false, isSortable: true, style: 'width: 30%', renderType: 'standard' },
    { field: 'images', header: 'Images', isFilterable: false, isSortable: true, style: 'width: 6rem; text-align: right', renderType: 'standard' },
  ];

  // the vulnerabilities of the table
  protected readonly vulnerabilityRows = computed<VulnerabilityRow[]>(() =>
    (this.detail()?.vulnerabilities ?? []).map((v, index) => ({
      index,
      vulnerabilityId: v.vulnerabilityId,
      severityId: v.severityId,
      fixedVersion: v.fixedVersion ?? 'no fix',
      images: v.imageIndexes.length,
    })),
  );

  // the purl that was just copied (its button shows a check for a moment)
  protected readonly copiedPurl = signal<string | undefined>(undefined);
  private copiedTimer: ReturnType<typeof setTimeout> | undefined;

  protected override fetchList(query: string, namespaces: string[] | undefined, limit: number): Observable<HomeWhereIsPackageDto> {
    return this.homeService.getHomeWhereIsPackage({ query, namespaces, limit });
  }

  protected override fetchDetail(item: HomeWhereIsPackageItemDto, namespaces: string[] | undefined): Observable<HomeWhereIsPackageDetailDto> {
    return this.homeService.getHomeWhereIsPackageDetailed({ packageKey: item.packageKey, version: item.version, namespaces });
  }

  override itemKey(item: HomeWhereIsPackageItemDto): string {
    return item.purl;
  }

  protected override isDetailOf(dto: HomeWhereIsPackageDetailDto, item: HomeWhereIsPackageItemDto): boolean {
    return dto.packageKey === item.packageKey && dto.version === item.version;
  }

  // no vulnerabilities: nothing to put in the bottom half
  override hasDetailBottom(detail: HomeWhereIsPackageDetailItemDto): boolean {
    return detail.vulnerabilities.length > 0;
  }

  // the copy button of a row: not a click on the row (it would select it)
  protected async copyPurl(purl: string, event: Event): Promise<void> {
    event.stopPropagation();
    try {
      await navigator.clipboard.writeText(purl);
    } catch {
      // no clipboard (insecure page, or refused): nothing was copied, so no check
      return;
    }
    this.copiedPurl.set(purl);
    clearTimeout(this.copiedTimer);
    this.copiedTimer = setTimeout(() => this.copiedPurl.set(undefined), 1500);
  }
}
