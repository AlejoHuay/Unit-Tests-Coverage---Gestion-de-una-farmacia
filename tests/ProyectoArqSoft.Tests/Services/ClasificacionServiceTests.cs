using System.Data;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class ClasificacionServiceTests
{
    private readonly Mock<IClasificacionRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<IResult<Clasificacion>> _validador = new(MockBehavior.Strict);

    private ClasificacionService CrearServicio() => new(_repository.Object, _validador.Object);

    private void VerificarSinOtrasLlamadas()
    {
        _repository.VerifyNoOtherCalls();
        _validador.VerifyNoOtherCalls();
    }

    [Fact]
    public void ObtenerTodos_SinFiltro_DevuelveLaTablaDelRepositorio()
    {
        var tabla = new DataTable();
        _repository.Setup(r => r.GetAll()).Returns(tabla);

        var resultado = CrearServicio().ObtenerTodos();

        Assert.Same(tabla, resultado);
        _repository.Verify(r => r.GetAll(), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerTodos_ConFiltro_EnviaElFiltroSinModificar()
    {
        const string filtro = "  Analgesicos   adultos  ";
        var tabla = new DataTable();
        _repository.Setup(r => r.GetAll(filtro)).Returns(tabla);

        var resultado = CrearServicio().ObtenerTodos(filtro);

        Assert.Same(tabla, resultado);
        _repository.Verify(r => r.GetAll(filtro), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerPorId_EnviaElIdYDevuelveLaClasificacionDelRepositorio()
    {
        var clasificacion = new Clasificacion { Id = 17, Nombre = "Analgesicos" };
        _repository.Setup(r => r.GetById(17)).Returns(clasificacion);

        var resultado = CrearServicio().ObtenerPorId(17);

        Assert.Same(clasificacion, resultado);
        _repository.Verify(r => r.GetById(17), Times.Once);
        VerificarSinOtrasLlamadas();
    }
}
