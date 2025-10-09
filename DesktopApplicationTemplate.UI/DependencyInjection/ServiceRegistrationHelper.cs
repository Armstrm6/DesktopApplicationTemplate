using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    internal static class ServiceRegistrationHelper
    {
        internal static IServiceDescriptor GetDescriptorOrThrow(IServiceCatalog catalog, string descriptorId)
        {
            if (catalog is null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (descriptorId is null)
            {
                throw new ArgumentNullException(nameof(descriptorId));
            }

            if (catalog.TryGetById(descriptorId, out var descriptor))
            {
                return descriptor;
            }

            throw new InvalidOperationException($"Descriptor '{descriptorId}' is not registered.");
        }

        internal static void QueueServiceAddition<TOptions>(MainView mainView, ServiceType serviceType, string serviceName, TOptions options)
        {
            async Task ExecuteAsync()
            {
                await mainView.TryAddServiceAsync(serviceType, new ServiceFactoryOptions<TOptions>(serviceName, options));
            }

            var factory = App.UiThreadTaskFactory;
            if (factory is null)
            {
                ObserveTaskFailure(ExecuteAsync(), serviceType, serviceName);
                return;
            }

            var joinableTask = factory.RunAsync(async () =>
            {
                await factory.SwitchToMainThreadAsync();
                await ExecuteAsync();
            });

            ObserveTaskFailure(joinableTask.Task, serviceType, serviceName);
        }

        private static void ObserveTaskFailure(Task task, ServiceType serviceType, string serviceName)
        {
            _ = task.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception is { } exception)
                {
                    var logger = App.AppHost.Services.GetService<ILogger<App>>();
                    logger?.LogError(exception, "Failed to add {ServiceType} service {ServiceName}", serviceType, serviceName);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }
}
