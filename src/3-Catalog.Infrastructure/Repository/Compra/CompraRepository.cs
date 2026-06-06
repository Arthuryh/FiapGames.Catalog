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
    }
}