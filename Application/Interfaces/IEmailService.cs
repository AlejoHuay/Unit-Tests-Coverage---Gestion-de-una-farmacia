using ProyectoArqSoft.Domain.Validators;

namespace ProyectoArqSoft.Application.Interfaces
{
    public interface IEmailService
    {
        Result EnviarCorreoActivacionCuenta(
            string emailDestino,
            string nombres,
            string userName,
            string passwordTemporal,
            string enlaceActivacion
        );
    }
}
