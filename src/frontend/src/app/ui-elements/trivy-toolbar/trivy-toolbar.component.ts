import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  inject,
  OnDestroy,
  signal,
  ViewChild,
} from '@angular/core';

@Component({
  selector: 'app-trivy-toolbar',
  standalone: true,
  imports: [],
  templateUrl: './trivy-toolbar.component.html',
  styleUrl: './trivy-toolbar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrivyToolbarComponent implements AfterViewInit, OnDestroy {
  @ViewChild('scrollContainer') scrollContainer?: ElementRef;

  readonly showLeftButton = signal(false);
  readonly showRightButton = signal(false);

  private resizeObserver?: ResizeObserver;

  private readonly cdr = inject(ChangeDetectorRef);

  ngAfterViewInit() {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;

    this.updateScrollState();
    el.addEventListener('scroll', this.handleScroll);
    this.resizeObserver = new ResizeObserver(() => {
      this.updateScrollState();
    });
    this.resizeObserver.observe(el);
  }

  ngOnDestroy() {
    const el = this.scrollContainer?.nativeElement;
    if (el) el.removeEventListener('scroll', this.handleScroll);
    this.resizeObserver?.disconnect();
  }

  handleScroll = () => {
    this.updateScrollState();
  };

  scrollLeft() {
    this.scrollContainer?.nativeElement.scrollBy({ left: -150, behavior: 'smooth' });
  }

  scrollRight() {
    this.scrollContainer?.nativeElement.scrollBy({ left: 150, behavior: 'smooth' });
  }

  private updateScrollState() {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;

    const { scrollWidth, clientWidth, scrollLeft } = el;

    this.showLeftButton.set(scrollLeft > 0);
    this.showRightButton.set(scrollLeft + clientWidth < scrollWidth);

    this.cdr.markForCheck();
  }
}
