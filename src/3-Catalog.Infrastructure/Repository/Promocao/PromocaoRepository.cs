using Context;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class PromocaoRepository(CatalogContext _context) : IPromocaoRepository
    {
        public async Task Add(Promocao promocao)
        {
            await _context.Promocoes.AddAsync(promocao);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Promocao>> GetAll()
        {
            return await _context.Promocoes.ToListAsync();
        }

        public async Task<Promocao> GetById(int id)
        {
            return await _context.Promocoes
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task Update(Promocao promocao)
        {
            _context.Promocoes.Update(promocao);
            await _context.SaveChangesAsync();
        }
    }
}