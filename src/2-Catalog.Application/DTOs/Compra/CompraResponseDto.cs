namespace DTOs;

public record CompraResponseDto(
    int Id,
    int UsuarioId,
    DateTime DataCompra,
    decimal ValorTotalBruto,
    decimal ValorTotalLiquido,
    string Status,
    string? MotivoRecusa,
    DateTime? DataProcessamentoPagamento,
    IReadOnlyCollection<CompraJogoResponseDto> Jogos);
