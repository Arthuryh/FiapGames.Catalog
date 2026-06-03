namespace DTOs
{
    public record PromocaoResponseDto
    (
        int Id,
        string Nome,
        int TaxaDesconto,
        DateTime DataInicio,
        DateTime DataFim,
        bool Ativo
    );
}
