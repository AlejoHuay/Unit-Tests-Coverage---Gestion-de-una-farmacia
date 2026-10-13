using System.Data;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class VentaServiceTests
{
    [Fact]
    public void ObtenerTodos_SinFiltro_DevuelveLaTablaDelRepositorio()
    {
        // Preparar: una tabla conocida y dependencias sin acceso a base de datos.
        var tablaEsperada = new DataTable();
        tablaEsperada.Columns.Add("Id", typeof(int));
        tablaEsperada.Columns.Add("Total", typeof(decimal));
        tablaEsperada.Rows.Add(1, 39.80m);

        var repository = new Mock<IVentaRepository>(MockBehavior.Strict);
        var medicamentoRepository = new Mock<IMedicamentoRepository>(MockBehavior.Strict);
        var validador = new Mock<IResult<Venta>>(MockBehavior.Strict);
        repository.Setup(repo => repo.GetAll()).Returns(tablaEsperada);

        // Esta construccion tambien ejecuta las tres asignaciones del constructor.
        var servicio = new VentaService(
            repository.Object, medicamentoRepository.Object, validador.Object);

        // Actuar: recorrer el unico camino de ObtenerTodos() sin parametros.
        var resultado = servicio.ObtenerTodos();

        // Comprobar: el servicio entrega la misma tabla, sin sustituirla.
        Assert.Same(tablaEsperada, resultado);
        repository.Verify(repo => repo.GetAll(), Times.Once);
        repository.VerifyNoOtherCalls();
        medicamentoRepository.VerifyNoOtherCalls();
        validador.VerifyNoOtherCalls();
    }
}
