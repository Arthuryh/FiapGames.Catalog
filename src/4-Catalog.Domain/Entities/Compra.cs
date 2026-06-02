namespace Entities
{
    public class Compra
    {
        public int Id { get; private set; }
        public DateTime DataCompra { get; private set; }
        public decimal ValorTotalBruto { get; private set; }
        public decimal ValorTotalLiquido { get; private set; }
        public virtual List<CompraJogo> CompraJogos { get; private set; }

        public Compra()
        {
        }

        public Compra(int id)
        {
            Id = id;
            DataCompra = DateTime.Now;
        }

        public void AdicionarItem(Jogo jogo)
        {
            var preco = jogo.ObterPrecoAtual();
            CompraJogos = [new CompraJogo(jogo.Id, preco)];

            ValorTotalBruto += jogo.Preco;
            ValorTotalLiquido += preco;
        }
    }
}