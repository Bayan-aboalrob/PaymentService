using PaymentService.Application;
using PaymentService.Infrastructure;
using PaymentService.Infrastructure.Persistence.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPaymentApplication();
builder.Services.AddPaymentInfrastructure(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services.AddHostedService<InfluxMetricsCollector>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
