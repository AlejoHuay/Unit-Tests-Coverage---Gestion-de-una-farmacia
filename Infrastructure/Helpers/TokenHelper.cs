using System.Security.Cryptography;
using System.Text;

namespace ProyectoArqSoft.Infrastructure.Helpers
{
    public static class TokenHelper
    {
        public static string GenerarTokenPlano(int cantidadBytes = 32)
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(cantidadBytes);
            return Convert.ToBase64String(tokenBytes);
        }

        public static string GenerarTokenHash(string tokenPlano)
        {
            if (string.IsNullOrWhiteSpace(tokenPlano))
                throw new ArgumentException("El token no puede ser nulo o vacío.", nameof(tokenPlano));

            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(tokenPlano);
            byte[] hashBytes = sha256.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        public static DateTime GenerarFechaExpiracion(int minutosExpiracion)
        {
            if (minutosExpiracion <= 0)
                throw new ArgumentException("Los minutos de expiración deben ser mayores a cero.", nameof(minutosExpiracion));

            return DateTime.UtcNow.AddMinutes(minutosExpiracion);
        }
    }
}