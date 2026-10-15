using System.Data;
using Moq;
using ProyectoArqSoft.Application.Facades;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Facades;

[Trait("Category", "Unit")]
public class VentaFacadeTests
{
    private readonly Mock<IVentaService> _ventas = new(MockBehavior.Strict);
    private readonly Mock<IVentaService> _anulaciones = new(MockBehavior.Strict);
    private readonly Mock<IMedicamentoService> _medicamentos = new(MockBehavior.Strict);
    private readonly Mock<IClienteService> _clientes = new(MockBehavior.Strict);
    private readonly VentaFacade _facade;

    public VentaFacadeTests()
    {
        // Las fachadas concretas no son virtuales: se mantienen reales y se
        // simulan sus servicios. La anulacion usa otro mock para detectar rutas incorrectas.
        _facade = new VentaFacade(
            new FachadaVenta(_ventas.Object),
            new FachadaAnular(_anulaciones.Object),
            new FachadaActualizarStock(_medicamentos.Object),
            _clientes.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  Efectivo  ")]
    public void ObtenerVentas_EnviaFiltroSinModificarYDevuelveLaMismaTabla(string filtro)
    {
        var esperado = new DataTable();
        _ventas.Setup(s => s.ObtenerTodos(filtro)).Returns(esperado);

        var resultado = _facade.ObtenerVentas(filtro);

        Assert.Same(esperado, resultado);
        _ventas.Verify(s => s.ObtenerTodos(filtro), Times.Once);
        SinOtrasLlamadas();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ObtenerVentaPorId_EnviaIdYConservaVentaONull(bool existe)
    {
        Venta? esperado = existe ? new Venta { Id = 17, Total = 24.50m } : null;
        _ventas.Setup(s => s.ObtenerPorId(17)).Returns(esperado);

        var resultado = _facade.ObtenerVentaPorId(17);

        Assert.Same(esperado, resultado);
        _ventas.Verify(s => s.ObtenerPorId(17), Times.Once);
        SinOtrasLlamadas();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ObtenerDetalles_EnviaIdYDevuelveLaMismaLista(bool conDetalles)
    {
        var esperado = new List<DetalleVenta>();
        if (conDetalles)
            esperado.Add(new DetalleVenta { IdVenta = 17, IdMedicamento = 8, Cantidad = 2, PrecioUnitario = 12.25m });
        _ventas.Setup(s => s.ObtenerDetallesPorVenta(17)).Returns(esperado);

        var resultado = _facade.ObtenerDetalles(17);

        Assert.Same(esperado, resultado);
        Assert.Equal(conDetalles ? 1 : 0, resultado.Count);
        _ventas.Verify(s => s.ObtenerDetallesPorVenta(17), Times.Once);
        SinOtrasLlamadas();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CrearVenta_EnviaArgumentosYDevuelveElMismoResultado(bool exito)
    {
        var detalles = DetallesEntrada();
        var esperado = exito ? Result.Ok() : Result.Fail("Stock insuficiente.");
        _ventas.Setup(s => s.Crear(17, 9, "  Efectivo  ",
                It.Is<List<DetalleVentaInputDto>>(d => ReferenceEquals(d, detalles))))
            .Returns(esperado);

        var resultado = _facade.CrearVenta(17, 9, "  Efectivo  ", detalles);

        Assert.Same(esperado, resultado);
        ComprobarDetallesSinCambios(detalles);
        _ventas.Verify(s => s.Crear(17, 9, "  Efectivo  ",
            It.Is<List<DetalleVentaInputDto>>(d => ReferenceEquals(d, detalles))), Times.Once);
        SinOtrasLlamadas();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ActualizarVenta_EnviaIdsDistintosYDevuelveElMismoResultado(bool exito)
    {
        var detalles = DetallesEntrada();
        var esperado = exito ? Result.Ok() : Result.Fail("La venta no existe.");
        _ventas.Setup(s => s.Actualizar(31, 17, "Tarjeta",
                It.Is<List<DetalleVentaInputDto>>(d => ReferenceEquals(d, detalles)), 9))
            .Returns(esperado);

        var resultado = _facade.ActualizarVenta(31, 17, "Tarjeta", detalles, 9);

        Assert.Same(esperado, resultado);
        ComprobarDetallesSinCambios(detalles);
        _ventas.Verify(s => s.Actualizar(31, 17, "Tarjeta",
            It.Is<List<DetalleVentaInputDto>>(d => ReferenceEquals(d, detalles)), 9), Times.Once);
        SinOtrasLlamadas();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AnularVenta_UsaFachadaAnularYConservaResultado(bool exito)
    {
        var esperado = exito ? Result.Ok() : Result.Fail("La venta ya fue anulada.");
        _anulaciones.Setup(s => s.EliminarLogicamente(31, 9)).Returns(esperado);

        var resultado = _facade.AnularVenta(31, 9);

        Assert.Same(esperado, resultado);
        _anulaciones.Verify(s => s.EliminarLogicamente(31, 9), Times.Once);
        SinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerClientes_ConsultaServicioYDevuelveLaMismaTabla()
    {
        var esperado = new DataTable();
        _clientes.Setup(s => s.ObtenerTodos()).Returns(esperado);

        var resultado = _facade.ObtenerClientes();

        Assert.Same(esperado, resultado);
        _clientes.Verify(s => s.ObtenerTodos(), Times.Once);
        SinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerMedicamentos_UsaFachadaStockYDevuelveLaMismaTabla()
    {
        var esperado = new DataTable();
        _medicamentos.Setup(s => s.ObtenerTodos()).Returns(esperado);

        var resultado = _facade.ObtenerMedicamentos();

        Assert.Same(esperado, resultado);
        _medicamentos.Verify(s => s.ObtenerTodos(), Times.Once);
        SinOtrasLlamadas();
    }

    private static List<DetalleVentaInputDto> DetallesEntrada() => new()
    {
        new DetalleVentaInputDto { IdMedicamento = 8, Cantidad = 2, PrecioUnitario = 12.25m }
    };

    private static void ComprobarDetallesSinCambios(List<DetalleVentaInputDto> detalles)
    {
        var detalle = Assert.Single(detalles);
        Assert.Equal(8, detalle.IdMedicamento);
        Assert.Equal(2, detalle.Cantidad);
        Assert.Equal(12.25m, detalle.PrecioUnitario);
    }

    private void SinOtrasLlamadas()
    {
        _ventas.VerifyNoOtherCalls();
        _anulaciones.VerifyNoOtherCalls();
        _medicamentos.VerifyNoOtherCalls();
        _clientes.VerifyNoOtherCalls();
    }
}
