using System.Text.Json.Serialization;

namespace PaymentService.Application.Dtos
{
    public sealed class OrderCreatedIntegrationEvent
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        // note: string, because your producer sends a string
        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }
    }
}
