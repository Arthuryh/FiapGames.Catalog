using Context;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class BibliotecaRepository(CatalogContext _context) : IBibliotecaRepository
    {
        public async Task Adicionar(Biblioteca biblioteca)
        {
            await _context.Bibliotecas.AddAsync(biblioteca);
            await _context.SaveChangesAsync();
        }

        public async Task Atualizar(Biblioteca biblioteca)
        {
            _context.Bibliotecas.Update(biblioteca);
            await _context.SaveChangesAsync();
        }

        public async Task<Biblioteca> ObterPorConta(int contaId)
        {
            var biblioteca = await _context.Bibliotecas
                .Include(b => b.Jogos)
                .FirstOrDefaultAsync(b => b.IdConta == contaId);

            if (biblioteca == null)
            {
                biblioteca = new Biblioteca(contaId);

                await _context.Bibliotecas.AddAsync(biblioteca);
                await _context.SaveChangesAsync();
            }

            return biblioteca;
        }
    }
}