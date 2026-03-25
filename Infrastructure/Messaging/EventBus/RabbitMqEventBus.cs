using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class RabbitMqEventBus : IEventBus
{
    private IConnection _connection;
    private IChannel _channel;
    private readonly IConfiguration _configuration;

    public RabbitMqEventBus(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:HostName"],
            UserName = _configuration["RabbitMq:UserName"],
            Password = _configuration["RabbitMq:Password"],
            Port = int.Parse(_configuration["RabbitMq:Port"])
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task Publish<T>(string queue, T message)
    {
        await _channel.QueueDeclareAsync(queue, false, false, false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: queue,
            body: body);
    }

    public async void Subscribe<T>(string queue, Func<T, Task> handler)
    {
        await _channel.QueueDeclareAsync(queue, false, false, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var message = JsonSerializer.Deserialize<T>(json);

            await handler(message);
        };

        await _channel.BasicConsumeAsync(queue, true, consumer);
    }
}