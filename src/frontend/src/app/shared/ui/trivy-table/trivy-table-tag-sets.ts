import { SemaphoreStatusUtils } from '../../utils/semaphore-status.utils';

export type TagSetName = 'semaphore' | 'boolean' | 'logLevel';

export interface TagOption {
  value: string | boolean;
  label: string;
  style: { [key: string]: string };
  icon?: string;
}

// colors are theme independent, so every set is built once and shared by all tables
const semaphoreOption = (value: string, semaphoreName: string = value, label: string = value): TagOption => ({
  value,
  label,
  style: { background: SemaphoreStatusUtils.getCssColorByName(semaphoreName) },
});

const booleanOption = (value: boolean, label: string): TagOption => ({
  value,
  label,
  style: { background: getComputedStyle(document.documentElement).getPropertyValue(`--p-${value ? 'blue' : 'gray'}-400`) },
  icon: value ? 'pi pi-verified' : 'pi pi-times-circle',
});

const tagSetBuilders: Record<TagSetName, () => TagOption[]> = {
  semaphore: () => ['Green', 'Yellow', 'Red', 'Neutral', 'Unknown'].map((name) => semaphoreOption(name)),
  boolean: () => [booleanOption(true, 'True'), booleanOption(false, 'False')],
  // values are the LogLevel strings; colors reuse semaphore. Keep the fallback (last) option as debug/blue
  logLevel: () => [
    semaphoreOption('error', 'Red', 'Error'),
    semaphoreOption('warn', 'Yellow', 'Warn'),
    semaphoreOption('info', 'Neutral', 'Info'),
    semaphoreOption('debug', 'Unknown', 'Debug'),
  ],
};

export class TrivyTableTagSets {
  private static readonly cache = new Map<TagSetName, TagOption[]>();

  static getOptions(tagSet: TagSetName): TagOption[] {
    let options = this.cache.get(tagSet);
    if (!options) {
      options = tagSetBuilders[tagSet]();
      this.cache.set(tagSet, options);
    }
    return options;
  }

  // falls back to a neutral-looking option for values outside the set (e.g. a status added later)
  static resolve(tagSet: TagSetName, value: unknown): TagOption {
    const options = this.getOptions(tagSet);
    const found = options.find((x) => x.value === value || String(x.value).toLowerCase() === String(value).toLowerCase());
    return found ?? { value: value as string, label: String(value ?? 'N/A'), style: options[options.length - 1].style };
  }
}
