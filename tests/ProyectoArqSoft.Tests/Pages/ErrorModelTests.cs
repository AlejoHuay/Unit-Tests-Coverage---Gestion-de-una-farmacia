using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoArqSoft.Pages;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class ErrorModelTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("solicitud-17", true)]
    public void ShowRequestId_IndicaSiHayIdentificador(string? identificador, bool esperado)
    {
        var pagina = new ErrorModel { RequestId = identificador };

        Assert.Equal(esperado, pagina.ShowRequestId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnGet_PriorizaActividadOUsaIdentificadorHttp(bool conActividad)
    {
        var actividadAnterior = Activity.Current;
        Activity.Current = null;
        try
        {
            using var actividad = conActividad ? new Activity("prueba-error").Start() : null;
            var pagina = new ErrorModel
            {
                PageContext = new PageContext
                {
                    HttpContext = new DefaultHttpContext { TraceIdentifier = "solicitud-http" }
                }
            };
            string esperado = conActividad ? actividad!.Id! : "solicitud-http";

            pagina.OnGet();

            Assert.Equal(esperado, pagina.RequestId);
            Assert.True(pagina.ShowRequestId);
        }
        finally
        {
            Activity.Current = actividadAnterior;
        }
    }
}

