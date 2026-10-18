using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Pages;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class ClasificacionModelTests
{
    [Theory]
    [InlineData(null, null, null, "")]
    [InlineData("  Origen   Natural ", "Guardado.", "Error anterior", "Origen Natural")]
    public void OnGet_FiltroValido_NormalizaYConservaTablaDelServicio(
        string? filtro, string? mensaje, string? error, string esperado)
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        var tabla = new DataTable();
        servicio.Setup(s => s.ObtenerTodos(esperado)).Returns(tabla);
        var pagina = new ClasificacionModel(servicio.Object);

        pagina.OnGet(filtro, mensaje, error);

        Assert.Equal(esperado, pagina.Estado.FiltroActual);
        Assert.Equal(mensaje ?? string.Empty, pagina.Estado.Mensaje);
        Assert.Equal(string.Empty, pagina.Estado.MensajeError);
        Assert.Same(tabla, pagina.ClasificacionDataTable);
        servicio.Verify(s => s.ObtenerTodos(esperado), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnGet_FiltroInvalido_MuestraErrorSinConsultarServicio()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        var pagina = new ClasificacionModel(servicio.Object);
        var tablaInicial = pagina.ClasificacionDataTable;

        pagina.OnGet(" ab ", null, null);

        Assert.Equal("ab", pagina.Estado.FiltroActual);
        Assert.Equal("El criterio debe tener al menos 3 caracteres.", pagina.Estado.MensajeError);
        Assert.Same(tablaInicial, pagina.ClasificacionDataTable);
        servicio.VerifyNoOtherCalls();
    }
}

