using DTOs;
using Entities;
using Interfaces;

namespace Services
{
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _repo;
        private readonly IJogoRepository _jogoRepo;
        /*private readonly IContaService _contaServico;

        QUEBROU PRECISA REFATORAR E VERIFICAR COMO FAZER COMUNICAÇÃO COM MICROSSERVIÇO DE CONTA, 
        POIS O SERVIÇO DE COMPRA PRECISA DEBITAR O SALDO DO USUÁRIO, 
        ENTÃO PRECISA CHAMAR O SERVIÇO DE CONTA PARA FAZER ISSO.

        */
        private readonly IBibliotecaService _bibliotecaServico;

        public CompraService
        (
            ICompraRepository repo, 
            IJogoRepository jogoRepo,
            //IContaService contaServico, REFATORAR PARA CHAMAR O MICROSSERVIÇO DE CONTA
            IBibliotecaService bibliotecaServico
        )
        {
            _repo = repo;
            _jogoRepo = jogoRepo;
            //_contaServico = contaServico; REFATORAR PARA CHAMAR O MICROSSERVIÇO DE CONTA
            _bibliotecaServico = bibliotecaServico;
        }

        public async Task CriarCompra(CriarCompraDto dto)
        {
            var compra = new Compra(0);

            foreach (var jogoId in dto.JogosIds)
            {
                var jogo = await _jogoRepo.JogoPorId(jogoId);
                if (jogo == null)
                    throw new ArgumentException("Jogo não encontrado: " + jogoId);
                compra.AdicionarItem(jogo);
            }

            await _repo.Add(compra);

            /*
            NECESSÁRIO DEBITAR O SALDO DO USUÁRIO, ENTÃO PRECISA CHAMAR O MICROSSERVIÇO DE CONTA PARA FAZER ISSO.
            
            await _contaServico.DebitarSaldo(new DTOs.Conta.ContaDto(dto.IdUsuario, compra.ValorTotalLiquido));

            foreach (var jogoId in dto.JogosIds)
            {
                await _bibliotecaServico.AdicionarJogo(dto.IdUsuario, jogoId);
            }*/
        }
    }
}
