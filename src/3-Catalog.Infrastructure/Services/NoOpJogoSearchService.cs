using DTOs;
using Entities;
using Interfaces;

namespace Services;

public sealed class NoOpJogoSearchService : IJogoSearchService
{
    public Task IndexarAsync(Jogo jogo, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<JogoResponseDto>> BuscarAsync(
        string termo,
        int limite,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<JogoResponseDto>>([]);
}