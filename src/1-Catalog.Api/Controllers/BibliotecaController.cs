using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /*[Authorize]
     *COMENTADO COM INTUITO DE TESTE, MAS DEVE SER DESCOMENTADO PARA PRODUÇÃO
     */
    public class BibliotecaController(IBibliotecaService bibliotecaService) : ControllerBase
    {
        [HttpDelete("{contaId}/jogos/{jogoId}")]
        public async Task<IActionResult> RemoverJogo(int contaId, int jogoId)
        {
            try
            {
                await bibliotecaService.RemoverJogo(contaId, jogoId);
                return Ok(new { mensagem = "Jogo removido da biblioteca" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpGet("{contaId}")]
        public async Task<IActionResult> BibliotecaUsuario(int contaId)
        {
            var biblioteca = await bibliotecaService.BibliotecaUsuario(contaId);

            return Ok(biblioteca);
        }
    }
}
