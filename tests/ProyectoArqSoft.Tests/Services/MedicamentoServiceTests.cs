using System.Data;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class MedicamentoServiceTests
{

    [Fact]
    public void ObtenerDestacados_DevuelveLaTablaDelRepositorio()
    {
        var tabla = new DataTable();
        _repository.Setup(r => r.GetDestacados()).Returns(tabla);

        var resultado = CrearServicio().ObtenerDestacados();

        Assert.Same(tabla, resultado);
        _repository.Verify(r => r.GetDestacados(), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerPorId_EnviaElIdYDevuelveElMedicamentoDelRepositorio()
    {
        var medicamento = new Medicamento { Id = 17 };
        _repository.Setup(r => r.GetById(17)).Returns(medicamento);

        var resultado = CrearServicio().ObtenerPorId(17);

        Assert.Same(medicamento, resultado);
        _repository.Verify(r => r.GetById(17), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerTodos_ConFiltro_EnviaElFiltroSinModificar()
    {
        var tabla = new DataTable();
        const string filtro = "  Paracetamol  ";
        _repository.Setup(r => r.GetAll(filtro)).Returns(tabla);

        var resultado = CrearServicio().ObtenerTodos(filtro);

        Assert.Same(tabla, resultado);
        _repository.Verify(r => r.GetAll(filtro), Times.Once);
        VerificarSinOtrasLlamadas();
    }
    private readonly Mock<IMedicamentoRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<IResult<Medicamento>> _validador = new(MockBehavior.Strict);
    private readonly Mock<IResult<MovimientoStockDto>> _movimientoValidador = new(MockBehavior.Strict);

    private MedicamentoService CrearServicio() =>
        new(_repository.Object, _validador.Object, _movimientoValidador.Object);

    private void VerificarSinOtrasLlamadas()
    {
        _repository.VerifyNoOtherCalls();
        _validador.VerifyNoOtherCalls();
        _movimientoValidador.VerifyNoOtherCalls();
    }

    [Fact]
    public void ObtenerTodos_SinFiltro_DevuelveLaTablaDelRepositorio()
    {
        // Preparar.
        var tabla = new DataTable();
        _repository.Setup(r => r.GetAll()).Returns(tabla);

        // Actuar.
        var resultado = CrearServicio().ObtenerTodos();

        // Comprobar.
        Assert.Same(tabla, resultado);
        _repository.Verify(r => r.GetAll(), Times.Once);
        VerificarSinOtrasLlamadas();
    }
}
