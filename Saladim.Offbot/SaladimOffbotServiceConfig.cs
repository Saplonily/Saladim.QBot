namespace Saladim.Offbot.Services;

public class SaladimOffbotServiceConfig
{
    public string GoCqHttpWebSocketAddress { get; set; }

    public SaladimOffbotServiceConfig(string goCqHttpWebSocketAddress)
    {
        this.GoCqHttpWebSocketAddress = goCqHttpWebSocketAddress;
    }
}
