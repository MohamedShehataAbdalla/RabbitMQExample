using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "RabbitMQ Example";
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Send message (Producer)");
            Console.WriteLine("2. Receive message (Consumer)");
            Console.Write("Your choice: ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                RunProducer();
            }
            else if (choice == "2")
            {
                RunConsumer();
            }
            else
            {
                Console.WriteLine("Invalid choice!");
            }
        }

        static void RunProducer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declare the exchange and queue
            const string exchangeName = "emailExchange";
            const string queueName = "emailQueue";
            const string routingKey = "emailKey";

            channel.ExchangeDeclare(exchange: exchangeName, type: "direct");
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

            Console.WriteLine("Enter your message:");
            var message = Console.ReadLine();
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: body);
            Console.WriteLine($"Message sent: {message}");
        }

        static void RunConsumer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            const string queueName = "emailQueue";

            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received message: {message}");
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Waiting for messages. Press [Enter] to exit.");
            Console.ReadLine();
        }
    }
}
