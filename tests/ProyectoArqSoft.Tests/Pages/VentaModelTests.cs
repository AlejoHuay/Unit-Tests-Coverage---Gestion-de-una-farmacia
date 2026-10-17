using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Pages;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class VentaModelTests
{
    [Theory]
    [InlineData(null, null, null, "")]
    [InlineData("", "Venta registrada.", null, "")]
    [InlineData("   ", null, "Error anterior", "")]
    [InlineData("  Venta   123  ", "Venta registrada.", "Error anterior", "Venta 123")]
    [InlineData("abc", null, null, "abc")]
    public void OnGet_FiltroValido_LimpiaFiltroYConservaTablaDelServicio(
        string? filtro, string? mensaje, string? error, string filtroEsperado)
    {
        ComprobarConsultaValida(filtro, mensaje, error, filtroEsperado);
    }

    [Fact]
    public void OnGet_FiltroDe45Caracteres_ConsultaVentas()
    {
        var filtro = new string('a', 45);
        ComprobarConsultaValida(filtro, null, null, filtro);
    }

    [Theory]
    [InlineData("ab", "El criterio debe tener al menos 3 caracteres.")]
    [InlineData("abc@", "Criterio inválido.")]
    public void OnGet_FiltroInvalido_NoConsultaYConservaTablaInicial(string filtro, string error)
    {
        ComprobarFiltroInvalido(filtro, error);
    }

    [Fact]
    public void OnGet_FiltroDe46Caracteres_NoConsultaVentas()
    {
        ComprobarFiltroInvalido(new string('a', 46), "El criterio no puede tener más de 45 caracteres.");
    }

    [Fact]
    public void OnPostEliminarVentaLogicamente_SinSesion_RecargaVentasSinAnular()
    {
        var facade = new Mock<IVentaFacade>(MockBehavior.Strict);
        var pagina = PrepararPagina(facade, null);
        var tabla = new DataTable();
        facade.Setup(f => f.ObtenerVentas("Efectivo")).Returns(tabla);

        var resultado = pagina.OnPostEliminarVentaLogicamente(17);

        Assert.IsType<PageResult>(resultado);
        Assert.Equal("No se pudo identificar el usuario.", pagina.Estado.MensajeError);
        Assert.Same(tabla, pagina.VentaDataTable);
        Assert.Equal("Efectivo", pagina.Estado.FiltroActual);
        facade.Verify(f => f.ObtenerVentas("Efectivo"), Times.Once);
        facade.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostEliminarVentaLogicamente_Rechazo_PropagaErrorYRecargaVentas()
    {
        var facade = new Mock<IVentaFacade>(MockBehavior.Strict);
        var pagina = PrepararPagina(facade, 9);
        var tabla = new DataTable();
        facade.Setup(f => f.AnularVenta(17, 9)).Returns(Result.Fail("La venta ya fue anulada."));
        facade.Setup(f => f.ObtenerVentas("Efectivo")).Returns(tabla);

        var resultado = pagina.OnPostEliminarVentaLogicamente(17);

        Assert.IsType<PageResult>(resultado);
        Assert.Equal("La venta ya fue anulada.", pagina.Estado.MensajeError);
        Assert.Same(tabla, pagina.VentaDataTable);
        Assert.Equal("Efectivo", pagina.Estado.FiltroActual);
        facade.Verify(f => f.AnularVenta(17, 9), Times.Once);
        facade.Verify(f => f.ObtenerVentas("Efectivo"), Times.Once);
        facade.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostEliminarVentaLogicamente_Exito_RedirigeConMensajeSinConsultarVentas()
    {
        var facade = new Mock<IVentaFacade>(MockBehavior.Strict);
        var pagina = PrepararPagina(facade, 9);
        facade.Setup(f => f.AnularVenta(17, 9)).Returns(Result.Ok());

        var resultado = Assert.IsType<RedirectToPageResult>(pagina.OnPostEliminarVentaLogicamente(17));

        Assert.Equal("Venta", resultado.PageName);
        Assert.NotNull(resultado.RouteValues);
        Assert.Equal("Venta anulada correctamente.", resultado.RouteValues["mensaje"]);
        Assert.False(resultado.Permanent);
        facade.Verify(f => f.AnularVenta(17, 9), Times.Once);
        facade.VerifyNoOtherCalls();
    }

    private static void ComprobarConsultaValida(string? filtro, string? mensaje, string? error, string filtroEsperado)
    {
        var facade = new Mock<IVentaFacade>(MockBehavior.Strict);
        var tabla = new DataTable();
        tabla.Columns.Add("id", typeof(int));
        tabla.Rows.Add(17);
        facade.Setup(f => f.ObtenerVentas(filtroEsperado)).Returns(tabla);
        var pagina = new VentaModel(facade.Object);

        pagina.OnGet(filtro, mensaje, error);

        Assert.Equal(filtroEsperado, pagina.Estado.FiltroActual);
        Assert.Equal(mensaje ?? "", pagina.Estado.Mensaje);
        // Comportamiento actual: la validacion exitosa sobrescribe el error recibido.
        Assert.Equal("", pagina.Estado.MensajeError);
        Assert.Same(tabla, pagina.VentaDataTable);
        facade.Verify(f => f.ObtenerVentas(filtroEsperado), Times.Once);
        facade.VerifyNoOtherCalls();
    }

    private static void ComprobarFiltroInvalido(string filtro, string errorEsperado)
    {
        var facade = new Mock<IVentaFacade>(MockBehavior.Strict);
        var pagina = new VentaModel(facade.Object);
        var tablaInicial = pagina.VentaDataTable;

        pagina.OnGet(filtro, "Mensaje conservado", "Error anterior");

        Assert.Equal(errorEsperado, pagina.Estado.MensajeError);
        Assert.Equal("Mensaje conservado", pagina.Estado.Mensaje);
        Assert.Same(tablaInicial, pagina.VentaDataTable);
        Assert.Empty(pagina.VentaDataTable.Rows.Cast<DataRow>());
        facade.VerifyNoOtherCalls();
    }

    private static VentaModel PrepararPagina(Mock<IVentaFacade> facade, int? idUsuario)
    {
        var sesion = new Mock<ISession>();
        byte[] valor = null!;
        sesion.Setup(s => s.Set("IdUsuario", It.IsAny<byte[]>()))
            .Callback<string, byte[]>((_, datos) => valor = datos);
        // Usar la extension real para codificar el entero como lo hace ASP.NET Core.
        if (idUsuario.HasValue) sesion.Object.SetInt32("IdUsuario", idUsuario.Value);
        sesion.Setup(s => s.TryGetValue("IdUsuario", out valor)).Returns(idUsuario.HasValue);
        var pagina = new VentaModel(facade.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { Session = sesion.Object }
            }
        };
        pagina.Estado.FiltroActual = "Efectivo";
        return pagina;
    }
}
