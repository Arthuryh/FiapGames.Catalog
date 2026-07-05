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
    public class PromocaoController(IPromocaoService promocaoService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Criar(CriarPromocaoDto dto)
        {
            await promocaoService.Criar(dto);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Atualizar(AtualizarPromocaoDto dto)
        {
            await promocaoService.Atualizar(dto);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await promocaoService.ObterTodos();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await promocaoService.ObterPorId(id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await promocaoService.Deletar(id);
            return Ok();
        }
    }
}
