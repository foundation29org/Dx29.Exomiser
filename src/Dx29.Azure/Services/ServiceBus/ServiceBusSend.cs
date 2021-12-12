using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

namespace Dx29.Services
{
    public class ServiceBusSend
    {
        public ServiceBusSend(string connectionString, string entityPath)
        {
            Queue = new QueueClient(connectionString, entityPath);
        }

        public QueueClient Queue { get; }

        public async Task SendMessageAsync(object message, int delayMinutes = -1)
        {
            string json = message.Serialize();
            await SendMessageAsync(json, delayMinutes);
        }
        public async Task SendMessageAsync(string body, int delayMinutes = -1)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            var message = new Message(bytes);
            if (delayMinutes > 0)
            {
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMinutes(delayMinutes);
            }
            await Queue.SendAsync(message);
        }

        public async Task CloseAsync()
        {
            await Queue.CloseAsync();
        }
    }
}
