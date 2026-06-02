namespace Entities
{
    public class Promocao
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public int TaxaDesconto { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public bool Ativo { get; private set; }
        public Promocao() { }

        public Promocao(string nome, int taxa, DateTime inicio, DateTime fim)
        {
            if (taxa <= 0 || taxa > 100)
                throw new ArgumentException("Taxa inválida");

            if (fim <= inicio)
                throw new ArgumentException("Período inválido");

            Nome = nome;
            TaxaDesconto = taxa;
            DataInicio = inicio;
            DataFim = fim;
            Ativo = true;
        }

        public bool EstaAtiva()
            => Ativo && DateTime.Now >= DataInicio && DateTime.Now <= DataFim;

        public void Desativar()
        {
            Ativo = false;
        }

        public void Atualizar(string nome, int taxa, DateTime inicio, DateTime fim)
        {
            Nome = nome;
            TaxaDesconto = taxa;
            DataInicio = inicio;
            DataFim = fim;
        }
    }
}