using DTOs;

namespace Interfaces
{
    public interface IBibliotecaService
    {
        Task AdicionarJogo(int contaId, int jogoId);
        Task RemoverJogo(int contaId, int jogoId);
        Task<bool> PossuiJogo(int contaId, int jogoId);
        Task<JogoResponseDto> BibliotecaUsuario(int contaId);
    }
}
