using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Infrastructure.Helpers;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Domain.DTOs;

namespace ProyectoArqSoft.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IUsuarioTokenRepository _repository;

        public TokenService(IUsuarioTokenRepository repository)
        {
            _repository = repository;
        }

        public (Result, string) GenerarToken(UsuarioTokenGeneracionDto dto, out string tokenPlano)
        {
            tokenPlano = TokenHelper.GenerarTokenPlano();
            string tokenHash = TokenHelper.GenerarTokenHash(tokenPlano);
            DateTime fechaExpiracion = TokenHelper.GenerarFechaExpiracion(dto.MinutosExpiracion);

            UsuarioToken token = new UsuarioToken(
                dto.IdUsuario,
                tokenHash,
                dto.TipoToken,
                fechaExpiracion
            );

            int filasAfectadas = _repository.Insert(token);

            if (filasAfectadas > 0)
            {
                // Para ActivacionCuenta o ResetPassword, retornar el tokenPlano (para la URL)
                if (dto.TipoToken == TipoTokenConstantes.ActivacionCuenta || dto.TipoToken == TipoTokenConstantes.ResetPassword)
                {
                    return (Result.Ok(), tokenPlano);  // Retorna el tokenPlano para usar en la URL
                }

                // Para otros tipos (como login), generar JWT
                string secretKey = Environment.GetEnvironmentVariable("JWT_KEY")!;
                string issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
                string audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, dto.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, dto.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Role, dto.Role ?? string.Empty)
                };

                var tokenJWT = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: fechaExpiracion,
                    signingCredentials: credentials
                );

                string jwtToken = new JwtSecurityTokenHandler().WriteToken(tokenJWT);
                return (Result.Ok(), jwtToken);  // Retorna el JWT para login
            }
            else
            {
                tokenPlano = string.Empty;
                return (Result.Fail("No se pudo generar el token."), string.Empty);
            }
        }

        // Validar token
        public UsuarioToken? ValidarToken(string tokenPlano, string tipoToken)
        {
            string tokenHash = TokenHelper.GenerarTokenHash(tokenPlano);
            UsuarioToken? token = _repository.GetTokenActivo(tokenHash, tipoToken);

            if (token == null || token.FechaExpiracion <= DateTime.UtcNow)
                return null;

            return token;
        }

        // Marcar token como usado
        public Result MarcarComoUsado(int idUsuarioToken)
        {
            if (idUsuarioToken <= 0)
                return Result.Fail("El id del token debe ser mayor a cero.");

            int filasAfectadas = _repository.MarcarComoUsado(idUsuarioToken);

            return filasAfectadas > 0
                ? Result.Ok()
                : Result.Fail("No se pudo marcar el token como usado.");
        }

        // Revocar tokens activos
        public Result RevocarTokensActivos(int idUsuario, string tipoToken)
        {
            if (idUsuario <= 0)
                return Result.Fail("El id del usuario debe ser mayor a cero.");

            tipoToken = tipoToken?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tipoToken))
                return Result.Fail("El tipo de token es obligatorio.");

            _repository.RevocarTokensActivos(idUsuario, tipoToken);
            return Result.Ok();
        }

        // Eliminar tokens obsoletos
        public Result EliminarTokensObsoletos(int dias)
        {
            if (dias <= 0)
                return Result.Fail("La cantidad de días debe ser mayor a cero.");

            _repository.EliminarTokensObsoletos(dias);
            return Result.Ok();
        }
    }
}