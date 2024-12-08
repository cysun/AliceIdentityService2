using MessagePack;
using Microsoft.Extensions.Options;
using MimeKit;
using RabbitMQ.Client;

namespace AliceIdentityService.Services;

public class RabbitMQSettings
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = ConnectionFactory.DefaultUser;
    public string Password { get; set; } = ConnectionFactory.DefaultPass;
    public string QueueName { get; set; } = "alice-mail-service";
}

public class RabbitService : IDisposable
{
    private readonly RabbitMQSettings _settings;

    private readonly ConnectionFactory _factory;

    // Since RabbitMQ .NET Client 7, all operations are async, including connection creation.
    // Because we want to use one connection for the lifetime of the service for performance reasons,
    // and because constructors cannot be async, we follow the example at
    // https://stackoverflow.com/questions/43240405/async-iserviceprovider-in-net-core-di and use
    // a Lazy<Task<IConnection>>. As explained by https://learn.microsoft.com/en-us/dotnet/api/system.lazy-1?view=net-8.0,
    // Lazy<T> allows us to defer the creation of the connection until it is first used, and it is thread-safe.
    private readonly Lazy<Task<IConnection>> _connection;

    private readonly ILogger<RabbitService> _logger;

    public RabbitService(IOptions<RabbitMQSettings> settings, ILogger<RabbitService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password
        };
        _connection = new Lazy<Task<IConnection>>(async () =>
        {
            var connection = await _factory.CreateConnectionAsync();
            _logger.LogInformation("Connected to RabbitMQ server at {host}", _settings.HostName);
            return connection;
        });
    }

    public async Task SendAsync(MimeMessage msg)
    {
        var connection = await _connection.Value;
        await using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Alice Mail Service expects a list of byte arrays, each being an MimeMessage
        List<byte[]> messages = new List<byte[]>();
        using (MemoryStream stream = new MemoryStream())
        {
            msg.WriteTo(stream);
            messages.Add(stream.ToArray());
        }
        var body = MessagePackSerializer.Serialize(messages);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _settings.QueueName,
            body: body
        );
        _logger.LogInformation("Publish {n} message to RabbitMQ queue {queue}", messages.Count, _settings.QueueName);
    }

    public void Dispose()
    {
        if (_connection.IsValueCreated)
            _connection.Value.Result.Dispose();
    }
}
