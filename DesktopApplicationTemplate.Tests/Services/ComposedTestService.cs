using System;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Tests.Services;

/// <summary>
/// Demonstrates composing existing validation and screen services.
/// </summary>
public class ComposedTestService
{
    private readonly IServiceRule _rule;
    private readonly IServiceScreen<TestServiceOptions> _screen;

    /// <summary>
    /// Gets the last validation error, if any.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComposedTestService"/> class.
    /// </summary>
    public ComposedTestService(IServiceRule rule, IServiceScreen<TestServiceOptions> screen)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
    }

    /// <summary>
    /// Validates and saves the provided options via the composed screen service.
    /// </summary>
    /// <param name="name">Service name to validate.</param>
    /// <param name="options">Options to save.</param>
    /// <returns><c>true</c> when valid; otherwise, <c>false</c>.</returns>
    public bool Save(string? name, TestServiceOptions options)
    {
        LastError = _rule.ValidateRequired(name, "Name") ?? _rule.ValidatePort(options.Port);
        if (LastError != null)
        {
            return false;
        }

        _screen.Save(name!, options);
        return true;
    }
}

/// <summary>
/// Options consumed by <see cref="ComposedTestService"/>.
/// </summary>
/// <param name="Port">Port to validate.</param>
public record TestServiceOptions(int Port);

