using DTOs;
using Entities;

namespace Interfaces
{
    public interface IJogoSearchService
    {
        Task IndexarAsync(Jogo jogo, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JogoResponseDto>> BuscarAsync(
            string termo,
            int limite,
            CancellationToken cancellationToken = default);
    }
}