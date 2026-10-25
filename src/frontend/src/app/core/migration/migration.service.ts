import { Injectable } from '@angular/core';
import { injectLogger } from '../logging/inject-logger';
import { LocalStorageUtils } from '@shared/utils/local-storage.utils';
import { VersionUtils } from '@shared/utils/version.utils';

export type MigrationAction =
  | 'deleteAllTableDefinitions'
  | 'deleteTableDefinition'
  | 'deleteTableField'
  | 'addTableField'
  | 'renameTableField';

export interface MigrationStep {
  version: string;
  action: MigrationAction;
  table?: string;
  oldFieldName?: string;
  newFieldName?: string;
  width?: number;
  anchorLeft?: string;
  anchorRight?: string;
}

@Injectable({
  providedIn: 'root',
})
export class MigrationService {
  private readonly log = injectLogger('Migration');

  applyTableMigrations(fromVersion: string, toVersion: string, migrationSteps: readonly MigrationStep[]) {
    for (const step of migrationSteps) {
      if (!this.isFirstVersionOlder(fromVersion, step.version) || this.isFirstVersionOlder(toVersion, step.version))
        continue;
      this.log.debug(`Applying migration step: ${step.action}`, { data: { version: step.version } });

      switch (step.action) {
        case 'deleteAllTableDefinitions': {
          const keys = LocalStorageUtils.getKeysWithPrefix(LocalStorageUtils.trivyTableKeyPrefix);
          for (const key of keys) {
            LocalStorageUtils.removeItem(key);
          }
          break;
        }
        case 'deleteTableDefinition': {
          if (!step.table) break;
          const key = this.getTableKey(step.table);
          if (key) LocalStorageUtils.removeItem(key);
          break;
        }
        case 'deleteTableField': {
          if (!step.table || !step.oldFieldName) break;
          const key = this.getTableKey(step.table);
          const state = this.getTableDefinition(key);
          if (!state || !state.columnOrder) break;

          const indexToRemove = state.columnOrder.indexOf(step.oldFieldName);
          if (indexToRemove === -1) break;

          state.columnOrder.splice(indexToRemove, 1);

          const widths = state.columnWidths?.split(',') ?? [];
          widths.splice(indexToRemove, 1);
          state.columnWidths = widths.join(',');

          LocalStorageUtils.setItem(key, JSON.stringify(state));
          break;
        }
        case 'addTableField': {
          if (!step.table || !step.newFieldName) break;
          const key = this.getTableKey(step.table);
          const state = this.getTableDefinition(key);
          if (!state || !state.columnOrder || state.columnOrder.includes(step.newFieldName)) break;

          let insertAt = state.columnOrder.length; // default to end

          if (step.anchorLeft && state.columnOrder.includes(step.anchorLeft)) {
            insertAt = state.columnOrder.indexOf(step.anchorLeft) + 1;
          } else if (step.anchorRight && state.columnOrder.includes(step.anchorRight)) {
            insertAt = state.columnOrder.indexOf(step.anchorRight);
          }

          state.columnOrder.splice(insertAt, 0, step.newFieldName);
          state.columnWidths = this.insertWidth(state.columnWidths, insertAt, step.width ?? 100);

          LocalStorageUtils.setItem(key, JSON.stringify(state));
          break;
        }
        case 'renameTableField': {
          if (!step.table || !step.oldFieldName || !step.newFieldName) break;
          const key = this.getTableKey(step.table);
          const state = this.getTableDefinition(key);
          if (!state || !state.columnOrder) break;
          if (state.columnOrder.includes(step.newFieldName)) break;

          const renameIndex = state.columnOrder.indexOf(step.oldFieldName);
          if (renameIndex !== -1) {
            state.columnOrder[renameIndex] = step.newFieldName;
            LocalStorageUtils.setItem(key, JSON.stringify(state));
          }
          break;
        }
        default:
          this.log.warn(`Unknown migration action: ${step.action}`, { toast: false });
      }
    }
  }

  private isFirstVersionOlder(firstVersion: string, secondVersion: string): boolean {
    return VersionUtils.parseVersion(firstVersion) < VersionUtils.parseVersion(secondVersion);
  }

  private getTableKey(tableName: string): string {
    return `${LocalStorageUtils.trivyTableKeyPrefix}${tableName}`;
  }

  private getTableDefinition(key: string): any {
    const raw = LocalStorageUtils.getItem(key);
    return raw ? this.safelyParse(raw) : null;
  }

  private safelyParse(value: string): any {
    try {
      return JSON.parse(value);
    } catch {
      this.log.warn('Failed to parse localStorage value.', { toast: false });
      return {};
    }
  }

  private insertWidth(existing: string = '', index: number, width: number): string {
    const arr = existing.split(',');
    arr.splice(index, 0, String(width));
    return arr.join(',');
  }
}
