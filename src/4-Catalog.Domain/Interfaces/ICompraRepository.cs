using Entities;

namespace Interfaces
{
    public interface ICompraRepository
    {
        Task Add(Compra compra);
        Task<Compra?> ObterPorId(int compraId);
        Task<IReadOnlyCollection<Compra>> ObterPorUsuario(int usuarioId);
        Task Atualizar(Compra compra);
    }
}
