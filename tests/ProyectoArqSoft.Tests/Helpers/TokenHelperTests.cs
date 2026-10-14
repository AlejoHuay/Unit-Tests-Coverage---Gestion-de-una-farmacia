
using System;
using System.Security.Cryptography;
using System.Text;
using ProyectoArqSoft.Infrastructure.Helpers;
using Xunit;

namespace ProyectoArqSoft.Tests.Helpers;

public class TokenHelperTests
{
    // =====================================================
    // METODO 1: GenerarTokenPlano()
    // =====================================================

    [Fact]
    public void GenerarTokenPlano_Con32Bytes_RetornaBase64Valido()
    {
        // Arrange
        int cantidadBytes = 32;

        // Act
        string resultado = TokenHelper.GenerarTokenPlano(cantidadBytes);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(resultado));

        byte[] bytesDecodificados = Convert.FromBase64String(resultado);
        Assert.Equal(cantidadBytes, bytesDecodificados.Length);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(64)]
    public void GenerarTokenPlano_ConCantidadesValidas_RetornaLongitudCorrecta(
        int cantidadBytes)
    {
        // Arrange
        int longitudEsperada = cantidadBytes;

        // Act
        string resultado = TokenHelper.GenerarTokenPlano(cantidadBytes);

        // Assert
        byte[] bytesDecodificados = Convert.FromBase64String(resultado);
        Assert.Equal(longitudEsperada, bytesDecodificados.Length);
    }

    [Fact]
    public void GenerarTokenPlano_SinArgumento_Utiliza32Bytes()
    {
        // Arrange
        const int cantidadEsperada = 32;

        // Act
        string resultado = TokenHelper.GenerarTokenPlano();

        // Assert
        byte[] bytesDecodificados = Convert.FromBase64String(resultado);
        Assert.Equal(cantidadEsperada, bytesDecodificados.Length);
    }

    [Fact]
    public void GenerarTokenPlano_ConCeroBytes_RetornaCadenaVacia()
    {
        // Arrange
        const int cantidadBytes = 0;

        // Act
        string resultado = TokenHelper.GenerarTokenPlano(cantidadBytes);

        // Assert
        Assert.Equal(string.Empty, resultado);
    }

    [Fact]
    public void GenerarTokenPlano_ConCantidadNegativa_LanzaExcepcion()
    {
        // Arrange
        const int cantidadBytes = -1;

        // Act
        Action accion = () => TokenHelper.GenerarTokenPlano(cantidadBytes);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(accion);
    }

    // =====================================================
    // METODO 2: GenerarTokenHash()
    // =====================================================

    // TC1, TC1-A, TC1-B
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerarTokenHash_ConTokenInvalido_LanzaArgumentException(
        string? tokenPlano)
    {
        // Arrange
        string? tokenInvalido = tokenPlano;

        // Act
        Action accion = () => TokenHelper.GenerarTokenHash(tokenInvalido!);

        // Assert
        ArgumentException excepcion = Assert.Throws<ArgumentException>(accion);
        Assert.Equal("tokenPlano", excepcion.ParamName);
    }

    // TC3 y TC3-A
    [Fact]
    public void GenerarTokenHash_ConAbc_RetornaSha256Esperado()
    {
        // Arrange
        const string tokenPlano = "abc";
        const string hashEsperado =
            "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

        // Act
        string resultado = TokenHelper.GenerarTokenHash(tokenPlano);

        // Assert
        Assert.Equal(hashEsperado, resultado);
        Assert.Equal(64, resultado.Length);
    }

    // TC3-B y TC3-C
    [Theory]
    [InlineData("Token123")]
    [InlineData("áéí")]
    public void GenerarTokenHash_ConTokenValido_CoincideConSha256Utf8(
        string tokenPlano)
    {
        // Arrange
        byte[] bytes = Encoding.UTF8.GetBytes(tokenPlano);
        byte[] hash = SHA256.HashData(bytes);
        string hashEsperado = Convert.ToHexString(hash).ToLowerInvariant();

        // Act
        string resultado = TokenHelper.GenerarTokenHash(tokenPlano);

        // Assert
        Assert.Equal(hashEsperado, resultado);
        Assert.Equal(64, resultado.Length);
    }

    // =====================================================
// METODO 3: GenerarFechaExpiracion()
// =====================================================

// TC1, TC1-A y TC1-B: minutos invalidos
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-60)]
public void GenerarFechaExpiracion_ConMinutosInvalidos_LanzaArgumentException(
    int minutosExpiracion)
{
    // Arrange
    int minutosInvalidos = minutosExpiracion;

    // Act
    Action accion = () =>
        TokenHelper.GenerarFechaExpiracion(minutosInvalidos);

    // Assert
    ArgumentException excepcion = Assert.Throws<ArgumentException>(accion);
    Assert.Equal("minutosExpiracion", excepcion.ParamName);
}

// TC2, TC2-A y TC2-B: minutos validos
[Theory]
[InlineData(30)]
[InlineData(1)]
[InlineData(60)]
public void GenerarFechaExpiracion_ConMinutosValidos_RetornaFechaUtcCorrecta(
    int minutosExpiracion)
{
    // Arrange
    DateTime antes = DateTime.UtcNow;

    // Act
    DateTime resultado =
        TokenHelper.GenerarFechaExpiracion(minutosExpiracion);

    DateTime despues = DateTime.UtcNow;

    // Assert
    Assert.Equal(DateTimeKind.Utc, resultado.Kind);
    Assert.InRange(
        resultado,
        antes.AddMinutes(minutosExpiracion),
        despues.AddMinutes(minutosExpiracion)
    );
}
}
