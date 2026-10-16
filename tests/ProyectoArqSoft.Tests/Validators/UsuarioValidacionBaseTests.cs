using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

public class UsuarioValidacionBaseTests
{
    private sealed class ValidadorPrueba : UsuarioValidacionBase
    {
        public static Result? ProbarRequerido(string? valor, string mensaje)
        {
            return Requerido(valor, mensaje);
        }

        public static Result? ProbarMaximo(string valor, int max, string mensaje)
        {
            return Maximo(valor, max, mensaje);
        }

        public static Result? ProbarRegexValido(string valor, string patron, string mensaje)
        {
            return RegexValido(valor, patron, mensaje);
        }

        public static Result? ProbarTextoOpcional(string valor, string campo, int maximo)
        {
            return TextoOpcional(valor, campo, maximo);
        }

        public static Result? ProbarTextoRequerido(string valor, string campo, int maximo)
        {
            return TextoRequerido(valor, campo, maximo);
        }

        public static Result? ProbarEmailValido(string? email, bool obligatorio = true)
        {
            return EmailValido(email!, obligatorio);
        }

        public static bool ProbarExtensionesValidas(string extension)
        {
            return ExtensionesValidas.Contains(extension);
        }
    }

    [Fact]
    public void Requerido_ValorNulo_RetornaError()
    {
        // Arrange
        string? valor = null;
        string mensaje = "Campo obligatorio";

        // Act
        Result? resultado = ValidadorPrueba.ProbarRequerido(valor, mensaje);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(mensaje, resultado.Error);
    }

    [Fact]
    public void Requerido_ValorValido_RetornaNull()
    {
        // Arrange
        string valor = "Ana";
        string mensaje = "Campo obligatorio";

        // Act
        Result? resultado = ValidadorPrueba.ProbarRequerido(valor, mensaje);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void Maximo_ValorExcedeLimite_RetornaError()
    {
        // Arrange
        string valor = "Alejandro";
        int max = 5;
        string mensaje = "Excede el máximo";

        // Act
        Result? resultado = ValidadorPrueba.ProbarMaximo(valor, max, mensaje);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(mensaje, resultado.Error);
    }

    [Fact]
    public void Maximo_ValorDentroDelLimite_RetornaNull()
    {
        // Arrange
        string valor = "Ana";
        int max = 5;
        string mensaje = "Excede el máximo";

        // Act
        Result? resultado = ValidadorPrueba.ProbarMaximo(valor, max, mensaje);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void RegexValido_FormatoValido_RetornaNull()
    {
        // Arrange
        string valor = "12345678";
        string patron = @"^\d{8}$";
        string mensaje = "Formato inválido";

        // Act
        Result? resultado = ValidadorPrueba.ProbarRegexValido(valor, patron, mensaje);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void RegexValido_FormatoInvalido_RetornaError()
    {
        // Arrange
        string valor = "ABC123";
        string patron = @"^\d{8}$";
        string mensaje = "Formato inválido";

        // Act
        Result? resultado = ValidadorPrueba.ProbarRegexValido(valor, patron, mensaje);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(mensaje, resultado.Error);
    }

    [Fact]
    public void TextoOpcional_ValorVacio_RetornaNull()
    {
        // Arrange
        string valor = "";
        string campo = "Nombre";
        int maximo = 5;

        // Act
        Result? resultado = ValidadorPrueba.ProbarTextoOpcional(valor, campo, maximo);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void TextoOpcional_ValorExcedeMaximo_RetornaError()
    {
        // Arrange
        string valor = "Alejandro";
        string campo = "Nombre";
        int maximo = 5;

        // Act
        Result? resultado = ValidadorPrueba.ProbarTextoOpcional(valor, campo, maximo);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El campo Nombre no puede tener más de 5 caracteres.",
            resultado.Error);
    }

    [Fact]
    public void TextoRequerido_ValorVacio_RetornaError()
    {
        // Arrange
        string valor = "";
        string campo = "Nombre";
        int maximo = 5;

        // Act
        Result? resultado = ValidadorPrueba.ProbarTextoRequerido(valor, campo, maximo);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El campo Nombre es obligatorio.",
            resultado.Error);
    }

    [Fact]
    public void TextoRequerido_ValorExcedeMaximo_RetornaError()
    {
        // Arrange
        string valor = "Alejandro";
        string campo = "Nombre";
        int maximo = 5;

        // Act
        Result? resultado = ValidadorPrueba.ProbarTextoRequerido(valor, campo, maximo);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El campo Nombre no puede tener más de 5 caracteres.",
            resultado.Error);
    }

    [Fact]
    public void EmailValido_ObligatorioVacio_RetornaError()
    {
        // Arrange
        string email = "";
        bool obligatorio = true;

        // Act
        Result? resultado = ValidadorPrueba.ProbarEmailValido(email, obligatorio);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El correo electrónico es obligatorio.", resultado.Error);
    }

    [Fact]
    public void EmailValido_OpcionalNulo_RetornaNull()
    {
        // Arrange
        string? email = null;
        bool obligatorio = false;

        // Act
        Result? resultado = ValidadorPrueba.ProbarEmailValido(email, obligatorio);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void EmailValido_ExcedeMaximo_RetornaError()
    {
        // Arrange
        string email = new string('a', 101);
        bool obligatorio = true;

        // Act
        Result? resultado = ValidadorPrueba.ProbarEmailValido(email, obligatorio);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El correo electrónico no puede tener más de 100 caracteres.",
            resultado.Error);
    }

    [Fact]
    public void EmailValido_FormatoInvalido_RetornaError()
    {
        // Arrange
        string email = "correo-invalido";
        bool obligatorio = false;

        // Act
        Result? resultado = ValidadorPrueba.ProbarEmailValido(email, obligatorio);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El formato del correo electrónico no es válido.",
            resultado.Error);
    }

    [Fact]
    public void EmailValido_ObligatorioValido_RetornaNull()
    {
        // Arrange
        string email = "ana@correo.com";
        bool obligatorio = true;

        // Act
        Result? resultado = ValidadorPrueba.ProbarEmailValido(email, obligatorio);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void EmailValido_OpcionalConEspacios_RetornaNull()
    {
        // Arrange
        string email = " ana@correo.com ";
        bool obligatorio = false;

        // Act
        Result? resultado = ValidadorPrueba.ProbarEmailValido(email, obligatorio);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void ExtensionesValidas_ContieneDepartamentos_SinDistinguirMayusculas()
    {
        // Arrange
        string extension = "lp";

        // Act
        bool resultado = ValidadorPrueba.ProbarExtensionesValidas(extension);

        // Assert
        Assert.True(resultado);
    }
}
