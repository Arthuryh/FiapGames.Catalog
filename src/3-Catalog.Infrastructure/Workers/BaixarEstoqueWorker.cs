using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using IntegrationEvents;
using Interfaces;

namespace Workers;

public class BaixarEstoqueWorker : BackgroundService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<BaixarEstoqueWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private IConnection? _connection;
    private IChannel? _channel;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string ExchangeName = "pagamento.exchange";
    private const string PagamentoAprovadoRoutingKey = "pagamento.aprovado";
    private const string PagamentoRecusadoRoutingKey = "pagamento.recusado";

    private const string QueueName = "catalogo.pagamento.processado";

    private const string DlxExchangeName = "pagamento.dlx.exchange";
    private const string DlqQueueName = "catalogo.pagamento.processado.dlq";
    private const string DlxRoutingKey = "pagamento.falha";

    public BaixarEstoqueWorker(
        IConnectionFactory connectionFactory,
        ILogger<BaixarEstoqueWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await ConfigurarTopologiaAsync();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        _logger.LogInformation("Worker do Catalog iniciado e aguardando resultado de pagamentos...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ConfigurarTopologiaAsync()
    {
        await _channel!.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, true, false);
        await _channel.ExchangeDeclareAsync(DlxExchangeName, ExchangeType.Direct, true, false);

        await _channel.QueueDeclareAsync(DlqQueueName, true, false, false, null);
        await _channel.QueueBindAsync(DlqQueueName, DlxExchangeName, DlxRoutingKey);

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", DlxExchangeName },
            { "x-dead-letter-routing-key", DlxRoutingKey }
        };
        await _channel.QueueDeclareAsync(QueueName, true, false, false, mainQueueArgs);

        await _channel.QueueBindAsync(QueueName, ExchangeName, PagamentoAprovadoRoutingKey);
        await _channel.QueueBindAsync(QueueName, ExchangeName, PagamentoRecusadoRoutingKey);
        await _channel.BasicQosAsync(0, 1, false);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var rastreioId = ea.BasicProperties.CorrelationId ?? "SEM-RASTREIO";

        using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["RastreioId"] = rastreioId });

        try
        {
            var body = ea.Body.ToArray();
            var json = System.Text.Encoding.UTF8.GetString(body);
            var evento = JsonSerializer.Deserialize<PagamentoProcessadoIntegrationEvent>(json, JsonOptions);

            if (evento is null) throw new JsonException("Evento nulo.");

            _logger.LogInformation(
                "Pagamento processado para a compra {CompraId}. Status: {Status}. Aprovado: {Aprovado}.",
                evento.CompraId,
                evento.Status,
                evento.Aprovado);

            using var scope = _scopeFactory.CreateScope();
            var compraService = scope.ServiceProvider.GetRequiredService<ICompraService>();
            await compraService.ProcessarPagamento(evento);

            if (evento.Aprovado)
            {
                _logger.LogInformation(
                    "Compra {CompraId} aprovada para o usuario {UsuarioId}. Biblioteca liberada.",
                    evento.CompraId,
                    evento.UsuarioId);
            }
            else
            {
                _logger.LogWarning(
                    "Compra {CompraId} recusada para o usuario {UsuarioId}. Motivo: {MotivoRecusa}",
                    evento.CompraId,
                    evento.UsuarioId,
                    evento.MotivoRecusa ?? "Nao informado");
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
            _logger.LogInformation("Resultado do pagamento da compra {CompraId} consumido com sucesso.", evento.CompraId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro crítico ao baixar estoque. Movendo para DLQ.");
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    public override async void Dispose()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        base.Dispose();
    }
}
