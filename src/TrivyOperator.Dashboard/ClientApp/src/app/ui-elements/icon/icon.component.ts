import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { SafeHtml } from '@angular/platform-browser';
import { IconRegistryService } from '../../services/icon-registry.service';

@Component({
  selector: 'app-icon',
  imports: [CommonModule],
  templateUrl: './icon.component.html',
  styleUrl: './icon.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconComponent {
  name = input.required<string>();
  class = input<string>('');

  svgContent = signal<SafeHtml>(''); // safe HTML for template

  private readonly registry = inject(IconRegistryService);

  constructor() {
    effect(() => {
      const iconName = this.name();
      if (!iconName) return;

      this.registry.getIcon(iconName).then((svg) => {
        this.svgContent.set(svg);
      });
    });
  }
}
