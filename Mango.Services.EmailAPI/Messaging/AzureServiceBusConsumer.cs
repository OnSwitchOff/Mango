using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string emailCartQueue;
        private readonly string registerUserQueue;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        private ServiceBusProcessor _emailCartProcessor;
        private ServiceBusProcessor _registerUserProcessor;


        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _emailService = emailService;
            _configuration = configuration;
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
            registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");

            var client = new ServiceBusClient(serviceBusConnectionString);
            _emailCartProcessor = client.CreateProcessor(emailCartQueue);
            _registerUserProcessor = client.CreateProcessor(registerUserQueue);

        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += _emailCartProcessor_ProcessMessageAsync;
            _emailCartProcessor.ProcessErrorAsync += _emailCartProcessor_ProcessErrorAsync;
            _registerUserProcessor.ProcessErrorAsync += _registerUserProcessor_ProcessErrorAsync;
            _registerUserProcessor.ProcessMessageAsync += _registerUserProcessor_ProcessMessageAsync;
            await _emailCartProcessor.StartProcessingAsync();
            await _registerUserProcessor.StartProcessingAsync();
        }

        private async Task _registerUserProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var message = arg.Message;
            var email = Encoding.UTF8.GetString(message.Body);


            try
            {
                await _emailService.RegisterUserEmailAndLog(email);
                await arg.CompleteMessageAsync(arg.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private Task _registerUserProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();
            await _registerUserProcessor.StopProcessingAsync();
            await _registerUserProcessor.DisposeAsync();
        }

        private Task _emailCartProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }
        private async Task _emailCartProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            CartDto objMessage = JsonConvert.DeserializeObject<CartDto>(body);

            try
            {
                await _emailService.EmailCartAndLog(objMessage);
                await arg.CompleteMessageAsync(arg.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }




    }
}
