Registration

```c#
services.AddSingleton<IPersistenceMigrationHistory, RedisPersistenceMigrationHistory>();
services.AddSingleton<IPersistenceMigrationRunner, PersistenceMigrationRunner>();

services.AddSingleton<IPersistenceMigration, CreateFooIndexMigration>();
services.AddSingleton<IPersistenceMigration, BackfillBarMigration>();
services.AddSingleton<IPersistenceMigration, ...>();
```

Run

```c#
var runner = app.Services.GetRequiredService<IPersistenceMigrationRunner>();

await runner.RunAsync();
```