namespace DTOs
{
    public record JogoResponseDto
    (
        int Id,
        string Nome,
        decimal Preco,
        decimal PrecoAtual,
        string Descricao,
        DateTime DataLancamento,
        PromocaoResponseDto Promocao
    );
}
