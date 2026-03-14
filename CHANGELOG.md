# Changelog

All notable changes to this project are documented in this file.

## [8.1.0] - 2026-07-18

### Breaking changes

- `UseEasyCoreEntityChange` now requires `IServiceProvider` (call from `AddDbContext((sp, opts) => ...)`). `BuildServiceProvider()` is removed.
- `EasyCoreEntityChange()` / `EasyCoreUnitOfWork()` no longer scan all DLLs by default. Use `.AddHandler<T>()` / `.RegisterSaveChangesFor<TService,TImpl>()` or opt-in `.EnableAssemblyScanning()`.
- Tenant filter is **fail-closed**: empty tenant id yields no rows (was: filter skipped).
- `Delete` on non-soft-delete entities now hard-deletes (was: silent no-op).
- `GetFirstAsync` throws when not found (was: returned null).
- Package license metadata aligned with repository `LICENSE` (Mulan PSL v2) via `PackageLicenseFile`.
- Mongo registration file renamed: `EasyCoreMongDbRepository.cs` → `EasyCoreMongoDbRepository.cs`.

### Added

- Entity markers and filter contracts remain inside each library (`EasyCore.EFCoreRepository` / `EasyCore.MongoDbRepository`) as independent packages.
- `ITenantProvider` / `HttpContextTenantProvider`.
- Ordered paging: `GetPagedList(..., orderBy, ascending)`.
- `EntityChangeOptions.SuppressHandlerExceptions` (default `false`).
- Soft-delete (`IsDeleted` false→true) dispatches deleted handlers.
- xUnit tests, GitHub Actions CI, `Directory.Build.props` (analyzers, XML docs, symbols).

### Fixed

- `UpdateMany` / `UpdateManyAsync` invalid `Entry(entities)` usage.
- `DeleteDirect*` incorrectly applying data filters.
- Async delete paths using sync `ToList()`.
- Soft-delete / tenant filters using non-translatable interface casts (`EF.Property` now).
- Unit of Work interceptor using sync `SaveChanges` / `BeginTransaction` inside async.
- Soft-delete detach tracking (entities are `Update`d when not tracked).
- Replaced obsolete `Microsoft.AspNetCore.Mvc` 2.1.3 with `Microsoft.AspNetCore.Http`.
