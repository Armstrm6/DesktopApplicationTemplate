# Plug-in Samples

The `Samples` folder contains plug-in projects that exercise the `ServicePlugin.Packaging` MSBuild integration:

1. **TcpRelayPlugin** packages a TCP descriptor and runtime factory that log lifecycle events.
2. **HttpRelayPlugin** demonstrates the same workflow for an HTTP descriptor.

Each project imports `ServicePlugin.Packaging` so building them produces `.peakiot` archives (and legacy `.ccp`/`.chapp` packages for backward compatibility) under `bin/<configuration>/<tfm>/plugins`. Use these samples to validate packaging changes locally or in CI pipelines by adding `dotnet build Samples/TcpRelayPlugin` and `dotnet build Samples/HttpRelayPlugin` to the workflow.
