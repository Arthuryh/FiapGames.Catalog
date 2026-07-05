using Entities;

namespace Interfaces
{
    public interface IBibliotecaRepository
    {
        Task<Biblioteca> ObterPorConta(int contaId);
        Task Adicionar(Biblioteca biblioteca);
        Task Atualizar(Biblioteca biblioteca);
    }
}
