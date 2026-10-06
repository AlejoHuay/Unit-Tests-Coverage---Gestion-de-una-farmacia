using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;

namespace ProyectoArqSoft.Application.Interfaces
{
    public interface IUsuarioTokenService
    {
        Result GenerarToken(UsuarioTokenGeneracionDto dto, out string tokenPlano);
        UsuarioToken? ValidarToken(string tokenPlano, string tipoToken);
        Result MarcarComoUsado(int idUsuarioToken);
        Result RevocarTokensActivos(int idUsuario, string tipoToken);
        Result EliminarTokensObsoletos(int dias);
    }
}
