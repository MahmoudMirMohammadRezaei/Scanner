using System.Net;
using NAPS2.Escl.Client;
using NAPS2.Escl.Server;
using Xunit;

namespace NAPS2.Escl.Tests;

public class ClientServerTests
{
    [Fact]
    public async Task ClientServer()
    {
        using var server = new EsclServer(new EsclServerConfig
        {
            Capabilities = new EsclCapabilities
            {
                Version = "2.0",
                MakeAndModel = "HP Blah",
                SerialNumber = "123abc"
            }
        }) { Port = 9801 };
        server.Start();
        var client = new EsclClient(new EsclService
        {
            Ip = IPAddress.IPv6Loopback,
            Port = 9801,
            RootUrl = "escl",
            Tls = false
        });
        var caps = await client.GetCapabilities();
        Assert.Equal("2.0", caps.Version);
        Assert.Equal("HP Blah", caps.MakeAndModel);
        Assert.Equal("123abc", caps.SerialNumber);
    }
}