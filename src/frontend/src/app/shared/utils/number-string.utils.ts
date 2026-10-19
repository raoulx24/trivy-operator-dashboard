export class NumberStringUtil {
  public static FormatOrdinal(n: number): string {
    const suffixes = {
      zero: 'th',
      one: 'st',
      two: 'nd',
      few: 'rd',
      other: 'th',
      many: 'th',
    };
    const pr = new Intl.PluralRules('en-US', { type: 'ordinal' });
    const rule = pr.select(n);
    return `${n}${suffixes[rule] || 'th'}`;
  }
}
