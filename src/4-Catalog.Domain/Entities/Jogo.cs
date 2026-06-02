namespace Entities
{
    public class Jogo
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public decimal Preco { get; private set; }
        public string Descricao { get; private set; }
        public DateTime DataLancamento { get; private set; }
        public int? PromocaoId { get; private set; }
        public virtual Promocao? Promocao { get; private set; }

        public Jogo() { }
        public Jogo(string nome, decimal preco, string descricao, int idJogo = 0)
        {
            if (string.IsNullOrEmpty(nome))
                throw new ArgumentException("Nome do jogo é obrigatório");

            if (string.IsNullOrEmpty(descricao))
                throw new ArgumentException("Descricao do jogo é obrigatório");

            if (preco <= 0)
                throw new ArgumentException("Preço precisa ser maior que 0.00");

            if (idJogo >= 0)
                Id = idJogo;

            Nome = nome;
            Preco = preco;
            Descricao = descricao;
            DataLancamento = DateTime.Now;
        }

        public decimal ObterPrecoAtual()
        {
            if (Promocao != null && Promocao.EstaAtiva())
                return Preco - (Preco * Promocao.TaxaDesconto / 100);

            return Preco;
        }

        public void AplicarPromocao(Promocao promocao)
        {
            if (!promocao.EstaAtiva())
                throw new ArgumentException("Promoção inválida");

            Promocao = promocao;
            PromocaoId = promocao.Id;
        }

        public void RemoverPromocao()
        {
            Promocao = null;
        }
    }
}