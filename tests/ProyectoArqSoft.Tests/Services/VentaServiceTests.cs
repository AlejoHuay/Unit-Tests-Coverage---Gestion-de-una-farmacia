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
    public void ObtenerDetallesPorVenta_ConDetalles_EnviaElIdYDevuelveLaMismaLista()
    {
        // Preparar: dos detalles conocidos para una venta especifica.
        const int idVenta = 17;
        var primerDetalle = new DetalleVenta
        {
            IdVenta = idVenta, IdMedicamento = 3, Cantidad = 2, PrecioUnitario = 12.25m
        };
        var segundoDetalle = new DetalleVenta
        {
            IdVenta = idVenta, IdMedicamento = 8, Cantidad = 3, PrecioUnitario = 5.10m
        };
        var detallesEsperados = new List<DetalleVenta> { primerDetalle, segundoDetalle };
        var repository = new Mock<IVentaRepository>(MockBehavior.Strict);
        var medicamentoRepository = new Mock<IMedicamentoRepository>(MockBehavior.Strict);
        var validador = new Mock<IResult<Venta>>(MockBehavior.Strict);
        repository.Setup(repo => repo.GetDetallesByVentaId(idVenta)).Returns(detallesEsperados);

        var servicio = new VentaService(
            repository.Object, medicamentoRepository.Object, validador.Object);

        // Actuar: recorrer el unico camino del metodo.
        var resultado = servicio.ObtenerDetallesPorVenta(idVenta);

        // Comprobar: conservar la lista, sus elementos y su orden.
        Assert.Same(detallesEsperados, resultado);
        Assert.Collection(resultado,
            detalle => Assert.Same(primerDetalle, detalle),
            detalle => Assert.Same(segundoDetalle, detalle));
        repository.Verify(repo => repo.GetDetallesByVentaId(idVenta), Times.Once);
        repository.VerifyNoOtherCalls();
        medicamentoRepository.VerifyNoOtherCalls();
        validador.VerifyNoOtherCalls();
    }

    [Fact]
    public void ObtenerPorId_ConVentaExistente_EnviaElIdYDevuelveLaMismaVenta()
    {
        // Preparar: una venta conocida y una consulta para un ID especifico.
        const int id = 17;
        var ventaEsperada = new Venta { Id = id, Total = 39.80m };
        var repository = new Mock<IVentaRepository>(MockBehavior.Strict);
        var medicamentoRepository = new Mock<IMedicamentoRepository>(MockBehavior.Strict);
        var validador = new Mock<IResult<Venta>>(MockBehavior.Strict);
        repository.Setup(repo => repo.GetById(id)).Returns(ventaEsperada);

        var servicio = new VentaService(
            repository.Object, medicamentoRepository.Object, validador.Object);

        // Actuar.
        var resultado = servicio.ObtenerPorId(id);

        // Comprobar: conservar el ID consultado y la instancia del repositorio.
        Assert.Same(ventaEsperada, resultado);
        repository.Verify(repo => repo.GetById(id), Times.Once);
        repository.VerifyNoOtherCalls();
        medicamentoRepository.VerifyNoOtherCalls();
        validador.VerifyNoOtherCalls();
    }

    [Fact]
    public void ObtenerPorId_ConVentaInexistente_DevuelveNull()
    {
        // Preparar: el repositorio indica que no encontro la venta solicitada.
        const int id = 999;
        var repository = new Mock<IVentaRepository>(MockBehavior.Strict);
        var medicamentoRepository = new Mock<IMedicamentoRepository>(MockBehavior.Strict);
        var validador = new Mock<IResult<Venta>>(MockBehavior.Strict);
        repository.Setup(repo => repo.GetById(id)).Returns((Venta?)null);

        var servicio = new VentaService(
            repository.Object, medicamentoRepository.Object, validador.Object);

        // Actuar: el mismo camino del servicio, con un resultado diferente.
        var resultado = servicio.ObtenerPorId(id);

        // Comprobar: preservar null sin crear una venta ni consultar otras fuentes.
        Assert.Null(resultado);
        repository.Verify(repo => repo.GetById(id), Times.Once);
        repository.VerifyNoOtherCalls();
        medicamentoRepository.VerifyNoOtherCalls();
        validador.VerifyNoOtherCalls();
    }

    [Fact]
    public void ObtenerTodos_ConFiltro_EnviaElFiltroYDevuelveLaTablaDelRepositorio()
    {
        // Preparar: el filtro debe llegar al repositorio sin modificaciones.
        const string filtro = "  Cliente de prueba  ";
        var tablaEsperada = new DataTable();
        tablaEsperada.Columns.Add("Id", typeof(int));
        tablaEsperada.Columns.Add("Total", typeof(decimal));
        tablaEsperada.Rows.Add(2, 24.50m);

        var repository = new Mock<IVentaRepository>(MockBehavior.Strict);
        var medicamentoRepository = new Mock<IMedicamentoRepository>(MockBehavior.Strict);
        var validador = new Mock<IResult<Venta>>(MockBehavior.Strict);
        repository.Setup(repo => repo.GetAll(filtro)).Returns(tablaEsperada);

        var servicio = new VentaService(
            repository.Object, medicamentoRepository.Object, validador.Object);

        // Actuar: invocar exclusivamente la sobrecarga que recibe el filtro.
        var resultado = servicio.ObtenerTodos(filtro);

        // Comprobar: conservar el argumento y devolver la misma tabla recibida.
        Assert.Same(tablaEsperada, resultado);
        repository.Verify(repo => repo.GetAll(filtro), Times.Once);
        repository.VerifyNoOtherCalls();
        medicamentoRepository.VerifyNoOtherCalls();
        validador.VerifyNoOtherCalls();
    }

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
