namespace ProyectoArqSoft.Domain.Models

{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string RemitenteNombre { get; set; } = string.Empty;
        public string RemitenteEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSsl { get; set; }
    }
}
