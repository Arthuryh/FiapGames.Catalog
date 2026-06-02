using Entities;
using Microsoft.EntityFrameworkCore;

namespace Context
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
        {
        }
        public DbSet<Biblioteca> Bibliotecas { get; set; }
        public DbSet<BibliotecaJogo> BibliotecaJogos { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<CompraJogo> CompraJogos { get; set; }
        public DbSet<Jogo> Jogos { get; set; }
        public DbSet<Promocao> Promocoes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Biblioteca>(entity =>
            {
                entity.HasKey(x => x.IdBiblioteca);

                entity.HasMany(x => x.Jogos)
                      .WithOne()
                      .HasForeignKey("BibliotecaId")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<BibliotecaJogo>(entity =>
            {
                entity.HasKey(x => new { x.BibliotecaId, x.JogoId });

                entity.Property(x => x.DataAdicao)
                      .IsRequired();

                entity.HasOne<Jogo>()
                      .WithMany()
                      .HasForeignKey(x => x.JogoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Compra>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasMany(x => x.CompraJogos)
                      .WithOne()
                      .HasForeignKey(x => x.CompraId);
            });

            builder.Entity<CompraJogo>(entity =>
            {
                entity.HasKey(x => new { x.CompraId, x.JogoId });
            });

            builder.Entity<Jogo>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Nome).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Preco).HasColumnType("decimal(10,2)");

                entity.HasOne(x => x.Promocao)
                      .WithMany()
                      .HasForeignKey(x => x.PromocaoId);
            });

            builder.Entity<Promocao>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
        }
    }
}