
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class AzureServiceBusMessageBus : IMessageBus
    {
        private const string connectionString = "Endpoint=sb://mangorestaurantpapd.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=GvUkzPLg74vZ2aFkHQCqURqQsbGdebax8cKIiEt3Kaw=";
        public async Task PublishMessageAsync(BaseMessage message, string topicName)
        {
            await using var client = new ServiceBusClient(connectionString);

            var sender = client.CreateSender(topicName);

            var jsonMessage = JsonConvert.SerializeObject(message);

            var finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            await sender.SendMessageAsync(finalMessage);
            await client.DisposeAsync();
        }
    }
}
