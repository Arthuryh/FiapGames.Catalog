using DTOs;
using Entities;
using Interfaces;

namespace Services
{
    public class PromocaoService : IPromocaoService
    {
        private readonly IPromocaoRepository _repo;

        public PromocaoService(IPromocaoRepository repo)
        {
            _repo = repo;
        }

        public async Task Criar(CriarPromocaoDto dto)
        {
            var promocao = new Promocao(
                dto.Nome,
                dto.TaxaDesconto,
                dto.DataInicio,
                dto.DataFim
            );

            await _repo.Add(promocao);
        }

        public async Task Atualizar(AtualizarPromocaoDto dto)
        {
            var promocao = await _repo.GetById(dto.Id);

            if (promocao == null)
                throw new ArgumentException("Promoção não encontrada");

            promocao.Atualizar(
                dto.Nome,
                dto.TaxaDesconto,
                dto.DataInicio,
                dto.DataFim
            );

            await _repo.Update(promocao);
        }

        public async Task<List<PromocaoResponseDto>> ObterTodos()
        {
            var lista = await _repo.GetAll();

            return lista.Select(x => new PromocaoResponseDto(
                x.Id,
                x.Nome,
                x.TaxaDesconto,
                x.DataInicio,
                x.DataFim,
                x.Ativo
            )).ToList();
        }

        public async Task<PromocaoResponseDto> ObterPorId(int id)
        {
            var x = await _repo.GetById(id);

            if (x == null)
                throw new ArgumentException("Promoção não encontrada");

            return new PromocaoResponseDto(
                x.Id,
                x.Nome,
                x.TaxaDesconto,
                x.DataInicio,
                x.DataFim,
                x.Ativo
            );
        }

        public async Task Deletar(int id)
        {
            var promocao = await _repo.GetById(id);

            if (promocao == null)
                throw new ArgumentException("Promoção não encontrada");

            promocao.Desativar();

            await _repo.Update(promocao);
        }
    }
}
