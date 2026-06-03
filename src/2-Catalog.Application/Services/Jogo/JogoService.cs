using DTOs;
using Entities;
using Interfaces;

namespace Services
{
    public class JogoServico : IJogoService
    {
        private readonly IJogoRepository _repo;
        private readonly IPromocaoRepository _promoRepo;

        public JogoServico(IJogoRepository repo, IPromocaoRepository promoRepo)
        {
            _repo = repo;
            _promoRepo = promoRepo;
        }

        public async Task Criar(CriarJogoDto dto)
        {
            var jogo = new Jogo(dto.Nome, dto.Preco, dto.Descricao);
            await _repo.Add(jogo);
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
        }

        public async Task<IEnumerable<JogoResponseDto>> ListaJogos()
        {
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

            return listaJogos;
        }

        public async Task<JogoResponseDto> JogoPorId(int idJogo)
        {
            var jogo = await _repo.JogoPorId(idJogo);

            return new JogoResponseDto(
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
        }
    }
}