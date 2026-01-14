export class VersionUtils {
  public static parseVersion(version: string): number {
    const parts = version.replace('v', '').split('.');
    const x = parseInt(parts[0], 10) || 0;
    const y = parseInt(parts[1], 10) || 0;
    const z = parseInt(parts[2], 10) || 0;
    return x * 10000 + y * 100 + z;
  }
}
