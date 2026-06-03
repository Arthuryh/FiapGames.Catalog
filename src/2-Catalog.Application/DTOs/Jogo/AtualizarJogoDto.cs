using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record AtualizarJogoDto(
        [Required(ErrorMessage = "O ID do jogo é obrigatório.")]
        int Id,

        [Required(ErrorMessage = "O nome do jogo é obrigatório.")]
        string Nome,

        [Required(ErrorMessage = "O preço do jogo é obrigatório.")]
        decimal Preco,

        [Required(ErrorMessage = "A descrição do jogo é obrigatória.")]
        string Descricao
    );
}
