using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Pages;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

internal sealed class VentaCreateTestContext
{
    public Mock<IVentaFacade> Facade { get; } = new(MockBehavior.Strict);
    public Mock<IClienteService> Clientes { get; } = new(MockBehavior.Strict);
    public Mock<IMedicamentoService> Medicamentos { get; } = new(MockBehavior.Strict);
    public VentaCreateModel Page { get; }
    public DataTable CatalogoClientes { get; } = new();
    public DataTable CatalogoMedicamentos { get; } = new();

    public VentaCreateTestContext(bool conUsuario = true)
    {
        var http = new DefaultHttpContext { Session = new SesionEnMemoria() };
        if (conUsuario) http.Session.SetInt32("IdUsuario", 9);
        Page = new VentaCreateModel(Facade.Object, Clientes.Object, Medicamentos.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = http,
                RouteData = new RouteData(),
                ActionDescriptor = new CompiledPageActionDescriptor
                {
                    ViewEnginePath = "/Venta/VentaCreate",
                    RouteValues = new Dictionary<string, string?> { ["page"] = "/Venta/VentaCreate" }
                }
            },
            IdCliente = 17,
            DetallesJson = "[{\"IdMedicamento\":8,\"Cantidad\":2}]"
        };
        Page.RouteData.Values["page"] = "/Venta/VentaCreate";
    }

    public void PrepararCatalogos()
    {
        Facade.Setup(f => f.ObtenerClientes()).Returns(CatalogoClientes);
        Facade.Setup(f => f.ObtenerMedicamentos()).Returns(CatalogoMedicamentos);
    }

    public void ComprobarCatalogos()
    {
        Assert.Same(CatalogoClientes, Page.ClienteDataTable);
        Assert.Same(CatalogoMedicamentos, Page.MedicamentoDataTable);
        Facade.Verify(f => f.ObtenerClientes(), Times.Once);
        Facade.Verify(f => f.ObtenerMedicamentos(), Times.Once);
    }

    public void SinOtrasLlamadas()
    {
        Facade.VerifyNoOtherCalls();
        Clientes.VerifyNoOtherCalls();
        Medicamentos.VerifyNoOtherCalls();
    }

    public static JsonElement Json(IActionResult resultado) =>
        JsonSerializer.SerializeToElement(Assert.IsType<JsonResult>(resultado).Value);

    public static DataTable TablaClientes()
    {
        var tabla = new DataTable();
        tabla.Columns.Add("id", typeof(int));
        tabla.Columns.Add("nit", typeof(string));
        tabla.Columns.Add("razon_social", typeof(string));
        return tabla;
    }

    private sealed class SesionEnMemoria : ISession
    {
        private readonly Dictionary<string, byte[]> _datos = new();
        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _datos.Keys;
        public void Clear() => _datos.Clear();
        public void Remove(string key) => _datos.Remove(key);
        public void Set(string key, byte[] value) => _datos[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _datos.TryGetValue(key, out value!);
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
