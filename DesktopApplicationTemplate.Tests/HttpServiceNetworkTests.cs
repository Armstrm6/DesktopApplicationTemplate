using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Tests;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        var respondTask = Task.Run(async () =>
        {
            var ctx = await listener.GetContextAsync();
            var buffer = Encoding.UTF8.GetBytes("ok");
            ctx.Response.ContentLength64 = buffer.Length;
            await ctx.Response.OutputStream.WriteAsync(buffer);
            ctx.Response.OutputStream.Close();
            listener.Stop();
        });

        var vm = new HttpServiceViewModel { Url = $"http://localhost:{port}/" };
        await vm.SendRequestAsync();

        await respondTask;

        Assert.Equal(200, vm.StatusCode);
        Assert.Equal("ok", vm.ResponseBody);

        ConsoleTestLogger.LogPass();
    }
}
