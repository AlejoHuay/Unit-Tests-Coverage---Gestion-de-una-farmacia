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

public class AuthServiceTests
{
    [Fact]
    public void Constructor_RepositorioNulo_LanzaArgumentNullException()
    {
        // Arrange
        IUsuarioRepository? repository = null;
        var tokenService = new Mock<ITokenService>().Object;

        // Act
        var excepcion = Assert.Throws<ArgumentNullException>(
            () => new AuthService(repository!, tokenService));

        // Assert
        Assert.Equal("usuarioRepository", excepcion.ParamName);
    }

    [Fact]
    public void Constructor_ServicioTokenNulo_LanzaArgumentNullException()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>().Object;
        ITokenService? tokenService = null;

        // Act
        var excepcion = Assert.Throws<ArgumentNullException>(
            () => new AuthService(repository, tokenService!));

        // Assert
        Assert.Equal("tokenService", excepcion.ParamName);
    }

    [Fact]
    public void IniciarSesion_DtoNulo_RetornaErrorSinConsultarDependencias()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var servicio = new AuthService(repository.Object, tokenService.Object);

        // Act
        Result resultado = servicio.IniciarSesion(null!, out UsuarioLoginResponseDto? respuesta);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("Los datos de acceso no pueden ser nulos.", resultado.Error);
        Assert.Null(respuesta);
        repository.VerifyNoOtherCalls();
        tokenService.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("", "Password123", "El email o nombre de usuario es obligatorio.")]
    [InlineData("   ", "Password123", "El email o nombre de usuario es obligatorio.")]
    [InlineData("ana@example.test", "", "La contraseña es obligatoria.")]
    [InlineData("ana@example.test", "   ", "La contraseña es obligatoria.")]
    [InlineData("ana@example.test", "corta", "La contraseña debe tener al menos 8 caracteres.")]
    public void IniciarSesion_DatosInvalidos_RetornaErrorSinConsultarDependencias(
        string emailOUserName,
        string password,
        string errorEsperado)
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var servicio = new AuthService(repository.Object, tokenService.Object);
        var dto = new UsuarioLoginRequestDto
        {
            EmailOUserName = emailOUserName,
            Password = password
        };

        // Act
        Result resultado = servicio.IniciarSesion(dto, out UsuarioLoginResponseDto? respuesta);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(errorEsperado, resultado.Error);
        Assert.Null(respuesta);
        repository.VerifyNoOtherCalls();
        tokenService.VerifyNoOtherCalls();
    }

    [Fact]
    public void IniciarSesion_UsuarioNoEncontrado_BuscaPorEmailYLuegoPorUserName()
    {
        // Arrange
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetByEmail("ana")).Returns((Usuario?)null);
        repository.Setup(r => r.GetByUserName("ana")).Returns((Usuario?)null);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var servicio = new AuthService(repository.Object, tokenService.Object);
        var dto = new UsuarioLoginRequestDto { EmailOUserName = " ana ", Password = "Password123" };

        // Act
        Result resultado = servicio.IniciarSesion(dto, out UsuarioLoginResponseDto? respuesta);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("Las credenciales son incorrectas.", resultado.Error);
        Assert.Null(respuesta);
        repository.Verify(r => r.GetByEmail("ana"), Times.Once);
        repository.Verify(r => r.GetByUserName("ana"), Times.Once);
        repository.VerifyNoOtherCalls();
        tokenService.VerifyNoOtherCalls();
    }

    [Fact]
    public void IniciarSesion_UsuarioInactivo_RetornaErrorSinGenerarToken()
    {
        // Arrange
        var usuario = CrearUsuario("Password123");
        usuario.Activo = 0;
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetByEmail("ana@example.test")).Returns(usuario);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var servicio = new AuthService(repository.Object, tokenService.Object);
        var dto = new UsuarioLoginRequestDto
        {
            EmailOUserName = "ana@example.test",
            Password = "Password123"
        };

        // Act
        Result resultado = servicio.IniciarSesion(dto, out UsuarioLoginResponseDto? respuesta);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El usuario se encuentra inactivo.", resultado.Error);
        Assert.Null(respuesta);
        repository.Verify(r => r.GetByEmail("ana@example.test"), Times.Once);
        tokenService.VerifyNoOtherCalls();
    }

    [Fact]
    public void IniciarSesion_ContrasenaIncorrecta_RetornaErrorSinGenerarToken()
    {
        // Arrange
        var usuario = CrearUsuario("Password123");
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetByEmail("ana@example.test")).Returns(usuario);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var servicio = new AuthService(repository.Object, tokenService.Object);
        var dto = new UsuarioLoginRequestDto
        {
            EmailOUserName = "ana@example.test",
            Password = "OtraPassword"
        };

        // Act
        Result resultado = servicio.IniciarSesion(dto, out UsuarioLoginResponseDto? respuesta);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("Las credenciales son incorrectas.", resultado.Error);
        Assert.Null(respuesta);
        tokenService.VerifyNoOtherCalls();
    }

    [Fact]
    public void IniciarSesion_UsuarioPorUserNameYTokenFallido_RetornaErrorDeToken()
    {
        // Arrange
        var usuario = CrearUsuario("Password123");
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetByEmail("ana")).Returns((Usuario?)null);
        repository.Setup(r => r.GetByUserName("ana")).Returns(usuario);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService
            .Setup(s => s.GenerarToken(
                It.Is<UsuarioTokenGeneracionDto>(dto =>
                    dto.IdUsuario == usuario.IdUsuario &&
                    dto.TipoToken == "INICIO_SESION" &&
                    dto.MinutosExpiracion == 60 &&
                    dto.UserName == usuario.UserName &&
                    dto.Role == usuario.Role),
                out It.Ref<string>.IsAny))
            .Returns((UsuarioTokenGeneracionDto _, out string tokenPlano) =>
            {
                tokenPlano = string.Empty;
                return (Result.Fail("No se pudo generar el token."), string.Empty);
            });
        var servicio = new AuthService(repository.Object, tokenService.Object);
        var dto = new UsuarioLoginRequestDto { EmailOUserName = "ana", Password = "Password123" };

        // Act
        Result resultado = servicio.IniciarSesion(dto, out UsuarioLoginResponseDto? respuesta);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("No se pudo generar el token.", resultado.Error);
        Assert.Null(respuesta);
        repository.Verify(r => r.GetByEmail("ana"), Times.Once);
        repository.Verify(r => r.GetByUserName("ana"), Times.Once);
        tokenService.Verify(s => s.GenerarToken(
            It.IsAny<UsuarioTokenGeneracionDto>(),
            out It.Ref<string>.IsAny), Times.Once);
        tokenService.VerifyNoOtherCalls();
    }

    [Fact]
    public void IniciarSesion_CredencialesValidas_RetornaDatosDeSesion()
    {
        // Arrange
        var usuario = CrearUsuario("Password123");
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetByEmail("ana@example.test")).Returns(usuario);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService
            .Setup(s => s.GenerarToken(
                It.Is<UsuarioTokenGeneracionDto>(dto =>
                    dto.IdUsuario == usuario.IdUsuario &&
                    dto.TipoToken == "INICIO_SESION" &&
                    dto.MinutosExpiracion == 60),
                out It.Ref<string>.IsAny))
            .Returns((UsuarioTokenGeneracionDto _, out string tokenPlano) =>
            {
                tokenPlano = "token-plano";
                return (Result.Ok(), "token-firmado");
            });
        var servicio = new AuthService(repository.Object, tokenService.Object);
        var dto = new UsuarioLoginRequestDto
        {
            EmailOUserName = " ana@example.test ",
            Password = " Password123 "
        };

        // Act
        Result resultado = servicio.IniciarSesion(dto, out UsuarioLoginResponseDto? respuesta);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
        var datosSesion = Assert.IsType<UsuarioLoginResponseDto>(respuesta);
        Assert.Equal(usuario.IdUsuario, datosSesion.IdUsuario);
        Assert.Equal(usuario.UserName, datosSesion.UserName);
        Assert.Equal(usuario.Role, datosSesion.Role);
        Assert.True(datosSesion.MustChangePassword);
        Assert.Equal("token-firmado", datosSesion.Token);
        Assert.Equal(60, datosSesion.ExpiraEn);
        repository.Verify(r => r.GetByEmail("ana@example.test"), Times.Once);
        repository.Verify(r => r.GetByUserName(It.IsAny<string>()), Times.Never);
        tokenService.Verify(s => s.GenerarToken(
            It.IsAny<UsuarioTokenGeneracionDto>(),
            out It.Ref<string>.IsAny), Times.Once);
        repository.VerifyNoOtherCalls();
        tokenService.VerifyNoOtherCalls();
    }

    private static Usuario CrearUsuario(string password)
    {
        return new Usuario
        {
            IdUsuario = 12,
            Nombres = "Ana",
            Email = "ana@example.test",
            UserName = "ana",
            Role = "Admin",
            Activo = 1,
            MustChangePassword = 1,
            PasswordHash = PasswordHelper.Hash(password)
        };
    }
}
