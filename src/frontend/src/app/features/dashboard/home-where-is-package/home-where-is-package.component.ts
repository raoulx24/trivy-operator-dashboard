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
import { LayeredGraphColumn, LayeredGraphEdge, LayeredGraphNode } from '@shared/ui/layered-graph/layered-graph.types';
import { TrivyTableColumn } from '@shared/ui/trivy-table/trivy-table.types';

import { NodeFiltersDetail } from '../home-pivot/home-node-filters.component';
import { HomeSelectedNodesComponent, SelectedImage, SelectedNamespace, SelectedPackage } from '../home-pivot/home-selected-nodes.component';
import { HomeWhereIsBase } from '../home-where-is-base/home-where-is.base';
import { HomeWhereIsShellComponent } from '../home-where-is-base/home-where-is-shell.component';
import { HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';

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
    HomeSelectedNodesComponent,
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
  protected readonly help: HomeWidgetHelp = {
    question: `Where does a <strong>package</strong> run, and in which versions? So you know what an update or a bad release touches.`,
    goodToKnow: [
      `One row per version (a purl with its version). Type a name or the beginning of a purl, at least 3 characters, then <strong>Enter</strong>.`,
      `The packages come from the <strong>SBOM reports</strong>; without them only the vulnerable packages are found, and there are no licenses.`,
      `Only the <strong>namespaces filter</strong> applies. A change of it does not search again; the refresh button does.`,
    ],
    expanded: [
      `Every purl found (up to 500) on the left; on the right the namespaces and images where it runs, its licenses and its vulnerabilities.`,
    ],
    tips: [
      `Add a version to narrow down: <em>pkg:npm/axios@1.6</em>.`,
      `Collapsed: click one of the 5 purls that reach the most images to expand on it.`,
    ],
  };

  // the height of a row of the list (a scroller needs the same for all): the name and version (two lines at most), the tags, the counts
  protected readonly rowHeight = 104;

  // as the other widgets: the body is as tall as theirs, and the list is as thin (a part of the collapsed width)
  protected readonly expandedHeight = '56rem';
  protected readonly listRatio = 0.525;

  protected readonly noun = 'purls';

  // the list sorts by severity (the highest of its vulnerabilities, a purl with none goes last) and name
  protected override readonly sortable = true;
  protected override sortValues(item: HomeWhereIsPackageItemDto): { severity: number; name: string } {
    return { severity: item.maxSeverityId ?? 99, name: item.purl };
  }

  private readonly homeService = inject(HomeService);

  // "Show all": every purl found (the rows of the list, up to 500), flat, in a table
  protected readonly allRowsColumns: TrivyTableColumn[] = [
    { field: 'name', header: 'Package', isFilterable: true, isSortable: true, style: 'width: 22%', renderType: 'standard' },
    { field: 'version', header: 'Version', isFilterable: true, isSortable: true, style: 'width: 14%', renderType: 'standard' },
    { field: 'type', header: 'Type', isFilterable: true, autoFilter: true, isSortable: true, style: 'width: 8rem', renderType: 'standard' },
    { field: 'maxSeverityId', header: 'Sev', isFilterable: true, isSortable: true, multiSelectType: 'severities', style: 'width: 7rem', renderType: 'severityBadge' },
    { field: 'vulnerabilityIds', header: 'Vulnerabilities', isFilterable: false, isSortable: true, style: 'width: 8rem', renderType: 'standard' },
    { field: 'images', header: 'Images', isFilterable: false, isSortable: true, style: 'width: 6rem', renderType: 'standard' },
    { field: 'namespaces', header: 'Namespaces', isFilterable: false, isSortable: true, style: 'width: 7rem', renderType: 'standard' },
    { field: 'workloads', header: 'Workloads', isFilterable: false, isSortable: true, style: 'width: 7rem', renderType: 'standard' },
    { field: 'purl', header: 'Purl', isFilterable: true, isSortable: true, style: 'width: 25%', renderType: 'standard' },
  ];

  // ---- the graph: vulnerabilities -> images -> namespaces (the first column is always there, also when the package has none).
  // The id of a node is its kind and its index in the detail: 'p:2' (a vulnerability: the first kind of the selects), 'i:0', 'n:1'
  protected readonly graphColumns: LayeredGraphColumn[] = [
    { label: 'Vulnerabilities', color: 'var(--tod-layered-graph-package)', textColor: 'var(--tod-layered-graph-package-text)', width: 192 },
    { label: 'Images', color: 'var(--tod-layered-graph-image)', textColor: 'var(--tod-layered-graph-image-text)', width: 210, subColumns: 2 },
    { label: 'Namespaces', color: 'var(--tod-layered-graph-namespace)', textColor: 'var(--tod-layered-graph-namespace-text)' },
  ];

  protected readonly graphData = computed<{ nodes: LayeredGraphNode[]; edges: LayeredGraphEdge[] }>(() => {
    const d = this.detail();
    if (!d) {
      return { nodes: [], edges: [] };
    }

    const nodes: LayeredGraphNode[] = [];
    const edges: LayeredGraphEdge[] = [];
    const imagesOfNamespace = new Array<number>(d.namespaceNames.length).fill(0);
    d.imageNamespaceIndexes.forEach((list) => list.forEach((n) => (imagesOfNamespace[n] = (imagesOfNamespace[n] ?? 0) + 1)));

    d.vulnerabilities.forEach((v, i) => {
      nodes.push({
        id: `p:${i}`,
        column: 0,
        label: v.vulnerabilityId,
        tag: v.isKnownExploited ? 'exploited' : null,
        value: v.imageIndexes.length,
        weight: v.imageIndexes.length,
      });
      v.imageIndexes.forEach((image) => edges.push({ from: `p:${i}`, to: `i:${image}` }));
    });
    d.imageNames.forEach((name, i) => {
      const namespaces = d.imageNamespaceIndexes[i] ?? [];
      nodes.push({ id: `i:${i}`, column: 1, label: name, value: namespaces.length, weight: namespaces.length });
      namespaces.forEach((n) => edges.push({ from: `i:${i}`, to: `n:${n}` }));
    });
    d.namespaceNames.forEach((name, n) => {
      nodes.push({ id: `n:${n}`, column: 2, label: name, value: imagesOfNamespace[n], weight: imagesOfNamespace[n] });
    });

    return { nodes, edges };
  });

  protected readonly graphTitle = computed(() => {
    const d = this.detail();
    if (!d) {
      return 'Where it runs';
    }

    const vulnerabilities = d.vulnerabilities.length > 0 ? `${d.vulnerabilities.length} ${d.vulnerabilities.length === 1 ? 'vulnerability' : 'vulnerabilities'}, ` : '';

    return `Where it runs (${vulnerabilities}${d.imageNames.length} ${d.imageNames.length === 1 ? 'image' : 'images'}, ${d.namespaceNames.length} ns)`;
  });

  // what the selects of the nodes need: the vulnerabilities are their first kind
  protected readonly filterDetail = computed<NodeFiltersDetail | undefined>(() => {
    const d = this.detail();

    return d
      ? {
          packages: d.vulnerabilities.map((v) => ({ packageName: v.vulnerabilityId, imageIndexes: v.imageIndexes })),
          imageNames: d.imageNames,
          imageNamespaceIndexes: d.imageNamespaceIndexes,
          namespaceNames: d.namespaceNames,
        }
      : undefined;
  });

  // ---- the data of the selected nodes, brief (the Selected box)
  private selectedIndex(prefix: string): number | null {
    const id = this.selectedNodes().find((x) => x.startsWith(prefix));
    return id === undefined ? null : Number(id.substring(2));
  }

  // the first kind is a vulnerability: its id, severity, fixed version and images (the box has no fifth field here)
  protected readonly selectedVulnerability = computed<SelectedPackage | null>(() => {
    const index = this.selectedIndex('p:');
    const v = index === null ? undefined : this.detail()?.vulnerabilities[index];

    return v
      ? {
          name: v.vulnerabilityId,
          severityId: v.severityId,
          installed: String(v.imageIndexes.length),
          fixedVersion: v.fixedVersion ?? 'no fix',
          extra: '',
        }
      : null;
  });

  protected readonly selectedImageInfo = computed<SelectedImage | null>(() => {
    const d = this.detail();
    const index = this.selectedIndex('i:');
    const name = index === null ? undefined : d?.imageNames[index];

    return d && index !== null && name !== undefined
      ? {
          name,
          packages: d.vulnerabilities.filter((v) => v.imageIndexes.includes(index)).length,
          namespaces: d.imageNamespaceIndexes[index]?.length ?? 0,
        }
      : null;
  });

  protected readonly selectedNamespaceInfo = computed<SelectedNamespace | null>(() => {
    const d = this.detail();
    const index = this.selectedIndex('n:');
    const name = index === null ? undefined : d?.namespaceNames[index];

    return d && index !== null && name !== undefined
      ? { name, images: d.imageNamespaceIndexes.filter((x) => x.includes(index)).length }
      : null;
  });

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
