using System;
using System.Text.Json;
using System.Xml.Linq;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;


namespace TestQueueNew
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            // Your real Azure Storage connection string
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=adeolastore;AccountKey=9kRIriHodafBJe+0wjhTUmpVZwONaFgVr3pnTaGbR/KJYVxoXZSBUuQ9s0a6mvk64o/TXHWgJUVH+AStblsg8g==;EndpointSuffix=core.windows.net";
            // Queue name must match the one your function listens to
            var queueClient = new QueueClient(
                connectionString,
                "retailque",
                new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 } //ensure its base64
            );
            // Create queue if it doesn't exist
            await queueClient.CreateIfNotExistsAsync();
            // Build test object

            var person = new { Name = "Wandile Smith", Email = "wandile@iiemsa.com" };
            // Serialize object to JSON
            string json = JsonSerializer.Serialize(person);
            // Send as plain JSON string
            await queueClient.SendMessageAsync(json);
            Console.WriteLine($"Message sent: {json}");
        }
    }
}
