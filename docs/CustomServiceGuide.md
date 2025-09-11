# Custom Service Guide

This guide explains how to extend the application with new services by reusing existing components and `IServiceModule`.

## Reuse shared components

- **ServiceRule**: central validation helper injected into create and edit view models for required field checks.
- **ServiceScreen**: wraps user interactions for options models and exposes events that views can bind to.
- **Base view models**: inherit from `ServiceCreateViewModelBase` and `ServiceEditViewModelBase` to get common save logic, logging, and navigation.
- **Shared controls**: components such as `ServiceLogView` and `ServiceMessageTableView` provide consistent UI panels that can be embedded in new service pages.

## Register dependencies with `IServiceModule`

`IServiceModule` allows each service to package its DI registrations. Implement the interface for your service and expose the corresponding `ServiceType`:

```csharp
public class SampleServiceModule : IServiceModule
{
    public ServiceType Type => ServiceType.Sample;

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<SampleService>();
        services.AddTransient<SampleCreateViewModel>();
        services.AddTransient<SampleEditViewModel>();
        services.AddTransient<SamplePage>();
    }
}
```

Modules are discovered at startup by calling `services.AddServiceModules()` during host configuration.

## Adding a new service type

1. **Update the enum** – add a value to `ServiceType` to identify the service.
2. **Create options and implementation** – define an options class and the runtime service that executes the work.
3. **Build the UI** – create view models and views, reusing `ServiceRule`, `ServiceScreen`, and base classes for common behavior.
4. **Implement an `IServiceModule`** – register the service, view models, and views with the DI container.
5. **Wire up navigation** – add create and edit handlers or factory entries so the main window can navigate to the new views.
6. **Test and document** – run `dotnet` commands and update docs as needed.
