using DTOs;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /*[Authorize]
     *COMENTADO COM INTUITO DE TESTE, MAS DEVE SER DESCOMENTADO PARA PRODUÇÃO
     */
    public class JogoController(IJogoService jogoService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Criar(CriarJogoDto dto)
        {
            await jogoService.Criar(dto);
            return Ok();
        }

        [HttpPost("promocao")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AplicarPromo(AplicarPromocaoDto dto)
        {
            await jogoService.AplicarPromocao(dto);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Jogos()
        {
            var jogos = await jogoService.ListaJogos();
            return Ok(jogos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> JogoPorId(int id)
        {
            var jogo = await jogoService.JogoPorId(id);
            return Ok(jogo);
        }
    }
}
