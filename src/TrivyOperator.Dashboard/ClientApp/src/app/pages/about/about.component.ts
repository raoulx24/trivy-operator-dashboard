import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, OnInit, signal } from '@angular/core';

import { MarkdownModule, provideMarkdown } from 'ngx-markdown';
import { CardModule } from 'primeng/card';
import { PanelModule } from 'primeng/panel';
import { TagModule } from 'primeng/tag';

import { AppVersion } from '../../../api/models/app-version';
import { GitHubReleaseDto } from '../../../api/models/git-hub-release-dto';
import { AppVersionsService } from '../../../api/services/app-versions.service';
import { VersionUtils } from '../../utils/version.utils';
import { AboutCredits } from './about.types';

import { BooleanCssStylePipe } from '../../pipes/boolean-css-style.pipe';
import { CapitalizeFirstPipe } from '../../pipes/capitalize-first.pipe';
import { MainAppInitService } from '../../services/main-app-init.service';

export interface BackendFeature {
  feature: string;
  enabled: boolean;
}

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, MarkdownModule, CardModule, PanelModule, TagModule, BooleanCssStylePipe, CapitalizeFirstPipe],
  providers: [provideMarkdown()],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AboutComponent implements OnInit {
  releaseNotes = signal<GitHubReleaseDto[]>([]);
  currentVersion = signal<AppVersion | undefined>(undefined);
  latestVersion = signal<string | undefined>(undefined);
  newVersionAvailable = signal<boolean>(false);
  experimentalVersion = signal<boolean>(false);
  backendFeatures = signal<BackendFeature[]>([]);

  credits: AboutCredits[] = [
    {
      name: 'Trivy Operator',
      imgSrc: 'assets/trivy-operator-logo.png',
      imgAlt: 'Trivy Operator Logo',
      imgExtraStyle: 'filter: invert(1)',
      homeUrl: 'https://trivy.dev/latest/',
      gitUrl: 'https://github.com/aquasecurity/trivy-operator',
      docsUrl: 'https://aquasecurity.github.io/trivy-operator/latest/',
    },
    {
      name: '.NET',
      imgSrc: 'assets/dotnet.png',
      imgAlt: '.NET Logo',
      homeUrl: 'https://dotnet.microsoft.com/en-us/',
      gitUrl: 'https://github.com/dotnet/core',
      docsUrl: 'https://learn.microsoft.com/en-us/dotnet/',
    },
    {
      name: 'Angular',
      imgSrc: 'assets/angular-js.png',
      imgAlt: 'Angular Logo',
      homeUrl: 'https://angular.dev/',
      gitUrl: 'https://github.com/angular/angular',
      docsUrl: 'https://angular.dev/overview',
    },
    {
      name: 'PrimeNG',
      imgSrc: 'assets/primeng.png',
      imgAlt: 'PrimeNG Logo',
      homeUrl: 'https://primeng.org/',
      gitUrl: 'https://github.com/primefaces/primeng',
      docsUrl: 'https://primeng.org/installation',
    },
    {
      name: 'Tailwind CSS',
      imgSrc: 'assets/tailwind.png',
      imgAlt: 'Tailwind CSS Logo',
      homeUrl: 'https://tailwindcss.com/',
      gitUrl: 'https://github.com/tailwindlabs/tailwindcss',
      docsUrl: 'https://tailwindcss.com/docs/installation/using-vite',
    },
    {
      name: 'Open Telemetry',
      imgSrc: 'assets/opentelemetry.png',
      imgAlt: 'OpenTelemetry Logo',
      homeUrl: 'https://opentelemetry.io/',
      gitUrl: 'https://github.com/open-telemetry',
      docsUrl: 'https://opentelemetry.io/docs/',
    },
  ];

  private readonly appVersionsService = inject(AppVersionsService);
  private readonly mainAppInitService = inject(MainAppInitService);

  constructor() {
    effect(() => {
      const backendSettings = this.mainAppInitService.backendSettingsDto();

      const newFeatures: BackendFeature[] = [];
      newFeatures.push({ feature: 'Use Default Context', enabled: backendSettings.isDefaultContextUsed });
      newFeatures.push({ feature: 'Static Namespace List', enabled: backendSettings.isNamespaceListUsed });
      newFeatures.push({ feature: 'Custom kube.config', enabled: backendSettings.isKubeConfigUsed });
      newFeatures.push({ feature: 'Alternative Storage', enabled: backendSettings.isFileRepositoryUsed });

      this.backendFeatures.set(newFeatures);
    });
  }

  ngOnInit() {
    this.getReleaseNotesDtos();
  }

  getReleaseNotesDtos() {
    this.appVersionsService.getGitHubVersions().subscribe({
      next: (res) => this.onReleaseNoteDtos(res),
      error: (err) => console.error(err),
    });
    this.appVersionsService.getCurrentVersion().subscribe({
      next: (res) => this.onCurrentVersion(res),
      error: (err) => console.error(err),
    });
  }

  private onReleaseNoteDtos(data: GitHubReleaseDto[]) {
    this.releaseNotes.set(
      data.sort((a, b) => VersionUtils.parseVersion(b.tagName ?? '') - VersionUtils.parseVersion(a.tagName ?? '')),
    );
    this.latestVersion.set(data.find((x) => x.isLatest)?.tagName?.replace('v', ''));
    this.checkNewVersionAvailable();
  }

  private onCurrentVersion(data: AppVersion) {
    this.currentVersion.set(data);
    this.checkNewVersionAvailable();
  }

  private checkNewVersionAvailable() {
    const localCurrentVersion = this.currentVersion();
    const localReleaseNotes = this.releaseNotes();

    if (!localCurrentVersion || !localReleaseNotes || !localReleaseNotes[0]) {
      return;
    }

    const parsedCurrentVersion = VersionUtils.parseVersion(localCurrentVersion.fileVersion ?? '0.0');
    const parsedLastVersion = VersionUtils.parseVersion(localReleaseNotes[0].tagName ?? '0.0');

    this.newVersionAvailable.set(parsedLastVersion - parsedCurrentVersion > 0);
    this.experimentalVersion.set(parsedLastVersion - parsedCurrentVersion < 0);
  }
}
