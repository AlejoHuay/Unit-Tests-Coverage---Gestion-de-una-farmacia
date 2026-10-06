using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;

namespace ProyectoArqSoft.Application.Ports.Output
{
    public interface IUsuarioTokenRepository
    {
        int Insert(UsuarioToken token);
        UsuarioToken? GetByTokenHash(string tokenHash);
        UsuarioToken? GetTokenActivo(string tokenHash, string tipoToken);
        int MarcarComoUsado(int idUsuarioToken);
        int RevocarTokensActivos(int idUsuario, string tipoToken);
        int EliminarTokensObsoletos(int dias);

    }
}
