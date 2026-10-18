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

    [Fact]
    public void OnPostEliminarClasificacionLogicamente_SinSesion_RecargaSinEliminar()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        var tabla = new DataTable();
        servicio.Setup(s => s.ObtenerTodos("Natural")).Returns(tabla);
        var pagina = PrepararPagina(servicio, null);

        var resultado = pagina.OnPostEliminarClasificacionLogicamente(17);

        Assert.IsType<PageResult>(resultado);
        Assert.Equal("No se pudo identificar el usuario que realiza la operación.", pagina.Estado.MensajeError);
        Assert.Same(tabla, pagina.ClasificacionDataTable);
        servicio.Verify(s => s.ObtenerTodos("Natural"), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostEliminarClasificacionLogicamente_Rechazo_MuestraErrorYRecargaTabla()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        var tabla = new DataTable();
        servicio.Setup(s => s.EliminarLogicamente(17, 9)).Returns(Result.Fail("Tiene medicamentos asociados."));
        servicio.Setup(s => s.ObtenerTodos("Natural")).Returns(tabla);
        var pagina = PrepararPagina(servicio, 9);

        var resultado = pagina.OnPostEliminarClasificacionLogicamente(17);

        Assert.IsType<PageResult>(resultado);
        Assert.Equal("Tiene medicamentos asociados.", pagina.Estado.MensajeError);
        Assert.Same(tabla, pagina.ClasificacionDataTable);
        Assert.Equal("Natural", pagina.Estado.FiltroActual);
        servicio.Verify(s => s.EliminarLogicamente(17, 9), Times.Once);
        servicio.Verify(s => s.ObtenerTodos("Natural"), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostEliminarClasificacionLogicamente_Exito_RedirigeSinRecargar()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        servicio.Setup(s => s.EliminarLogicamente(17, 9)).Returns(Result.Ok());
        var pagina = PrepararPagina(servicio, 9);

        var resultado = Assert.IsType<RedirectToPageResult>(pagina.OnPostEliminarClasificacionLogicamente(17));

        Assert.Equal("Clasificacion", resultado.PageName);
        Assert.Equal("Clasificación eliminada correctamente", resultado.RouteValues!["mensaje"]);
        servicio.Verify(s => s.EliminarLogicamente(17, 9), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    private static ClasificacionModel PrepararPagina(Mock<IClasificacionService> servicio, int? idUsuario)
    {
        var pagina = new ClasificacionModel(servicio.Object) { PageContext = PaginaTestContext.Crear(idUsuario) };
        pagina.Estado.FiltroActual = "Natural";
        return pagina;
    }

}

