using DTOs;
using Entities;
using Interfaces;

namespace Services
{
    public class BibliotecaService : IBibliotecaService
    {
        private readonly IBibliotecaRepository _repo;
        private readonly IJogoRepository _jogoRepo;
        private readonly ICatalogCacheService _cacheService;

        public BibliotecaService(IBibliotecaRepository repo, IJogoRepository jogoRepo, ICatalogCacheService cacheService)
        {
            _repo = repo;
            _jogoRepo = jogoRepo;
            _cacheService = cacheService;
        }

        public async Task AdicionarJogo(int contaId, int jogoId)
        {
            var biblioteca = await _repo.ObterPorConta(contaId);
            var jogo = await _jogoRepo.JogoPorId(jogoId);

            if (jogo == null)
                throw new ArgumentException("Jogo não encontrado: " + jogoId);

            if (biblioteca.PossuiJogo(jogoId))
                return;

            biblioteca.AdicionarJogo(jogo);

            await _repo.Atualizar(biblioteca);
            await _cacheService.RemoveAsync($"catalog:biblioteca:{contaId}");
        }

        public async Task RemoverJogo(int contaId, int jogoId)
        {
            var biblioteca = await _repo.ObterPorConta(contaId);

            biblioteca.RemoverJogo(jogoId);

            await _repo.Atualizar(biblioteca);
            await _cacheService.RemoveAsync($"catalog:biblioteca:{contaId}");
        }

        public async Task<bool> PossuiJogo(int contaId, int jogoId)
        {
            var biblioteca = await _repo.ObterPorConta(contaId);
            return biblioteca.PossuiJogo(jogoId);
        }

        public async Task<BibliotecaResponse> BibliotecaUsuario(int contaId)
        {
            var cacheKey = $"catalog:biblioteca:{contaId}";
            var cached = await _cacheService.GetAsync<BibliotecaResponse>(cacheKey);
            if (cached != null)
                return cached;

            var biblioteca = await _repo.ObterPorConta(contaId);
            var listaJogos = new List<Jogo>();

            foreach (var item in biblioteca.Jogos)
            {
                var jogo = await _jogoRepo.JogoPorId(item.JogoId);
                if (jogo == null)
                    throw new ArgumentException("Jogo não encontrado: " + item.JogoId);

                listaJogos.Add(jogo);
            }

            var response = new BibliotecaResponse
            (
                biblioteca.IdConta,
                listaJogos.Select(x => new BibliotecaJogoResponseDto
                (
                    x.Id,
                    x.Nome,
                    x.Preco,
                    x.ObterPrecoAtual(),
                    x.Descricao,
                    x.DataLancamento
                )).ToList()
            );

            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(2));
            return response;
        }
    }
}
