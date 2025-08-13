# Changelog

## [Unreleased]
### Added
- Register `ILoggingService` and helper services with the DI container.
- Refactored save/close confirmation helpers to use constructor injection.
- Views now accept `ILoggingService` instances instead of creating loggers.
- Updated unit tests to inject mock loggers.
- Application logo displayed in the main window navigation bar.
- Consolidated GitHub Actions into a single `CI` workflow and introduced `AGENTS.md` with instructions to review collaboration docs.
- Added `/test` comment workflow to run CI on demand.
- Created `CONTRIBUTING.md` and PR template enforcing CI-only testing with a CI badge in the README.
- Introduced `AsyncRelayCommand` for asynchronous UI actions.

### Changed
- CI workflow now runs on pushes to `feature/**` and `bugfix/**` branches and supports manual triggers, ensuring tests execute on GitHub.
- `self-heal` workflow now monitors the unified `CI` pipeline.
- `TcpServiceViewModel` now evaluates scripts asynchronously and streamlined server toggle logging.

### Removed
- Placeholder "Desktop Template" text from the navigation bar.
- Legacy GitHub workflows (`dotnet.yml`, `dotnet-desktop-ci.yml`, `ci.yml`).
- Unused `RichTextLogger` service and installer `CustomControl1` control.

### Fixed
- Corrected logo resource path so the image renders in the navigation bar.
- Updated GitHub workflows to install the WPF workload instead of the deprecated windowsdesktop workload.
- Setup script now skips WPF workload installation on non-Windows platforms to prevent errors.
