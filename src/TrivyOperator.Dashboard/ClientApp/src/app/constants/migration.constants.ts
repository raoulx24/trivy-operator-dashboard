import { MigrationStep } from '../services/migration.service';

export const trivyMigrations: readonly MigrationStep[] = [
  { version: '1.4', action: 'deleteAllTableDefinitions' },

  { version: '1.7', action: 'renameTableField', table: 'SBOM Reports - Detailed', oldFieldName: 'repository', newFieldName: 'imageRepository' },
  { version: '1.7', action: 'addTableField', table: 'Watcher States', newFieldName: 'eventsGauge', width: 155 },
  { version: '1.7', action: 'addTableField', table: 'Watcher States', newFieldName: 'recreateAction', width: 185 },
  { version: '1.9', action: 'addTableField', table: 'Vulnerability Reports History - Compared', newFieldName: 'modified', width: 90, anchorLeft: 'second' },
];
