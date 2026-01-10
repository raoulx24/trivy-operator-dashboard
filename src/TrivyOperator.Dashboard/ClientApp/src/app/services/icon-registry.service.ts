import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { firstValueFrom } from 'rxjs';
import { TrivyMessageService } from './trivy-message.service';

@Injectable({ providedIn: 'root' })
export class IconRegistryService {
  private readonly http = inject(HttpClient);
  private readonly sanitizer = inject(DomSanitizer);

  private readonly cache = new Map<string, SafeHtml>();

  private trivyMessageService = inject(TrivyMessageService);

  async getIcon(name: string): Promise<SafeHtml> {
    const cached = this.cache.get(name);
    if (cached) return cached;

    const path = `assets/icons/${name}.svg`;

    try {
      const raw = await firstValueFrom(this.http.get(path, { responseType: 'text' }));

      const safe = this.sanitizeSvg(raw);
      this.cache.set(name, safe);
      return safe;
    } catch (err) {
      this.trivyMessageService.pushSimple(`Failed to load icon: ${name}`, "Icon registry", "error", err, false);
      const fallback = this.sanitizer.bypassSecurityTrustHtml('<svg><!-- fallback --></svg>');
      this.cache.set(name, fallback);
      return fallback;
    }
  }

  private sanitizeSvg(raw: string): SafeHtml {
    const cleaned = raw
      .replace(/<script[\s\S]*?>[\s\S]*?<\/script>/gi, '') // remove <script>
      .replace(/\son\w+="[^"]*"/gi, '') // remove onload="..."
      .replace(/\son\w+='[^']*'/gi, '') // remove onload='...'
      .replace(/javascript:/gi, ''); // remove javascript:

    return this.sanitizer.bypassSecurityTrustHtml(cleaned);
  }
}
