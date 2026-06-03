namespace DTOs
{
    public record BibliotecaJogoResponseDto
    (
         int Id,
         string Nome,
         decimal Preco,
         decimal PrecoAtual,
         string Descricao,
         DateTime DataLancamento
    );
}
