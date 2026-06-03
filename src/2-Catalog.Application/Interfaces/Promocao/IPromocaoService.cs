using DTOs;

namespace Interfaces
{
    public interface IPromocaoService
    {
        Task Criar(CriarPromocaoDto dto);
        Task Atualizar(AtualizarPromocaoDto dto);
        Task<List<PromocaoResponseDto>> ObterTodos();
        Task<PromocaoResponseDto> ObterPorId(int id);
        Task Deletar(int id);
    }
}
