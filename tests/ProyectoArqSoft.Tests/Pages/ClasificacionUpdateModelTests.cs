using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Pages;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class ClasificacionUpdateModelTests
{
    [Fact]
    public void OnGet_IniciaFormularioSinConsultarServicio()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        var pagina = new ClasificacionUpdateModel(servicio.Object);

        pagina.OnGet();

        Assert.Equal(0, pagina.IdClasificacion);
        Assert.Equal(string.Empty, pagina.Nombre);
        Assert.Equal(string.Empty, pagina.Origen);
        Assert.Equal(string.Empty, pagina.Descripcion);
        servicio.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostCargarClasificacionParaEdicion_Inexistente_RedirigeConError()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        servicio.Setup(s => s.ObtenerPorId(17)).Returns((Clasificacion?)null);
        var pagina = new ClasificacionUpdateModel(servicio.Object);

        var resultado = Assert.IsType<RedirectToPageResult>(pagina.OnPostCargarClasificacionParaEdicion(17));

        Assert.Equal("Clasificacion", resultado.PageName);
        Assert.Equal("Clasificación no encontrada", resultado.RouteValues!["error"]);
        servicio.Verify(s => s.ObtenerPorId(17), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostCargarClasificacionParaEdicion_Existente_CopiaDatosDelServicio()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        servicio.Setup(s => s.ObtenerPorId(17)).Returns(
            new Clasificacion("Analgesicos", "Natural", "Alivio del dolor") { Id = 17 });
        var pagina = new ClasificacionUpdateModel(servicio.Object);

        var resultado = pagina.OnPostCargarClasificacionParaEdicion(17);

        Assert.IsType<PageResult>(resultado);
        Assert.Equal(17, pagina.IdClasificacion);
        Assert.Equal("Analgesicos", pagina.Nombre);
        Assert.Equal("Natural", pagina.Origen);
        Assert.Equal("Alivio del dolor", pagina.Descripcion);
        servicio.Verify(s => s.ObtenerPorId(17), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostActualizarClasificacion_SinSesion_NoLlamaAlServicio()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        var pagina = PrepararPagina(servicio, null);

        var resultado = pagina.OnPostActualizarClasificacion();

        Assert.IsType<PageResult>(resultado);
        Assert.Equal("No se pudo identificar el usuario que realiza la operación.", pagina.Estado.MensajeError);
        servicio.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnPostActualizarClasificacion_ResultadoDelServicio_MuestraErrorORedirige(bool exito)
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        servicio.Setup(s => s.Actualizar(17, "Analgesicos", "Natural", "Alivio del dolor", 9))
            .Returns(exito ? Result.Ok() : Result.Fail("Nombre repetido."));
        var pagina = PrepararPagina(servicio, 9);

        var resultado = pagina.OnPostActualizarClasificacion();

        if (exito)
        {
            var redireccion = Assert.IsType<RedirectToPageResult>(resultado);
            Assert.Equal("Clasificacion", redireccion.PageName);
            Assert.Equal("Clasificación actualizada correctamente", redireccion.RouteValues!["mensaje"]);
        }
        else
        {
            Assert.IsType<PageResult>(resultado);
            Assert.Equal("Nombre repetido.", pagina.Estado.MensajeError);
            Assert.Equal("Analgesicos", pagina.Nombre);
            Assert.Equal("Natural", pagina.Origen);
            Assert.Equal("Alivio del dolor", pagina.Descripcion);
        }
        servicio.Verify(s => s.Actualizar(17, "Analgesicos", "Natural", "Alivio del dolor", 9), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    private static ClasificacionUpdateModel PrepararPagina(Mock<IClasificacionService> servicio, int? idUsuario) =>
        new(servicio.Object)
        {
            PageContext = PaginaTestContext.Crear(idUsuario),
            IdClasificacion = 17,
            Nombre = "Analgesicos",
            Origen = "Natural",
            Descripcion = "Alivio del dolor"
        };

}

