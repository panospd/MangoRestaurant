using Azure.Messaging.ServiceBus;
using Mango.Services.OrderAPI.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Mango.Services.Email.Messages;

namespace Mango.Services.Email.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly EmailRepository _emailRepository;
        private readonly ServiceBusProcessor _orderUpdatePaymentStatusResultProcessor;

        public AzureServiceBusConsumer(EmailRepository emailRepository, IConfiguration configuration)
        {
            _emailRepository = emailRepository;
            var serviceBusConnectionString = configuration.GetValue<string>("ServiceBusConnectionString");
            var subscriptionEmail = configuration.GetValue<string>("SubscriptionName");
            var orderUpdatePaymentResultTopic = configuration.GetValue<string>("OrderUpdatePaymentResultTopic");

            var client = new ServiceBusClient(serviceBusConnectionString);
            _orderUpdatePaymentStatusResultProcessor = client.CreateProcessor(orderUpdatePaymentResultTopic, subscriptionEmail);
        }

        public async Task Start()
        {
            _orderUpdatePaymentStatusResultProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            _orderUpdatePaymentStatusResultProcessor.ProcessErrorAsync += ErrorHandler;
            await _orderUpdatePaymentStatusResultProcessor.StartProcessingAsync();
        }

        private async Task OnOrderPaymentUpdateReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            var paymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);

            await _emailRepository.SendAndLogEmail(paymentResultMessage);
        }

        public async Task Stop()
        {
            await _orderUpdatePaymentStatusResultProcessor.StopProcessingAsync();
            await _orderUpdatePaymentStatusResultProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
