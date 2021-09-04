using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Mango.MessageBus;
using PaymentProcessor;
using Mango.Services.PaymentAPI.Messages;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly IMessageBus _messageBus;
        private readonly ServiceBusProcessor _orderPaymentProcessor;
        private readonly IProcessPayment _processPayment;
        private readonly string _orderPaymentProcessTopic;
        private readonly string _orderUpdatePaymentResultTopic;

        public AzureServiceBusConsumer(IConfiguration configuration, IMessageBus messageBus, IProcessPayment processPayment)
        {
            _messageBus = messageBus;
            _processPayment = processPayment;
            var serviceBusConnectionString = configuration.GetValue<string>("ServiceBusConnectionString");
            var subscriptionPayment = configuration.GetValue<string>("OrderPaymentProcessSubscription");
            _orderPaymentProcessTopic = configuration.GetValue<string>("OrderPaymentProcessTopic");
            _orderUpdatePaymentResultTopic = configuration.GetValue<string>("OrderUpdatePaymentResultTopic");

            var client = new ServiceBusClient(serviceBusConnectionString);
            _orderPaymentProcessor = client.CreateProcessor(_orderPaymentProcessTopic, subscriptionPayment);
        }

        public async Task Start()
        {
            _orderPaymentProcessor.ProcessMessageAsync += ProcessPayments;
            _orderPaymentProcessor.ProcessErrorAsync += ErrorHandler;

            await _orderPaymentProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _orderPaymentProcessor.StopProcessingAsync();
            await _orderPaymentProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task ProcessPayments(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            var paymentRequest = JsonConvert.DeserializeObject<PaymentRequestMessage>(body);

            var result = _processPayment.PaymentProcessor();

            var updatePaymentResultMessage = new UpdatePaymentResultMessage
            {
                OrderId = paymentRequest.OrderId,
                Status = result
            };

            await _messageBus.PublishMessageAsync(updatePaymentResultMessage, _orderUpdatePaymentResultTopic);
        }
    }
}
