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
}

