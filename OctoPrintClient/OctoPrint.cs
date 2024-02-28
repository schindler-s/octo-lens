

public class OctoPrint
{
    private readonly string _octoPrintUrl;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public OctoPrint(string octoPrintUrl, string apiKey)
    {
        _octoPrintUrl = octoPrintUrl;
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_octoPrintUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
    }

    public async Task<string> GetPrinterStatusAsync()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("printer");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "Error: " + response.StatusCode;
            }
        }
        catch (HttpRequestException e)
        {
            return "HTTP Request Exception: " + e.Message;
        }
    }
}