namespace ProyectoArqSoft.Domain.Models

{
    public class UsuarioToken
    {
        public int IdUsuarioToken { get; set; }
        public int UsuarioIdUsuario { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public string TipoToken { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaExpiracion { get; set; }
        public sbyte Revocado { get; set; } = 0;
        public sbyte Usado { get; set; } = 0;
        public DateTime? FechaUso { get; set; }
        public DateTime? FechaRevocacion { get; set; }

        public UsuarioToken() { }

        public UsuarioToken(int usuarioIdUsuario, string tokenHash, string tipoToken, DateTime fechaExpiracion)
        {
            UsuarioIdUsuario = usuarioIdUsuario;
            TokenHash = tokenHash;
            TipoToken = tipoToken;
            FechaExpiracion = fechaExpiracion;
            Revocado = 0;
            Usado = 0;
        }
    }
}
