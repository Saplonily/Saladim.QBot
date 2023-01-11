namespace Saladim.Offbot.Services;

public class HttpRequesterService
{
    protected HttpClient httpClient;

    public HttpClient HttpClient => httpClient;

    public HttpRequesterService()
    {
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Saladim.Offbot external value fetcher");
    }
}
