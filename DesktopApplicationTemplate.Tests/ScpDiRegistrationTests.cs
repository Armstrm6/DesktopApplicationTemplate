using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Common;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Create;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Advanced;
using DesktopApplicationTemplate.UI.Views.Scp.Create;
using DesktopApplicationTemplate.UI.Views.Scp.Edit;
using DesktopApplicationTemplate.UI.Views.Scp.Advanced;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ScpDiRegistrationTests
{
    [Fact]
    public void ServiceProvider_Resolves_ScpTypes()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
        services.AddSingleton<ILoggingService, LoggingService>();
        // Register service rule for SCP validation.
        services.AddCommonServices();
        services.AddSingleton<SaveConfirmationHelper>();
        services.AddTransient<ScpCreateServiceViewModel>();
        services.AddTransient<ScpCreateServiceView>();
        services.AddTransient<ScpEditServiceViewModel>();
        services.AddTransient<ScpEditServiceView>();
        services.AddTransient<ScpAdvancedConfigViewModel>();
        services.AddTransient<ScpAdvancedConfigView>();
        services.AddOptions<ScpServiceOptions>();

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<ScpCreateServiceViewModel>());
        var editVm = provider.GetRequiredService<ScpEditServiceViewModel>();
        editVm.Load("svc", new ScpServiceOptions());
        Assert.NotNull(editVm);
        Assert.NotNull(provider.GetRequiredService<ScpAdvancedConfigViewModel>());
    }
}
