export class SemaphoreStatusUtils {
  private static get colorIntensity(): number {
    return 400;
  }

  public static getCssColorByName(statusName: string): string {
    const documentStyle = getComputedStyle(document.documentElement);
    const color: string = '--p-' + this.getColorByName(statusName) + '-' + (this.colorIntensity + 100);

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
      case 'unknown':
        return 'blue';
      default:
        return 'blue';
    }
  }
}
