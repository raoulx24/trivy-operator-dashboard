import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { HomeWhereIsLicenseDetailDto } from '@api/models/home-where-is-license-detail-dto';
import { HomeWhereIsLicenseDetailItemDto } from '@api/models/home-where-is-license-detail-item-dto';
import { HomeWhereIsLicenseDto } from '@api/models/home-where-is-license-dto';
import { HomeWhereIsLicenseItemDto } from '@api/models/home-where-is-license-item-dto';
import { HomeService } from '@api/services/home.service';
import { TrivyBoxComponent } from '@shared/ui/trivy-box/trivy-box.component';
import { TrivyTableComponent } from '@shared/ui/trivy-table/trivy-table.component';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

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

  private readonly homeService = inject(HomeService);

  protected readonly packageColumns: TrivyTableColumn[] = [
    { field: 'name', header: 'Package', isFilterable: false, isSortable: true, style: 'width: 40%', renderType: 'standard' },
    { field: 'version', header: 'Version', isFilterable: false, isSortable: true, style: 'width: 30%', renderType: 'standard' },
    { field: 'type', header: 'Type', isFilterable: false, isSortable: true, style: 'width: 6rem', renderType: 'standard' },
    { field: 'images', header: 'Images', isFilterable: false, isSortable: true, style: 'width: 6rem; text-align: right', renderType: 'standard' },
  ];

  // the package versions that the selected namespace and image let through (a package version is in when one of its
  // images is: the allowed images are those of the selected namespace, the selected image must be one of its images)
  protected readonly packageRows = computed<PackageRow[]>(() => {
    const allowed = this.allowedImages();
    const image = this.selectedImage();

    return (this.detail()?.packageVersions ?? []).flatMap((p, index) => {
      const images = p.imageIndexes.filter((i) => allowed.has(i) && (image === null || i === image)).length;
      return images > 0 ? [{ index, name: p.name, version: p.version, type: p.type ?? '', images }] : [];
    });
  });

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
