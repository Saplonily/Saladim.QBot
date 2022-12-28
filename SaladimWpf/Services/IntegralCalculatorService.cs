using System.Net.Http;

namespace SaladimWpf.Services;

public class IntegralCalculatorService
{
    private readonly HttpRequesterService httpRequesterService;

    public IntegralCalculatorService(HttpRequesterService httpRequesterService)
    {
        this.httpRequesterService = httpRequesterService;
    }

    public async Task<string?> IntegralOf(string s)
    {
        HttpContent httpContent = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            ["function"] = s,
            ["var"] = "x"
        });
        var response = await httpRequesterService.HttpClient.PostAsync(
            "https://zh.numberempire.com/integralcalculator.php",
            httpContent
            ).ConfigureAwait(false);
        var yourString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var st = "<span id=result1>";
        var ed = "</span>&nbsp;<a class='noprint copy'";
        var stLoc = yourString.IndexOf(st) + st.Length;
        var edLoc = yourString.IndexOf(ed);
        if (stLoc <= 0 || edLoc <= 0)
        {
            return null;
        }
        var result = yourString[stLoc..edLoc];
        return result;
    }
}
