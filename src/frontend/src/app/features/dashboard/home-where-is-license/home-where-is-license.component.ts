import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { HomeWhereIsLicenseDetailDto } from '@api/models/home-where-is-license-detail-dto';
import { HomeWhereIsLicenseDetailItemDto } from '@api/models/home-where-is-license-detail-item-dto';
import { HomeWhereIsLicenseDto } from '@api/models/home-where-is-license-dto';
import { HomeWhereIsLicenseItemDto } from '@api/models/home-where-is-license-item-dto';
import { HomeService } from '@api/services/home.service';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { SelectedDtosEvent, TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { NodeFiltersDetail } from '../home-pivot/home-node-filters.component';
import { HomeWhereIsBase } from '../home-where-is-base/home-where-is.base';
import { HomeWhereIsShellComponent } from '../home-where-is-base/home-where-is-shell.component';

interface PackageRow {
  // the place of the package version in the package versions of the detail
  index: number;
  name: string;
  version: string;
  type: string;
  // the images that have it and that the selections of the namespaces and images let through
  images: number;
}

// Where is... license: a license (an SPDX id, or a part of a name) and where it runs
// - one row per license, as the SBOM reports have it; the counts are of its package versions (a purl with its version)
// - the search, the list, the expand and the filters of the namespaces and images are in HomeWhereIsBase and
//   HomeWhereIsShellComponent (the same as of the package widget); here are the calls, the rows, and the package versions
//   of the selected license, which the selections of the namespaces and the images filter too
@Component({
  selector: 'app-home-where-is-license',
  standalone: true,
  imports: [HomeWhereIsShellComponent, TrivyBoxComponent, TrivyTableComponent],
  templateUrl: './home-where-is-license.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeWhereIsLicenseComponent extends HomeWhereIsBase<
  HomeWhereIsLicenseItemDto,
  HomeWhereIsLicenseDto,
  HomeWhereIsLicenseDetailItemDto,
  HomeWhereIsLicenseDetailDto
> {
  // the help dialog of the frame (html): short, the important parts in bold
  protected readonly help = `
    <p class="mb-2">Where a <strong>license</strong> runs now. Type an <strong>SPDX id</strong> (e.g. <em>MIT</em>, <em>GPL-3.0</em>) or a <strong>part of a license name</strong> and press <strong>Enter</strong> (at least 2 characters; case does not matter). One row per license.</p>
    <p class="mb-2"><strong>Collapsed</strong>: the 5 licenses that reach the most images. A click on a license expands the widget on it.</p>
    <p class="mb-2"><strong>Expanded</strong>: on the left every license found (up to 500; a click selects one), on the right the namespaces and the images where the selected license runs, and the <strong>package versions</strong> that have it (a click on a namespace or an image shows only the packages that go with it).</p>
    <p class="mb-1"><strong>A row</strong></p>
    <p class="mb-2">The counts: <strong>packages</strong> (versions: a purl with its version), <strong>images</strong>, <strong>namespaces</strong> and <strong>workloads</strong>.</p>
    <p class="mb-0 tod-text-muted-color">This needs the <strong>SBOM reports</strong> (the licenses come from them). Only the <strong>namespaces filter</strong> applies; a change of it does not run the search again, the refresh button does.</p>
  `;

  // the height of a row of the list (a scroller needs the same for all): the license (two lines at most), the counts
  protected readonly rowHeight = 76;

  protected readonly noun = 'licenses';

  // as the other widgets: the body is as tall as theirs, and the list is as thin (a part of the collapsed width)
  protected readonly expandedHeight = '56rem';
  protected readonly listRatio = 0.525;

  // the list sorts by name (a license has no severity)
  protected override readonly sortable = true;
  protected override readonly sortBySeverity = false;
  protected override sortValues(item: HomeWhereIsLicenseItemDto): { severity: number; name: string } {
    return { severity: 0, name: item.license };
  }

  // "Show all": every license found (the rows of the list, up to 500), flat, in a table
  protected readonly allRowsColumns: TrivyTableColumn[] = [
    { field: 'license', header: 'License', isFilterable: true, isSortable: true, style: 'width: 40%', renderType: 'standard' },
    { field: 'packages', header: 'Packages', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'standard' },
    { field: 'images', header: 'Images', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'standard' },
    { field: 'namespaces', header: 'Namespaces', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'standard' },
    { field: 'workloads', header: 'Workloads', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'standard' },
  ];

  // what the selects of the namespaces and the images need (a license has no first kind)
  protected readonly filterDetail = computed<NodeFiltersDetail | undefined>(() => {
    const d = this.detail();

    return d
      ? {
          packages: d.packageVersions.map((p) => ({ packageName: `${p.name} ${p.version}`, imageIndexes: p.imageIndexes })),
          imageNames: d.imageNames, imageNamespaceIndexes: d.imageNamespaceIndexes, namespaceNames: d.namespaceNames }
      : undefined;
  });

  private readonly homeService = inject(HomeService);

  protected readonly packageColumns: TrivyTableColumn[] = [
    { field: 'name', header: 'Package', isFilterable: false, isSortable: true, style: 'width: 40%', renderType: 'standard' },
    { field: 'version', header: 'Version', isFilterable: false, isSortable: true, style: 'width: 30%', renderType: 'standard' },
    { field: 'type', header: 'Type', isFilterable: false, isSortable: true, style: 'width: 6rem', renderType: 'standard' },
    { field: 'images', header: 'Images', isFilterable: false, isSortable: true, style: 'width: 6rem; text-align: right', renderType: 'standard' },
  ];

  // the package versions, all of them: the selected namespace and image fade the ones that have none of the images they let
  // through (and those cannot be selected). The selection of a package is not a filter of its own list, as in the other
  // lists: it is the others that follow it. images = the images that the namespace and the image let through
  protected readonly packageRows = computed<PackageRow[]>(() => {
    const d = this.detail();
    const namespace = this.selectedNamespace();
    const image = this.selectedImage();
    const inNamespace = (i: number): boolean => namespace === null || (d?.imageNamespaceIndexes[i]?.includes(namespace) ?? false);

    return (d?.packageVersions ?? []).map((p, index) => ({
      index,
      name: p.name,
      version: p.version,
      type: p.type ?? '',
      images: p.imageIndexes.filter((i) => inNamespace(i) && (image === null || i === image)).length,
    }));
  });

  protected readonly packageRowDimmer = (row: PackageRow): boolean => row.images === 0;
  protected readonly packagesAllowed = computed(() => this.packageRows().filter((x) => x.images > 0).length);

  // the selected row of the table follows the selection of the package (also when a select makes it)
  protected readonly packageSelection = computed<SelectedDtosEvent<PackageRow>>(() => {
    const index = this.selectedFirst();

    return { selectedDtos: this.packageRows().filter((x) => x.index === index), source: 'programmatic' };
  });

  protected onPackageSelection(event: SelectedDtosEvent<PackageRow>): void {
    if (event.source === 'user') {
      this.selectedFirst.set(event.selectedDtos[0]?.index ?? null);
    }
  }

  protected override imagesOfFirst(index: number): readonly number[] | undefined {
    return this.detail()?.packageVersions[index]?.imageIndexes;
  }

  protected override fetchList(query: string, namespaces: string[] | undefined, limit: number): Observable<HomeWhereIsLicenseDto> {
    return this.homeService.getHomeWhereIsLicense({ query, namespaces, limit });
  }

  protected override fetchDetail(item: HomeWhereIsLicenseItemDto, namespaces: string[] | undefined): Observable<HomeWhereIsLicenseDetailDto> {
    return this.homeService.getHomeWhereIsLicenseDetailed({ license: item.license, namespaces });
  }

  override itemKey(item: HomeWhereIsLicenseItemDto): string {
    return item.license;
  }

  protected override isDetailOf(dto: HomeWhereIsLicenseDetailDto, item: HomeWhereIsLicenseItemDto): boolean {
    return dto.license === item.license;
  }
}
