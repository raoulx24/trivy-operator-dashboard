export class LocalStorageUtils {
  public static readonly csvFileNameKeyPrefix: string = 'csvFileName.';
  public static readonly trivyTableKeyPrefix: string = 'trivyTable.';

  public static getKeysWithPrefix(prefix: string): string[] {
    const keys: string[] = [];
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key && key.startsWith(prefix)) {
        keys.push(key);
      }
    }
    return keys;
  }

  public static getBoolKeyValue(keyName: string): boolean | null {
    return LocalStorageUtils.convertStringToBoolean(localStorage.getItem(keyName));
  }

  public static getNumberKeyValue(keyName: string): number | null {
    return LocalStorageUtils.convertStringToNumber(localStorage.getItem(keyName));
  }

  public static convertStringToBoolean(value: string | null): boolean | null {
    switch (value?.toLowerCase().trim()) {
      case 'true':
      case 'yes':
      case '1':
        return true;
      case 'false':
      case 'no':
      case '0':
        return false;
      default:
        return null;
    }
  }

  public static convertStringToNumber(value: string | null): number | null {
    if (value === null) {
      return null;
    }
    const parsedNumber = Number(value);

    return isNaN(parsedNumber) ? null : parsedNumber;
  }

  public static toCamelCase(text: string): string {
    return text
      .toLowerCase()
      .replace(/[^a-zA-Z0-9.]+/g, ' ') // keep letters, digits, dot; turn others into spaces
      .trim()
      .split(/\s+/) // split on spaces
      .map((word, index) => (index === 0 ? word : word.charAt(0).toUpperCase() + word.slice(1)))
      .join('');
  }
}
