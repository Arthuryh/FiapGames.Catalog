namespace Entities
{
    public class Biblioteca
    {
        public int IdBiblioteca { get; private set; }
        public int IdConta { get; private set; }

        public virtual List<BibliotecaJogo> Jogos { get; private set; }

        public Biblioteca() { }

        public Biblioteca(int idConta)
        {
            if (idConta <= 0)
                throw new ArgumentException("Conta inválida");

            IdConta = idConta;
            Jogos = new List<BibliotecaJogo>();
        }

        public void AdicionarJogo(Jogo jogo)
        {
            if (jogo == null)
                throw new ArgumentException("Jogo inválido");

            if (Jogos.Any(x => x.JogoId == jogo.Id))
                throw new ArgumentException("Jogo já está na biblioteca");

            Jogos.Add(new BibliotecaJogo(jogo.Id));
        }

        public void RemoverJogo(int jogoId)
        {
            var item = Jogos.FirstOrDefault(x => x.JogoId == jogoId);

            if (item == null)
                throw new ArgumentException("Jogo não encontrado na biblioteca");

            Jogos.Remove(item);
        }

        public bool PossuiJogo(int jogoId)
        {
            return Jogos.Any(x => x.JogoId == jogoId);
        }
    }
}