namespace SaladimQBot.GoCqHttp;

public interface IHttpRequestService
{
    Task<string> GetAsync(string url);
}
