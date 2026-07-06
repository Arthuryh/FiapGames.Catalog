namespace IntegrationEvents;

public record PagamentoProcessadoIntegrationEvent(
    int CompraId,
    int UsuarioId,
    string? EmailUsuario,
    bool Aprovado,
    decimal ValorTotal,
    string Status,
    DateTime ProcessadoEm,
    string RastreioId,
    string? MotivoRecusa);
