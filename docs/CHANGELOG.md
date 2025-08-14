# Changelog

## [Unreleased]
### Added
- Expanded MQTT service with option-based connections, TLS/WebSocket support, and structured logging.
- Register `ILoggingService` and helper services with the DI container.
- Refactored save/close confirmation helpers to use constructor injection.
- Views now accept `ILoggingService` instances instead of creating loggers.
- Updated unit tests to inject mock loggers.
- Application logo displayed in the main window navigation bar.
- Consolidated GitHub Actions into a single `CI` workflow and introduced `AGENTS.md` with instructions to review collaboration docs.
- Added `/test` comment workflow to run CI on demand.
- Created `CONTRIBUTING.md` and PR template enforcing CI-only testing with a CI badge in the README.
- Introduced `AsyncRelayCommand` for asynchronous UI actions.
- Extended MQTT service with TLS support and safe reconnect behavior.
- Added MQTT view model token resolution and multi-topic publishing tests.
- Tooltips for MQTT endpoint configuration fields.
- Expanded MQTT configuration with token-based message routing using `{ServiceName.Message}` tokens and multiple endpoint mappings.
- Registered `MqttServiceOptions` and `MessageRoutingService` with DI and injected options into MQTT components.
- Added UI for configuring MQTT endpoint-message pairs with placeholders and tooltips.
- Added `MqttServiceOptions` with validation and tokenized endpoint/message publishing in `MqttServiceViewModel`.
- Added `MessageRoutingService` to track latest messages per service and resolve `{ServiceName.Message}` tokens before MQTT publishing.
- `MqttService` can now resolve message tokens and publish multiple messages per endpoint.
- Introduced `MqttServiceOptions` for configuring MQTT connection parameters.

### Changed
- CI workflow now runs on pushes to `feature/**` and `bugfix/**` branches and supports manual triggers, ensuring tests execute on GitHub.
- `self-heal` workflow now monitors the unified `CI` pipeline.
- `TcpServiceViewModel` now evaluates scripts asynchronously and streamlined server toggle logging.
- Refactored `MqttService` with a single options-based constructor, clean reconnect logic, and consolidated publish methods.
- Extracted `IMessageRoutingService`, removed legacy `Route` API, and improved thread-safe message tracking with optional logging.
- Simplified `MqttServiceViewModel` to use `MqttServiceOptions` for settings and delegate token resolution to `MessageRoutingService`.

### Removed
- Placeholder "Desktop Template" text from the navigation bar.
- Legacy GitHub workflows (`dotnet.yml`, `dotnet-desktop-ci.yml`, `ci.yml`).
- Unused `RichTextLogger` service and installer `CustomControl1` control.
- `PublishTopic` and `PublishMessage` fields from `MqttServiceViewModel`.

### Fixed
- Corrected logo resource path so the image renders in the navigation bar.
- Updated GitHub workflows to install the WPF workload instead of the deprecated windowsdesktop workload.
- MQTT service now disconnects before reconnecting when settings change.
