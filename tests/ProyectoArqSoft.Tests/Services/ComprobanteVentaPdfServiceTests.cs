using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.DTOs;
using QuestPDF.Infrastructure;
using UglyToad.PdfPig;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

// El directorio actual y la licencia de QuestPDF son estado global del proceso.
// Esta coleccion no se ejecuta en paralelo con ninguna otra coleccion.
[CollectionDefinition("Generacion de comprobantes PDF", DisableParallelization = true)]
public class ComprobantesPdfCollection;

[Collection("Generacion de comprobantes PDF")]
[Trait("Category", "Integration")]
public sealed class ComprobanteVentaPdfServiceTests : IDisposable
{
    private readonly string _directorioOriginal = Directory.GetCurrentDirectory();
    private readonly CultureInfo _culturaOriginal = CultureInfo.CurrentCulture;
    private readonly LicenseType? _licenciaOriginal = QuestPDF.Settings.License;
    private readonly string _directorioTemporal = Path.Combine(
        Path.GetTempPath(), "VitalCare-PdfTests", Guid.NewGuid().ToString("N"));

    public ComprobanteVentaPdfServiceTests()
    {
        Directory.CreateDirectory(_directorioTemporal);
        Directory.SetCurrentDirectory(_directorioTemporal);
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public void Generar_SegunLogoYDetalles_ProduceComprobanteConContenidoCorrecto(
        bool conLogo, int cantidadDetalles)
    {
        // Preparar: controlar File.Exists sin modificar los archivos de la web.
        if (conLogo)
        {
            var carpetaImagenes = Path.Combine(_directorioTemporal, "wwwroot", "images");
            Directory.CreateDirectory(carpetaImagenes);
            File.Copy(Path.Combine(AppContext.BaseDirectory, "TestAssets", "vitalcare.png"),
                Path.Combine(carpetaImagenes, "vitalcare.png"));
        }

        var comprobante = new ComprobanteVentaPdfDto
        {
            Fecha = new DateTime(2026, 10, 10, 14, 30, 45),
            Nit = "1234567012",
            RazonSocial = "Cliente de prueba",
            Cajero = "Cajero de prueba"
        };
        if (cantidadDetalles >= 1)
            comprobante.Detalles.Add(new ComprobanteVentaDetallePdfDto
            {
                Cantidad = 2, Descripcion = "Paracetamol 500 mg", PrecioUnitario = 12.25m
            });
        if (cantidadDetalles >= 2)
            comprobante.Detalles.Add(new ComprobanteVentaDetallePdfDto
            {
                Cantidad = 3, Descripcion = "Ibuprofeno 400 mg", PrecioUnitario = 5.10m
            });

        // Oraculos literales independientes del helper y del calculo del servicio.
        comprobante.Total = cantidadDetalles switch { 0 => 0m, 1 => 24.50m, _ => 39.80m };
        var totalEsperado = cantidadDetalles switch { 0 => "0.00", 1 => "24.50", _ => "39.80" };
        var literalEsperado = cantidadDetalles switch
        {
            0 => "Son cero 00/100 Bolivianos",
            1 => "Son veinte y cuatro 50/100 Bolivianos",
            _ => "Son treinta y nueve 80/100 Bolivianos"
        };

        // Actuar: ejecutar el servicio real y el renderizador QuestPDF.
        byte[] bytes = ComprobanteVentaPdfService.Generar(comprobante);

        // Comprobar estructura, tamano A4 y contenido, no solo bytes no vacios.
        Assert.True(bytes.Length > 5);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
        using var pdf = PdfDocument.Open(bytes);
        Assert.Equal(1, pdf.NumberOfPages);
        var pagina = pdf.GetPage(1);
        Assert.InRange((double)pagina.Width, 594, 597);
        Assert.InRange((double)pagina.Height, 840, 843);
        var texto = Regex.Replace(pagina.Text, @"\s+", " ");

        Assert.Contains("Farmacia VitalCare", texto);
        Assert.Contains("COMPROBANTE DE VENTA", texto);
        Assert.Contains("Fecha: 10/10/2026 14:30:45", texto);
        Assert.Contains("CI / NIT: 1234567012", texto);
        Assert.Contains("Cliente de prueba", texto);
        Assert.Contains("Cajero: Cajero de prueba", texto);
        Assert.Contains("Cant.", texto);
        Assert.Contains("P. Unit.", texto);
        Assert.Contains("Importe", texto);
        Assert.Contains($"TOTAL Bs.: {totalEsperado}", texto);
        Assert.Contains(literalEsperado, texto);
        Assert.Contains("Gracias por su compra.", texto);

        if (conLogo)
        {
            Assert.NotEmpty(pagina.GetImages());
            Assert.DoesNotContain("LOGO", texto);
        }
        else
        {
            Assert.Empty(pagina.GetImages());
            Assert.Contains("LOGO", texto);
        }

        // Verificar cada fila completa, incluyendo cantidad, precio e importe.
        var textoCompacto = Regex.Replace(texto, @"\s+", "");
        if (cantidadDetalles >= 1)
            Assert.Contains("2Paracetamol500mg12.2524.50", textoCompacto);
        else
            Assert.DoesNotContain("Paracetamol", texto);
        if (cantidadDetalles >= 2)
            Assert.Contains("3Ibuprofeno400mg5.1015.30", textoCompacto);
        else
            Assert.DoesNotContain("Ibuprofeno", texto);

        // Evidencia opcional para inspeccion visual, fuera del codigo fuente.
        var evidencia = Environment.GetEnvironmentVariable("COMPROBANTE_PDF_EVIDENCE_DIR");
        if (!string.IsNullOrWhiteSpace(evidencia))
        {
            Directory.CreateDirectory(evidencia);
            File.WriteAllBytes(Path.Combine(evidencia,
                $"comprobante-logo-{conLogo}-detalles-{cantidadDetalles}.pdf"), bytes);
        }
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_directorioOriginal);
        CultureInfo.CurrentCulture = _culturaOriginal;
        QuestPDF.Settings.License = _licenciaOriginal;
        // El destino es exclusivamente la carpeta unica creada por esta instancia.
        Directory.Delete(_directorioTemporal, recursive: true);
    }
}
