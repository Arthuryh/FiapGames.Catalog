using DTOs;

namespace Interfaces
{
    public interface IJogoService
    {
        Task Criar(CriarJogoDto dto);
        Task AplicarPromocao(AplicarPromocaoDto dto);
        Task<IEnumerable<JogoResponseDto>> ListaJogos();
        Task<JogoResponseDto> JogoPorId(int idJogo);
    }
}
