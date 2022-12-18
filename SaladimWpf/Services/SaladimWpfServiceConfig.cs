namespace SaladimWpf.Services;

public class SaladimWpfServiceConfig
{
    public string GoCqHttpWebSocketAddress { get; set; }

    public SaladimWpfServiceConfig(string goCqHttpWebSocketAddress)
    {
        this.GoCqHttpWebSocketAddress = goCqHttpWebSocketAddress;
    }
}
