using ProyectoArqSoft.Domain.DTOs;
using System.IdentityModel.Tokens.Jwt;
using ProyectoArqSoft.Infrastructure.Helpers;
using ProyectoArqSoft.Domain.Models;
using Moq;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class TokenServiceTests
{
    [Fact]
    public void EliminarTokensObsoletos_ConDiasCero_DevuelveErrorYNoLlamaRepositorio()
    {
        // Arrange
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.EliminarTokensObsoletos(0);

        // Assert
        Assert.False(resultado.IsSuccess);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void EliminarTokensObsoletos_ConDiasPositivos_DevuelveExitoYLlamaRepositorio()
    {
        // Arrange
        const int dias = 1;
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        repository.Setup(r => r.EliminarTokensObsoletos(dias)).Returns(1);
        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.EliminarTokensObsoletos(dias);

        // Assert
        Assert.True(resultado.IsSuccess);
        repository.Verify(r => r.EliminarTokensObsoletos(dias), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void EliminarTokensObsoletos_ConDiasNegativos_DevuelveErrorYNoLlamaRepositorio()
    {
        // Arrange
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.EliminarTokensObsoletos(-1);

        // Assert
        Assert.False(resultado.IsSuccess);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void MarcarComoUsado_ConIdCero_DevuelveErrorSinConsultarRepositorio()
    {
        // Arrange - TC1: 1-2-3-8
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.MarcarComoUsado(0);

        // Assert
        Assert.False(resultado.IsSuccess);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void MarcarComoUsado_ConFilasAfectadas_DevuelveExito()
    {
        // Arrange - TC2: 1-2-4-5-6-8
        const int idUsuarioToken = 1;
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        repository.Setup(r => r.MarcarComoUsado(idUsuarioToken)).Returns(1);
        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.MarcarComoUsado(idUsuarioToken);

        // Assert
        Assert.True(resultado.IsSuccess);
        repository.Verify(r => r.MarcarComoUsado(idUsuarioToken), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void MarcarComoUsado_SinFilasAfectadas_DevuelveError()
    {
        // Arrange - TC3: 1-2-4-5-7-8
        const int idUsuarioToken = 1;
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        repository.Setup(r => r.MarcarComoUsado(idUsuarioToken)).Returns(0);
        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.MarcarComoUsado(idUsuarioToken);

        // Assert
        Assert.False(resultado.IsSuccess);
        repository.Verify(r => r.MarcarComoUsado(idUsuarioToken), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void RevocarTokensActivos_ConIdCero_DevuelveErrorSinLlamarRepositorio()
    {
        // Arrange - TC1: 1-2-3-11
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.RevocarTokensActivos(0, "ACTIVACION");

        // Assert
        Assert.False(resultado.IsSuccess);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void RevocarTokensActivos_ConTipoNulo_DevuelveErrorSinLlamarRepositorio()
    {
        // Arrange - TC2: 1-2-4-5-7-8-11
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.RevocarTokensActivos(1, null!);

        // Assert
        Assert.False(resultado.IsSuccess);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void RevocarTokensActivos_ConTipoSoloEspacios_DevuelveErrorSinLlamarRepositorio()
    {
        // Arrange - TC3: 1-2-4-6-7-8-11
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.RevocarTokensActivos(1, "   ");

        // Assert
        Assert.False(resultado.IsSuccess);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void RevocarTokensActivos_ConDatosValidos_RecortaTipoYDevuelveExito()
    {
        // Arrange - TC4: 1-2-4-6-7-9-10-11
        const int idUsuario = 1;
        const string tipoNormalizado = "ACTIVACION";

        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

        repository
            .Setup(r => r.RevocarTokensActivos(idUsuario, tipoNormalizado))
            .Returns(1);

        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.RevocarTokensActivos(
            idUsuario,
            " ACTIVACION "
        );

        // Assert
        Assert.True(resultado.IsSuccess);

        repository.Verify(
            r => r.RevocarTokensActivos(idUsuario, tipoNormalizado),
            Times.Once
        );

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void ValidarToken_CuandoNoExiste_DevuelveNull()
    {
        // Arrange - TC1: 1-2-3-4-6-8
        const string tokenPlano = "token-de-prueba";
        const string tipoToken = "ACTIVACION";

        string tokenHash = TokenHelper.GenerarTokenHash(tokenPlano);

        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        repository
            .Setup(r => r.GetTokenActivo(tokenHash, tipoToken))
            .Returns((UsuarioToken?)null);

        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.ValidarToken(tokenPlano, tipoToken);

        // Assert
        Assert.Null(resultado);
        repository.Verify(
            r => r.GetTokenActivo(tokenHash, tipoToken),
            Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void ValidarToken_CuandoEstaExpirado_DevuelveNull()
    {
        // Arrange - TC2: 1-2-3-4-5-6-8
        const string tokenPlano = "token-de-prueba";
        const string tipoToken = "ACTIVACION";

        string tokenHash = TokenHelper.GenerarTokenHash(tokenPlano);

        var tokenExpirado = new UsuarioToken(
            1,
            tokenHash,
            tipoToken,
            DateTime.UtcNow.AddDays(-1)
        );

        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        repository
            .Setup(r => r.GetTokenActivo(tokenHash, tipoToken))
            .Returns(tokenExpirado);

        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.ValidarToken(tokenPlano, tipoToken);

        // Assert
        Assert.Null(resultado);
        repository.Verify(
            r => r.GetTokenActivo(tokenHash, tipoToken),
            Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void ValidarToken_CuandoEstaVigente_DevuelveMismoToken()
    {
        // Arrange - TC3: 1-2-3-4-5-7-8
        const string tokenPlano = "token-de-prueba";
        const string tipoToken = "ACTIVACION";

        string tokenHash = TokenHelper.GenerarTokenHash(tokenPlano);

        var tokenVigente = new UsuarioToken(
            1,
            tokenHash,
            tipoToken,
            DateTime.UtcNow.AddDays(1)
        );

        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        repository
            .Setup(r => r.GetTokenActivo(tokenHash, tipoToken))
            .Returns(tokenVigente);

        var servicio = new TokenService(repository.Object);

        // Act
        var resultado = servicio.ValidarToken(tokenPlano, tipoToken);

        // Assert
        Assert.Same(tokenVigente, resultado);
        repository.Verify(
            r => r.GetTokenActivo(tokenHash, tipoToken),
            Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void GenerarToken_CuandoInsertFalla_DevuelveErrorYTokenVacio()
    {
        // Arrange - TC1: 1-2-3-4-5-15-16-17
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        repository.Setup(r => r.Insert(It.IsAny<UsuarioToken>())).Returns(0);

        var servicio = new TokenService(repository.Object);
        var dto = new UsuarioTokenGeneracionDto
        {
            IdUsuario = 1,
            TipoToken = TipoTokenConstantes.ActivacionCuenta,
            MinutosExpiracion = 60
        };

        // Act
        var (resultado, token) = servicio.GenerarToken(dto, out string tokenPlano);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(string.Empty, token);
        Assert.Equal(string.Empty, tokenPlano);

        repository.Verify(r => r.Insert(It.IsAny<UsuarioToken>()), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void GenerarToken_ActivacionCuenta_RetornaTokenPlano()
    {
        // Arrange - TC2: 1-2-3-4-5-6-8-17
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        repository.Setup(r => r.Insert(It.IsAny<UsuarioToken>())).Returns(1);

        var servicio = new TokenService(repository.Object);
        var dto = new UsuarioTokenGeneracionDto
        {
            IdUsuario = 1,
            TipoToken = TipoTokenConstantes.ActivacionCuenta,
            MinutosExpiracion = 60
        };

        // Act
        var (resultado, token) = servicio.GenerarToken(dto, out string tokenPlano);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(tokenPlano));
        Assert.Equal(tokenPlano, token);

        repository.Verify(
            r => r.Insert(It.Is<UsuarioToken>(t =>
                t.UsuarioIdUsuario == dto.IdUsuario &&
                t.TipoToken == dto.TipoToken &&
                t.TokenHash == TokenHelper.GenerarTokenHash(tokenPlano))),
            Times.Once);

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void GenerarToken_ResetPassword_RetornaTokenPlano()
    {
        // Arrange - TC3: 1-2-3-4-5-6-7-8-17
        var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
        repository.Setup(r => r.Insert(It.IsAny<UsuarioToken>())).Returns(1);

        var servicio = new TokenService(repository.Object);
        var dto = new UsuarioTokenGeneracionDto
        {
            IdUsuario = 1,
            TipoToken = TipoTokenConstantes.ResetPassword,
            MinutosExpiracion = 60
        };

        // Act
        var (resultado, token) = servicio.GenerarToken(dto, out string tokenPlano);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(tokenPlano));
        Assert.Equal(tokenPlano, token);

        repository.Verify(
            r => r.Insert(It.Is<UsuarioToken>(t =>
                t.UsuarioIdUsuario == dto.IdUsuario &&
                t.TipoToken == dto.TipoToken &&
                t.TokenHash == TokenHelper.GenerarTokenHash(tokenPlano))),
            Times.Once);

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void GenerarToken_InicioSesion_RetornaJwtValido()
    {
        // Arrange - TC4: 1-2-3-4-5-6-7-9-10-11-12-13-14-17
        const string keyName = "JWT_KEY";
        const string issuerName = "JWT_ISSUER";
        const string audienceName = "JWT_AUDIENCE";

        string? keyAnterior = Environment.GetEnvironmentVariable(keyName);
        string? issuerAnterior = Environment.GetEnvironmentVariable(issuerName);
        string? audienceAnterior = Environment.GetEnvironmentVariable(audienceName);

        const string issuer = "VitalCareTests";
        const string audience = "VitalCareTestUsers";

        try
        {
            Environment.SetEnvironmentVariable(
                keyName,
                "VitalCareTestSigningKey_0123456789_ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            );
            Environment.SetEnvironmentVariable(issuerName, issuer);
            Environment.SetEnvironmentVariable(audienceName, audience);

            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            repository.Setup(r => r.Insert(It.IsAny<UsuarioToken>())).Returns(1);

            var servicio = new TokenService(repository.Object);
            var dto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = 1,
                TipoToken = "INICIO_SESION",
                MinutosExpiracion = 60,
                UserName = "usuario.prueba",
                Role = "ADMIN"
            };

            // Act
            var (resultado, token) = servicio.GenerarToken(dto, out string tokenPlano);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(tokenPlano));
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.NotEqual(tokenPlano, token);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.Equal(issuer, jwt.Issuer);
            Assert.Contains(audience, jwt.Audiences);
            Assert.True(jwt.ValidTo > DateTime.UtcNow);

            repository.Verify(
                r => r.Insert(It.IsAny<UsuarioToken>()),
                Times.Once);

            repository.VerifyNoOtherCalls();
        }
        finally
        {
            Environment.SetEnvironmentVariable(keyName, keyAnterior);
            Environment.SetEnvironmentVariable(issuerName, issuerAnterior);
            Environment.SetEnvironmentVariable(audienceName, audienceAnterior);
        }
    }
}