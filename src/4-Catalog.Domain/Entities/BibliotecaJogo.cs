namespace Entities
{
    public class BibliotecaJogo
    {
        public int BibliotecaId { get; private set; }
        public int JogoId { get; private set; }
        public DateTime DataAdicao { get; private set; }

        public BibliotecaJogo() { }

        public BibliotecaJogo(int jogoId)
        {
            if (jogoId <= 0)
                throw new ArgumentException("JogoId inválido");

            JogoId = jogoId;
            DataAdicao = DateTime.UtcNow;
        }
    }
}