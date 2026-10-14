using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class VentaServiceCrearTests
{
    private readonly Mock<IVentaRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<IMedicamentoRepository> _medicamentos = new(MockBehavior.Strict);
    private readonly Mock<IResult<Venta>> _validador = new(MockBehavior.Strict);

    [Fact]
    public void Crear_ConValidacionExitosa_RegistraLaVentaValidadaYDevuelveElResultado()
    {
        var servicio = PrepararServicioConMedicamento();
        var esperado = Result.Ok();
        Venta? ventaValidada = null;
        Venta? ventaRegistrada = null;
        _validador.Setup(v => v.Validar(It.IsAny<Venta>()))
            .Callback<Venta>(venta => ventaValidada = venta).Returns(Result.Ok());
        _repository.Setup(r => r.RegistrarVenta(It.IsAny<Venta>()))
            .Callback<Venta>(venta => ventaRegistrada = venta).Returns(esperado);

        var resultado = servicio.Crear(7, 3, "  Efectivo  ", DetallesValidos());

        Assert.Same(esperado, resultado);
        Assert.NotNull(ventaValidada);
        Assert.Same(ventaValidada, ventaRegistrada);
        Assert.Equal(0, ventaValidada.Id);
        Assert.Equal(7, ventaValidada.IdCliente);
        Assert.Equal(3, ventaValidada.IdUsuario);
        Assert.Null(ventaValidada.IdUsuarioEditor);
        Assert.Equal("Efectivo", ventaValidada.MetodoPago);
        Assert.Equal(24.50m, ventaValidada.Total);
        var detalle = Assert.Single(ventaValidada.Detalles);
        Assert.Equal(8, detalle.IdMedicamento);
        Assert.Equal(2, detalle.Cantidad);
        Assert.Equal(12.25m, detalle.PrecioUnitario);
        _validador.Verify(v => v.Validar(It.IsAny<Venta>()), Times.Once);
        _repository.Verify(r => r.RegistrarVenta(It.IsAny<Venta>()), Times.Once);
        VerificarConsultaMedicamentoYSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_ConValidacionFallida_DevuelveElRechazoSinRegistrarVenta()
    {
        var servicio = PrepararServicioConMedicamento();
        var rechazo = Result.Fail("Cliente no valido.");
        _validador.Setup(v => v.Validar(It.IsAny<Venta>())).Returns(rechazo);

        var resultado = servicio.Crear(7, 3, "Efectivo", DetallesValidos());

        Assert.Same(rechazo, resultado);
        _validador.Verify(v => v.Validar(It.IsAny<Venta>()), Times.Once);
        // Ninguna configuracion permite registrar: se exige ausencia de llamadas.
        VerificarConsultaMedicamentoYSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_SiConstruirVentaLanzaExcepcion_DevuelveFalloSinValidarNiRegistrar()
    {
        var servicio = new VentaService(_repository.Object, _medicamentos.Object, _validador.Object);

        // Una lista vacia hace fallar la construccion real, sin simular el metodo privado.
        var resultado = servicio.Crear(7, 3, "Efectivo", new List<DetalleVentaInputDto>());

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Debe agregar al menos un medicamento.", resultado.Error);
        _repository.VerifyNoOtherCalls();
        _medicamentos.VerifyNoOtherCalls();
        _validador.VerifyNoOtherCalls();
    }

    [Fact]
    public void Crear_ConDetallesNulos_DevuelveFalloSinConsultarDependencias()
    {
        var servicio = new VentaService(_repository.Object, _medicamentos.Object, _validador.Object);

        // Null recorre el cortocircuito que una lista vacia no cubre.
        var resultado = servicio.Crear(7, 3, "Efectivo", null!);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Debe agregar al menos un medicamento.", resultado.Error);
        _repository.VerifyNoOtherCalls();
        _medicamentos.VerifyNoOtherCalls();
        _validador.VerifyNoOtherCalls();
    }

    [Fact]
    public void Crear_ConMedicamentoInexistente_DevuelveFalloSinValidarNiRegistrar()
    {
        _medicamentos.Setup(r => r.GetById(8)).Returns((Medicamento?)null);
        var servicio = new VentaService(_repository.Object, _medicamentos.Object, _validador.Object);

        var resultado = servicio.Crear(7, 3, "Efectivo", DetallesValidos());

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Medicamento con ID 8 no encontrado.", resultado.Error);
        VerificarConsultaMedicamentoYSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_ConStockInsuficiente_DevuelveFalloSinValidarNiRegistrar()
    {
        var servicio = PrepararServicioConMedicamento();
        var detalles = DetallesValidos();
        detalles[0].Cantidad = 11;

        var resultado = servicio.Crear(7, 3, "Efectivo", detalles);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Stock insuficiente para Paracetamol. Disponible: 10.", resultado.Error);
        VerificarConsultaMedicamentoYSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_SiValidadorLanzaExcepcion_DevuelveSuMensajeSinRegistrar()
    {
        var servicio = PrepararServicioConMedicamento();
        _validador.Setup(v => v.Validar(It.IsAny<Venta>()))
            .Throws(new InvalidOperationException("Error de validacion simulado."));

        var resultado = servicio.Crear(7, 3, "Efectivo", DetallesValidos());

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Error de validacion simulado.", resultado.Error);
        _validador.Verify(v => v.Validar(It.IsAny<Venta>()), Times.Once);
        VerificarConsultaMedicamentoYSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_SiRegistroLanzaExcepcion_DevuelveSuMensajeComoFallo()
    {
        var servicio = PrepararServicioConMedicamento();
        _validador.Setup(v => v.Validar(It.IsAny<Venta>())).Returns(Result.Ok());
        _repository.Setup(r => r.RegistrarVenta(It.IsAny<Venta>()))
            .Throws(new InvalidOperationException("Error de registro simulado."));

        var resultado = servicio.Crear(7, 3, "Efectivo", DetallesValidos());

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Error de registro simulado.", resultado.Error);
        _validador.Verify(v => v.Validar(It.IsAny<Venta>()), Times.Once);
        _repository.Verify(r => r.RegistrarVenta(It.IsAny<Venta>()), Times.Once);
        VerificarConsultaMedicamentoYSinOtrasLlamadas();
    }

    private VentaService PrepararServicioConMedicamento()
    {
        _medicamentos.Setup(r => r.GetById(8)).Returns(new Medicamento
        {
            Id = 8, Nombre = "Paracetamol", Stock = 10, Precio = 12.25m
        });
        return new VentaService(_repository.Object, _medicamentos.Object, _validador.Object);
    }

    private static List<DetalleVentaInputDto> DetallesValidos() => new()
    {
        // El precio de entrada no debe reemplazar al precio del medicamento.
        new DetalleVentaInputDto { IdMedicamento = 8, Cantidad = 2, PrecioUnitario = 999m }
    };

    private void VerificarConsultaMedicamentoYSinOtrasLlamadas()
    {
        _medicamentos.Verify(r => r.GetById(8), Times.Once);
        _medicamentos.VerifyNoOtherCalls();
        _validador.VerifyNoOtherCalls();
        _repository.VerifyNoOtherCalls();
    }
}
