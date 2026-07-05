using DTOs;
using Entities;
using Interfaces;

namespace Services
{
    public class BibliotecaService : IBibliotecaService
    {
        private readonly IBibliotecaRepository _repo;
        private readonly IJogoRepository _jogoRepo;

        public BibliotecaService(IBibliotecaRepository repo, IJogoRepository jogoRepo)
        {
            _repo = repo;
            _jogoRepo = jogoRepo;
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
        }

        public async Task RemoverJogo(int contaId, int jogoId)
        {
            var biblioteca = await _repo.ObterPorConta(contaId);

            biblioteca.RemoverJogo(jogoId);

            await _repo.Atualizar(biblioteca);
        }

        public async Task<bool> PossuiJogo(int contaId, int jogoId)
        {
            var biblioteca = await _repo.ObterPorConta(contaId);
            return biblioteca.PossuiJogo(jogoId);
        }

        public async Task<BibliotecaResponse> BibliotecaUsuario(int contaId)
        {
            var biblioteca = await _repo.ObterPorConta(contaId);
            var listaJogos = new List<Jogo>();

            foreach (var item in biblioteca.Jogos)
            {
                var jogo = await _jogoRepo.JogoPorId(item.JogoId);
                if (jogo == null)
                    throw new ArgumentException("Jogo não encontrado: " + item.JogoId);

                listaJogos.Add(jogo);
            }

            return new BibliotecaResponse
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
        }
    }
}
