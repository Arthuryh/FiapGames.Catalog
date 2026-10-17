using DTOs;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JogoController(IJogoService jogoService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Criar(CriarJogoDto dto)
        {
            await jogoService.Criar(dto);
            return Ok();
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Atualizar(AtualizarJogoDto dto)
        {
            await jogoService.Atualizar(dto);
            return NoContent();
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

        [HttpGet("/search")]
        public async Task<IActionResult> Buscar(
            [FromQuery] string q,
            [FromQuery] int limite = 20,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { erro = "O parâmetro 'q' é obrigatório." });

            var resultados = await jogoService.Buscar(q.Trim(), Math.Clamp(limite, 1, 100), cancellationToken);
            return Ok(resultados);
        }
    }
}
