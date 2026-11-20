import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';

// the pages of an image, in a new tab (the same way the image dependency tree and the Base OS widget open them)
@Injectable({ providedIn: 'root' })
export class HomeImageHistoryService {
  private readonly router = inject(Router);

  // the vulnerability reports history of the image
  open(digest: string): void {
    this.openPage('vulnerability-reports-history', digest);
  }

  // the vulnerability report of the image
  openReport(digest: string): void {
    this.openPage('vulnerability-reports', digest);
  }

  // the exposed secrets report of the image
  openExposedSecrets(digest: string): void {
    this.openPage('exposed-secret-reports', digest);
  }

  private openPage(page: string, digest: string): void {
    const url = this.router.serializeUrl(this.router.createUrlTree(['/open-trivy-rep'], { queryParams: { page, digest } }));
    window.open(url, '_blank');
  }
}
