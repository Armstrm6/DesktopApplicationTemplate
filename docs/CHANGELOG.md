# Changelog

## [Unreleased]
### Added
- Register `ILoggingService` and helper services with the DI container.
- Refactored save/close confirmation helpers to use constructor injection.
- Views now accept `ILoggingService` instances instead of creating loggers.
- Updated unit tests to inject mock loggers.
- Application logo displayed in the main window navigation bar.

### Removed
- Placeholder "Desktop Template" text from the navigation bar.

### Fixed
- Corrected logo resource path so the image renders in the navigation bar.
