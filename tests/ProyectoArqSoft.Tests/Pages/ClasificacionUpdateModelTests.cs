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
}

