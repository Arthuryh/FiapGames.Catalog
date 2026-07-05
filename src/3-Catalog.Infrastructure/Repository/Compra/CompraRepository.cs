using Context;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class CompraRepository(CatalogContext _context) : ICompraRepository
    {
        public async Task Add(Compra compra)
        {
            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();
        }

        public async Task<Compra?> ObterPorId(int compraId)
        {
            return await _context.Compras
                .Include(x => x.CompraJogos)
                .FirstOrDefaultAsync(x => x.Id == compraId);
        }

        public async Task<IReadOnlyCollection<Compra>> ObterPorUsuario(int usuarioId)
        {
            return await _context.Compras
                .Include(x => x.CompraJogos)
                .Where(x => x.UsuarioId == usuarioId)
                .OrderByDescending(x => x.DataCompra)
                .ToListAsync();
        }

        public async Task Atualizar(Compra compra)
        {
            _context.Compras.Update(compra);
            await _context.SaveChangesAsync();
        }
    }
}
