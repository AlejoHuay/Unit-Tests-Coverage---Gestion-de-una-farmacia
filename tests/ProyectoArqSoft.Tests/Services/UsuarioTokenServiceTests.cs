using ProyectoArqSoft.Domain.DTOs;
using Moq;
using Xunit;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Application.Ports.Output;

namespace ProyectoArqSoft.Tests.Services
{
    public class UsuarioTokenServiceTests
    {
        [Fact]
        public void EliminarTokensObsoletos_ConDiasCero_DevuelveErrorSinLlamarRepositorio()
        {
            // Arrange - TC1: 1 -> 2 -> 3 -> 6
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.EliminarTokensObsoletos(0);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(resultado.Error));

            repository.Verify(
                r => r.EliminarTokensObsoletos(It.IsAny<int>()),
                Times.Never);

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void EliminarTokensObsoletos_ConDiasPositivos_DevuelveExitoYLlamaRepositorio()
        {
            // Arrange - TC2: 1 -> 2 -> 4 -> 5 -> 6
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

            repository
                .Setup(r => r.EliminarTokensObsoletos(7))
                .Returns(1);

            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.EliminarTokensObsoletos(7);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(string.Empty, resultado.Error);

            repository.Verify(
                r => r.EliminarTokensObsoletos(7),
                Times.Once);

            repository.VerifyNoOtherCalls();
        }
    
        [Fact]
        public void RevocarTokensActivos_ConIdCero_DevuelveErrorSinLlamarRepositorio()
        {
            // Arrange - TC1: 1 -> 2 -> 3 -> 9
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.RevocarTokensActivos(
                0,
                "RESET_PASSWORD"
            );

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El id del usuario debe ser mayor a cero.",
                resultado.Error
            );

            repository.Verify(
                r => r.RevocarTokensActivos(
                    It.IsAny<int>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void RevocarTokensActivos_ConTipoSoloEspacios_DevuelveErrorSinLlamarRepositorio()
        {
            // Arrange - TC2: 1 -> 2 -> 4 -> 5 -> 6 -> 9
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.RevocarTokensActivos(
                1,
                "   "
            );

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El tipo de token es obligatorio.",
                resultado.Error
            );

            repository.Verify(
                r => r.RevocarTokensActivos(
                    It.IsAny<int>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void RevocarTokensActivos_ConDatosValidos_RecortaTipoYDevuelveExito()
        {
            // Arrange - TC3: 1 -> 2 -> 4 -> 5 -> 7 -> 8 -> 9
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

            repository
                .Setup(r => r.RevocarTokensActivos(1, "RESET_PASSWORD"))
                .Returns(1);

            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.RevocarTokensActivos(
                1,
                " RESET_PASSWORD "
            );

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(string.Empty, resultado.Error);

            repository.Verify(
                r => r.RevocarTokensActivos(1, "RESET_PASSWORD"),
                Times.Once
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void MarcarComoUsado_ConIdCero_DevuelveErrorSinLlamarRepositorio()
        {
            // Arrange - TC1: 1 -> 2 -> 3 -> 8
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.MarcarComoUsado(0);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El id del token debe ser mayor a cero.",
                resultado.Error
            );

            repository.Verify(
                r => r.MarcarComoUsado(It.IsAny<int>()),
                Times.Never
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void MarcarComoUsado_ConFilasAfectadas_DevuelveExito()
        {
            // Arrange - TC2: 1 -> 2 -> 4 -> 5 -> 6 -> 8
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

            repository
                .Setup(r => r.MarcarComoUsado(1))
                .Returns(1);

            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.MarcarComoUsado(1);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(string.Empty, resultado.Error);

            repository.Verify(
                r => r.MarcarComoUsado(1),
                Times.Once
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void MarcarComoUsado_SinFilasAfectadas_DevuelveError()
        {
            // Arrange - TC3: 1 -> 2 -> 4 -> 5 -> 7 -> 8
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

            repository
                .Setup(r => r.MarcarComoUsado(1))
                .Returns(0);

            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.MarcarComoUsado(1);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "No se pudo marcar el token como usado.",
                resultado.Error
            );

            repository.Verify(
                r => r.MarcarComoUsado(1),
                Times.Once
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void ValidarToken_ConTokenPlanoVacio_DevuelveNullSinConsultarRepositorio()
        {
            // Arrange - TC1: 1 -> 2 -> 3 -> 5 -> 12
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.ValidarToken(
                "   ",
                "RESET_PASSWORD"
            );

            // Assert
            Assert.Null(resultado);

            repository.Verify(
                r => r.GetTokenActivo(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void ValidarToken_ConTipoTokenVacio_DevuelveNullSinConsultarRepositorio()
        {
            // Arrange - TC2: 1 -> 2 -> 3 -> 4 -> 5 -> 12
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.ValidarToken(
                "token-valido",
                "   "
            );

            // Assert
            Assert.Null(resultado);

            repository.Verify(
                r => r.GetTokenActivo(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void ValidarToken_CuandoNoExiste_DevuelveNull()
        {
            // Arrange - TC3: 1 -> 2 -> 3 -> 4 -> 6 -> 7 -> 8 -> 10 -> 12
            const string tokenPlano = "token-inexistente";
            const string tipoToken = "RESET_PASSWORD";

            string tokenHash =
                ProyectoArqSoft.Infrastructure.Helpers.TokenHelper
                    .GenerarTokenHash(tokenPlano);

            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

            repository
                .Setup(r => r.GetTokenActivo(tokenHash, tipoToken))
                .Returns((ProyectoArqSoft.Domain.Models.UsuarioToken?)null);

            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.ValidarToken(tokenPlano, tipoToken);

            // Assert
            Assert.Null(resultado);

            repository.Verify(
                r => r.GetTokenActivo(tokenHash, tipoToken),
                Times.Once
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void ValidarToken_CuandoEstaExpirado_DevuelveNull()
        {
            // Arrange - TC4: 1 -> 2 -> 3 -> 4 -> 6 -> 7 -> 8 -> 9 -> 10 -> 12
            const string tokenPlano = "token-expirado";
            const string tipoToken = "RESET_PASSWORD";

            string tokenHash =
                ProyectoArqSoft.Infrastructure.Helpers.TokenHelper
                    .GenerarTokenHash(tokenPlano);

            var token = new ProyectoArqSoft.Domain.Models.UsuarioToken(
                1,
                tokenHash,
                tipoToken,
                DateTime.UtcNow.AddHours(-1)
            );

            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

            repository
                .Setup(r => r.GetTokenActivo(tokenHash, tipoToken))
                .Returns(token);

            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.ValidarToken(tokenPlano, tipoToken);

            // Assert
            Assert.Null(resultado);

            repository.Verify(
                r => r.GetTokenActivo(tokenHash, tipoToken),
                Times.Once
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void ValidarToken_CuandoEstaVigente_DevuelveMismoToken()
        {
            // Arrange - TC5: 1 -> 2 -> 3 -> 4 -> 6 -> 7 -> 8 -> 9 -> 11 -> 12
            const string tokenPlano = "token-vigente";
            const string tipoToken = "RESET_PASSWORD";

            string tokenHash =
                ProyectoArqSoft.Infrastructure.Helpers.TokenHelper
                    .GenerarTokenHash(tokenPlano);

            var token = new ProyectoArqSoft.Domain.Models.UsuarioToken(
                1,
                tokenHash,
                tipoToken,
                DateTime.UtcNow.AddHours(1)
            );

            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

            repository
                .Setup(r => r.GetTokenActivo(tokenHash, tipoToken))
                .Returns(token);

            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.ValidarToken(
                " token-vigente ",
                " RESET_PASSWORD "
            );

            // Assert
            Assert.NotNull(resultado);
            Assert.Same(token, resultado);

            repository.Verify(
                r => r.GetTokenActivo(tokenHash, tipoToken),
                Times.Once
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void GenerarToken_ConDtoNulo_DevuelveErrorSinLlamarRepositorio()
        {
            // Arrange - TC1: 1 -> 2 -> 3 -> 4 -> 5 -> 16
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            // Act
            var resultado = service.GenerarToken(
                null!,
                out string tokenPlano
            );

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "Los datos del token no pueden ser nulos.",
                resultado.Error
            );
            Assert.Equal(string.Empty, tokenPlano);

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void GenerarToken_CuandoInsertFalla_DevuelveErrorYTokenVacio()
        {
            // Arrange - TC2: 1 -> 2 -> 3 -> 4 -> 6 -> 7 -> 8
            // -> 9 -> 10 -> 11 -> 12 -> 13 -> 14 -> 16

            var operaciones = new MockSequence();

            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

            repository.InSequence(operaciones)
                .Setup(r => r.EliminarTokensObsoletos(7))
                .Returns(1);

            repository.InSequence(operaciones)
                .Setup(r => r.RevocarTokensActivos(1, "RESET_PASSWORD"))
                .Returns(1);

            repository.InSequence(operaciones)
                .Setup(r => r.Insert(
                    It.IsAny<ProyectoArqSoft.Domain.Models.UsuarioToken>()
                ))
                .Returns(0);

            var service = new UsuarioTokenService(repository.Object);

            var dto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = 1,
                TipoToken = " RESET_PASSWORD ",
                MinutosExpiracion = 60
            };

            // Act
            var resultado = service.GenerarToken(
                dto,
                out string tokenPlano
            );

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "No se pudo generar el token.",
                resultado.Error
            );
            Assert.Equal(string.Empty, tokenPlano);

            repository.Verify(r => r.EliminarTokensObsoletos(7), Times.Once);
            repository.Verify(
                r => r.RevocarTokensActivos(1, "RESET_PASSWORD"),
                Times.Once
            );
            repository.Verify(
                r => r.Insert(
                    It.IsAny<ProyectoArqSoft.Domain.Models.UsuarioToken>()
                ),
                Times.Once
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void GenerarToken_CuandoInsertEsExitoso_DevuelveTokenPlano()
        {
            // Arrange - TC3: 1 -> 2 -> 3 -> 4 -> 6 -> 7 -> 8
            // -> 9 -> 10 -> 11 -> 12 -> 15 -> 16

            var operaciones = new MockSequence();

            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);

            repository.InSequence(operaciones)
                .Setup(r => r.EliminarTokensObsoletos(7))
                .Returns(1);

            repository.InSequence(operaciones)
                .Setup(r => r.RevocarTokensActivos(1, "RESET_PASSWORD"))
                .Returns(1);

            repository.InSequence(operaciones)
                .Setup(r => r.Insert(
                    It.IsAny<ProyectoArqSoft.Domain.Models.UsuarioToken>()
                ))
                .Returns(1);

            var service = new UsuarioTokenService(repository.Object);

            var dto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = 1,
                TipoToken = " RESET_PASSWORD ",
                MinutosExpiracion = 60
            };

            // Act
            var resultado = service.GenerarToken(
                dto,
                out string tokenPlano
            );

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(string.Empty, resultado.Error);
            Assert.False(string.IsNullOrWhiteSpace(tokenPlano));

            repository.Verify(r => r.EliminarTokensObsoletos(7), Times.Once);
            repository.Verify(
                r => r.RevocarTokensActivos(1, "RESET_PASSWORD"),
                Times.Once
            );
            repository.Verify(
                r => r.Insert(
                    It.Is<ProyectoArqSoft.Domain.Models.UsuarioToken>(t =>
                        t.UsuarioIdUsuario == dto.IdUsuario &&
                        t.TipoToken == "RESET_PASSWORD" &&
                        t.TokenHash ==
                            ProyectoArqSoft.Infrastructure.Helpers.TokenHelper
                                .GenerarTokenHash(tokenPlano) &&
                        t.FechaExpiracion > DateTime.UtcNow
                    )
                ),
                Times.Once
            );

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void ValidarGeneracionToken_ConIdUsuarioCero_DevuelveErrorSinLlamarRepositorio()
        {
            // Arrange - TC2: 1 -> 2 -> 4 -> 5 -> 14
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            var dto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = 0,
                TipoToken = "RESET_PASSWORD",
                MinutosExpiracion = 60
            };

            // Act
            var resultado = service.GenerarToken(dto, out string tokenPlano);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El id del usuario debe ser mayor a cero.",
                resultado.Error
            );
            Assert.Equal(string.Empty, tokenPlano);

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void ValidarGeneracionToken_ConTipoVacio_DevuelveErrorSinLlamarRepositorio()
        {
            // Arrange - TC3: 1 -> 2 -> 4 -> 6 -> 7 -> 8 -> 14
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            var dto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = 1,
                TipoToken = "   ",
                MinutosExpiracion = 60
            };

            // Act
            var resultado = service.GenerarToken(dto, out string tokenPlano);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El tipo de token es obligatorio.",
                resultado.Error
            );
            Assert.Equal(string.Empty, tokenPlano);

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void ValidarGeneracionToken_ConTipoMayorA50_DevuelveErrorSinLlamarRepositorio()
        {
            // Arrange - TC4: 1 -> 2 -> 4 -> 6 -> 7 -> 9 -> 10 -> 14
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            var dto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = 1,
                TipoToken = new string('A', 51),
                MinutosExpiracion = 60
            };

            // Act
            var resultado = service.GenerarToken(dto, out string tokenPlano);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El tipo de token no puede tener más de 50 caracteres.",
                resultado.Error
            );
            Assert.Equal(string.Empty, tokenPlano);

            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public void ValidarGeneracionToken_ConMinutosCero_DevuelveErrorSinLlamarRepositorio()
        {
            // Arrange - TC5: 1 -> 2 -> 4 -> 6 -> 7 -> 9 -> 11 -> 12 -> 14
            var repository = new Mock<IUsuarioTokenRepository>(MockBehavior.Strict);
            var service = new UsuarioTokenService(repository.Object);

            var dto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = 1,
                TipoToken = "RESET_PASSWORD",
                MinutosExpiracion = 0
            };

            // Act
            var resultado = service.GenerarToken(dto, out string tokenPlano);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "Los minutos de expiración deben ser mayores a cero.",
                resultado.Error
            );
            Assert.Equal(string.Empty, tokenPlano);

            repository.VerifyNoOtherCalls();
        }
}
}