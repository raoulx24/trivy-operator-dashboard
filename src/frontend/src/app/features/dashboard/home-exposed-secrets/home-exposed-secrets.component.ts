import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';

import { HomeExposedSecretImageDto } from '@api/models/home-exposed-secret-image-dto';
import { HomeService } from '@api/services/home.service';
import { SeverityCssStyleByIdPipe } from '@shared/pipes/severity-css-style-by-id.pipe';
import { SeverityNameByIdPipe } from '@shared/pipes/severity-name-by-id.pipe';
import { ScrollerComponent } from '@shared/ui/scroller/scroller.component';

import { HomeImageHistoryService } from '../home-pivot/home-image-history.service';
import { HomeWidgetBase } from '../home-widget-base/home-widget.base';
import { HomeWidgetFrameComponent, HomeWidgetHelp } from '../home-widget-frame/home-widget-frame.component';
import { HomeWidgetState } from '../home-widget-placeholder/home-widget-placeholder.types';
import { IconComponent } from '@shared/ui/icon/icon.component';

// Exposed secrets: the running images with secrets found in them (an access key in a file, a token...), the worst first.
// Collapsed only. It loads by itself (a change of a filter, or of refresh, reloads it); only the namespaces filter
// applies. The secret value is never sent by the backend: the widget shows the rule and the file only.
// no secrets is the normal state: it says how many images were scanned and when, so the zero is not "no data"
@Component({
  selector: 'app-home-exposed-secrets',
  standalone: true,
  imports: [IconComponent, HomeWidgetFrameComponent, ButtonModule, ScrollerComponent, TagModule, SeverityCssStyleByIdPipe, SeverityNameByIdPipe],
  templateUrl: './home-exposed-secrets.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // a grid cell: wide content must wrap, not widen the cell
  host: { class: 'block min-w-0' },
})
export class HomeExposedSecretsComponent extends HomeWidgetBase {
  // images asked for (the server allows 500), scrolled in a viewport of maxVisibleImages; the count above them is the real one
  private static readonly limit = 500;
  private static readonly maxVisibleImages = 2;

  // the height of an image row (the same for all, so the viewport is known): the name line and the types line, one line each;
  // the scroller puts a gap between the rows (space-y-4)
  protected readonly rowHeight = 64;
  private static readonly rowGap = 16;

  private readonly homeService = inject(HomeService);
  private readonly imageHistoryService = inject(HomeImageHistoryService);

  hint = input<string>('');
  namespaces = input.required<string[]>();
  // changes (e.g. a counter) to reload with unchanged filters
  refresh = input<number>(0);

  protected readonly help: HomeWidgetHelp = {
    question: `Which <strong>running images contain secrets</strong> (keys, tokens, passwords found in their files)? These need action whatever their severity.`,
    goodToKnow: [
      `The secret values are <strong>never shown</strong>, only the type of each one, colored by its severity.`,
      `Worst severity first, then the images with more secrets.`,
      `Only the <strong>namespaces filter</strong> applies, not the severities.`,
      `With no secrets the widget says how many images were scanned and when.`,
    ],
    tips: [
      `Rotate the secret first, then rebuild the image.`,
      `The link opens the exposed secrets report of the image.`,
    ],
  };

  private readonly scope = computed(() => ({
    namespaces: this.namespaces().length > 0 ? this.namespaces() : undefined,
    refresh: this.refresh(),
    localRefresh: this.localRefresh(),
  }));

  private readonly data = rxResource({
    params: this.scope,
    stream: ({ params }) =>
      this.homeService.getHomeExposedSecrets({
        namespaces: params.namespaces,
        limit: HomeExposedSecretsComponent.limit,
      }),
  });

  protected readonly dto = computed(() => this.data.value());
  protected readonly items = computed(() => this.dto()?.items ?? []);
  protected readonly totalSecrets = computed(() => this.dto()?.totalSecrets ?? 0);
  protected readonly totalImages = computed(() => this.dto()?.totalImages ?? 0);
  // the viewport: the rows when there are few, else maxVisibleImages of them
  protected readonly viewportHeight = computed(() => this.rowsHeight(Math.min(this.items().length, HomeExposedSecretsComponent.maxVisibleImages)));
  // all the rows: the most the box takes when the widget has room to spare
  protected readonly contentHeight = computed(() => this.rowsHeight(this.items().length));
  protected readonly hasSecrets = computed(() => this.totalSecrets() > 0);
  protected readonly hiddenImages = computed(() => Math.max(0, this.totalImages() - this.items().length));
  protected readonly scanned = computed(() => {
    const dto = this.dto();
    if (!dto) {
      return '';
    }
    const images = `${dto.imagesScanned} ${dto.imagesScanned === 1 ? 'image' : 'images'} scanned`;
    return dto.lastScanAt ? `${images} · last scan ${this.ago(dto.lastScanAt)}` : images;
  });

  protected readonly state = computed<HomeWidgetState>(() => {
    if (this.data.error()) {
      return { error: (this.data.error() as Error)?.message ?? 'request failed' };
    }

    const dto = this.dto();
    if (!dto) {
      return {};
    }

    return {
      isAvailable: dto.isAvailable,
      unavailableReason: dto.unavailableReason,
      summary: `${dto.totalSecrets} secrets in ${dto.totalImages} images`,
    };
  });

  protected imageName(item: HomeExposedSecretImageDto): string {
    return `${item.repository}:${item.tag}`;
  }

  protected openReport(item: HomeExposedSecretImageDto): void {
    this.imageHistoryService.openExposedSecrets(item.digest);
  }

  private rowsHeight(rows: number): number {
    return rows * this.rowHeight + Math.max(0, rows - 1) * HomeExposedSecretsComponent.rowGap;
  }

  // all the types of an image, for the title of its (clipped) line
  protected typesText(item: HomeExposedSecretImageDto): string {
    return item.byType.map((x) => `${x.type}: ${x.count}`).join(', ');
  }

  protected namespacesText(item: HomeExposedSecretImageDto): string {
    return item.namespaces === 1 ? item.namespaceNames[0] : `${item.namespaces} ns`;
  }

  private ago(value: string): string {
    const minutes = Math.max(0, Math.floor((Date.now() - new Date(value).getTime()) / 60_000));
    if (minutes < 1) {
      return 'just now';
    }
    if (minutes < 60) {
      return `${minutes}m ago`;
    }
    const hours = Math.floor(minutes / 60);
    return hours < 48 ? `${hours}h ago` : `${Math.floor(hours / 24)}d ago`;
  }
}
