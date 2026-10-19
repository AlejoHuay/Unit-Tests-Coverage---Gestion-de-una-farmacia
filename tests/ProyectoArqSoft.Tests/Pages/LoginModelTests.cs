using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Pages.Auth;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

public class LoginModelTests
{
    [Fact]
    public void OnPost_AutenticacionFallida_MuestraErrorYRetornaPage()
    {
        // Arrange - Camino C1
        var authService = new Mock<IAuthService>(MockBehavior.Strict);
        UsuarioLoginResponseDto? respuesta = null;

        authService
            .Setup(s => s.IniciarSesion(
                It.IsAny<UsuarioLoginRequestDto>(),
                out respuesta))
            .Returns(Result.Fail("Credenciales incorrectas"));

        var modelo = new LoginModel(authService.Object)
        {
            PageContext = PaginaTestContext.Crear(
                idUsuario: null, usuario: null, rol: null)
        };

        // Act
        IActionResult resultado = modelo.OnPost();

        // Assert
        Assert.IsType<PageResult>(resultado);
        Assert.Equal("Credenciales incorrectas", modelo.MensajeError);

        authService.Verify(s => s.IniciarSesion(
            It.IsAny<UsuarioLoginRequestDto>(),
            out respuesta), Times.Once);
    }

    [Fact]
    public void OnPost_RespuestaNula_MuestraErrorYRetornaPage()
    {
        // Arrange - Camino C2
        var authService = new Mock<IAuthService>(MockBehavior.Strict);
        UsuarioLoginResponseDto? respuesta = null;

        authService
            .Setup(s => s.IniciarSesion(
                It.IsAny<UsuarioLoginRequestDto>(),
                out respuesta))
            .Returns(Result.Ok());

        var modelo = new LoginModel(authService.Object)
        {
            PageContext = PaginaTestContext.Crear(
                idUsuario: null, usuario: null, rol: null)
        };

        // Act
        IActionResult resultado = modelo.OnPost();

        // Assert
        Assert.IsType<PageResult>(resultado);
        Assert.Equal("No se pudo iniciar sesi\u00F3n.", modelo.MensajeError);

        authService.Verify(s => s.IniciarSesion(
            It.IsAny<UsuarioLoginRequestDto>(),
            out respuesta), Times.Once);
    }

    [Fact]
    public void OnPost_AutenticacionExitosa_GuardaSesionYRedirige()
    {
        // Arrange - Camino C3
        var authService = new Mock<IAuthService>(MockBehavior.Strict);

        UsuarioLoginResponseDto? respuesta = new()
        {
            Token = "token-prueba",
            IdUsuario = 25,
            UserName = "admin",
            Role = "Administrador",
            MustChangePassword = true
        };

        authService
            .Setup(s => s.IniciarSesion(
                It.IsAny<UsuarioLoginRequestDto>(),
                out respuesta))
            .Returns(Result.Ok());

        var modelo = new LoginModel(authService.Object)
        {
            PageContext = PaginaTestContext.Crear(
                idUsuario: null, usuario: null, rol: null)
        };

        // Act
        IActionResult resultado = modelo.OnPost();

        // Assert
        var redireccion = Assert.IsType<RedirectToPageResult>(resultado);
        Assert.Equal("/Index", redireccion.PageName);

        var sesion = modelo.HttpContext.Session;

        Assert.Equal("token-prueba", sesion.GetString("Token"));
        Assert.Equal(25, sesion.GetInt32("IdUsuario"));
        Assert.Equal("admin", sesion.GetString("UserName"));
        Assert.Equal("Administrador", sesion.GetString("Role"));
        Assert.Equal("True", sesion.GetString("MustChangePassword"));

        authService.Verify(s => s.IniciarSesion(
            It.IsAny<UsuarioLoginRequestDto>(),
            out respuesta), Times.Once);
    }

    [Fact]
    public void OnGet_ConTokenEnSesion_RedirigeAIndex()
    {
        // Arrange - Camino C1
        var authService = new Mock<IAuthService>(MockBehavior.Strict);
        var modelo = new LoginModel(authService.Object)
        {
            PageContext = PaginaTestContext.Crear(
                idUsuario: null, usuario: null, rol: null)
        };

        modelo.HttpContext.Session.SetString("Token", "token-valido");

        // Act
        IActionResult resultado = modelo.OnGet();

        // Assert
        var redireccion = Assert.IsType<RedirectToPageResult>(resultado);
        Assert.Equal("/Index", redireccion.PageName);
        authService.VerifyNoOtherCalls();
    }

    [Fact]
    public void OnGet_SinTokenEnSesion_RetornaPage()
    {
        // Arrange - Camino C2
        var authService = new Mock<IAuthService>(MockBehavior.Strict);
        var modelo = new LoginModel(authService.Object)
        {
            PageContext = PaginaTestContext.Crear(
                idUsuario: null, usuario: null, rol: null)
        };

        // Act
        IActionResult resultado = modelo.OnGet();

        // Assert
        Assert.IsType<PageResult>(resultado);
        authService.VerifyNoOtherCalls();
    }
}