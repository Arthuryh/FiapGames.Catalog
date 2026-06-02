using Entities;

namespace Interfaces
{
    public interface IJogoRepository
    {
        Task<Jogo> JogoPorId(int id);
        Task Add(Jogo jogo);
        Task Update(Jogo jogo);
        Task<IEnumerable<Jogo>> GetListaJogos();
    }
}
