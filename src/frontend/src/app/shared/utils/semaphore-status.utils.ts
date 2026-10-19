export class SemaphoreStatusUtils {
  private static get colorIntensity(): number {
    return 400;
  }

  public static getCssColorByName(statusName: string): string {
    const documentStyle = getComputedStyle(document.documentElement);
    const colorName = this.getColorByName(statusName);
    const color: string = '--p-' + colorName + '-' + this.getIntensityByColor(colorName);

    return documentStyle.getPropertyValue(color);
  }

  public static getColorByName(statusName: string): string {
    switch (statusName.toLowerCase()) {
      case 'green':
        return 'green';
      case 'yellow':
        return 'yellow';
      case 'red':
        return 'red';
      case 'neutral':
        return 'sky';
      case 'unknown':
        return 'blue';
      default:
        return 'blue';
    }
  }

  // neutral (sky) sits one step lighter than the other colours
  private static getIntensityByColor(colorName: string): number {
    return colorName === 'sky' ? this.colorIntensity : this.colorIntensity + 100;
  }
}
