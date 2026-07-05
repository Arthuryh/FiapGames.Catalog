namespace Entities
{
    public class Compra
    {
        public const string StatusPendente = "Pendente";
        public const string StatusAprovado = "Aprovado";
        public const string StatusReprovado = "Reprovado";

        public int Id { get; private set; }
        public int UsuarioId { get; private set; }
        public DateTime DataCompra { get; private set; }
        public decimal ValorTotalBruto { get; private set; }
        public decimal ValorTotalLiquido { get; private set; }
        public string Status { get; private set; }
        public string? MotivoRecusa { get; private set; }
        public DateTime? DataProcessamentoPagamento { get; private set; }
        public virtual List<CompraJogo> CompraJogos { get; private set; }

        public Compra()
        {
            Status = StatusPendente;
            CompraJogos = [];
        }

        public Compra(int id, int usuarioId)
        {
            Id = id;
            UsuarioId = usuarioId;
            DataCompra = DateTime.Now;
            Status = StatusPendente;
            CompraJogos = [];
        }

        public void AdicionarItem(Jogo jogo)
        {
            var preco = jogo.ObterPrecoAtual();
            CompraJogos.Add(new CompraJogo(jogo.Id, preco));

            ValorTotalBruto += jogo.Preco;
            ValorTotalLiquido += preco;
        }

        public void AprovarPagamento(DateTime processadoEm)
        {
            Status = StatusAprovado;
            MotivoRecusa = null;
            DataProcessamentoPagamento = processadoEm;
        }

        public void ReprovarPagamento(DateTime processadoEm, string? motivoRecusa)
        {
            Status = StatusReprovado;
            MotivoRecusa = motivoRecusa;
            DataProcessamentoPagamento = processadoEm;
        }
    }
}
