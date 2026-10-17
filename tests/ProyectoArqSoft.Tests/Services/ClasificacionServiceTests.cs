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

    [Fact]
    public void Crear_ConValidacionRechazada_DevuelveElRechazoSinConsultarNiInsertar()
    {
        var rechazo = Result.Fail("Clasificacion invalida.");
        _validador.Setup(v => v.Validar(It.IsAny<Clasificacion>())).Returns(rechazo);

        var resultado = CrearServicio().Crear("Analgesicos", "Sintetico", "Tratamiento del dolor", 9);

        Assert.Same(rechazo, resultado);
        _validador.Verify(v => v.Validar(It.IsAny<Clasificacion>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_ConNombreActivoDuplicado_DevuelveErrorSinInsertar()
    {
        _validador.Setup(v => v.Validar(It.IsAny<Clasificacion>())).Returns(Result.Ok());
        _repository.Setup(r => r.ExisteNombreActivo("Analgesicos")).Returns(true);

        var resultado = CrearServicio().Crear("  Analgesicos  ", "Sintetico", "Tratamiento del dolor", 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Ya existe una clasificación activa con ese nombre.", resultado.Error);
        _validador.Verify(v => v.Validar(It.IsAny<Clasificacion>()), Times.Once);
        _repository.Verify(r => r.ExisteNombreActivo("Analgesicos"), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_SinFilasInsertadas_DevuelveError()
    {
        _validador.Setup(v => v.Validar(It.IsAny<Clasificacion>())).Returns(Result.Ok());
        _repository.Setup(r => r.ExisteNombreActivo("Analgesicos")).Returns(false);
        _repository.Setup(r => r.Insert(It.IsAny<Clasificacion>())).Returns(0);

        var resultado = CrearServicio().Crear("Analgesicos", "Sintetico", "Tratamiento del dolor", 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No se pudo registrar la clasificación.", resultado.Error);
        _validador.Verify(v => v.Validar(It.IsAny<Clasificacion>()), Times.Once);
        _repository.Verify(r => r.ExisteNombreActivo("Analgesicos"), Times.Once);
        _repository.Verify(r => r.Insert(It.IsAny<Clasificacion>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }
}
