# Changelog

> Entries are grouped by feature-level headings with `Added`, `Changed`, and `Fixed` subsections.
> Use one bullet per topic.

## [Unreleased]

### Core Framework
#### Added
- Reusable service rule and screen abstractions with DI registration and view model integration.
- Shared service utilities moved into `DesktopApplicationTemplate.Services.Common` with logging base class and DI extension.
- Unified creation and edit workflows under `ServiceEditorViewModelBase<TOptions>` exposing `SaveCommand` and customizable `SaveButtonText`.
- Service manager tracks task start times and writes statuses to `activeservices.txt` for running services.
- Service factories convert options into initialized services and pages with a unified `AddServiceAsync` workflow.
- `ServiceType` enum clarifies supported service categories.
- Migration routine converts legacy service type names to `ServiceType` and logs unmapped values.
- `IServiceModule` interface enables service-specific DI registration and modules are discovered and registered automatically at startup.

#### Changed
- Clarified environment instruction precedence in `AGENTS.md`.
- Renamed root `CollaborationAndDebugTips.txt` to `CollaborationGuidelines.txt` and clarified distinction from `docs/CollaborationAndDebugTips.txt`.
- Updated `global.json` to require the .NET 8 SDK version `8.0.404`.
- Disabled default `AutoStart` and set environment configuration files to `"AutoStart": false`.
- Core library targets `net8.0` to avoid Windows targeting pack restore errors.
- Adjusted solution and project references so cross-platform assemblies depend only on the core while Windows projects also reference `DesktopApplicationTemplate.Windows`.
- Replaced `ServiceCreated`/`ServiceUpdated` with unified `ServiceSaved` events and centralized `ServiceName` validation in `ServiceEditorViewModelBase`.
- Windows service host sets explicit `ServiceName` and `DisplayName` values with the application name and installer uses the same constant.
- Replaced verbose `ServiceName` strings with `ServiceType` enum properties for log view models.
- Service persistence now stores `ServiceType` as short codes and reads legacy string names.
- Service creation and navigation now resolve services via `ServiceType` enum lookups instead of string-based switches.
- `ServiceManager` loads default services from configuration using `ServiceType` short codes and accepts legacy names.

#### Fixed
- Event raising helpers in `ServiceEditorViewModelBase` invoked themselves recursively; now invoke events directly.
- Removed unsupported `DisplayName` assignment from Windows service options to restore service build.

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
- Reusable `AdvancedConfigViewModelBase<TOptions>` and `AdvancedConfigButtonBar` unify Save/Back logic across advanced configuration views.
- Reusable `ServiceLogView` control and `ServiceLogViewModel` provide consistent log panels for the main window and services.
- Reusable `ServiceMessageTableView` and `ServiceMessageTableViewModel` display recent service messages with default columns, integrated into the TCP service view.
- Script editor window with Roslyn-based syntax highlighting and run/save commands.
- TCP service messages view adds an Edit Script button for modifying scripts and test messages.
- TCP service messages view displays script output next to the test message.
- FTP service view hosts a log panel displaying service-specific entries.
- Service list displays the last execution duration and most recent input message beneath each service name.
- Application registers global exception handlers that log errors, release input hooks, and shut down gracefully.
- Unit test ensures `VisualTreeHelperExtensions.FindParent` locates the containing `TextBlock` for inline elements.
- Tests confirm the main window delegates edit requests to service-type handlers.

#### Changed
- Service selection window wraps service icons within bounds using a fixed-width panel.
- Main window routes MQTT service creation through a dedicated create view and opens the tag subscriptions view after setup.
- Service context menus invoke `EditServiceCommand`; editing an MQTT service opens the connection view with current options preloaded.
- CSV create and edit views consolidated into a shared editor with dynamic save button text.
- MQTT create, edit, and subscription views follow design spacing with shared form styles and accessibility names.
- Service creation flows now display within the main view, removing the separate Create Service window and placeholder navigation text.
- Create service page limits options to TCP, MQTT, HTTP, FTP, SCP, and CSV services.
- Main window height constrained to the work area to prevent overlapping the taskbar.
- MQTT create and edit views include tooltips on text fields to clarify expected input.
- Create and edit service view models inject `IServiceRule` to validate required fields with XAML error tooltips.
- Service editor views share a reusable `EditorButtonBar` control with consistent automation names.
- Consolidated save and close dialogs into a configurable `ConfirmationWindow` with optional suppression.
- Event handlers use C# property pattern matching instead of casting `sender` and accessing `DataContext`.
- Replaced `as` cast and null check with pattern matching in `SettingsPage.NavigateBack`.
- Removed `KeyboardHelper` and exit hooks; simulated key presses manage their own cleanup.
- Removed `MainView` KeyDown handler; pressing Escape no longer returns to the home page.
- Removed unused `HomePage` view and `PackUriSchemeInitializer`.
- TCP create and edit views inline UDP and mode options, removing the separate advanced configuration view.
- TCP create and edit view models remove string-based service type fields, using `ServiceType` enum for navigation.
- TCP service messages view removes script editors and adds a test message input bound to view model.
- TCP scripting workflow consolidated into the messages view, enabling inline script editing and execution.
- TCP options persist the last test message and preload it when reopening the service.
- Service message table retains only the five most recent entries.
- TCP service messages view wrapped in a ScrollViewer with adjusted row sizing.
- ServiceMessageTableView limits height to keep the table compact.
- Replaced main window navigation logo with a text header.
- Centralized service navigation through `INavigationHandler` with DI-resolved handlers.
- Script editor exposes the built-in script via `DefaultScript`, and TCP service messages view loads it when no script is provided.
- Script editor raises an `OutputGenerated` event and TCP service messages view updates its output message when scripts run.
- TcpServiceMessagesViewModel executes scripts asynchronously and awaits results when saving.
- TcpServiceMessagesViewModel logs script execution results and exceptions.
- Restricted `TcpServiceMessagesViewModel.OutputMessage` setter to internal to prevent external modification.
- TcpServiceMessagesViewModel runs the initial script asynchronously to avoid blocking.
- App domain unhandled exception handler is asynchronous and awaits dispatcher shutdown.
- Main window resolves edit workflows through a DI-injected handler dictionary instead of a large if/else chain.
- Edit handlers register with DI keyed by `ServiceType`, and the main window receives a dictionary constructed from those registrations.

#### Fixed
- TCP and SCP edit workflows now load existing options via `Load` methods, enabling DI-friendly construction.
- Service list averages use one-way bindings to avoid runtime parse exceptions.
- Main window declares behaviors namespace to prevent XAML parse errors.
- Forms theme and views explicitly reference the `DesktopApplicationTemplate.UI` assembly for behaviors to ensure `TextBoxHintBehavior` is discovered.
- System namespace references and form style resources compiled to eliminate XAML parse failures.
- Marked main window `ContentFrame` public to allow navigation inspection.
- Included `Forms.xaml` in theme resources with Page build action.
- Installer window references `TextBoxHintBehavior.AutoToolTip` without design-time warnings.
- `TextBoxHintBehavior` now uses `DependencyObject` parameters so the installer recognizes `AutoToolTip`.
- Added missing helper namespace in `App.xaml.cs`, restoring `SaveConfirmationHelper`, `CloseConfirmationHelper`, and `DependencyChecker` registrations.
- Application startup tolerates a missing `MainView` service, preventing test crashes when the window isn't registered.
- Application shutdown tolerates a missing `MainViewModel` service, preventing test crashes when it's not registered.
- SCP service creation validates required fields and disables the Create command when inputs are invalid.
- Script editor Run command now persists test messages and outputs, falling back to the last routed message when no prior test exists.
- TCP service messages view always updates the output message, even when unchanged, ensuring script results appear.
- TCP script execution returns the processing result instead of null by returning the value from `Process`.
- Script editor Run command returns the processed message so the output field updates instead of showing null.
- Service message table disables auto-generated columns to prevent duplicate columns.
- `VisualTreeHelperExtensions.FindParent` now falls back to the logical tree for non-visual ancestors.
- `FindParent<T>` now supports non-visual elements (e.g., `Run`) without throwing.

- Marked main window as Windows-only to silence cross-platform analyzer warnings.
- Corrected `LogEntry` namespace usage and removed obsolete global log handler to restore build after introducing the shared log view.
- Fixed ServiceLogView namespace references and command delegates so main, HTTP, and TCP views compile consistently.
- Qualified helper and behavior namespaces and dropped installer TextBox hint to resolve remaining XAML build errors.
- Removed assembly qualifiers from internal namespaces and explicitly referenced shared views so converters, behaviors, and shared controls resolve across XAML views.
- Qualified shared log control references and removed redundant assembly-qualified namespaces so converters, behaviors, and editors resolve correctly across views.
- Aligned ServiceLogViewModel with the shared `LogEntry` model and added explicit assembly namespaces so logs and converters compile across service views.
- ServiceLogView.xaml included as a Page and HttpServiceView references it via the project namespace, resolving designer errors.
- Added parameterless constructor resolving dependencies for `HttpServiceView`, enabling the XAML designer to load.
- Removed assembly-qualified namespaces from HTTP and MQTT views so converters and enums resolve during build.
- Updated Roslyn scripting packages to 4.12.0 to resolve dependency conflicts.
- Replaced DocumentLine.BackgroundColor usage with a line transformer and switched async lambdas to AsyncRelayCommand to satisfy threading analyzers.
- Extracted `StringNullOrEmptyToVisibilityConverter` into its own file so XAML views compile independently.
- Qualified helper namespaces with explicit assembly references so `StringNullOrEmptyToVisibilityConverter` resolves across service views.
- EditorButtonBar included as a Page with code-behind dependency and referenced via an assembly-qualified views namespace across consuming views.
- Output message textbox uses a one-way binding to avoid runtime errors on the read-only `OutputMessage` property.
- Script editor window removes `Text` binding from AvalonEdit control to prevent XAML parse exceptions.
- Extracted `ScriptGlobals` into a public top-level class with a `Message` property so scripts can access test messages without protection-level errors.
- Made `ScriptEditorViewModel.Globals` public so scripts can access the `message` field without protection-level errors.
- Script editor dispatches output and error highlights to the UI thread so results appear when Run is clicked.
- Script editor uses a `JoinableTaskFactory` to switch to the UI thread and remove VSTHRD001 analyzer warnings.
- Script editor unsubscribes handlers on close and mirrors test message changes to the TCP messages view.
- Renamed `SaveServices` to `SaveServicesAsync` and updated callers to await it, removing blocking calls.
- Replaced Xceed `ColorCanvas` with `ColorPicker` to prevent XAML parse exceptions when selecting service colors.
- Edit service workflow logs and ignores unrecognized service types.


### HID Service
#### Added
- HID service view includes a data flow diagram showing incoming, processed, and outgoing data bound to the view model.
- HID service creation, edit, and advanced configuration views with navigation tests.

#### Fixed
- HidViewModel now logs formatting errors and resets simulated keyboard state, skipping message forwarding when templates are malformed.
- HidViewModel releases simulated key presses on disposal, and `App.OnExit` invokes this cleanup to restore the keyboard state.

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
- Registered generic service screen in App startup so FTP create view model resolves via DI.

### TCP Service
#### Added
- TCP service creation and message viewer enabling configuration and inspection of endpoint traffic.
- Dedicated TCP edit and advanced configuration views with navigation tests.
- View and view model for displaying TCP service messages with log-level filtering and log management commands.
- Advanced TCP options support script-based message transformation with input and output previews.

#### Changed
- Registered transient TCP view models and bound `TcpServiceOptions` configuration; each `ServiceListModel` retains its own options.
- Service persistence saves and restores `TcpServiceOptions` for TCP services.
- Replaced `TcpServiceView` with `TcpServiceMessagesView` hosting a reusable message table.

#### Fixed
- Replaced `UriParser.GetSyntax` usage with `IsKnownScheme` and guarded TCP option restoration to avoid null references.
- Registered the WPF pack URI scheme so BubblyWindow resources load without invalid URI errors.
- Loading TCP options no longer dereferences a null reference.

### MQTT Service
#### Added
- Wizard-style service creation view capturing broker, credentials, TLS, will message, and connection options.
- MQTT connections support option-based configuration, TLS/WebSocket, QoS, retain flag, keep-alive, clean session, reconnect delay, and structured logging.
- UI for configuring MQTT endpoint–message pairs with placeholders, tooltips, per-tag outgoing test messages, and will-message support.
- Tag subscriptions support per-topic endpoints, QoS selection, subscribe/unsubscribe commands, and visual feedback.
- Dedicated window for editing MQTT connection settings with update, cancel, and unsubscribe commands.
- Tag subscriptions view displays a connection status indicator.
- Tag subscriptions view shows log entries for connection events and errors.
- Tag subscriptions view hosts a collapsible log panel and forwards entries to the main log view.

#### Changed
- `MqttService` refactored with options-based constructor, clean reconnect logic, and consolidated publish methods.
- `MqttServiceViewModel` uses `MqttServiceOptions` for settings and delegates token resolution to `MessageRoutingService`.
- `MessageRoutingService` tracks latest messages per service and resolves `{ServiceName.Message}` tokens before publishing.
- `MqttTagSubscriptionsViewModel` consolidated to a single subscription collection with unified properties.
- `MqttTagSubscriptionsViewModel` passes updated options to `MqttService.ConnectAsync` and logs connection success or failure.
- Topics now appear in the subscription list before broker subscribe and log errors when the call fails; the Add button disables when no topic is provided.
- Removed obsolete MQTT options model and duplicate subscribe implementations.
- MQTT service creation now occurs within the main window frame and returns after completion.
- Expanded connection types to include MQTT/WebSocket variants with optional TLS and updated connection views.
- `MqttService` logs connection state changes and errors through the injected logging service.
- Guarded client certificate loading with Windows checks and platform exceptions.
- Replaced explicit null checks with `ArgumentNullException.ThrowIfNull` in `MqttService`.

#### Fixed
- MQTT service disconnects before reconnecting when settings change and converts blank will-topic/payload fields to `null`.
- QoS enum binding resolved; selecting the MQTT service no longer throws runtime exceptions.
- Removed invalid `MouseDoubleClick` XAML handlers and duplicate styles that caused build failures.
- `MqttCreateServiceView` no longer hosts duplicate `StackPanel` elements, resolving build errors.
- Added missing `MQTTnet.Protocol` using to restore `MqttQualityOfServiceLevel` references.
- Host validation now accepts domain names and rejects underscores.
- Restored `MqttEndpointMessage` namespace in MQTT service view model to fix build errors.
- Logged connection failures through the disconnected handler and removed obsolete `ConnectingFailedAsync` usage.

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
- Guarded `CsvServiceOptions` property access against null references during service persistence.
- Csv service editor view now compiles on all platforms by using a `Page` root instead of inheriting from `ServiceEditorView`.

### HTTP Service
#### Added
- HTTP service creation and edit views with advanced configuration for authentication and TLS certificate paths.

#### Changed
- Exposed `AdvancedConfigCommand` directly in HTTP service creation and edit views, removing the redundant wrapper command.

### File Observer
#### Added
- File search service with async caching and DI integration for File Observer.
- File Observer create, edit, and advanced configuration views with navigation and DI registration.
- Browse button in File Observer create view to select folders via dialog.

### Logging
#### Added
- Logging service loads existing log file on startup and reloads entries when the minimum level changes.
- Core `ILoggingService` interface and `LogLevel` enum shared across projects.
- Information log level inserted between Debug and Warning with a default blue color.

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
- Moved `ILoggingService`, `LogLevel`, and `LogEntry` into the core library so tests no longer depend on the Windows project.
- `LogEntry` now stores colors as hex strings instead of `System.Windows.Media.Brush`.
- Log displays now use a common style and show newest entries first.

### Documentation & CI
#### Added
- Section on working in restricted environments and reminder to log limitations in collaboration docs.
- Documented architecture and coding standards in `AGENTS.md`.
- `CONTRIBUTING.md` and PR template enforcing CI-only testing with a CI badge in the README.
- `/test` comment workflow to run CI on demand.
- `TestCommon` library providing shared test helpers and fixtures referenced by all test projects.
- `TestHelpers.CreateService` simplifies creating `ServiceListModel` instances in tests to reduce duplication.
- Test-only composite service demonstrates reuse of `ServiceRule` and `ServiceScreen` in unit tests.
- Comparison tests ensure composed sample service matches original implementation results and log formatting.
- Collaboration tips note that WPF projects require Windows or the WindowsDesktop runtime and fail with `InitializeComponent` and `NETSDK1100` errors if missing.
- Guide on creating custom services and registering dependencies via `IServiceModule`.
- README now explains `ServiceType`, dictionary-based edit handlers, and dynamic DI modules with a sample for adding a new service.

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
- Removed `DesktopApplicationTemplate.UI.Tests` project and WPF-specific unit tests.
- Clarified DI configuration with comments explaining service module discovery and service-type handler dictionary construction.

#### Fixed
- Added missing `FluentAssertions` package reference to the test project and documented dependency checks to avoid build failures.
- Removed duplicate using directives and missing namespace references that prevented solution builds.
- Guarded WPF test thread apartment configuration with an OS check to avoid CA1416 build errors on non-Windows hosts.
- Added `StubFileDialogService` to test project to support file dialog operations.
- Console test logger writes plain text messages to avoid Visual Studio RPC errors when expanding test results.

