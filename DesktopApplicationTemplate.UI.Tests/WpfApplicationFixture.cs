using System;
using System.Windows;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public sealed class WpfApplicationFixture : IDisposable
{
    public WpfApplicationFixture()
    {
        if (Application.Current is null)
        {
            _ = new DesktopApplicationTemplate.UI.App();
        }

        ApplicationResourceHelper.EnsureApplication();
    }

    public void Dispose()
    {
    }
}

[CollectionDefinition("WpfTests", DisableParallelization = true)]
public sealed class WpfTestsCollection : ICollectionFixture<WpfApplicationFixture>
{
}
