using DTOs;
using Entities;
using Interfaces;

/*
    NECESSÁRIO REFATORAR A IMPLEMENTAÇÃO, POIS DIVERSOS MÉTODOS ESTÃO UTILIZANDO A CONTA DE USUÁRIO COMO PARÂMETRO
 */

namespace Services
{
    public class BibliotecaServico : IBibliotecaService
    {
        private readonly IBibliotecaRepository _repo;
        private readonly IJogoRepository _jogoRepo;

        public BibliotecaServico(IBibliotecaRepository repo, IJogoRepository jogoRepo)
        {
            _repo = repo;
            _jogoRepo = jogoRepo;
        }

        public async Task AdicionarJogo(int contaId, int jogoId)
        {
            var biblioteca = await _repo.ObterPorConta(contaId);
            var jogo = await _jogoRepo.JogoPorId(jogoId);


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
            return biblioteca.Jogos.Any(x => x.JogoId == jogoId);
        }

        public async Task<JogoResponseDto> BibliotecaUsuario(int contaId)
        {
            var biblioteca = await _repo.ObterPorConta(contaId);
            if (biblioteca == null)
                throw new ArgumentException("Biblioteca não encontrada para a conta informada.");

            var listaJogos = new List<Jogo>();

            foreach (var item in biblioteca.Jogos)
            {
                var jogo = await _jogoRepo.JogoPorId(item.JogoId);
                if (jogo == null)
                    throw new ArgumentException("Jogo não encontrado");

                listaJogos.Add(jogo);
            }


            var bibliotecaResponse = new BibliotecaResponse
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

            /*return bibliotecaResponse; 
             * IMPLEMENTAÇÃO ABAIXO APENAS PARA NÃO GERAR ERRO
             */

            return new JogoResponseDto
            (
                Id: 0,
                Nome: string.Empty,
                Preco: 0,
                PrecoAtual: 0,
                Descricao: string.Empty,
                DataLancamento: DateTime.MinValue,
                Promocao: null
            );
        }
    }
}
