using IntegrationEvents;

namespace Interfaces;

public interface ICompraEventPublisher
{
    Task PublicarCompraSolicitadaAsync(CompraSolicitadaIntegrationEvent evento, CancellationToken cancellationToken = default);
}
