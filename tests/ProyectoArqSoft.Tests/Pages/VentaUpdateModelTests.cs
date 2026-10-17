using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Pages;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class VentaUpdateModelTests
{
    private readonly Mock<IVentaFacade> _facade = new(MockBehavior.Strict);
    private readonly DataTable _clientes = new();
    private readonly DataTable _medicamentos = new();

    [Fact]
    public void OnGet_CargaCatalogosYConservaValoresIniciales()
    {
        var pagina = new VentaUpdateModel(_facade.Object);
        PrepararCatalogos();
        pagina.OnGet();
        Assert.Equal(0, pagina.IdVenta);
        Assert.Equal(0, pagina.IdCliente);
        Assert.Equal("Efectivo", pagina.MetodoPago);
        Assert.Equal("[]", pagina.DetallesJson);
        ComprobarCatalogos(pagina);
        _facade.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostCargarVenta_Inexistente_RedirigeSinConsultarDetallesNiCatalogos()
    {
        var pagina = new VentaUpdateModel(_facade.Object);
        _facade.Setup(f => f.ObtenerVentaPorId(31)).Returns((Venta?)null);
        var resultado = Assert.IsType<RedirectToPageResult>(pagina.OnPostCargarVenta(31));
        Assert.Equal("Venta", resultado.PageName);
        Assert.Equal("Venta no encontrada.", resultado.RouteValues!["error"]);
        _facade.Verify(f => f.ObtenerVentaPorId(31), Times.Once);
        _facade.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, "Efectivo")]
    [InlineData("", "Efectivo")]
    [InlineData(" \t ", "Efectivo")]
    [InlineData(" Tarjeta ", " Tarjeta ")]
    public void OnPostCargarVenta_Existente_CopiaDatosYSerializaDetalles(string? metodo, string esperado)
    {
        var pagina = new VentaUpdateModel(_facade.Object);
        _facade.Setup(f => f.ObtenerVentaPorId(31)).Returns(new Venta { Id = 31, IdCliente = 17, MetodoPago = metodo! });
        _facade.Setup(f => f.ObtenerDetalles(31)).Returns(new List<DetalleVenta>
        {
            new() { IdVenta = 31, IdMedicamento = 8, Cantidad = 2, PrecioUnitario = 12.25m },
            new() { IdVenta = 31, IdMedicamento = 6, Cantidad = 3, PrecioUnitario = 5.10m }
        });
        PrepararCatalogos();

        Assert.IsType<PageResult>(pagina.OnPostCargarVenta(31));

        Assert.Equal(31, pagina.IdVenta);
        Assert.Equal(17, pagina.IdCliente);
        Assert.Equal(esperado, pagina.MetodoPago);
        var detalles = JsonSerializer.Deserialize<List<DetalleVentaDto>>(pagina.DetallesJson)!;
        Assert.Collection(detalles,
            d => { Assert.Equal(8, d.IdMedicamento); Assert.Equal(2, d.Cantidad); Assert.Equal(12.25m, d.PrecioUnitario); },
            d => { Assert.Equal(6, d.IdMedicamento); Assert.Equal(3, d.Cantidad); Assert.Equal(5.10m, d.PrecioUnitario); });
        _facade.Verify(f => f.ObtenerVentaPorId(31), Times.Once);
        _facade.Verify(f => f.ObtenerDetalles(31), Times.Once);
        ComprobarCatalogos(pagina);
        _facade.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostCargarVenta_SinDetalles_SerializaListaVacia()
    {
        var pagina = new VentaUpdateModel(_facade.Object);
        _facade.Setup(f => f.ObtenerVentaPorId(31)).Returns(new Venta { Id = 31, IdCliente = 17, MetodoPago = "Efectivo" });
        _facade.Setup(f => f.ObtenerDetalles(31)).Returns(new List<DetalleVenta>());
        PrepararCatalogos();
        Assert.IsType<PageResult>(pagina.OnPostCargarVenta(31));
        Assert.Equal("[]", pagina.DetallesJson);
        _facade.Verify(f => f.ObtenerVentaPorId(31), Times.Once);
        _facade.Verify(f => f.ObtenerDetalles(31), Times.Once);
        ComprobarCatalogos(pagina);
        _facade.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostActualizarVenta_SinSesion_RedirigeAlLoginSinLlamarFacade()
    {
        var pagina = PrepararPagina(false);
        var resultado = Assert.IsType<RedirectToPageResult>(pagina.OnPostActualizarVenta());
        Assert.Equal("/Auth/Login", resultado.PageName);
        _facade.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OnPostActualizarVenta_EnviaDatosYProcesaResultado(bool exito)
    {
        var pagina = PrepararPagina();
        List<DetalleVentaInputDto>? recibidos = null;
        _facade.Setup(f => f.ActualizarVenta(31, 17, "Tarjeta", It.IsAny<List<DetalleVentaInputDto>>(), 9))
            .Callback<int, int, string, List<DetalleVentaInputDto>, int>((_, _, _, d, _) => recibidos = d)
            .Returns(exito ? Result.Ok() : Result.Fail("Stock insuficiente."));
        if (!exito) PrepararCatalogos();

        var resultado = pagina.OnPostActualizarVenta();

        var detalle = Assert.Single(recibidos!);
        Assert.Equal(8, detalle.IdMedicamento);
        Assert.Equal(2, detalle.Cantidad);
        Assert.Equal(12.25m, detalle.PrecioUnitario);
        if (exito)
        {
            var redireccion = Assert.IsType<RedirectToPageResult>(resultado);
            Assert.Equal("Venta", redireccion.PageName);
            Assert.Equal("Venta actualizada correctamente.", redireccion.RouteValues!["mensaje"]);
        }
        else
        {
            Assert.IsType<PageResult>(resultado);
            Assert.Equal("Stock insuficiente.", pagina.Estado.MensajeError);
            ComprobarCatalogos(pagina);
        }
        VerificarActualizacion();
        _facade.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    public void OnPostActualizarVenta_ListaNulaOVacia_DelegaListaVacia(string json)
    {
        var pagina = PrepararPagina();
        pagina.DetallesJson = json;
        _facade.Setup(f => f.ActualizarVenta(31, 17, "Tarjeta",
            It.Is<List<DetalleVentaInputDto>>(d => d != null && d.Count == 0), 9))
            .Returns(Result.Fail("Debe agregar al menos un medicamento."));
        PrepararCatalogos();
        Assert.IsType<PageResult>(pagina.OnPostActualizarVenta());
        Assert.Equal("Debe agregar al menos un medicamento.", pagina.Estado.MensajeError);
        VerificarActualizacion();
        ComprobarCatalogos(pagina);
        _facade.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("{")]
    [InlineData("[{\"Cantidad\":\"no numerica\"}]")]
    public void OnPostActualizarVenta_JsonInvalido_MuestraErrorSinActualizar(string json)
    {
        var pagina = PrepararPagina();
        pagina.DetallesJson = json;
        PrepararCatalogos();
        Assert.IsType<PageResult>(pagina.OnPostActualizarVenta());
        Assert.Equal("El detalle de la venta no es válido.", pagina.Estado.MensajeError);
        ComprobarCatalogos(pagina);
        _facade.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostActualizarVenta_JsonExceptionDesdeFacade_TambienEsCapturada()
    {
        var pagina = PrepararPagina();
        _facade.Setup(f => f.ActualizarVenta(31, 17, "Tarjeta", It.IsAny<List<DetalleVentaInputDto>>(), 9))
            .Throws(new JsonException("Error de la dependencia."));
        PrepararCatalogos();
        Assert.IsType<PageResult>(pagina.OnPostActualizarVenta());
        Assert.Equal("El detalle de la venta no es válido.", pagina.Estado.MensajeError);
        VerificarActualizacion();
        ComprobarCatalogos(pagina);
        _facade.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostActualizarVenta_JsonExceptionAlRecargarCatalogos_EntraAlCatch()
    {
        var pagina = PrepararPagina();
        _facade.Setup(f => f.ActualizarVenta(31, 17, "Tarjeta", It.IsAny<List<DetalleVentaInputDto>>(), 9))
            .Returns(Result.Fail("Rechazo inicial."));
        _facade.SetupSequence(f => f.ObtenerClientes()).Throws(new JsonException()).Returns(_clientes);
        _facade.Setup(f => f.ObtenerMedicamentos()).Returns(_medicamentos);
        Assert.IsType<PageResult>(pagina.OnPostActualizarVenta());
        Assert.Equal("El detalle de la venta no es válido.", pagina.Estado.MensajeError);
        Assert.Same(_clientes, pagina.ClienteDataTable);
        Assert.Same(_medicamentos, pagina.MedicamentoDataTable);
        VerificarActualizacion();
        _facade.Verify(f => f.ObtenerClientes(), Times.Exactly(2));
        _facade.Verify(f => f.ObtenerMedicamentos(), Times.Once);
        _facade.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostActualizarVenta_ExcepcionDistintaDeJson_SePropaga()
    {
        var pagina = PrepararPagina();
        var esperado = new InvalidOperationException("Fallo de la dependencia.");
        _facade.Setup(f => f.ActualizarVenta(31, 17, "Tarjeta", It.IsAny<List<DetalleVentaInputDto>>(), 9)).Throws(esperado);
        Assert.Same(esperado, Assert.Throws<InvalidOperationException>(() => pagina.OnPostActualizarVenta()));
        VerificarActualizacion();
        _facade.VerifyNoOtherCalls();
    }

    private void PrepararCatalogos()
    {
        _facade.Setup(f => f.ObtenerClientes()).Returns(_clientes);
        _facade.Setup(f => f.ObtenerMedicamentos()).Returns(_medicamentos);
    }

    private void ComprobarCatalogos(VentaUpdateModel pagina)
    {
        Assert.Same(_clientes, pagina.ClienteDataTable);
        Assert.Same(_medicamentos, pagina.MedicamentoDataTable);
        _facade.Verify(f => f.ObtenerClientes(), Times.Once);
        _facade.Verify(f => f.ObtenerMedicamentos(), Times.Once);
    }

    private void VerificarActualizacion() => _facade.Verify(f => f.ActualizarVenta(
        31, 17, "Tarjeta", It.IsAny<List<DetalleVentaInputDto>>(), 9), Times.Once);

    private VentaUpdateModel PrepararPagina(bool conUsuario = true)
    {
        var sesion = new Mock<ISession>();
        byte[]? valor = null;
        sesion.Setup(s => s.Set("IdUsuario", It.IsAny<byte[]>())).Callback<string, byte[]>((_, d) => valor = d);
        if (conUsuario) sesion.Object.SetInt32("IdUsuario", 9);
        sesion.Setup(s => s.TryGetValue("IdUsuario", out valor)).Returns(conUsuario);
        return new VentaUpdateModel(_facade.Object)
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext { Session = sesion.Object } },
            IdVenta = 31, IdCliente = 17, MetodoPago = "Tarjeta",
            DetallesJson = "[{\"IdMedicamento\":8,\"Cantidad\":2,\"PrecioUnitario\":12.25}]"
        };
    }
}
