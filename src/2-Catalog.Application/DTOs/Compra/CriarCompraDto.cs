using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record CriarCompraDto(
        [Required(ErrorMessage = "A compra precisa ter pelo menos um ID de um Jogo")]
        List<int> JogosIds,

        [Required(ErrorMessage = "O ID do usuário é obrigatório para realizar a compra")]
        int IdUsuario
    );
}
