using DTOs;
using Entities;
using Interfaces;

namespace Services
{
    public class JogoService : IJogoService
    {
        private readonly IJogoRepository _repo;
        private readonly IPromocaoRepository _promoRepo;
        private readonly ICatalogCacheService _cacheService;

        public JogoService(IJogoRepository repo, IPromocaoRepository promoRepo, ICatalogCacheService cacheService)
        {
            _repo = repo;
            _promoRepo = promoRepo;
            _cacheService = cacheService;
        }

        public async Task Criar(CriarJogoDto dto)
        {
            var jogo = new Jogo(dto.Nome, dto.Preco, dto.Descricao);
            await _repo.Add(jogo);
            await _cacheService.RemoveAsync("catalog:jogos:todos");
        }

        public async Task AplicarPromocao(AplicarPromocaoDto dto)
        {
            var jogo = await _repo.JogoPorId(dto.JogoId);
            if (dto.PromocaoId == null || dto.PromocaoId == 0)
            {
                jogo.RemoverPromocao();
            }
            else
            {
                var promo = await _promoRepo.GetById(dto.PromocaoId);
                if (promo == null)
                    throw new ArgumentException("Promocao Inválida");

                jogo.AplicarPromocao(promo);
            }

            await _repo.Update(jogo);
            await _cacheService.RemoveAsync("catalog:jogos:todos");
            await _cacheService.RemoveAsync($"catalog:jogo:{dto.JogoId}");
        }

        public async Task<IEnumerable<JogoResponseDto>> ListaJogos()
        {
            const string cacheKey = "catalog:jogos:todos";
            var cached = await _cacheService.GetAsync<IEnumerable<JogoResponseDto>>(cacheKey);
            if (cached != null)
                return cached;

            var jogos = await _repo.GetListaJogos();
            var listaJogos = jogos.Select(j => new JogoResponseDto
            (
                j.Id,
                j.Nome,
                j.Preco,
                j.ObterPrecoAtual(),
                j.Descricao,
                j.DataLancamento,
                j.Promocao == null ? null : new PromocaoResponseDto(
                    j.Promocao.Id,
                    j.Promocao.Nome,
                    j.Promocao.TaxaDesconto,
                    j.Promocao.DataInicio,
                    j.Promocao.DataFim,
                    j.Promocao.Ativo
                )
            )).ToList();

            await _cacheService.SetAsync(cacheKey, listaJogos, TimeSpan.FromMinutes(5));
            return listaJogos;
        }

        public async Task<JogoResponseDto> JogoPorId(int idJogo)
        {
            var cacheKey = $"catalog:jogo:{idJogo}";
            var cached = await _cacheService.GetAsync<JogoResponseDto>(cacheKey);
            if (cached != null)
                return cached;

            var jogo = await _repo.JogoPorId(idJogo);
            if (jogo == null)
                throw new ArgumentException("Jogo não encontrado");

            var response = new JogoResponseDto(
               jogo.Id,
               jogo.Nome,
               jogo.Preco,
               jogo.ObterPrecoAtual(),
               jogo.Descricao,
               jogo.DataLancamento,
               jogo.Promocao == null ? null : new PromocaoResponseDto(
                    jogo.Promocao.Id,
                    jogo.Promocao.Nome,
                    jogo.Promocao.TaxaDesconto,
                    jogo.Promocao.DataInicio,
                    jogo.Promocao.DataFim,
                    jogo.Promocao.Ativo
                )
           );

            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
            return response;
        }
    }
}