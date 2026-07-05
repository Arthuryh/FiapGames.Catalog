using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record CriarCompraDto(
        [Required(ErrorMessage = "A compra precisa ter pelo menos um ID de um Jogo")]
        List<int> JogosIds
    );
}
