using ProyectoArqSoft.Pages.EstadoPaginas;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class EstadoPaginaTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanFiltroYMensajesIndependientes()
    {
        var estado = new EstadoPagina();

        Assert.Equal(string.Empty, estado.FiltroActual);
        Assert.Equal(string.Empty, estado.Mensaje);
        Assert.Equal(string.Empty, estado.MensajeError);

        estado.FiltroActual = "paracetamol";
        estado.Mensaje = "Guardado correctamente.";
        estado.MensajeError = "No se pudo eliminar.";

        Assert.Equal("paracetamol", estado.FiltroActual);
        Assert.Equal("Guardado correctamente.", estado.Mensaje);
        Assert.Equal("No se pudo eliminar.", estado.MensajeError);
    }
}

