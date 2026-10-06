using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Infrastructure.Helpers;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Application.Interfaces;

namespace ProyectoArqSoft.Application.Services
{
    public class UsuarioTokenService : IUsuarioTokenService
    {
        private readonly IUsuarioTokenRepository _repository;

        public UsuarioTokenService(IUsuarioTokenRepository repository)
        {
            _repository = repository;
        }

        public Result GenerarToken(UsuarioTokenGeneracionDto dto, out string tokenPlano)
        {
            tokenPlano = string.Empty;

            Result validacionEntrada = ValidarGeneracionToken(dto);
            if (!validacionEntrada.IsSuccess)
                return validacionEntrada;

            string tipoToken = dto.TipoToken.Trim();

            _repository.EliminarTokensObsoletos(7);
            _repository.RevocarTokensActivos(dto.IdUsuario, tipoToken);

            tokenPlano = TokenHelper.GenerarTokenPlano();
            string tokenHash = TokenHelper.GenerarTokenHash(tokenPlano);
            DateTime fechaExpiracion = TokenHelper.GenerarFechaExpiracion(dto.MinutosExpiracion);

            UsuarioToken token = new UsuarioToken(
                dto.IdUsuario,
                tokenHash,
                tipoToken,
                fechaExpiracion
            );

            int filasAfectadas = _repository.Insert(token);

            if (filasAfectadas <= 0)
            {
                tokenPlano = string.Empty;
                return Result.Fail("No se pudo generar el token.");
            }

            return Result.Ok();
        }

        public UsuarioToken? ValidarToken(string tokenPlano, string tipoToken)
        {
            tokenPlano = tokenPlano?.Trim() ?? string.Empty;
            tipoToken = tipoToken?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(tokenPlano) || string.IsNullOrWhiteSpace(tipoToken))
                return null;

            string tokenHash = TokenHelper.GenerarTokenHash(tokenPlano);
            UsuarioToken? token = _repository.GetTokenActivo(tokenHash, tipoToken);

            if (token == null || token.FechaExpiracion <= DateTime.UtcNow)
                return null;

            return token;
        }

        public Result MarcarComoUsado(int idUsuarioToken)
        {
            if (idUsuarioToken <= 0)
                return Result.Fail("El id del token debe ser mayor a cero.");

            int filasAfectadas = _repository.MarcarComoUsado(idUsuarioToken);

            return filasAfectadas > 0
                ? Result.Ok()
                : Result.Fail("No se pudo marcar el token como usado.");
        }

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

        public Result EliminarTokensObsoletos(int dias)
        {
            if (dias <= 0)
                return Result.Fail("La cantidad de días debe ser mayor a cero.");

            _repository.EliminarTokensObsoletos(dias);
            return Result.Ok();
        }

        private static Result ValidarGeneracionToken(UsuarioTokenGeneracionDto dto)
        {
            if (dto == null)
                return Result.Fail("Los datos del token no pueden ser nulos.");

            if (dto.IdUsuario <= 0)
                return Result.Fail("El id del usuario debe ser mayor a cero.");

            string tipoToken = dto.TipoToken?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(tipoToken))
                return Result.Fail("El tipo de token es obligatorio.");

            if (tipoToken.Length > 50)
                return Result.Fail("El tipo de token no puede tener más de 50 caracteres.");

            if (dto.MinutosExpiracion <= 0)
                return Result.Fail("Los minutos de expiración deben ser mayores a cero.");

            return Result.Ok();
        }
    }
}
