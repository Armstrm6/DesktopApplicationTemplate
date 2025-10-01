# Plug-in Verification Checklist

Use this checklist to validate descriptor-based plug-ins before distributing them. Complete every numbered action item during QA and capture results in your test notes.

## Import and catalog

1. Import the `.peakiot` package (or a legacy `.ccp`/`.chapp` archive if required) through the plug-in import workflow and confirm the archive extracts without errors.
2. Inspect the log output to verify each descriptor id, version, and assembly was registered with `IServiceCatalog`.
3. Restart the application (or trigger catalog refresh) to ensure descriptors survive reload and appear in the service creation list.

## Descriptor rendering

1. Confirm the service list displays the descriptor label, category, icon glyph, and accent colors supplied by the descriptor metadata.
2. Open the create and edit views to validate descriptor descriptions, payload tooltips, and any payload summaries exposed via `DescribePayload`.
3. Trigger UI elements tagged with `ServiceDescriptorRegistrationAttribute` (navigation handlers, command bars, log panels) to ensure they resolve via the descriptor id.

## Persistence and migration

1. Create a service instance, save it, and verify the persisted record stores the descriptor id alongside any legacy type metadata.
2. Reopen the service to confirm options deserialize via the descriptor's `IServiceOptionsSerializer` implementation.
3. If migrating from an enum-based service, load an older configuration and ensure `ServicePersistence` upgrades it to the descriptor id without data loss.

## Runtime behavior

1. Execute runtime operations (start, stop, send test message, etc.) and observe logs for descriptor-scoped entries and errors.
2. Dispose or uninstall the plug-in to confirm descriptors are removed from the catalog and dependent services fail gracefully or prompt for migration.

## CI coverage

1. Ensure `dotnet build` succeeds for the plug-in project with `ServicePlugin.Packaging` imports enabled, producing archives under `bin/<configuration>/<tfm>/plugins`.
2. Review GitHub Actions output for `DesktopApplicationTemplate.Core.Tests` descriptor catalog tests and add new cases if descriptors expose additional metadata.
3. Document all findings (pass/fail and log links) in `Codex/docs/CollaborationAndDebugTips.txt` or the release tracking system.
