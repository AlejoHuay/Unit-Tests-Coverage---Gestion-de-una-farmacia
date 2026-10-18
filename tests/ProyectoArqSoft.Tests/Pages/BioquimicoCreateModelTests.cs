using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Pages.Bioquimico;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class BioquimicoCreateModelTests
{
    [Fact]
    public void OnGet_MuestraFormularioVacioSinLlamarAlServicio()
    {
        var servicio = new Mock<IUsuarioService>(MockBehavior.Strict);
        var pagina = new BioquimicoCreateModel(servicio.Object);

        var resultado = pagina.OnGet();

        Assert.IsType<PageResult>(resultado);
        Assert.Equal(string.Empty, pagina.Registro.Nombres);
        Assert.Equal(string.Empty, pagina.CiComplemento);
        Assert.Equal(string.Empty, pagina.MensajeError);
        Assert.Equal(string.Empty, pagina.MensajeOk);
        servicio.VerifyNoOtherCalls();
    }
}

