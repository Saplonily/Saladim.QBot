using System.Net.Http;

namespace SaladimWpf.Services;

public class HttpRequesterService
{
    protected HttpClient httpClient;

    public HttpClient HttpClient => httpClient;

    public HttpRequesterService()
    {
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Saladim.QBot external value fetcher");
    }
}
