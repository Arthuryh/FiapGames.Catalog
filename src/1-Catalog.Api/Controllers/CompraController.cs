using DTOs;
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
    public class CompraController(ICompraService compraService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Criar(CriarCompraDto dto)
        {
            await compraService.CriarCompra(dto);
            return Ok();
        }
    }
}
