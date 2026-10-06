using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using System.Data;

namespace ProyectoArqSoft.Application.Interfaces
{
    public interface IClienteService
    {
        DataTable ObtenerTodos();
        DataTable ObtenerTodos(string filtro);
        Cliente? ObtenerPorId(int id);

        Result Crear(
            bool esConsumidorFinal,
            string nit,
            string razonSocial,
            string? correoElectronico,
            int idUsuario);

        Result Actualizar(
            int id,
            bool esConsumidorFinal,
            string nit,
            string razonSocial,
            string? correoElectronico,
            int idUsuario);

        Result Eliminar(int id, int idUsuario);
    }
}

