using System.Data;
using Moq;
using ProyectoArqSoft.Application.Facades;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using Xunit;

namespace ProyectoArqSoft.Tests.Facades;

[Trait("Category", "Unit")]
public class DashboardFacadeTests
{
    [Theory]
    [InlineData(12, 7, 3, 25, true)]
    [InlineData(0, 0, 0, 0, false)]
    public void ObtenerDashboardCompleto_CombinaEstadisticasYConservaTablaDeDestacados(
        int medicamentos, int clientes, int usuarios, int ventas, bool conDestacados)
    {
        var medicamentoRepo = new Mock<IMedicamentoRepository>(MockBehavior.Strict);
        var clienteRepo = new Mock<IClienteRepository>(MockBehavior.Strict);
        var usuarioRepo = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var ventaRepo = new Mock<IVentaRepository>(MockBehavior.Strict);
        var medicamentoService = new Mock<IMedicamentoService>(MockBehavior.Strict);
        medicamentoRepo.Setup(r => r.Count()).Returns(medicamentos);
        clienteRepo.Setup(r => r.Count()).Returns(clientes);
        usuarioRepo.Setup(r => r.Count()).Returns(usuarios);
        ventaRepo.Setup(r => r.Count()).Returns(ventas);

        var destacados = new DataTable();
        destacados.Columns.Add("id", typeof(int));
        destacados.Columns.Add("nombre", typeof(string));
        if (conDestacados) destacados.Rows.Add(8, "Paracetamol");
        medicamentoService.Setup(s => s.ObtenerDestacados()).Returns(destacados);

        // EstadisticasService es concreto y no virtual: se ejecuta con repositorios simulados.
        var estadisticasService = new EstadisticasService(
            medicamentoRepo.Object, clienteRepo.Object, usuarioRepo.Object, ventaRepo.Object);
        var facade = new DashboardFacade(estadisticasService, medicamentoService.Object);

        var resultado = facade.ObtenerDashboardCompleto();

        Assert.NotNull(resultado);
        Assert.NotNull(resultado.Estadisticas);
        Assert.Equal(medicamentos, resultado.Estadisticas.TotalMedicamentos);
        Assert.Equal(clientes, resultado.Estadisticas.TotalClientes);
        Assert.Equal(usuarios, resultado.Estadisticas.TotalUsuarios);
        Assert.Equal(ventas, resultado.Estadisticas.TotalVentas);
        Assert.Same(destacados, resultado.MedicamentosDestacados);
        Assert.Equal(conDestacados ? 1 : 0, resultado.MedicamentosDestacados.Rows.Count);
        if (conDestacados)
        {
            Assert.Equal(8, resultado.MedicamentosDestacados.Rows[0]["id"]);
            Assert.Equal("Paracetamol", resultado.MedicamentosDestacados.Rows[0]["nombre"]);
        }

        medicamentoRepo.Verify(r => r.Count(), Times.Once);
        clienteRepo.Verify(r => r.Count(), Times.Once);
        usuarioRepo.Verify(r => r.Count(), Times.Once);
        ventaRepo.Verify(r => r.Count(), Times.Once);
        medicamentoService.Verify(s => s.ObtenerDestacados(), Times.Once);
        medicamentoRepo.VerifyNoOtherCalls();
        clienteRepo.VerifyNoOtherCalls();
        usuarioRepo.VerifyNoOtherCalls();
        ventaRepo.VerifyNoOtherCalls();
        medicamentoService.VerifyNoOtherCalls();
    }
}
