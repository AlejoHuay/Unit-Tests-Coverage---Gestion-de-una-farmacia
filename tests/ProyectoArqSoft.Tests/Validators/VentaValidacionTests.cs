using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

public class VentaValidacionTests
{
    private readonly VentaValidacion _validador = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_ClienteNoPositivo_DevuelveError(int id)
    {
        var venta = VentaValida();
        venta.IdCliente = id;
        ComprobarError(venta, "La venta debe tener un cliente válido.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_UsuarioNoPositivo_DevuelveError(int id)
    {
        var venta = VentaValida();
        venta.IdUsuario = id;
        ComprobarError(venta, "La venta debe tener un usuario registrador válido.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Validar_MetodoPagoAusente_DevuelveError(string? metodoPago)
    {
        var venta = VentaValida();
        venta.MetodoPago = metodoPago!;
        ComprobarError(venta, "El método de pago es obligatorio.");
    }

    [Fact]
    public void Validar_MetodoPagoDe46Caracteres_DevuelveError()
    {
        var venta = VentaValida();
        venta.MetodoPago = new string('a', 46);
        ComprobarError(venta, "El método de pago no puede exceder 45 caracteres.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(45)]
    public void Validar_MetodoPagoDentroDelLimiteTrasTrim_AceptaSinModificarVenta(int longitud)
    {
        var venta = VentaValida();
        var original = "  " + new string('a', longitud) + "  ";
        venta.MetodoPago = original;
        ComprobarExito(venta);
        Assert.Equal(original, venta.MetodoPago);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validar_DetallesNulosOVacios_DevuelveError(bool nulos)
    {
        var venta = VentaValida();
        venta.Detalles = nulos ? null! : new List<DetalleVenta>();
        ComprobarError(venta, "No se puede registrar una venta sin al menos un producto.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_MedicamentoNoPositivo_DevuelveError(int id)
    {
        var venta = VentaValida();
        venta.Detalles[0].IdMedicamento = id;
        ComprobarError(venta, "Todos los detalles deben tener un medicamento válido.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_CantidadNoPositiva_DevuelveError(int cantidad)
    {
        var venta = VentaValida();
        venta.Detalles[0].Cantidad = cantidad;
        ComprobarError(venta, "La cantidad vendida debe ser mayor a 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_PrecioNoPositivo_DevuelveError(int precio)
    {
        var venta = VentaValida();
        venta.Detalles[0].PrecioUnitario = precio;
        ComprobarError(venta, "El precio de venta debe ser mayor a 0.");
    }

    [Fact]
    public void Validar_MedicamentoRepetido_DevuelveError()
    {
        var venta = VentaValida();
        venta.Detalles.Add(DetalleValido(1));
        venta.Total = venta.Detalles.Sum(d => d.Subtotal);
        ComprobarError(venta, "Un medicamento no puede repetirse dentro de la misma venta.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_TotalNoPositivo_DevuelveError(int total)
    {
        var venta = VentaValida();
        venta.Total = total;
        ComprobarError(venta, "El total de la venta debe ser mayor a 0.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Validar_DetallesValidosConIdsDistintos_DevuelveExito(int cantidadDetalles)
    {
        var venta = VentaValida();
        venta.Detalles = Enumerable.Range(1, cantidadDetalles).Select(DetalleValido).ToList();
        venta.Total = venta.Detalles.Sum(d => d.Subtotal);
        ComprobarExito(venta);
    }

    [Theory]
    [InlineData(0, "La venta debe tener un cliente válido.")]
    [InlineData(1, "La venta debe tener un usuario registrador válido.")]
    [InlineData(2, "El método de pago es obligatorio.")]
    [InlineData(3, "No se puede registrar una venta sin al menos un producto.")]
    [InlineData(4, "El total de la venta debe ser mayor a 0.")]
    public void Validar_VariosErrores_DevuelveElPrimeroEnOrden(int validacionesPreviasValidas, string error)
    {
        var venta = VentaValida();
        if (validacionesPreviasValidas < 1) venta.IdCliente = 0;
        if (validacionesPreviasValidas < 2) venta.IdUsuario = 0;
        if (validacionesPreviasValidas < 3) venta.MetodoPago = "";
        if (validacionesPreviasValidas < 4) venta.Detalles = null!;
        venta.Total = 0;
        ComprobarError(venta, error);
    }

    private void ComprobarError(Venta venta, string error)
    {
        var resultado = _validador.Validar(venta);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(error, resultado.Error);
    }

    private void ComprobarExito(Venta venta)
    {
        var resultado = _validador.Validar(venta);
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    private static DetalleVenta DetalleValido(int id) => new()
    {
        IdMedicamento = id, Cantidad = 1, PrecioUnitario = 1m
    };

    private static Venta VentaValida() => new()
    {
        IdCliente = 1, IdUsuario = 1, MetodoPago = "Efectivo", Total = 1m,
        Detalles = new List<DetalleVenta> { DetalleValido(1) }
    };
}
