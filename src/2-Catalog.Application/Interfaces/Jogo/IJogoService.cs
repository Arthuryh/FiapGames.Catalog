using DTOs;

namespace Interfaces
{
    public interface IJogoService
    {
        Task Criar(CriarJogoDto dto);
        Task Atualizar(AtualizarJogoDto dto);
        Task AplicarPromocao(AplicarPromocaoDto dto);
        Task<IEnumerable<JogoResponseDto>> ListaJogos();
        Task<JogoResponseDto> JogoPorId(int idJogo);
        Task<IReadOnlyList<JogoResponseDto>> Buscar(string termo, int limite, CancellationToken cancellationToken = default);
    }
}
