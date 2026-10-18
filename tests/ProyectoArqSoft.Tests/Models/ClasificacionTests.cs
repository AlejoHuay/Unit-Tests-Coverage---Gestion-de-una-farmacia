using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Models;

[Trait("Category", "Unit")]
public class ClasificacionTests
{
    [Fact]
    public void ConstructorVacio_InicializaEstadoYValoresOpcionales()
    {
        var modelo = new Clasificacion();

        Assert.Equal(1, modelo.Estado);
        Assert.Equal(string.Empty, modelo.Nombre);
        Assert.Equal(string.Empty, modelo.Origen);
        Assert.Equal(string.Empty, modelo.Descripcion);
        Assert.Null(modelo.UltimaActualizacion);
        Assert.Null(modelo.IdUsuario);
    }

    [Fact]
    public void ConstructorConDatos_ConservaDatosYPermiteActualizarMetadatos()
    {
        var modelo = new Clasificacion("Analgesicos", "Natural", "Alivio del dolor");

        Assert.Equal("Analgesicos", modelo.Nombre);
        Assert.Equal("Natural", modelo.Origen);
        Assert.Equal("Alivio del dolor", modelo.Descripcion);

        modelo.Id = 7;
        modelo.Estado = 0;
        modelo.FechaRegistro = new DateTime(2026, 1, 10);
        modelo.UltimaActualizacion = new DateTime(2026, 1, 11);
        modelo.IdUsuario = 9;

        Assert.Equal(7, modelo.Id);
        Assert.Equal(0, modelo.Estado);
        Assert.Equal(new DateTime(2026, 1, 10), modelo.FechaRegistro);
        Assert.Equal(new DateTime(2026, 1, 11), modelo.UltimaActualizacion);
        Assert.Equal(9, modelo.IdUsuario);
    }
}

