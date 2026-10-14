using System.Data;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class ClienteServiceTests
{
    private readonly Mock<IRepository<Cliente>> _repository = new(MockBehavior.Strict);
    private readonly Mock<IResult<Cliente>> _validador = new(MockBehavior.Strict);

    private ClienteService CrearServicio() => new(_repository.Object, _validador.Object);

    private void VerificarSinOtrasLlamadas()
    {
        _repository.VerifyNoOtherCalls();
        _validador.VerifyNoOtherCalls();
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
