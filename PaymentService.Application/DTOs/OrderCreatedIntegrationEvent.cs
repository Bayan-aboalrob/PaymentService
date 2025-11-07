namespace PaymentService.Application.Dtos
{
    public sealed class OrderCreatedIntegrationEvent
    {
        public Guid Id { get; set; }          
        public Guid UserId { get; set; }        
        public Guid ProductId { get; set; }     
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public Guid? CorrelationId { get; set; }
    }
}
