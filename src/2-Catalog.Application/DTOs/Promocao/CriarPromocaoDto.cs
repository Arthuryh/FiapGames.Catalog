using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public record CriarPromocaoDto(
        [Required(ErrorMessage = "O nome da promoção é obrigatório.")]
        string Nome,

        [Required(ErrorMessage = "A taxa de desconto é obrigatória.")]
        int TaxaDesconto,

        [Required(ErrorMessage = "A data de início da promoção é obrigatória.")]
        DateTime DataInicio,

        [Required(ErrorMessage = "A data de término da promoção é obrigatória.")]
        DateTime DataFim
    );
}
