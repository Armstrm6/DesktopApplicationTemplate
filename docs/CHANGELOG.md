# Changelog

## [Unreleased]
### Added
- Each MQTT tag subscription now retains its own outgoing test message, and the test message box binds to the selected tag's message.
- Wizard-style MQTT service creation view capturing broker, credentials, TLS, and will message options.
- Expanded MQTT service with option-based connections, TLS/WebSocket support, and structured logging.
- MQTT connections now support will message configuration, QoS, retain flag, keep-alive period, clean session, and reconnect delay with retry and option logging.
- Register `ILoggingService` and helper services with the DI container.
- Refactored save/close confirmation helpers to use constructor injection.
- Views now accept `ILoggingService` instances instead of creating loggers.
- Updated unit tests to inject mock loggers.
- Application logo displayed in the main window navigation bar.
- Navigation bar `HeaderBar` now supports drag and toggles window state on double-click.
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
- Unit tests covering default service name generation for all service types.
- Integrated `CsvServiceView` page, embedding CSV Creator configuration within the main window.
- Logging service loads existing log file on startup and can reload entries when the minimum level changes.
- Popup-based `FilterPanel` user control for in-place service filtering.
- Active service counter displayed in the main window with real-time updates when services change.
- MQTT view model now exposes will-message and connection options with validation and bindings in create/edit views.
- Dedicated window for editing MQTT connection settings with update, cancel, and unsubscribe commands accessible from the topic subscription view.
- MqttTagSubscriptionsView and view model for managing MQTT topic subscriptions displayed when adding new MQTT services.
- xUnit tests for MQTT create, subscription, and connection edit view models covering validation, command behavior, and option mapping.
- MqttService tests now verify TLS and credential configuration alongside will messages and keep-alive options.
- Tag subscriptions now allow per-topic QoS selection with forwarding to the MQTT client.
- Subscribe/unsubscribe support for MQTT topics with QoS selection and visual feedback for subscription results.

- File dialog service registered for TLS certificate selection in MQTT views.

### Changed
- Updated `global.json` to require the .NET 8 SDK version `8.0.404`.
- Default `AutoStart` is now disabled and all environment configuration files set `"AutoStart": false`.
- CI workflow now runs on pushes to `feature/**` and `bugfix/**` branches and supports manual triggers, ensuring tests execute on GitHub.
- `MqttCreateServiceView`, `MqttTagSubscriptionsView`, and `MqttEditConnectionView` along with their view models are now registered as transient services.
- `self-heal` workflow now monitors the unified `CI` pipeline.
- `TcpServiceViewModel` now evaluates scripts asynchronously and streamlined server toggle logging.
- Refactored `MqttService` with a single options-based constructor, clean reconnect logic, and consolidated publish methods.
- Extracted `IMessageRoutingService`, removed legacy `Route` API, and improved thread-safe message tracking with optional logging.
- Simplified `MqttServiceViewModel` to use `MqttServiceOptions` for settings and delegate token resolution to `MessageRoutingService`.
- `MqttService` and `MqttServiceViewModel` now consume `IOptions<MqttServiceOptions>` and the direct singleton registration was removed.
- Added a DI container test ensuring the service provider builds with MQTT components.
- Service creation now presents service types as icon bubbles and auto-generates default names upon selection.
- Main window routes MQTT service creation through a dedicated create view and opens the tag subscriptions view after setup. Editing an existing MQTT service now uses the connection edit view and persists updated options.
- Service context menus invoke a new `EditServiceCommand`; editing an MQTT service opens the connection view with current options preloaded.

### Removed
- Placeholder "Desktop Template" text from the navigation bar.
- Legacy GitHub workflows (`dotnet.yml`, `dotnet-desktop-ci.yml`, `ci.yml`).
- Unused `RichTextLogger` service and installer `CustomControl1` control.
- `PublishTopic` and `PublishMessage` fields from `MqttServiceViewModel`.
- Standalone `FilterWindow` in favor of inline filter popup.

### Fixed
- Corrected logo resource path so the image renders in the navigation bar.
 - Service creation and renaming now append numeric suffixes to avoid duplicate names.
- Updated GitHub workflows to install the WPF workload instead of the deprecated windowsdesktop workload.
- MQTT service now disconnects before reconnecting when settings change.
- Removed obsolete MQTT options model that caused duplicate property definitions.
- Updated MQTT service for new WebSocket configuration API and client certificate handling.
- Replaced unsupported information-level logs with debug logs in message routing components.
- Corrected `AppSettings` namespace to ensure configuration binding compiles.
- Fixed missing `AppSettings` references in startup services.
- Adjusted MQTT view model tests to return typed MQTTnet results instead of `Task.CompletedTask`, preventing build failures when APIs return generic tasks.
- Removed obsolete `MQTTnet.Client.Publishing` import in MQTT view model tests to resolve missing namespace build errors.
- Guarded CSV viewer configuration serialization to prevent stack overflow when saving empty data.
- Main window sizes to content so UI elements are no longer clipped at the edges.
- Added missing `FluentAssertions` package reference to the test project and documented dependency checks to avoid build failures.
- Service and settings persistence now ignore reference cycles during JSON serialization to avoid stack overflow when saving configuration.
- CSV service now increments file index only when `FileNamePattern` contains `{index}`.
- Added tests confirming the main view ignores non-Escape key presses and service persistence handles cyclical references.
- Eliminated recursive logging in CSV service to prevent stack overflow and added guards that capture configuration snapshots on save failures.
- Removed invalid `MouseDoubleClick` XAML handler and cleaned up ambiguous WPF references causing build failures.
- Corrected service count bindings to use one-way mode, preventing runtime errors on read-only properties.
- Resolved WPF build error by removing duplicate StackPanel from `MqttCreateServiceView` so the page hosts a single `ScrollViewer`.
- Added missing `MQTTnet.Protocol` using in `MqttCreateServiceViewModel` to restore `MqttQualityOfServiceLevel` references.
- Removed obsolete MQTT service view and dialog-based edit calls that broke build-time helper resolution.
