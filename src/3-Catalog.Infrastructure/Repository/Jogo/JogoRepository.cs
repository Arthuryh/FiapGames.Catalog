using Context;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class JogoRepository(CatalogContext _context) : IJogoRepository
    {
        public async Task Add(Jogo jogo)
        {
            await _context.AddAsync(jogo);
            await _context.SaveChangesAsync();
        }

        public async Task<Jogo> JogoPorId(int id)
        {
            return await _context.Jogos
                .Include(x => x.Promocao)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Jogo>> GetListaJogos()
        {
            return await _context.Jogos
                .Include(x => x.Promocao)
                .ToListAsync();
        }

        public async Task Update(Jogo jogo)
        {
            _context.Update(jogo);
            await _context.SaveChangesAsync();
        }
    }
}