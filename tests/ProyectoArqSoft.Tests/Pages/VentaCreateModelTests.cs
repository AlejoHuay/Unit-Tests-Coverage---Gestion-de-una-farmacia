using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class VentaCreateModelTests
{
    [Fact]
    public void OnGet_CargaAmbosCatalogosYConservaValoresIniciales()
    {
        var c = new VentaCreateTestContext();
        c.PrepararCatalogos();

        c.Page.OnGet();

        Assert.Equal("Efectivo", c.Page.MetodoPago);
        Assert.False(c.Page.ClienteModalEsConsumidorFinal);
        Assert.Equal("", c.Page.ClienteModalNit);
        Assert.Equal("", c.Page.ClienteModalRazonSocial);
        Assert.Equal("", c.Page.ClienteModalCorreoElectronico);
        c.ComprobarCatalogos();
        c.SinOtrasLlamadas();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnGetMedicamentoStock_DevuelveDatosO404(bool existe)
    {
        var c = new VentaCreateTestContext();
        c.Medicamentos.Setup(s => s.ObtenerPorId(8)).Returns(existe
            ? new Medicamento { Id = 8, Nombre = "Paracetamol", Precio = 12.25m, Stock = 4 }
            : null);

        var json = VentaCreateTestContext.Json(c.Page.OnGetMedicamentoStock(8));

        Assert.Equal(existe ? 200 : 404, c.Page.Response.StatusCode);
        Assert.Equal(existe, json.GetProperty("success").GetBoolean());
        if (existe)
        {
            var medicamento = json.GetProperty("medicamento");
            Assert.Equal(8, medicamento.GetProperty("id").GetInt32());
            Assert.Equal("Paracetamol", medicamento.GetProperty("nombre").GetString());
            Assert.Equal(12.25m, medicamento.GetProperty("precio").GetDecimal());
            Assert.Equal(4, medicamento.GetProperty("stock").GetInt32());
        }
        else Assert.Equal("El medicamento no existe o no esta activo.", json.GetProperty("error").GetString());
        c.Medicamentos.Verify(s => s.ObtenerPorId(8), Times.Once);
        c.SinOtrasLlamadas();
    }

    [Fact]
    public void OnPostCrearVenta_SinSesion_NoRegistraYRecargaCatalogos()
    {
        var c = new VentaCreateTestContext(conUsuario: false);
        ComprobarRechazo(c, "No se pudo identificar el usuario que registra la venta.");
    }

    [Theory]
    [InlineData("{")]
    [InlineData(null)]
    [InlineData("[null]")]
    [InlineData("[{\"IdMedicamento\":8,\"Cantidad\":2147483647},{\"IdMedicamento\":8,\"Cantidad\":1}]")]
    public void OnPostCrearVenta_ErrorProcesandoDetalles_NoRegistra(string? json)
    {
        var c = new VentaCreateTestContext();
        c.Page.DetallesJson = json!;
        ComprobarRechazo(c, "El detalle de la venta no tiene un formato válido.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void OnPostCrearVenta_ClienteNoPositivo_NoRegistra(int id)
    {
        var c = new VentaCreateTestContext();
        c.Page.IdCliente = id;
        ComprobarRechazo(c, "Debe seleccionar un cliente válido.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void OnPostCrearVenta_MetodoPagoAusente_NoRegistra(string? metodo)
    {
        var c = new VentaCreateTestContext();
        c.Page.MetodoPago = metodo!;
        ComprobarRechazo(c, "Debe seleccionar un método de pago.");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("[{\"IdMedicamento\":0,\"Cantidad\":2},{\"IdMedicamento\":8,\"Cantidad\":0}]")]
    public void OnPostCrearVenta_SinDetallesValidos_NoRegistra(string json)
    {
        var c = new VentaCreateTestContext();
        c.Page.DetallesJson = json;
        ComprobarRechazo(c, "Debe agregar al menos un medicamento.");
        Assert.Equal("[]", c.Page.DetallesJson);
    }

    [Fact]
    public void OnPostCrearVenta_FiltraAgrupaYPropagaRechazoDeFacade()
    {
        var c = new VentaCreateTestContext();
        c.Page.DetallesJson = """
            [{"IdMedicamento":8,"Cantidad":2,"PrecioUnitario":999},
             {"IdMedicamento":8,"Cantidad":3}, {"IdMedicamento":9,"Cantidad":1},
             {"IdMedicamento":0,"Cantidad":1}, {"IdMedicamento":10,"Cantidad":0},
             {"IdMedicamento":-1,"Cantidad":1}, {"IdMedicamento":11,"Cantidad":-1}]
            """;
        List<DetalleVentaInputDto>? recibidos = null;
        c.Facade.Setup(f => f.CrearVenta(17, 9, "Efectivo", It.IsAny<List<DetalleVentaInputDto>>()))
            .Callback<int, int, string, List<DetalleVentaInputDto>>((_, _, _, d) => recibidos = d)
            .Returns(Result.Fail("Stock insuficiente."));
        c.PrepararCatalogos();

        Assert.IsType<PageResult>(c.Page.OnPostCrearVenta());

        Assert.Equal("Stock insuficiente.", c.Page.Estado.MensajeError);
        Assert.NotNull(recibidos);
        Assert.Collection(recibidos,
            d => { Assert.Equal(8, d.IdMedicamento); Assert.Equal(5, d.Cantidad); Assert.Equal(0m, d.PrecioUnitario); },
            d => { Assert.Equal(9, d.IdMedicamento); Assert.Equal(1, d.Cantidad); });
        var serializados = JsonSerializer.Deserialize<List<DetalleVentaInputDto>>(c.Page.DetallesJson)!;
        Assert.Equal(new[] { 8, 9 }, serializados.Select(d => d.IdMedicamento));
        Assert.Equal(new[] { 5, 1 }, serializados.Select(d => d.Cantidad));
        c.Facade.Verify(f => f.CrearVenta(17, 9, "Efectivo", It.IsAny<List<DetalleVentaInputDto>>()), Times.Once);
        c.ComprobarCatalogos();
        c.SinOtrasLlamadas();
    }

    [Fact]
    public void OnPostCrearVenta_ClienteNoRecuperadoTrasRegistro_DevuelveErrorSinReintentarVenta()
    {
        var c = new VentaCreateTestContext();
        c.Facade.Setup(f => f.CrearVenta(17, 9, "Efectivo", It.IsAny<List<DetalleVentaInputDto>>())).Returns(Result.Ok());
        c.Clientes.Setup(s => s.ObtenerPorId(17)).Returns((Cliente?)null);
        c.PrepararCatalogos();

        Assert.IsType<PageResult>(c.Page.OnPostCrearVenta());

        Assert.Equal("No se pudo obtener la información del cliente para el comprobante.", c.Page.Estado.MensajeError);
        c.Facade.Verify(f => f.CrearVenta(17, 9, "Efectivo", It.IsAny<List<DetalleVentaInputDto>>()), Times.Once);
        c.Clientes.Verify(s => s.ObtenerPorId(17), Times.Once);
        c.ComprobarCatalogos();
        c.SinOtrasLlamadas();
    }

    [Fact]
    public void OnPostCrearClienteModal_SinSesion_Devuelve401SinCrear()
    {
        var c = new VentaCreateTestContext(conUsuario: false);
        var json = VentaCreateTestContext.Json(c.Page.OnPostCrearClienteModal());
        Assert.Equal(401, c.Page.Response.StatusCode);
        Assert.False(json.GetProperty("success").GetBoolean());
        Assert.Equal("No se pudo identificar el usuario que realiza la operacion.", json.GetProperty("error").GetString());
        c.SinOtrasLlamadas();
    }

    [Fact]
    public void OnPostCrearClienteModal_ServicioRechaza_Devuelve400SinBuscar()
    {
        var c = PrepararModal(false);
        c.Clientes.Setup(s => s.Crear(false, " ab-17 ", "Cliente prueba", "cliente@example.test", 9))
            .Returns(Result.Fail("NIT duplicado."));
        var json = VentaCreateTestContext.Json(c.Page.OnPostCrearClienteModal());
        Assert.Equal(400, c.Page.Response.StatusCode);
        Assert.False(json.GetProperty("success").GetBoolean());
        Assert.Equal("NIT duplicado.", json.GetProperty("error").GetString());
        VerificarCrearModal(c, false);
        c.SinOtrasLlamadas();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnPostCrearClienteModal_NoEncuentraCliente_Devuelve500(bool conFilas)
    {
        var c = PrepararModal(false);
        c.Clientes.Setup(s => s.Crear(false, " ab-17 ", "Cliente prueba", "cliente@example.test", 9)).Returns(Result.Ok());
        var tabla = VentaCreateTestContext.TablaClientes();
        if (conFilas)
        {
            tabla.Rows.Add(1, DBNull.Value, "Sin NIT");
            tabla.Rows.Add(2, "otro", "Otro cliente");
        }
        c.Clientes.Setup(s => s.ObtenerTodos(" ab-17 ")).Returns(tabla);

        var json = VentaCreateTestContext.Json(c.Page.OnPostCrearClienteModal());

        Assert.Equal(500, c.Page.Response.StatusCode);
        Assert.False(json.GetProperty("success").GetBoolean());
        Assert.Equal("El cliente fue registrado, pero no se pudo recuperar para seleccionarlo.", json.GetProperty("error").GetString());
        VerificarCrearModal(c, false);
        c.Clientes.Verify(s => s.ObtenerTodos(" ab-17 "), Times.Once);
        c.SinOtrasLlamadas();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnPostCrearClienteModal_EncuentraNitSinDistinguirEspaciosOMayusculas(bool consumidorFinal)
    {
        var c = PrepararModal(consumidorFinal);
        c.Clientes.Setup(s => s.Crear(consumidorFinal, " ab-17 ", "Cliente prueba", "cliente@example.test", 9)).Returns(Result.Ok());
        var tabla = VentaCreateTestContext.TablaClientes();
        tabla.Rows.Add(1, "otro", "No seleccionar");
        string nitFila = consumidorFinal ? " cf " : " AB-17 ";
        tabla.Rows.Add(23, nitFila, consumidorFinal ? DBNull.Value : "Cliente encontrado");
        tabla.Rows.Add(99, nitFila, "Coincidencia posterior");
        string filtro = consumidorFinal ? "CF" : " ab-17 ";
        c.Clientes.Setup(s => s.ObtenerTodos(filtro)).Returns(tabla);

        var json = VentaCreateTestContext.Json(c.Page.OnPostCrearClienteModal());

        Assert.Equal(200, c.Page.Response.StatusCode);
        Assert.True(json.GetProperty("success").GetBoolean());
        var cliente = json.GetProperty("cliente");
        Assert.Equal(23, cliente.GetProperty("id").GetInt32());
        Assert.Equal(nitFila, cliente.GetProperty("nit").GetString());
        Assert.Equal(consumidorFinal ? "" : "Cliente encontrado", cliente.GetProperty("razonSocial").GetString());
        VerificarCrearModal(c, consumidorFinal);
        c.Clientes.Verify(s => s.ObtenerTodos(filtro), Times.Once);
        c.SinOtrasLlamadas();
    }

    private static void ComprobarRechazo(VentaCreateTestContext c, string mensaje)
    {
        c.PrepararCatalogos();
        Assert.IsType<PageResult>(c.Page.OnPostCrearVenta());
        Assert.Equal(mensaje, c.Page.Estado.MensajeError);
        c.ComprobarCatalogos();
        c.SinOtrasLlamadas();
    }

    private static VentaCreateTestContext PrepararModal(bool consumidorFinal)
    {
        var c = new VentaCreateTestContext();
        c.Page.ClienteModalEsConsumidorFinal = consumidorFinal;
        c.Page.ClienteModalNit = " ab-17 ";
        c.Page.ClienteModalRazonSocial = "Cliente prueba";
        c.Page.ClienteModalCorreoElectronico = "cliente@example.test";
        return c;
    }

    private static void VerificarCrearModal(VentaCreateTestContext c, bool consumidorFinal) =>
        c.Clientes.Verify(s => s.Crear(consumidorFinal, " ab-17 ", "Cliente prueba", "cliente@example.test", 9), Times.Once);
}
