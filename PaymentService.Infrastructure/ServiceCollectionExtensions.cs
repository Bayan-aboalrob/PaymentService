using FlashSaleDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Contracts;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Infrastructure.Persistence.Repositories;
using PaymentService.Infrastructure.Persistence.Services;

namespace PaymentService.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPaymentInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connStr = configuration.GetConnectionString("FlashSaleDb");

            services.AddDbContext<FlashSaleDbContext>(options =>
            {
                options.UseSqlServer(connStr);
            });

            services.AddScoped<IPaymentRepository, PaymentRepository>();

            services.AddScoped<IPaymentProcessor, FakePaymentProcessor>();
            
            services.AddScoped<IHttpClientUtils, HttpClientUtils>();

            services.AddSingleton<IBusPublisher, RabbitMqPublisher>();

         //   services.AddHostedService<OrderCreatedSingleConsumer>();

            return services;
        }
    }
}
