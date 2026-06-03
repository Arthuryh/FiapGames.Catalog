using DTOs;

namespace Interfaces
{
    public interface ICompraService
    {
        Task CriarCompra(CriarCompraDto dto);
    }
}
