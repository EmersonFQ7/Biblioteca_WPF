namespace Biblioteca.Entidades
{
    public class Autor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}