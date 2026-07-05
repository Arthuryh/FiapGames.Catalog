using Entities;

namespace Interfaces
{
    public interface IPromocaoRepository
    {
        Task<Promocao> GetById(int id);
        Task<List<Promocao>> GetAll();
        Task Add(Promocao promocao);
        Task Update(Promocao promocao);
    }
}
