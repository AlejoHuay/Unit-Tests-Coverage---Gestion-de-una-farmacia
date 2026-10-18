using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Pages.Bioquimico;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class BioquimicoModelTests
{
    [Theory]
    [InlineData(null, null, null, "")]
    [InlineData("  Ana   Rojas ", "Guardado.", "Error anterior", "Ana Rojas")]
    public void OnGet_FiltroValido_ConservaSoloBioquimicosSinModificarTablaOriginal(
        string? filtro, string? mensaje, string? error, string esperado)
    {
        var tabla = CrearTablaUsuarios();
        var servicio = new Mock<IUsuarioService>(MockBehavior.Strict);
        servicio.Setup(s => s.ObtenerTodos(esperado)).Returns(tabla);
        var pagina = new BioquimicoModel(servicio.Object);

        var resultado = pagina.OnGet(filtro, mensaje, error);

        Assert.IsType<PageResult>(resultado);
        Assert.Equal(esperado, pagina.Estado.FiltroActual);
        Assert.Equal(mensaje ?? "", pagina.Estado.Mensaje);
        Assert.Equal(string.Empty, pagina.Estado.MensajeError);
        Assert.NotSame(tabla, pagina.BioquimicoDataTable);
        Assert.Equal(new[] { 17, 18 }, pagina.BioquimicoDataTable.Rows.Cast<DataRow>()
            .Select(r => (int)r["id"]).ToArray());
        Assert.Equal(new[] { "id", "role" }, pagina.BioquimicoDataTable.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName).ToArray());
        Assert.Equal(4, tabla.Rows.Count);
        servicio.Verify(s => s.ObtenerTodos(esperado), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnGet_FiltroInvalido_ConservaTablaSinConsultarServicio()
    {
        var servicio = new Mock<IUsuarioService>(MockBehavior.Strict);
        var pagina = new BioquimicoModel(servicio.Object);
        var tablaInicial = pagina.BioquimicoDataTable;

        var resultado = pagina.OnGet(" ab ", null, null);

        Assert.IsType<PageResult>(resultado);
        Assert.Equal("ab", pagina.Estado.FiltroActual);
        Assert.Equal("El criterio debe tener al menos 3 caracteres.", pagina.Estado.MensajeError);
        Assert.Same(tablaInicial, pagina.BioquimicoDataTable);
        servicio.VerifyNoOtherCalls();
    }

    private static DataTable CrearTablaUsuarios()
    {
        var tabla = new DataTable();
        tabla.Columns.Add("id", typeof(int));
        tabla.Columns.Add("role", typeof(string));
        tabla.Rows.Add(17, "Bioquimico");
        tabla.Rows.Add(18, "bIoQuImIcO");
        tabla.Rows.Add(19, "Admin");
        tabla.Rows.Add(20, DBNull.Value);
        return tabla;
    }
}

