using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private string connection = "Endpoint=sb://mangoweb-vg.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=cXL5GySPL/HQbV0GbDaO32mDN0hR3t49j+ASbFlvZs4=";
        public async Task PublishMessage(object message, string topicQueueName)
        {
            await using var client = new ServiceBusClient(connection);

            ServiceBusSender sender = client.CreateSender(topicQueueName);

            var jsonMessage = JsonConvert.SerializeObject(message);

            ServiceBusMessage finalmessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString(),
            };

            await sender.SendMessageAsync(finalmessage);
            await client.DisposeAsync();
        }
    }
}
