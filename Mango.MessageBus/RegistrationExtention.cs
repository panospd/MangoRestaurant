using Microsoft.Extensions.DependencyInjection;

namespace Mango.MessageBus
{
    public static class RegistrationExtention
    {
        public static void AddMessageServices(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBus, AzureServiceBusMessageBus>();
        }
    }
}
