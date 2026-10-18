using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Models;

[Trait("Category", "Unit")]
public class MedicamentoTests
{
    [Fact]
    public void ConstructorVacio_InicializaEstadoYValoresOpcionales()
    {
        var modelo = new Medicamento();

        Assert.Equal(1, modelo.Estado);
        Assert.Equal(string.Empty, modelo.Nombre);
        Assert.Equal(string.Empty, modelo.Presentacion);
        Assert.Equal(string.Empty, modelo.Concentracion);
        Assert.Null(modelo.IdUsuario);
    }

    [Fact]
    public void ConstructorConDatos_ConservaDatosYPermiteActualizarMetadatos()
    {
        var modelo = new Medicamento("Paracetamol", "Tableta", 3, "500 mg", 12.50m, 40);

        Assert.Equal("Paracetamol", modelo.Nombre);
        Assert.Equal("Tableta", modelo.Presentacion);
        Assert.Equal(3, modelo.IdClasificacion);
        Assert.Equal("500 mg", modelo.Concentracion);
        Assert.Equal(12.50m, modelo.Precio);
        Assert.Equal(40, modelo.Stock);

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

