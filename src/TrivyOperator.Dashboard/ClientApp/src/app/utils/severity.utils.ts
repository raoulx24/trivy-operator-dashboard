import { SeverityDto } from '../../api/models/severity-dto';

interface SeverityExtendedDto extends SeverityDto {
  short: string;
}

export class SeverityUtils {
  static severityDtos: ReadonlyArray<SeverityExtendedDto> = [
    { id: 0, name: 'CRITICAL', short: 'CRIT' },
    { id: 1, name: 'HIGH', short: 'High' },
    { id: 2, name: 'MEDIUM', short: 'MED' },
    { id: 3, name: 'LOW', short: 'LOW' },
    { id: 4, name: 'UNKNOWN', short: 'UNK' },
    { id: 8, name: 'N/A', short: 'NA'},
  ];
  static severityShortDtos: ReadonlyArray<SeverityExtendedDto> = [
    { id: 0, name: 'CRITICAL', short: 'CRIT' },
    { id: 1, name: 'HIGH', short: 'High' },
    { id: 2, name: 'MEDIUM', short: 'MED' },
    { id: 3, name: 'LOW', short: 'LOW' },
  ];
  private static colorIntensity: number = 400;

  public static getCssColor(severityId: number): string {
    const documentStyle = getComputedStyle(document.documentElement);
    let colorIntensity = this.colorIntensity;
    // if (severityId === 11) {
    //   colorIntensity = colorIntensity - 100;
    // }
    const color: string = '--p-' + this.getColor(severityId) + '-' + (colorIntensity + 100);

    return documentStyle.getPropertyValue(color);
  }

  public static getCssColorHover(severityId: number): string {
    const documentStyle = getComputedStyle(document.documentElement);
    const color: string = '--p-' + this.getColor(severityId) + '-' + this.colorIntensity;

    return documentStyle.getPropertyValue(color);
  }

  public static getCssColorByName(severityName: string): string {
    const severityId = this.severityDtos.find((x) => x.name == severityName)?.id ?? 4;
    return this.getCssColor(severityId);
  }

  public static getColor(severityId: number): string {
    switch (severityId) {
      case 0:
        return 'red';
      case 1:
        return 'orange';
      case 2:
        return 'yellow';
      case 3:
        return 'cyan';
      case 4:
        return 'blue';
      case 10:
        return 'gray';
      case 11:
        return 'sky';
      default:
        return '';
    }
  }

  public static getName(severityId: number): string {
    return this.severityDtos.find((x) => x.id === severityId)?.name ?? '';
  }

  public static getShortName(severityId: number): string {
    return this.severityDtos.find((x) => x.id === severityId)?.short ?? '';
  }

  public static getCapitalizedName(severityId: number): string {
    if (severityId === 8)
      return SeverityUtils.getName(severityId);
    const severityName = SeverityUtils.getName(severityId).toLowerCase();
    return this.getCapitalizedString(severityName);
  }

  public static getCapitalizedShortName(severityId: number): string {
    const severityName = SeverityUtils.getShortName(severityId).toLowerCase();
    return this.getCapitalizedString(severityName);
  }

  public static getCapitalizedString(severityName: string): string {
    severityName = severityName.toLowerCase();
    return severityName.length == 0 ? '' : severityName.charAt(0).toUpperCase() + severityName.slice(1);
  }

  public static getSeverityIds(): number[] {
    return this.severityDtos ? SeverityUtils.severityDtos.map((x) => x.id).sort((a, b) => a - b) : [];
  }

  public static getNames(severityIds: number[], maxDisplay?: number): string {
    severityIds = severityIds ? severityIds : [];
    maxDisplay = maxDisplay ? maxDisplay : 0;

    if (severityIds.length == 0) {
      return '';
    }
    if (severityIds.length > maxDisplay) {
      return `${severityIds.length} selected`;
    } else {
      const selectedSeverityNames: string[] = [];
      severityIds
        .sort((a, b) => a - b)
        .forEach((x) => {
          selectedSeverityNames.push(this.getCapitalizedName(x));
        });
      return selectedSeverityNames.join(', ');
    }
  }
}
