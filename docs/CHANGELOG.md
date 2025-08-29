# Changelog

> Entries are grouped by feature-level headings with `Added`, `Changed`, and `Fixed` subsections.
> Use one bullet per topic.

## [Unreleased]

### Core Framework
#### Added
- Reusable service rule and screen abstractions with DI registration and view model integration.
- Generic `ServiceCreateViewModelBase<TOptions>` and `ServiceEditViewModelBase<TOptions>` consolidating save, cancel, and advanced configuration commands.
- Service manager tracks task start times and writes statuses to `activeservices.txt` for running services.

#### Changed
- Clarified environment instruction precedence in `AGENTS.md`.
- Renamed root `CollaborationAndDebugTips.txt` to `CollaborationGuidelines.txt` and clarified distinction from `docs/CollaborationAndDebugTips.txt`.
- Updated `global.json` to require the .NET 8 SDK version `8.0.404`.
- Disabled default `AutoStart` and set environment configuration files to `"AutoStart": false`.
- Core library targets `net8.0` to avoid Windows targeting pack restore errors.
- Adjusted solution and project references so cross-platform assemblies depend only on the core while Windows projects also reference `DesktopApplicationTemplate.Windows`.

### Navigation & UI
#### Added
- Navigation helpers for HTTP, HID, File Observer, Heartbeat, CSV Creator, and SCP services with tests ensuring double-click opens edit views.
- Application logo displayed in the main window navigation bar.
- Navigation bar `HeaderBar` supports drag and toggles window state on double-click.
- Popup-based `FilterPanel` user control for in-place service filtering.
- Active service counter displayed in the main window with real-time updates.
- Help window includes a close button.
- Average execution time displayed next to each service name in the service list.
- Text inputs now automatically display tooltips derived from bound property names, guiding expected user input.

#### Changed
- Service selection window wraps service icons within bounds using a fixed-width panel.
- Main window routes MQTT service creation through a dedicated create view and opens the tag subscriptions view after setup.
- Service context menus invoke `EditServiceCommand`; editing an MQTT service opens the connection view with current options preloaded.
- MQTT create, edit, and subscription views follow design spacing with shared form styles and accessibility names.
- Service creation flows now display within the main view, removing the separate Create Service window and placeholder navigation text.
- Main window height constrained to the work area to prevent overlapping the taskbar.
- MQTT create and edit views include tooltips on text fields to clarify expected input.

#### Fixed
- TCP and SCP edit workflows now load existing options via `Load` methods, enabling DI-friendly construction.
- Service list averages use one-way bindings to avoid runtime parse exceptions.
- Main window declares behaviors namespace to prevent XAML parse errors.
- Forms theme uses project namespaces without assembly qualifiers and relies on SDK implicit page inclusion to load the `FormField` style without duplicate XAML item errors.
- System namespace references and form style resources compiled to eliminate XAML parse failures.
- Marked main window `ContentFrame` public so UI tests can inspect navigation.
- Included `Forms.xaml` in theme resources with Page build action.
- Installer window references `TextBoxHintBehavior.AutoToolTip` without design-time warnings.
- Application startup tolerates a missing `MainView` service, preventing test crashes when the window isn't registered.

### HID Service
#### Added
- HID service view includes a data flow diagram showing incoming, processed, and outgoing data bound to the view model.
- HID service creation, edit, and advanced configuration views with navigation tests.

### FTP Service
#### Added
- FTP service creation, edit, and advanced configuration views with DI registration, validation, and navigation tests.
- FTP service view displays active transfer progress, connected client count, and status indicator.
- FTP server hosting service with start/stop methods, transfer events, and unit tests.

#### Changed
- FTP DI registration test now registers configuration to avoid missing `IConfiguration` errors.
- Downgraded FubarDev FTP server packages to version `3.1.2` to resolve missing NuGet feeds.

#### Fixed
- Selecting FTP service in the add services window no longer freezes; create service window closes after save or cancellation.
- FTP server create/edit windows preload options and default to the updated FTP service view with start/stop commands.
- FTP server edit view checks for a `null` view model before initializing XAML, preventing parse exceptions.
- Variable naming conflicts resolved in main window edit workflow to prevent build errors.
- FTP server creation no longer freezes when selecting the service type and closes the selection window after saving.
- Advanced configuration view injects logging via DI and initializes with a view model, resolving constructor instantiation errors.

### TCP Service
#### Added
- TCP service creation and message viewer enabling configuration and inspection of endpoint traffic.
- Dedicated TCP edit and advanced configuration views with navigation tests.
- View and view model for displaying TCP service messages with log-level filtering and log management commands.

#### Changed
- `TcpServiceViewModel` evaluates scripts asynchronously with streamlined server toggle logging.
- Registered transient TCP view models and bound `TcpServiceOptions` configuration; each `ServiceViewModel` retains its own options.
- Service persistence saves and restores `TcpServiceOptions` for TCP services.

#### Fixed
- Replaced `UriParser.GetSyntax` usage with `IsKnownScheme` and guarded TCP option restoration to avoid null references.
- Registered the WPF pack URI scheme so BubblyWindow resources load without invalid URI errors.

### MQTT Service
#### Added
- Wizard-style service creation view capturing broker, credentials, TLS, will message, and connection options.
- MQTT connections support option-based configuration, TLS/WebSocket, QoS, retain flag, keep-alive, clean session, reconnect delay, and structured logging.
- UI for configuring MQTT endpoint–message pairs with placeholders, tooltips, per-tag outgoing test messages, and will-message support.
- Tag subscriptions support per-topic endpoints, QoS selection, subscribe/unsubscribe commands, and visual feedback.
- Dedicated window for editing MQTT connection settings with update, cancel, and unsubscribe commands.

#### Changed
- `MqttService` refactored with options-based constructor, clean reconnect logic, and consolidated publish methods.
- `MqttServiceViewModel` uses `MqttServiceOptions` for settings and delegates token resolution to `MessageRoutingService`.
- `MessageRoutingService` tracks latest messages per service and resolves `{ServiceName.Message}` tokens before publishing.
- `MqttTagSubscriptionsViewModel` consolidated to a single subscription collection with unified properties.
- Removed obsolete MQTT options model and duplicate subscribe implementations.
- MQTT service creation now occurs within the main window frame and returns after completion.

#### Fixed
- MQTT service disconnects before reconnecting when settings change and converts blank will-topic/payload fields to `null`.
- QoS enum binding resolved; selecting the MQTT service no longer throws runtime exceptions.
- Removed invalid `MouseDoubleClick` XAML handlers and duplicate styles that caused build failures.
- `MqttCreateServiceView` no longer hosts duplicate `StackPanel` elements, resolving build errors.
- Added missing `MQTTnet.Protocol` using to restore `MqttQualityOfServiceLevel` references.
- Host validation now accepts domain names and rejects underscores.
- Restored `MqttEndpointMessage` namespace in MQTT service view model to fix build errors.

### CSV Service
#### Added
- CSV service creation and edit views with advanced configuration and navigation tests.
- CSV creator supports selecting an output directory and nested folder patterns when naming files.
- Integrated `CsvServiceView` into the main window; CSV creator no longer adds columns for itself or other CSV services.

#### Changed
- Removing a service deletes its columns from the CSV list and resets the output file.

#### Fixed
- CSV logging rotates to a new indexed file when services are removed without deleting existing output.
- Empty CSV configuration files no longer cause JSON parsing errors.
- CSV service increments file index only when `FileNamePattern` contains `{index}`.
- Guarded CSV viewer configuration serialization to prevent stack overflow on empty data.
- Eliminated recursive logging and added guards that capture configuration snapshots on save failures.

### HTTP Service
#### Added
- HTTP service creation and edit views with advanced configuration for authentication and TLS certificate paths.

### File Observer
#### Added
- File search service with async caching and DI integration for File Observer.
- File Observer create, edit, and advanced configuration views with navigation and DI registration.

### Logging
#### Added
- Logging service loads existing log file on startup and reloads entries when the minimum level changes.
- Core `ILoggingService` interface and `LogLevel` enum shared across projects.

#### Fixed
- Service persistence and logging tests stabilized by reloading options, awaiting file writes, and running settings-related tests sequentially.
- Logging minimum level changes now re-filter existing log entries without reloading from disk.
- Added tests confirming service persistence handles cyclical references and logging config changes.
- Moved `LogEntry` to core and restored logging event subscriptions, resolving missing reference build errors.
- Registered `LoggingService` with the DI container to satisfy `SaveConfirmationHelper` and prevent runtime errors.
- Marked log models and levels as Windows-specific to silence cross-platform analyzer warnings.
- Added missing WPF framework and logging abstraction references to core and UI projects, resolving `System.Windows.Media` and `ILogger` build errors.

#### Changed
- Removed custom `ILoggingService` and service registrations in favor of `Microsoft.Extensions.Logging` with console and debug providers.
- Relocated `LogEntry`, `LogLevel`, and `ILoggingService` to a new `DesktopApplicationTemplate.Windows` library to remove WPF dependencies from the core project.
- `LogEntry` now stores colors as hex strings instead of `System.Windows.Media.Brush`.

### Documentation & CI
#### Added
- Section on working in restricted environments and reminder to log limitations in collaboration docs.
- Documented architecture and coding standards in `AGENTS.md`.
- `CONTRIBUTING.md` and PR template enforcing CI-only testing with a CI badge in the README.
- `/test` comment workflow to run CI on demand.
- Dedicated `DesktopApplicationTemplate.UI.Tests` project targeting `net8.0-windows` for WPF-specific tests.
- `WindowsFact` and `WindowsTheory` attributes skipping tests on non-Windows hosts.
- `TestCommon` library providing shared test helpers and fixtures referenced by all test projects.
- Collaboration tips note that WPF projects require Windows or the WindowsDesktop runtime and fail with `InitializeComponent` and `NETSDK1100` errors if missing.

#### Changed
- Consolidated GitHub Actions into a single `CI` workflow with collaboration instructions in `AGENTS.md`.
- CI workflow runs on pushes to `feature/**` and `bugfix/**` branches, supports manual triggers, and skips checks for pull requests targeting `dev`.
- Updated GitHub workflows to install the WPF workload instead of the deprecated `windowsdesktop` workload.
- Reorganized collaboration log into topic-based blocks and added logging guidelines.
- Marked `TestCommon` as a non-test project to prevent `dotnet test` from discovering it.
- Clarified that new notes should extend existing topic blocks without repeating timestamps.
- Removed Codex-specific tests and categories, eliminating the `CodexSafe` trait and custom `TestCategoryAttribute`.
- Setup script now runs only the primary test suite after removing the Codex test project.
- Removed Windows desktop runtime checks from tests so they run when Visual Studio provides the runtime.
- Core unit test project targets cross-platform `net8.0` for broader compatibility.
- Removed WPF workload installation steps; WPF ships with the Windows .NET SDK.
- UI tests run on a dispatcher helper that captures unhandled dispatcher exceptions.
- Temporarily removed ThemeManager UI test that triggered dispatcher access errors.
- Migrated tests from `WpfFact`/`WpfTheory` to `WindowsFact`/`WindowsTheory` and dropped the WPF test collection fixture.

#### Fixed
- Added missing `FluentAssertions` package reference to the test project and documented dependency checks to avoid build failures.
- Removed duplicate using directives and missing namespace references that prevented solution builds.
- Guarded WPF test thread apartment configuration with an OS check to avoid CA1416 build errors on non-Windows hosts.
- Added `StubFileDialogService` to test project and updated MQTT and FTP UI tests for API changes.
- ThemeManager test executes on the current thread, removing manual thread usage.
- FTP view tests use a helper to initialize `Application` resources, eliminating manual thread setup.
- Console test logger writes plain text messages to avoid Visual Studio RPC errors when expanding test results.
- Dispatcher test helper pumps the message loop to prevent deadlocks in UI navigation tests.

