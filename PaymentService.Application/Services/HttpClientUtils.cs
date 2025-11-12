using System.Net.Http.Json;
using System.Text.Json;

namespace PaymentService.Application.Services;

public class HttpClientUtils : IHttpClientUtils
{
    private readonly HttpClient _httpClient;
    
    public HttpClientUtils(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task SendHttpRequest(string url, object payload, HttpMethod requestMethod)
    {
        var request = new HttpRequestMessage(requestMethod, url)
        {
            Content = JsonContent.Create(payload) 
        };

        var response = await _httpClient.SendAsync(request);
        
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"Request failed URL: {url}, Payload: {JsonSerializer.Serialize(response)}");
        }
    }
}