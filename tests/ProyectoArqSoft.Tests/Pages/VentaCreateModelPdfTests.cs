using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using QuestPDF.Infrastructure;
using UglyToad.PdfPig;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

// Comparte la coleccion existente: ningun test cambia el directorio global en paralelo.
[Collection("Generacion de comprobantes PDF")]
[Trait("Category", "Integration")]
public sealed class VentaCreateModelPdfTests : IDisposable
{
    private readonly string _directorioOriginal = Directory.GetCurrentDirectory();
    private readonly CultureInfo _culturaOriginal = CultureInfo.CurrentCulture;
    private readonly LicenseType? _licenciaOriginal = QuestPDF.Settings.License;
    private readonly string _directorioTemporal = Path.Combine(Path.GetTempPath(), "VentaCreate-PdfTests", Guid.NewGuid().ToString("N"));

    public VentaCreateModelPdfTests()
    {
        Directory.CreateDirectory(_directorioTemporal);
        Directory.SetCurrentDirectory(_directorioTemporal);
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory]
    [InlineData(0, "Ana", "/Venta?mensaje=ok")]
    [InlineData(1, null, null)]
    [InlineData(2, "Luis", "/Venta?mensaje=ok")]
    public void OnPostCrearVenta_Exitosa_DevuelvePdfConDatosTotalesYHeaders(
        int escenario, string? usuario, string? urlGenerada)
    {
        var c = new VentaCreateTestContext();
        if (usuario != null) c.Page.HttpContext.Session.SetString("UserName", usuario);
        // Escenario 0: dos productos. 1: medicamento ausente. 2: nombre nulo, precio disponible.
        c.Page.DetallesJson = escenario == 0
            ? "[{\"IdMedicamento\":8,\"Cantidad\":2},{\"IdMedicamento\":9,\"Cantidad\":3}]"
            : "[{\"IdMedicamento\":8,\"Cantidad\":2}]";
        List<DetalleVentaInputDto>? recibidos = null;
        c.Facade.Setup(f => f.CrearVenta(17, 9, "Efectivo", It.IsAny<List<DetalleVentaInputDto>>()))
            .Callback<int, int, string, List<DetalleVentaInputDto>>((_, _, _, d) => recibidos = d)
            .Returns(Result.Ok());
        c.Clientes.Setup(s => s.ObtenerPorId(17)).Returns(new Cliente { Nit = "1234567", RazonSocial = "Cliente PDF" });
        c.Medicamentos.Setup(s => s.ObtenerPorId(8)).Returns(escenario == 1 ? null : new Medicamento
        {
            Id = 8, Nombre = escenario == 2 ? null! : "Paracetamol", Precio = 12.25m
        });
        if (escenario == 0)
            c.Medicamentos.Setup(s => s.ObtenerPorId(9)).Returns(new Medicamento { Id = 9, Nombre = "Ibuprofeno", Precio = 5.10m });

        var url = new Mock<IUrlHelper>(MockBehavior.Strict);
        url.SetupGet(u => u.ActionContext).Returns(c.Page.PageContext);
        UrlRouteContext? ruta = null;
        url.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Callback<UrlRouteContext>(r => ruta = r).Returns(urlGenerada);
        c.Page.Url = url.Object;

        var resultado = Assert.IsType<FileContentResult>(c.Page.OnPostCrearVenta());

        Assert.Equal("application/pdf", resultado.ContentType);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(resultado.FileContents, 0, 5));
        using var documento = PdfDocument.Open(resultado.FileContents);
        Assert.Equal(1, documento.NumberOfPages);
        string texto = Regex.Replace(documento.GetPage(1).Text, @"\s+", " ");
        string compacto = Regex.Replace(texto, @"\s+", "");
        Assert.Contains("CI / NIT: 1234567", texto);
        Assert.Contains("Cliente PDF", texto);
        Assert.Contains($"Cajero: {usuario ?? "cajero"}", texto);
        Assert.Contains("COMPROBANTE DE VENTA", texto);
        string fila = escenario switch
        {
            0 => "2Paracetamol12.2524.50",
            1 => "2Medicamento#80.000.00",
            _ => "2Medicamento#812.2524.50"
        };
        Assert.Contains(fila, compacto);
        string total = escenario switch { 0 => "39.80", 1 => "0.00", _ => "24.50" };
        Assert.Contains($"TOTAL Bs.: {total}", texto);
        if (escenario == 0) Assert.Contains("3Ibuprofeno5.1015.30", compacto);
        Assert.Equal(200, c.Page.Response.StatusCode);
        Assert.Equal("inline", c.Page.Response.Headers.ContentDisposition.ToString());
        Assert.Equal("Venta registrada correctamente.", c.Page.Response.Headers["X-Mensaje-Exito"].ToString());
        Assert.Equal(urlGenerada ?? "/Venta?mensaje=Venta%20registrada%20correctamente.", c.Page.Response.Headers["X-Redirect-To"].ToString());
        Assert.NotNull(ruta);
        var valores = new RouteValueDictionary(ruta.Values);
        Assert.Equal("/Venta/Venta", valores["page"]);
        Assert.Equal("Venta registrada correctamente.", valores["mensaje"]);
        Assert.NotNull(recibidos);
        Assert.Equal(escenario == 0 ? new[] { 8, 9 } : new[] { 8 }, recibidos.Select(d => d.IdMedicamento));
        Assert.Equal(escenario == 0 ? new[] { 2, 3 } : new[] { 2 }, recibidos.Select(d => d.Cantidad));
        c.Facade.Verify(f => f.CrearVenta(17, 9, "Efectivo", It.IsAny<List<DetalleVentaInputDto>>()), Times.Once);
        c.Clientes.Verify(s => s.ObtenerPorId(17), Times.Once);
        c.Medicamentos.Verify(s => s.ObtenerPorId(8), Times.Once);
        if (escenario == 0) c.Medicamentos.Verify(s => s.ObtenerPorId(9), Times.Once);
        url.Verify(u => u.RouteUrl(It.IsAny<UrlRouteContext>()), Times.Once);
        c.SinOtrasLlamadas();
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_directorioOriginal);
        CultureInfo.CurrentCulture = _culturaOriginal;
        QuestPDF.Settings.License = _licenciaOriginal;
        // Solo se elimina la carpeta unica creada por esta instancia dentro de Temp.
        Directory.Delete(_directorioTemporal, recursive: true);
    }
}
