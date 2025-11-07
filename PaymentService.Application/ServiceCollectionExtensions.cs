using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PaymentService.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPaymentApplication(this IServiceCollection services)
        {
            services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
            return services;
        }
    }
}
