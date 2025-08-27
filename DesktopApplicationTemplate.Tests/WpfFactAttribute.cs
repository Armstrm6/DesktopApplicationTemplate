using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DesktopApplicationTemplate.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("DesktopApplicationTemplate.Tests.WpfFactDiscoverer", "DesktopApplicationTemplate.Tests")]
public sealed class WpfFactAttribute : FactAttribute { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("DesktopApplicationTemplate.Tests.WpfTheoryDiscoverer", "DesktopApplicationTemplate.Tests")]
public sealed class WpfTheoryAttribute : TheoryAttribute { }

internal sealed class WpfFactDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public WpfFactDiscoverer(IMessageSink diagnosticMessageSink) => _diagnosticMessageSink = diagnosticMessageSink;

    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        yield return new WpfTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);
    }
}

internal sealed class WpfTheoryDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public WpfTheoryDiscoverer(IMessageSink diagnosticMessageSink) => _diagnosticMessageSink = diagnosticMessageSink;

    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        yield return new WpfTheoryTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);
    }
}

internal class WpfTestCase : XunitTestCase
{
    [Obsolete("Called by the test runner", true)]
    public WpfTestCase() { }

    public WpfTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod)
        : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod) { }

    public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        var tcs = new TaskCompletionSource<RunSummary>();
        var thread = new Thread(() =>
        {
            var result = base.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource)
                .GetAwaiter().GetResult();
            tcs.SetResult(result);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return tcs.Task;
    }
}

internal class WpfTheoryTestCase : XunitTheoryTestCase
{
    [Obsolete("Called by the test runner", true)]
    public WpfTheoryTestCase() { }

    public WpfTheoryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod)
        : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod) { }

    public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        var tcs = new TaskCompletionSource<RunSummary>();
        var thread = new Thread(() =>
        {
            var result = base.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource)
                .GetAwaiter().GetResult();
            tcs.SetResult(result);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return tcs.Task;
    }
}
