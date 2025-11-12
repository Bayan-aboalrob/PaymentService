namespace PaymentService.Application.Services;

public interface IHttpClientUtils
{
    Task SendHttpRequest(string url, object payload, HttpMethod requestMethod);
}