namespace DTOs
{
    public record BibliotecaResponse
    (
         int ContaId,
         IEnumerable<BibliotecaJogoResponseDto> Jogos
    );
}
