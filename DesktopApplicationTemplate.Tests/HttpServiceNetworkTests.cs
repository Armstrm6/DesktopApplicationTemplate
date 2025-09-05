using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Tests;
using DesktopApplicationTemplate.Core.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Moq;
using Moq.Protected;
using System.Linq;

namespace DesktopApplicationTemplate.Tests;

public class HttpServiceNetworkTests
{
    [Fact]
    public async Task SendRequest_ReceivesLocalResponse()
    {
        var portPicker = new TcpListener(IPAddress.Loopback, 0);
        portPicker.Start();
        int port = ((IPEndPoint)portPicker.LocalEndpoint).Port;
        portPicker.Stop();

        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        var listenerTask = Task.Run(async () =>
        {
            var ctx = await listener.GetContextAsync();
            var buffer = Encoding.UTF8.GetBytes("ok");
            ctx.Response.ContentLength64 = buffer.Length;
            await ctx.Response.OutputStream.WriteAsync(buffer);
            ctx.Response.OutputStream.Close();
            listener.Stop();
        });

        var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
        var vm = new HttpServiceViewModel(helper) { Url = $"http://localhost:{port}/" };
        await vm.SendRequestAsync();

        await listenerTask;

        Assert.Equal(200, vm.StatusCode);
        Assert.Equal("ok", vm.ResponseBody);

        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public async Task SendRequest_SendsCorrectData()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        HttpRequestMessage? captured = null;
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            });

        var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
        var vm = new HttpServiceViewModel(helper)
        {
            Url = "http://localhost/",
            SelectedMethod = "POST",
            RequestBody = "data",
            MessageHandler = handlerMock.Object,
            Headers = { new HttpServiceViewModel.HeaderItem { Key = "X-Test", Value = "1" } }
        };

        await vm.SendRequestAsync();

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("1", captured.Headers.GetValues("X-Test").Single());
        Assert.Equal("data", await captured.Content!.ReadAsStringAsync());

        ConsoleTestLogger.LogPass();
    }
}
