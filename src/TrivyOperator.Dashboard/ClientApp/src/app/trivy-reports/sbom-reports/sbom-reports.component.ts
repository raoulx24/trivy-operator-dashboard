import { Component, inject, OnInit, signal } from '@angular/core';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

import { SbomReportImageDto } from '../../../api/models/sbom-report-image-dto';
import { TrivyReportResourceInfoDto } from '../../../api/models/trivy-report-resource-info-dto';
import { SbomReportImageMinimalDto } from '../../../api/models/sbom-report-image-minimal-dto';
import { SbomReportService } from '../../../api/services/sbom-report.service';

import { TreeNode } from 'primeng/api';
import { GenericSbomComponent } from '../../ui-elements/generic-sbom/generic-sbom.component';
import { ImageInfo, TrivyDependencyComponent } from '../../ui-elements/trivy-dependency/trivy-dependency.component';

import { SeverityCssStyleByIdPipe } from '../../pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '../../pipes/severity-name-by-id.pipe';
import { VulnerabilityCountPipe } from '../../pipes/vulnerability-count.pipe';

import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TreeTableModule } from 'primeng/treetable';

import { DataPageBase } from '../../abstracts/data-page-base';

@Component({
  selector: 'app-sbom-reports',
  standalone: true,
  imports: [
    GenericSbomComponent,
    TrivyDependencyComponent,
    SeverityCssStyleByIdPipe,
    SeverityNameByIdPipe,
    VulnerabilityCountPipe,
    DialogModule,
    TableModule,
    TagModule,
    TreeTableModule,
  ],
  templateUrl: './sbom-reports.component.html',
  styleUrl: './sbom-reports.component.scss',
})
export class SbomReportsComponent extends DataPageBase implements OnInit {
  dataDtos: SbomReportImageMinimalDto[] = [];
  fullSbomDataDto?: SbomReportImageDto;
  imageResourceDtos?: TrivyReportResourceInfoDto[];
  selectedImageId?: string;
  selectedSbomReportImageMinimalDto?: SbomReportImageMinimalDto;

  queryNamespaceName?: string;
  queryDigest?: string;
  isPreselected: boolean = false;

  compareFirstSelectedDto?: SbomReportImageDto;
  compareSecondSelectedDto?: SbomReportImageDto;

  // region dialog related vars
  isSbomReportOverviewDialogVisible = signal<boolean>(false);
  sbomReportDetailStatistics: Array<number | undefined> = [];
  sbomReportDetailPropertiesTreeNodes: TreeNode[] = [];
  sbomReportDetailLicensesTreeNodes: TreeNode[] = [];

  isDependencyTreeViewVisible = signal<boolean>(false);
  trivyImage?: ImageInfo;
  trivyDependencyDialogTitle: string = '';
  // endregion

  private readonly service = inject(SbomReportService);
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  ngOnInit() {
    const state = history.state;

    this.queryNamespaceName = state.namespaceName;
    this.queryDigest = state.digest;

    this.isPreselected = !!(this.queryNamespaceName && this.queryDigest);

    this.getTableDataDtos();
  }

  getTableDataDtos() {
    this.service.getSbomReportImageMinimalDtos().subscribe({
      next: (res) => this.onGetDataDtos(res),
      error: (err) => this.onError(err),
    });
  }

  onGetDataDtos(dtos: SbomReportImageMinimalDto[]) {
    this.dataDtos = dtos;
    if (this.isPreselected) {
      const queryDto = dtos.find(
        (x) => x.digest == this.queryDigest,
      );
      if (queryDto) {
        this.selectedImageId = queryDto.uid;
      }
      this.onSelectedImageIdChange(this.selectedImageId);

      this.isPreselected = false;
    }
  }

  onSelectedImageIdChange(imageId: string | undefined) {
    this.selectedSbomReportImageMinimalDto = this.dataDtos?.find((x) => x.uid === imageId);
    if (this.selectedSbomReportImageMinimalDto) {
      this.service
        .getSbomReportImageDtoByDigest({
          digest: this.selectedSbomReportImageMinimalDto.digest,
        })
        .subscribe({
          next: (res) => this.onGetSbomReportDtoByDigest(res),
          error: (err) => this.onError(err),
        });
    }
  }

  onGetSbomReportDtoByDigest(fullSbomDataDto: SbomReportImageDto) {
    this.fullSbomDataDto = fullSbomDataDto;
    this.imageResourceDtos = fullSbomDataDto.resources;
    this.sbomReportDetailPropertiesTreeNodes = [];
    this.sbomReportDetailLicensesTreeNodes = [];
    this.sbomReportDetailStatistics = [];
  }

  onRefreshRequestedChange() {
    this.getTableDataDtos();
  }

  onCompareFirstDtoRequested(id: string) {
    const dto = this.dataDtos.find((dto) => dto.uid === id);
    if (dto) {
      this.service
        .getSbomReportImageDtoByDigest({
          digest: dto.digest,
        })
        .subscribe({
          next: (res) => {
            this.compareFirstSelectedDto = res;
          },
          error: (err) => this.onError(err),
        });
    }
  }

  onCompareSecondDtoRequested(id: string) {
    const dto = this.dataDtos.find((dto) => dto.uid === id);
    if (dto) {
      this.service
        .getSbomReportImageDtoByDigest({
          digest: dto.digest,
        })
        .subscribe({
          next: (res) => {
            this.compareSecondSelectedDto = res;
          },
          error: (err) => this.onError(err),
        });
    }
  }

  onMultiActionEventChange(value: string) {
    switch (value) {
      case 'goToDetailedPage':
        this.goToDetailedPage();
        break;
      case 'Info':
        this.onSbomReportOverviewDialogOpen();
        break;
      case 'Export CycloneDX JSON':
        this.exportSbom('cyclonedx', 'json');
        break;
      case 'Export CycloneDX XML':
        this.exportSbom('cyclonedx', 'xml');
        break;
      case 'Export SPDX':
        this.exportSbom('spdx', 'json');
        break;
      case 'Dependency tree':
        this.goToDependencyTree();
        break;
      default:
        console.error('sbom - multi action call back - unknown: ', value);
    }
  }

  onSbomReportOverviewDialogOpen() {
    if (this.sbomReportDetailPropertiesTreeNodes.length == 0) {
      this.sbomReportDetailPropertiesTreeNodes = this.getSbomReportPropertyTreeNodes();
    }
    if (this.sbomReportDetailLicensesTreeNodes.length == 0) {
      this.sbomReportDetailLicensesTreeNodes = this.getSbomReportLicenseTreeNodes();
    }
    if (this.sbomReportDetailStatistics.length == 0) {
      this.sbomReportDetailStatistics.push(this.fullSbomDataDto?.criticalCount ?? -1);
      this.sbomReportDetailStatistics.push(this.fullSbomDataDto?.highCount ?? -1);
      this.sbomReportDetailStatistics.push(this.fullSbomDataDto?.mediumCount ?? -1);
      this.sbomReportDetailStatistics.push(this.fullSbomDataDto?.lowCount ?? -1);
      this.sbomReportDetailStatistics.push(this.fullSbomDataDto?.unknownCount ?? -1);
      this.sbomReportDetailStatistics.push(this.fullSbomDataDto?.details?.length ?? 0);
      this.sbomReportDetailStatistics.push(
        this.fullSbomDataDto?.details
          ?.map((item) => item.dependsOn)
          .filter((deps): deps is Array<string> => Array.isArray(deps))
          .reduce((sum, deps) => sum + deps.length, 0) ?? 0,
      );
    }

    this.isSbomReportOverviewDialogVisible.set(true);
  }

  exportSbom(fileFormat: 'cyclonedx' | 'spdx', contentType: 'json' | 'xml') {
    const apiRoot = this.service.rootUrl;
    const digest = encodeURIComponent(this.selectedSbomReportImageMinimalDto?.digest ?? '');
    const fileUrl = `${apiRoot}api/sbom-reports/${fileFormat}?digest=${digest}`;

    const headers = new HttpHeaders({
      Accept: contentType === 'json' || fileFormat === 'spdx' ? 'application/json' : 'application/xml',
    });

    this.http.get(fileUrl, { headers, responseType: 'text' }).subscribe({
      next: (response: string) => {
        const imageNameTag = `${this.selectedSbomReportImageMinimalDto?.imageName}:${this.selectedSbomReportImageMinimalDto?.imageTag}`;
        const blob = new Blob([response], {
          type: contentType === 'json' || fileFormat === 'spdx' ? 'application/json' : 'application/xml',
        });

        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;

        link.download = `sbom_${fileFormat}_${imageNameTag}.${contentType}`;
        link.click();

        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.onError(err);
      },
    });
  }

  private goToDependencyTree() {
    if (!this.selectedSbomReportImageMinimalDto) return;

    const digest = this.selectedSbomReportImageMinimalDto.digest;
    if (digest) {
      const imageRepository = this.selectedSbomReportImageMinimalDto.imageRepository ?? 'n/a';
      const imageName = this.selectedSbomReportImageMinimalDto.imageName ?? 'n/a';
      const imageTag = this.selectedSbomReportImageMinimalDto.imageTag ?? 'n/a';
      this.trivyDependencyDialogTitle = `Dependency Tree for Image ${imageRepository}/${imageName}:${imageTag}`;
      this.trivyImage = { digest: digest };
      this.isDependencyTreeViewVisible.set(true);
    }
  }

  private goToDetailedPage() {
    const url = this.router.serializeUrl(this.router.createUrlTree(['sbom-reports-detailed']));
    window.open(url, '_blank');
  }

  /**
   * Get Properties TreeNodes
   * First, it creates a Map<> with all PropertyName and {propertyValue, usedBy} - usedBy is the SbomDetail Name
   * Then, for each one, it creates a Set<> with propertyValue and then counts children
   * the final usedByCount for each propertyName is a unique sum of all usedBy children
   */
  private getSbomReportPropertyTreeNodes(): TreeNode[] {
    const tree: TreeNode[] = [];

    const dataMap = new Map<string, { propValue: string; usedBy: string }[]>();

    this.fullSbomDataDto?.details?.forEach((item) => {
      const usedBy = item.name ?? 'unknown';

      Object.entries(item.properties ?? {}).forEach(([propName, propValue]) => {
        if (propName === 'tod.group') return;

        if (!dataMap.has(propName)) {
          dataMap.set(propName, []);
        }

        dataMap.get(propName)!.push({ propValue, usedBy });
      });
    });

    dataMap.forEach((entries, propName) => {
      const uniqueUsedBySet = new Set<string>();

      const propNameNode: TreeNode = {
        data: { name: propName, usedByCount: 0 },
        children: [],
      };

      const propValueUsedByMap = new Map<string, Set<string>>();
      entries.forEach((entry) => {
        uniqueUsedBySet.add(entry.usedBy);

        if (!propValueUsedByMap.has(entry.propValue)) {
          propValueUsedByMap.set(entry.propValue, new Set<string>());
        }
        propValueUsedByMap.get(entry.propValue)!.add(entry.usedBy);
      });

      propValueUsedByMap.forEach((usedBySet, propValue) => {
        const propValueNode: TreeNode = {
          data: { name: propValue, usedByCount: usedBySet.size },
          children: [],
        };

        usedBySet.forEach((usedBy) => {
          propValueNode.children!.push({
            data: { name: usedBy, usedByCount: undefined },
            children: [],
          });
        });

        propNameNode.children!.push(propValueNode);
      });

      propNameNode.data.usedByCount = uniqueUsedBySet.size;

      tree.push(propNameNode);
    });

    return tree;
  }

  private getSbomReportLicenseTreeNodes(): TreeNode[] {
    const licenseMap = new Map<string, Set<string>>();

    this.fullSbomDataDto?.details?.forEach((item) => {
      (item.licenses || []).forEach((license) => {
        if (!licenseMap.has(license)) {
          licenseMap.set(license, new Set<string>());
        }
        licenseMap.get(license)!.add(item.name ?? 'unknown');
      });
    });

    const tree: TreeNode[] = [];
    licenseMap.forEach((names, license) => {
      const licenseNode: TreeNode = {
        data: { name: license, count: names.size },
        children: Array.from(names).map((name) => ({
          data: { name: name, count: undefined },
          children: [],
        })),
      };
      tree.push(licenseNode);
    });

    return tree;
  }
}
