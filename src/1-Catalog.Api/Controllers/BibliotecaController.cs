using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BibliotecaController(IBibliotecaService bibliotecaService) : ControllerBase
    {
        [HttpDelete("jogos/{jogoId}")]
        public async Task<IActionResult> RemoverJogo(int jogoId)
        {
            await bibliotecaService.RemoverJogo(ObterUsuarioId(), jogoId);
            return Ok(new { mensagem = "Jogo removido da biblioteca" });
        }

        [HttpGet]
        public async Task<IActionResult> BibliotecaUsuario()
        {
            var biblioteca = await bibliotecaService.BibliotecaUsuario(ObterUsuarioId());

            return Ok(biblioteca);
        }

        private int ObterUsuarioId()
        {
            var claimValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub") ??
                User.FindFirstValue("id") ??
                User.FindFirstValue("userId");

            if (!int.TryParse(claimValue, out var usuarioId))
                throw new UnauthorizedAccessException("Token sem identificador de usuario valido.");

            return usuarioId;
        }
    }
}
