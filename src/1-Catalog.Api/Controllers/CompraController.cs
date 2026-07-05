using DTOs;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompraController(ICompraService compraService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Criar(CriarCompraDto dto, CancellationToken cancellationToken)
        {
            var usuarioId = ObterUsuarioId();
            var emailUsuario = ObterEmailUsuario();

            await compraService.CriarCompra(dto, usuarioId, emailUsuario, cancellationToken);
            return Accepted();
        }

        [HttpGet]
        public async Task<IActionResult> ObterComprasDoUsuario()
        {
            var usuarioId = ObterUsuarioId();
            var compras = await compraService.ObterComprasDoUsuario(usuarioId);

            return Ok(compras);
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

        private string ObterEmailUsuario()
        {
            var email =
                User.FindFirstValue(ClaimTypes.Email) ??
                User.FindFirstValue("email") ??
                User.FindFirstValue("preferred_username") ??
                User.FindFirstValue("unique_name") ??
                User.FindFirstValue("upn");

            if (string.IsNullOrWhiteSpace(email))
                throw new UnauthorizedAccessException("Token sem e-mail do usuário.");

            return email;
        }
    }
}
