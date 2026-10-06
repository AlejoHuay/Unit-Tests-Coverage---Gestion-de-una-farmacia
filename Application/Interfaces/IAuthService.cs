using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Validators;

namespace ProyectoArqSoft.Application.Interfaces
{
    public interface IAuthService
    {
        Result IniciarSesion(UsuarioLoginRequestDto dto, out UsuarioLoginResponseDto? respuesta);
    }
}
