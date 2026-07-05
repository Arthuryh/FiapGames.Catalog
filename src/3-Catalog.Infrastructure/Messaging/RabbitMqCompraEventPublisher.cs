using System.Text;
using System.Text.Json;
using IntegrationEvents;
using Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Messaging;

public sealed class RabbitMqCompraEventPublisher : ICompraEventPublisher, IAsyncDisposable
{
    private const string ExchangeName = "catalogo.exchange";
    private const string QueueName = "pagamento.compra.solicitada";
    private const string RoutingKey = "catalogo.compra.solicitada";

    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqCompraEventPublisher> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;
    private bool _topologiaConfigurada;

    public RabbitMqCompraEventPublisher(
        IConnectionFactory connectionFactory,
        ILogger<RabbitMqCompraEventPublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task PublicarCompraSolicitadaAsync(
        CompraSolicitadaIntegrationEvent evento,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var channel = await ObterCanalAsync(cancellationToken);
            await ConfigurarTopologiaAsync(channel, cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evento));
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = evento.CompraId.ToString(),
                CorrelationId = evento.RastreioId,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Compra {CompraId} publicada para pagamento com rastreio {RastreioId}.",
                evento.CompraId,
                evento.RastreioId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<IChannel> ObterCanalAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
            return _channel;

        _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        return _channel;
    }

    private async Task ConfigurarTopologiaAsync(IChannel channel, CancellationToken cancellationToken)
    {
        if (_topologiaConfigurada)
            return;

        await channel.ExchangeDeclareAsync(
            ExchangeName,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            QueueName,
            ExchangeName,
            RoutingKey,
            cancellationToken: cancellationToken);

        _topologiaConfigurada = true;
    }

    public async ValueTask DisposeAsync()
    {
        _semaphore.Dispose();

        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
