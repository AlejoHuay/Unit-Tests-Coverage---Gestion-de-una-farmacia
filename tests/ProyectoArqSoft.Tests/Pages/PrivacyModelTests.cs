using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoArqSoft.Pages;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class PrivacyModelTests
{
    [Fact]
    public void OnGet_PermiteMostrarPaginaEstaticaSinModificarLaRespuesta()
    {
        var pagina = new PrivacyModel
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
        };

        // El handler esta vacio: solo se verifica que completa sin error.
        var error = Record.Exception(pagina.OnGet);

        Assert.Null(error);
        Assert.Equal(StatusCodes.Status200OK, pagina.Response.StatusCode);
    }
}

