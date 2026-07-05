using DTOs;
using IntegrationEvents;

namespace Interfaces
{
    public interface ICompraService
    {
        Task CriarCompra(CriarCompraDto dto, int usuarioId, string emailUsuario, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<CompraResponseDto>> ObterComprasDoUsuario(int usuarioId);
        Task ProcessarPagamento(PagamentoProcessadoIntegrationEvent evento);
    }
}
