namespace IntegrationEvents;

public record CompraSolicitadaIntegrationEvent(
    int CompraId,
    int UsuarioId,
    IReadOnlyCollection<int> JogosIds,
    decimal ValorTotal,
    DateTime SolicitadaEm,
    string EmailUsuario)
{
    public string RastreioId { get; init; } = Guid.NewGuid().ToString();
}
