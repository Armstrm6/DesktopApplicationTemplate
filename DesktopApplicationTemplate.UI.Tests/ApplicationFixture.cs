using System;
using System.Windows;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public sealed class ApplicationFixture : IDisposable
{
    public ApplicationFixture()
    {
        if (Application.Current is null)
        {
            _ = new Application();
        }

        ApplicationResourceHelper.EnsureApplication();
    }

    public void Dispose()
    {
    }
}

[CollectionDefinition("Application", DisableParallelization = true)]
public sealed class ApplicationCollection : ICollectionFixture<ApplicationFixture>
{
}
