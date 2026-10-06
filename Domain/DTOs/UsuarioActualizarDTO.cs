using System.Security.Cryptography.X509Certificates;

namespace ProyectoArqSoft.Domain.DTOs
{
    public class UsuarioActualizarDto
    {
         public int IdUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public byte Activo { get; set; } = 1;  // Estado de activación, opcionalmente puede no ser modificado

        public string? UserName { get; set; } = string.Empty;  // Agregado para mostrar el nombre de usuario en la vista de edición
        public string? Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string Ci { get; set; } = string.Empty;
        public string CiExtencion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;

        // Datos relacionados con la contraseña (si es necesario cambiarla)
        public byte MustChangePassword { get; set; } = 1;
    }
}

