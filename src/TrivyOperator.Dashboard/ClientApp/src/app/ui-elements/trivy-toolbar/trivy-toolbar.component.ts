import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';

@Component({
  selector: 'app-trivy-toolbar',
  standalone: true,
  imports: [],
  templateUrl: './trivy-toolbar.component.html',
  styleUrl: './trivy-toolbar.component.scss'
})
export class TrivyToolbarComponent implements AfterViewInit, OnDestroy {
  @ViewChild('scrollContainer') scrollContainer?: ElementRef;

  showLeftButton = false;
  showRightButton = false;

  private resizeObserver!: ResizeObserver;
  private resizeTimeout?: any;

  ngAfterViewInit() {
    // without setTimeout, there is a NG0100 - because the showLeft/RightButton are changing too fast.
    setTimeout(() => {
      this.updateScrollState();
      }, 0);

    if (this.scrollContainer?.nativeElement) {
      this.scrollContainer.nativeElement.addEventListener('scroll', this.handleScroll);

      this.resizeObserver = new ResizeObserver(() => {
        clearTimeout(this.resizeTimeout);
        this.resizeTimeout = setTimeout(() => {
          this.updateScrollState();
        }, 200); // Adjust timing as needed
      });
      this.resizeObserver.observe(this.scrollContainer.nativeElement);
    }
  }

  ngOnDestroy() {
    if (this.scrollContainer?.nativeElement) {
      this.scrollContainer.nativeElement.removeEventListener('scroll', this.handleScroll);
      this.resizeObserver.disconnect(); // Stop observing
    }
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

  updateScrollState() {
    if (!this.scrollContainer?.nativeElement) return;

    const { scrollWidth, clientWidth, scrollLeft } = this.scrollContainer.nativeElement;
    this.showLeftButton = scrollLeft > 0;
    this.showRightButton = scrollLeft + clientWidth < scrollWidth;
  }
}
