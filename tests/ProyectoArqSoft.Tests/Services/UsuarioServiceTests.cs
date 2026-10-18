using System.Data;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Infrastructure.Helpers;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class UsuarioServiceTests
{
    [Fact]
    public void CrearUsuario_DatosInvalidos_RetornaErrorSinInsertar()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>();
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        var servicio = CrearServicio(repository, tokenService, emailService);
        var dto = CrearRegistroValido();
        dto.Nombres = "";

        // Act
        Result resultado = servicio.CrearUsuario(dto, "Admin", 4);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El campo Nombres es obligatorio.", resultado.Error);
        repository.Verify(r => r.Insert(It.IsAny<Usuario>()), Times.Never);
        tokenService.VerifyNoOtherCalls();
        emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public void CrearUsuario_RepositorioNoInserta_RetornaError()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.Insert(It.IsAny<Usuario>())).Returns(0);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        var servicio = CrearServicio(repository, tokenService, emailService);

        // Act
        Result resultado = servicio.CrearUsuario(CrearRegistroValido(), "Admin", 4);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("No se pudo registrar el usuario.", resultado.Error);
        repository.Verify(r => r.Insert(It.IsAny<Usuario>()), Times.Once);
        repository.Verify(r => r.GetByEmail(It.IsAny<string>()), Times.Never);
        tokenService.VerifyNoOtherCalls();
        emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public void CrearUsuario_NoRecuperaUsuarioInsertado_RetornaError()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.Insert(It.IsAny<Usuario>())).Returns(1);
        repository.Setup(r => r.GetByEmail("ana@example.test")).Returns((Usuario?)null);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        var servicio = CrearServicio(repository, tokenService, emailService);

        // Act
        Result resultado = servicio.CrearUsuario(CrearRegistroValido(), "Admin", 4);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El usuario fue registrado, pero no se pudo recuperar su información.", resultado.Error);
        repository.Verify(r => r.Insert(It.IsAny<Usuario>()), Times.Once);
        repository.Verify(r => r.GetByEmail("ana@example.test"), Times.Once);
        tokenService.VerifyNoOtherCalls();
        emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public void CrearUsuario_NoGeneraToken_RetornaErrorDeToken()
    {
        // Arrange
        var usuario = UsuarioRegistrado();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.Insert(It.IsAny<Usuario>())).Returns(1);
        repository.Setup(r => r.GetByEmail("ana@example.test")).Returns(usuario);
        var tokenService = new Mock<ITokenService>();
        ConfigurarGeneracionToken(tokenService, Result.Fail("Fallo token"), "");
        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        var servicio = CrearServicio(repository, tokenService, emailService);

        // Act
        Result resultado = servicio.CrearUsuario(CrearRegistroValido(), "Admin", 4);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("Fallo token", resultado.Error);
        emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public void CrearUsuario_DatosValidos_RegistraUsuarioGeneraTokenYEnviaCorreo()
    {
        // Arrange
        Usuario? usuarioInsertado = null;
        var usuario = UsuarioRegistrado();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.Insert(It.IsAny<Usuario>()))
            .Callback<Usuario>(insertado => usuarioInsertado = insertado)
            .Returns(1);
        repository.Setup(r => r.GetByEmail("ana@example.test")).Returns(usuario);
        var tokenService = new Mock<ITokenService>();
        ConfigurarGeneracionToken(tokenService, Result.Ok(), "token con+signos");
        var emailService = new Mock<IEmailService>();
        emailService
            .Setup(s => s.EnviarCorreoActivacionCuenta(
                "ana@example.test",
                "Ana",
                "ana123",
                "Password123",
                "http://localhost:5081/Auth/ActivarCuenta?token=token-firmado"))
            .Returns(Result.Ok());
        var servicio = CrearServicio(repository, tokenService, emailService);

        // Act
        Result resultado = servicio.CrearUsuario(CrearRegistroValido(), " Admin ", 4);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
        var creado = Assert.IsType<Usuario>(usuarioInsertado);
        Assert.Equal("Ana", creado.Nombres);
        Assert.Equal("Admin", creado.Role);
        Assert.Equal(1, creado.Activo);
        Assert.Equal(1, creado.MustChangePassword);
        Assert.Equal(4, creado.IdUsuarioCreador);
        Assert.True(PasswordHelper.Verify("Password123", creado.PasswordHash));
        repository.Verify(r => r.Insert(It.IsAny<Usuario>()), Times.Once);
        tokenService.Verify(s => s.GenerarToken(
            It.Is<UsuarioTokenGeneracionDto>(dto =>
                dto.IdUsuario == usuario.IdUsuario &&
                dto.TipoToken == TipoTokenConstantes.ActivacionCuenta &&
                dto.MinutosExpiracion == 60),
            out It.Ref<string>.IsAny), Times.Once);
        emailService.Verify(s => s.EnviarCorreoActivacionCuenta(
            "ana@example.test",
            "Ana",
            "ana123",
            "Password123",
            "http://localhost:5081/Auth/ActivarCuenta?token=token-firmado"), Times.Once);
    }

    [Fact]
    public void ActualizarUsuario_DatosInvalidos_RetornaErrorSinConsultarRepositorio()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var servicio = CrearServicio(repository);
        var dto = CrearActualizacionValida();
        dto.Email = "invalido";

        // Act
        Result resultado = servicio.ActualizarUsuario(dto, 7);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El formato del email no es válido.", resultado.Error);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void ActualizarUsuario_UsuarioNoEncontrado_RetornaError()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns((Usuario?)null);
        var servicio = CrearServicio(repository);

        // Act
        Result resultado = servicio.ActualizarUsuario(CrearActualizacionValida(), 7);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El usuario no existe.", resultado.Error);
        repository.Verify(r => r.GetById(12), Times.Once);
        repository.Verify(r => r.Update(It.IsAny<Usuario>(), It.IsAny<int?>()), Times.Never);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void ActualizarUsuario_ResultadoDelRepositorioDeterminaResultado(int filasAfectadas, bool exitoEsperado)
    {
        // Arrange
        var usuario = UsuarioExistente();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns(usuario);
        repository.Setup(r => r.Update(It.IsAny<Usuario>(), 7)).Returns(filasAfectadas);
        var servicio = CrearServicio(repository);
        var dto = CrearActualizacionValida();
        dto.Nombres = "Nombre actualizado";
        dto.Role = "";

        // Act
        Result resultado = servicio.ActualizarUsuario(dto, 7);

        // Assert
        Assert.Equal(exitoEsperado, resultado.IsSuccess);
        Assert.Equal(exitoEsperado ? string.Empty : "No se pudo actualizar el usuario.", resultado.Error);
        Assert.Equal("Nombre actualizado", usuario.Nombres);
        Assert.Equal("Admin", usuario.Role);
        Assert.Equal("Perez", usuario.ApellidoPaterno);
        Assert.Equal("ana.nueva@example.test", usuario.Email);
        repository.Verify(r => r.Update(usuario, 7), Times.Once);
    }

    [Fact]
    public void ActualizarAccesoUsuario_UsuarioNoEncontrado_RetornaError()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetById(12)).Returns((Usuario?)null);
        var servicio = CrearServicio(repository);

        // Act
        Result resultado = servicio.ActualizarAccesoUsuario(CrearActualizacionValida(), 7);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El usuario no existe.", resultado.Error);
        repository.Verify(r => r.GetById(12), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("correo-invalido")]
    [InlineData("ana @example.test")]
    public void ActualizarAccesoUsuario_EmailInvalido_RetornaError(string? email)
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns(UsuarioExistente());
        var servicio = CrearServicio(repository);
        var dto = CrearActualizacionValida();
        dto.Email = email!;

        // Act
        Result resultado = servicio.ActualizarAccesoUsuario(dto, 7);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El correo electrónico no es válido.", resultado.Error);
        repository.Verify(r => r.GetByEmail(It.IsAny<string>()), Times.Never);
        repository.Verify(r => r.UpdateDatosEdicion(It.IsAny<Usuario>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public void ActualizarAccesoUsuario_EmailExcede255Caracteres_RetornaError()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns(UsuarioExistente());
        var servicio = CrearServicio(repository);
        var dto = CrearActualizacionValida();
        dto.Email = new string('a', 244) + "@example.test";

        // Act
        Result resultado = servicio.ActualizarAccesoUsuario(dto, 7);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El correo electrónico no es válido.", resultado.Error);
    }

    [Theory]
    [InlineData("Usuario", 1)]
    [InlineData("Admin", 2)]
    public void ActualizarAccesoUsuario_RolOEstadoInvalido_RetornaError(string role, byte activo)
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns(UsuarioExistente());
        var servicio = CrearServicio(repository);
        var dto = CrearActualizacionValida();
        dto.Role = role;
        dto.Activo = activo;

        // Act
        Result resultado = servicio.ActualizarAccesoUsuario(dto, 7);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El rol o estado no es válido.", resultado.Error);
        repository.Verify(r => r.GetByEmail(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ActualizarAccesoUsuario_EmailDeOtroUsuario_RetornaError()
    {
        // Arrange
        var otroUsuario = new Usuario { IdUsuario = 99, Email = "ana.nueva@example.test" };
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns(UsuarioExistente());
        repository.Setup(r => r.GetByEmail("ana.nueva@example.test")).Returns(otroUsuario);
        var servicio = CrearServicio(repository);

        // Act
        Result resultado = servicio.ActualizarAccesoUsuario(CrearActualizacionValida(), 7);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El correo ya está registrado.", resultado.Error);
        repository.Verify(r => r.UpdateDatosEdicion(It.IsAny<Usuario>(), It.IsAny<int?>()), Times.Never);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void ActualizarAccesoUsuario_EmailValidoActualizaDatos(int filasAfectadas, bool exitoEsperado)
    {
        // Arrange
        var usuario = UsuarioExistente();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns(usuario);
        repository.Setup(r => r.GetByEmail("ana.nueva@example.test")).Returns((Usuario?)null);
        repository.Setup(r => r.UpdateDatosEdicion(usuario, 7)).Returns(filasAfectadas);
        var servicio = CrearServicio(repository);
        var dto = CrearActualizacionValida();
        dto.Email = " ANA.NUEVA@EXAMPLE.TEST ";
        dto.Role = "Bioquimico";
        dto.Activo = 0;

        // Act
        Result resultado = servicio.ActualizarAccesoUsuario(dto, 7);

        // Assert
        Assert.Equal(exitoEsperado, resultado.IsSuccess);
        Assert.Equal(exitoEsperado ? string.Empty : "No se pudo actualizar el usuario.", resultado.Error);
        Assert.Equal("ana.nueva@example.test", usuario.Email);
        Assert.Equal("Bioquimico", usuario.Role);
        Assert.Equal((sbyte)0, usuario.Activo);
        repository.Verify(r => r.UpdateDatosEdicion(usuario, 7), Times.Once);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void EliminarUsuario_ResultadoDelRepositorioDeterminaResultado(int filasAfectadas, bool exitoEsperado)
    {
        // Arrange
        var usuario = UsuarioExistente();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns(usuario);
        repository.Setup(r => r.SoftDelete(usuario, 7)).Returns(filasAfectadas);
        var servicio = CrearServicio(repository);

        // Act
        Result resultado = servicio.EliminarUsuario(12, 7);

        // Assert
        Assert.Equal(exitoEsperado, resultado.IsSuccess);
        Assert.Equal(exitoEsperado ? string.Empty : "No se pudo eliminar el usuario.", resultado.Error);
        repository.Verify(r => r.GetById(12), Times.Exactly(2));
        repository.Verify(r => r.SoftDelete(usuario, 7), Times.Once);
    }

    [Fact]
    public void EliminarUsuario_IdInvalido_RetornaErrorSinConsultarRepositorio()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var servicio = CrearServicio(repository);

        // Act
        Result resultado = servicio.EliminarUsuario(0, 7);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El identificador del usuario no es válido.", resultado.Error);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void EliminarUsuario_UsuarioNoEncontrado_RetornaError()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns((Usuario?)null);
        var servicio = CrearServicio(repository);

        // Act
        Result resultado = servicio.EliminarUsuario(12, 7);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El usuario no existe.", resultado.Error);
        repository.Verify(r => r.GetById(12), Times.Once);
        repository.Verify(r => r.SoftDelete(It.IsAny<Usuario>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public void ObtenerUsuarioPorId_IdInvalido_RetornaNullSinConsultarRepositorio()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var servicio = CrearServicio(repository);

        // Act
        UsuarioDto? usuario = servicio.ObtenerUsuarioPorId(0);

        // Assert
        Assert.Null(usuario);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void ObtenerUsuarioPorId_UsuarioEncontrado_MapeaPropiedades()
    {
        // Arrange
        var usuarioExistente = UsuarioExistente();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns(usuarioExistente);
        var servicio = CrearServicio(repository);

        // Act
        UsuarioDto? usuario = servicio.ObtenerUsuarioPorId(12);

        // Assert
        var dto = Assert.IsType<UsuarioDto>(usuario);
        Assert.Equal(12, dto.IdUsuario);
        Assert.Equal("Nombre anterior", dto.Nombres);
        Assert.Equal("Perez", dto.ApellidoPaterno);
        Assert.Equal("ana@example.test", dto.Email);
        Assert.Equal("ana", dto.UserName);
        Assert.Equal("Admin", dto.Role);
        Assert.Equal((sbyte)1, dto.Activo);
        Assert.Equal((sbyte)1, dto.MustChangePassword);
    }

    [Fact]
    public void ObtenerUsuarioPorEmail_CadenaVacia_RetornaNullSinConsultarRepositorio()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var servicio = CrearServicio(repository);

        // Act
        UsuarioDto? usuario = servicio.ObtenerUsuarioPorEmail("   ");

        // Assert
        Assert.Null(usuario);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void ObtenerUsuarioPorEmail_NormalizaCorreoYMapeaUsuario()
    {
        // Arrange
        var usuarioExistente = UsuarioExistente();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetByEmail("ana@example.test")).Returns(usuarioExistente);
        var servicio = CrearServicio(repository);

        // Act
        UsuarioDto? usuario = servicio.ObtenerUsuarioPorEmail(" ANA@EXAMPLE.TEST ");

        // Assert
        var dto = Assert.IsType<UsuarioDto>(usuario);
        Assert.Equal("ana@example.test", dto.Email);
        repository.Verify(r => r.GetByEmail("ana@example.test"), Times.Once);
    }

    [Fact]
    public void ObtenerUsuarioPorUserName_CadenaVacia_RetornaNullSinConsultarRepositorio()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var servicio = CrearServicio(repository);

        // Act
        UsuarioDto? usuario = servicio.ObtenerUsuarioPorUserName("   ");

        // Assert
        Assert.Null(usuario);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void ObtenerUsuarioPorUserName_NormalizaNombreYMapeaUsuario()
    {
        // Arrange
        var usuarioExistente = UsuarioExistente();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetByUserName("Ana")).Returns(usuarioExistente);
        var servicio = CrearServicio(repository);

        // Act
        UsuarioDto? usuario = servicio.ObtenerUsuarioPorUserName(" Ana ");

        // Assert
        var dto = Assert.IsType<UsuarioDto>(usuario);
        Assert.Equal("ana", dto.UserName);
        repository.Verify(r => r.GetByUserName("Ana"), Times.Once);
    }

    [Fact]
    public void ObtenerTodos_SinFiltro_DevuelveTablaDelRepositorio()
    {
        // Arrange
        var tablaEsperada = new DataTable();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetAll()).Returns(tablaEsperada);
        var servicio = CrearServicio(repository);

        // Act
        DataTable tabla = servicio.ObtenerTodos();

        // Assert
        Assert.Same(tablaEsperada, tabla);
        repository.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public void ObtenerTodos_ConFiltro_LimpiaFiltroAntesDeConsultarRepositorio()
    {
        // Arrange
        const string filtroLimpio = "Ana Norte";
        var tablaEsperada = new DataTable();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetAll(filtroLimpio)).Returns(tablaEsperada);
        var servicio = CrearServicio(repository);

        // Act
        DataTable tabla = servicio.ObtenerTodos("  Ana   Norte  ");

        // Assert
        Assert.Same(tablaEsperada, tabla);
        repository.Verify(r => r.GetAll(filtroLimpio), Times.Once);
    }

    [Fact]
    public void ValidarActivacionCuenta_TokenVacio_RetornaErrorSinValidarToken()
    {
        // Arrange
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var servicio = CrearServicio(tokenService: tokenService);

        // Act
        Result resultado = servicio.ValidarActivacionCuenta("  ");

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El token de activación es inválido.", resultado.Error);
        tokenService.VerifyNoOtherCalls();
    }

    [Fact]
    public void ValidarActivacionCuenta_TokenInvalido_RetornaError()
    {
        // Arrange
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidarToken("token", TipoTokenConstantes.ActivacionCuenta))
            .Returns((UsuarioToken?)null);
        var servicio = CrearServicio(tokenService: tokenService);

        // Act
        Result resultado = servicio.ValidarActivacionCuenta(" token ");

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El token ha expirado o es inválido.", resultado.Error);
        tokenService.Verify(s => s.ValidarToken("token", TipoTokenConstantes.ActivacionCuenta), Times.Once);
    }

    [Fact]
    public void ValidarActivacionCuenta_TokenVigente_RetornaExito()
    {
        // Arrange
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidarToken("token", TipoTokenConstantes.ActivacionCuenta))
            .Returns(new UsuarioToken());
        var servicio = CrearServicio(tokenService: tokenService);

        // Act
        Result resultado = servicio.ValidarActivacionCuenta(" token ");

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
        tokenService.Verify(s => s.ValidarToken("token", TipoTokenConstantes.ActivacionCuenta), Times.Once);
    }

    [Theory]
    [InlineData("", "Password123", "El token de activación es inválido.")]
    [InlineData("token", "", "La nueva contraseña es obligatoria.")]
    [InlineData("token", "corta", "La contraseña debe tener al menos 8 caracteres.")]
    public void ActivarCuenta_DatosInvalidos_RetornaErrorSinValidarToken(
        string token,
        string nuevaPassword,
        string errorEsperado)
    {
        // Arrange
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var servicio = CrearServicio(tokenService: tokenService);

        // Act
        Result resultado = servicio.ActivarCuenta(token, nuevaPassword);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(errorEsperado, resultado.Error);
        tokenService.VerifyNoOtherCalls();
    }

    [Fact]
    public void ActivarCuenta_TokenNoValido_RetornaError()
    {
        // Arrange
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidarToken("token", TipoTokenConstantes.ActivacionCuenta))
            .Returns((UsuarioToken?)null);
        var servicio = CrearServicio(tokenService: tokenService);

        // Act
        Result resultado = servicio.ActivarCuenta(" token ", "Password123");

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El token ha expirado o es inválido.", resultado.Error);
    }

    [Fact]
    public void ActivarCuenta_UsuarioNoEncontrado_RetornaError()
    {
        // Arrange
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidarToken("token", TipoTokenConstantes.ActivacionCuenta))
            .Returns(new UsuarioToken { UsuarioIdUsuario = 12, IdUsuarioToken = 8 });
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns((Usuario?)null);
        var servicio = CrearServicio(repository, tokenService);

        // Act
        Result resultado = servicio.ActivarCuenta("token", "Password123");

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El usuario no existe.", resultado.Error);
        repository.Verify(r => r.GetById(12), Times.Once);
        repository.Verify(r => r.ActivarCuentaConToken(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void ActivarCuenta_ResultadoDelRepositorioDeterminaResultado(int filasAfectadas, bool exitoEsperado)
    {
        // Arrange
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidarToken("token", TipoTokenConstantes.ActivacionCuenta))
            .Returns(new UsuarioToken { UsuarioIdUsuario = 12, IdUsuarioToken = 8 });
        var usuario = UsuarioExistente();
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetById(12)).Returns(usuario);
        repository.Setup(r => r.ActivarCuentaConToken(
            12,
            8,
            It.Is<string>(hash => PasswordHelper.Verify("NuevaPassword123", hash))))
            .Returns(filasAfectadas);
        var servicio = CrearServicio(repository, tokenService);

        // Act
        Result resultado = servicio.ActivarCuenta("token", " NuevaPassword123 ");

        // Assert
        Assert.Equal(exitoEsperado, resultado.IsSuccess);
        Assert.Equal(exitoEsperado ? string.Empty : "No se pudo actualizar la contraseña.", resultado.Error);
        repository.Verify(r => r.ActivarCuentaConToken(
            12,
            8,
            It.Is<string>(hash => PasswordHelper.Verify("NuevaPassword123", hash))), Times.Once);
    }

    private static UsuarioService CrearServicio(
        Mock<IUsuarioRepository>? repository = null,
        Mock<ITokenService>? tokenService = null,
        Mock<IEmailService>? emailService = null)
    {
        repository ??= new Mock<IUsuarioRepository>();
        tokenService ??= new Mock<ITokenService>();
        emailService ??= new Mock<IEmailService>();
        var validador = new UsuarioValidacionGeneral(repository.Object);
        return new UsuarioService(repository.Object, validador, tokenService.Object, emailService.Object);
    }

    private static UsuarioRegistroDto CrearRegistroValido() => new()
    {
        Nombres = "Ana",
        ApellidoPaterno = "Perez",
        ApellidoMaterno = "Lopez",
        Ci = "12345678",
        CiExtencion = "LP",
        Telefono = "71234567",
        Email = "ana@example.test",
        UserName = "ana123",
        Password = "Password123"
    };

    private static UsuarioActualizarDto CrearActualizacionValida() => new()
    {
        IdUsuario = 12,
        Nombres = "Nombre nuevo",
        ApellidoPaterno = "Perez",
        ApellidoMaterno = "Lopez",
        Ci = "12345678",
        CiExtencion = "LP",
        Telefono = "71234567",
        Email = "ana.nueva@example.test",
        Role = "Admin",
        Activo = 1
    };

    private static Usuario UsuarioExistente() => new()
    {
        IdUsuario = 12,
        Nombres = "Nombre anterior",
        ApellidoPaterno = "Perez",
        ApellidoMaterno = "Lopez",
        Ci = "12345678",
        CiExtencion = "LP",
        Telefono = "71234567",
        Email = "ana@example.test",
        UserName = "ana",
        Role = "Admin",
        Activo = 1,
        MustChangePassword = 1
    };

    private static Usuario UsuarioRegistrado() => new()
    {
        IdUsuario = 22,
        Nombres = "Ana",
        Email = "ana@example.test",
        UserName = "ana123"
    };

    private static void ConfigurarGeneracionToken(
        Mock<ITokenService> tokenService,
        Result resultadoEsperado,
        string tokenPlano)
    {
        tokenService
            .Setup(s => s.GenerarToken(
                It.IsAny<UsuarioTokenGeneracionDto>(),
                out It.Ref<string>.IsAny))
            .Returns((UsuarioTokenGeneracionDto _, out string tokenDevuelto) =>
            {
                tokenDevuelto = tokenPlano;
                return (resultadoEsperado, "token-firmado");
            });
    }
}
