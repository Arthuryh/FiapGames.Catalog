using Entities;
using Microsoft.EntityFrameworkCore;

namespace Context
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(CatalogContext context)
        {
            try
            {
                //garante que o banco está atualizado
                await context.Database.MigrateAsync();

                //se já tem dados, não faz nada
                if (context.Jogos.Any())
                    return;

                var jogos = new List<Jogo>
                {
                    new Jogo("God of War", 200, "Ação"),
                    new Jogo("FIFA 24", 300, "Esporte"),
                    new Jogo("GTA V", 150, "Mundo aberto"),
                    new Jogo("The Witcher 3", 100, "RPG"),
                    new Jogo("Minecraft", 80, "Sandbox")
                };

                var promo = new Promocao(
                    "Promo Inicial",
                    10,
                    DateTime.Now.AddDays(-1),
                    DateTime.Now.AddDays(10)
                );

                context.Promocoes.Add(promo);

                // aplica promoção em alguns jogos
                jogos[0].AplicarPromocao(promo);
                jogos[1].AplicarPromocao(promo);

                context.Jogos.AddRange(jogos);

                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}