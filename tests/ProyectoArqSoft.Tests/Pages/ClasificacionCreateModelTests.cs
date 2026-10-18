using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Pages;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class ClasificacionCreateModelTests
{
    [Fact]
    public void OnGet_IniciaFormularioVacioSinConsultarServicio()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        var pagina = new ClasificacionCreateModel(servicio.Object);

        pagina.OnGet();

        Assert.Equal(string.Empty, pagina.Nombre);
        Assert.Equal(string.Empty, pagina.Origen);
        Assert.Equal(string.Empty, pagina.Descripcion);
        servicio.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnPostCrearClasificacion_SinSesion_NoLlamaAlServicio()
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        var pagina = PrepararPagina(servicio, null);

        var resultado = pagina.OnPostCrearClasificacion();

        Assert.IsType<PageResult>(resultado);
        Assert.Equal("No se pudo identificar el usuario que realiza la operación.", pagina.Estado.MensajeError);
        servicio.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnPostCrearClasificacion_ResultadoDelServicio_MuestraErrorORedirige(bool exito)
    {
        var servicio = new Mock<IClasificacionService>(MockBehavior.Strict);
        servicio.Setup(s => s.Crear("Analgesicos", "Natural", "Alivio del dolor", 9))
            .Returns(exito ? Result.Ok() : Result.Fail("Nombre repetido."));
        var pagina = PrepararPagina(servicio, 9);

        var resultado = pagina.OnPostCrearClasificacion();

        if (exito)
        {
            var redireccion = Assert.IsType<RedirectToPageResult>(resultado);
            Assert.Equal("Clasificacion", redireccion.PageName);
            Assert.Equal("Clasificación registrada correctamente", redireccion.RouteValues!["mensaje"]);
        }
        else
        {
            Assert.IsType<PageResult>(resultado);
            Assert.Equal("Nombre repetido.", pagina.Estado.MensajeError);
            Assert.Equal("Analgesicos", pagina.Nombre);
            Assert.Equal("Natural", pagina.Origen);
            Assert.Equal("Alivio del dolor", pagina.Descripcion);
        }
        servicio.Verify(s => s.Crear("Analgesicos", "Natural", "Alivio del dolor", 9), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    private static ClasificacionCreateModel PrepararPagina(Mock<IClasificacionService> servicio, int? idUsuario) =>
        new(servicio.Object)
        {
            PageContext = PaginaTestContext.Crear(idUsuario),
            Nombre = "Analgesicos",
            Origen = "Natural",
            Descripcion = "Alivio del dolor"
        };

}

