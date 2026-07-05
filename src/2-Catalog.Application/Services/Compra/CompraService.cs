using DTOs;
using Entities;
using IntegrationEvents;
using Interfaces;

namespace Services
{
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _repo;
        private readonly IJogoRepository _jogoRepo;
        private readonly ICompraEventPublisher _compraEventPublisher;
        private readonly IBibliotecaService _bibliotecaService;

        public CompraService
        (
            ICompraRepository repo,
            IJogoRepository jogoRepo,
            ICompraEventPublisher compraEventPublisher,
            IBibliotecaService bibliotecaService
        )
        {
            _repo = repo;
            _jogoRepo = jogoRepo;
            _compraEventPublisher = compraEventPublisher;
            _bibliotecaService = bibliotecaService;
        }

        public async Task CriarCompra(CriarCompraDto dto, int usuarioId, string emailUsuario, CancellationToken cancellationToken = default)
        {
            var jogosIds = dto.JogosIds.Distinct().ToList();

            if (jogosIds.Count != dto.JogosIds.Count)
                throw new ArgumentException("A compra possui jogos duplicados.");

            var compra = new Compra(0, usuarioId);

            foreach (var jogoId in jogosIds)
            {
                var jogo = await _jogoRepo.JogoPorId(jogoId);
                if (jogo == null)
                    throw new ArgumentException("Jogo não encontrado: " + jogoId);

                if (await _bibliotecaService.PossuiJogo(usuarioId, jogoId))
                    throw new ArgumentException("Jogo já adquirido pelo usuário: " + jogoId);

                compra.AdicionarItem(jogo);
            }

            await _repo.Add(compra);

            var evento = new CompraSolicitadaIntegrationEvent(
                compra.Id,
                usuarioId,
                jogosIds,
                compra.ValorTotalLiquido,
                DateTime.UtcNow,
                emailUsuario);

            await _compraEventPublisher.PublicarCompraSolicitadaAsync(evento, cancellationToken);
        }

        public async Task<IReadOnlyCollection<CompraResponseDto>> ObterComprasDoUsuario(int usuarioId)
        {
            var compras = await _repo.ObterPorUsuario(usuarioId);

            return compras.Select(MapearCompra).ToList();
        }

        public async Task ProcessarPagamento(PagamentoProcessadoIntegrationEvent evento)
        {
            var compra = await _repo.ObterPorId(evento.CompraId);

            if (compra is null)
                throw new ArgumentException("Compra não encontrada: " + evento.CompraId);

            if (compra.UsuarioId != evento.UsuarioId)
                throw new InvalidOperationException("Resultado de pagamento incompatível com o usuário da compra.");

            if (evento.Aprovado)
            {
                foreach (var item in compra.CompraJogos)
                {
                    await _bibliotecaService.AdicionarJogo(compra.UsuarioId, item.JogoId);
                }

                compra.AprovarPagamento(evento.ProcessadoEm);
            }
            else
            {
                compra.ReprovarPagamento(evento.ProcessadoEm, evento.MotivoRecusa);
            }

            await _repo.Atualizar(compra);
        }

        private static CompraResponseDto MapearCompra(Compra compra)
        {
            return new CompraResponseDto(
                compra.Id,
                compra.UsuarioId,
                compra.DataCompra,
                compra.ValorTotalBruto,
                compra.ValorTotalLiquido,
                compra.Status,
                compra.MotivoRecusa,
                compra.DataProcessamentoPagamento,
                compra.CompraJogos
                    .Select(x => new CompraJogoResponseDto(x.JogoId, x.PrecoAplicado))
                    .ToList());
        }
    }
}
