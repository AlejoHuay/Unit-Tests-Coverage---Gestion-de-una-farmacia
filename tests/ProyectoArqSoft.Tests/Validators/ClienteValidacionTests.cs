using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

public class ClienteValidacionTests
{
    [Fact]
    public void Validar_ConsumidorFinalAceptaSinRevisarOtrosCampos()
    {
        // Arrange
        var cliente = new Cliente { Nit = "cf", RazonSocial = "consumidor final", CorreoElectronico = "invalido" };
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Theory]
    [InlineData(null, "El NIT es obligatorio.")]
    [InlineData("", "El NIT es obligatorio.")]
    [InlineData("   ", "El NIT no debe contener solo espacios.")]
    [InlineData(" 12345", "El NIT no debe contener espacios al inicio o al final.")]
    [InlineData("123 45", "El NIT no debe contener espacios internos.")]
    [InlineData("12\t345", "El NIT no debe contener tabs ni saltos de linea.")]
    [InlineData("-12345", "El NIT no debe contener signos positivos ni negativos.")]
    [InlineData("+12345", "El NIT no debe contener signos positivos ni negativos.")]
    [InlineData("123.45", "El NIT no debe contener numeros decimales.")]
    [InlineData("ABCDE", "El NIT solo debe contener numeros, no letras.")]
    [InlineData("ABC\u00A0DE", "El NIT no debe contener texto ni espacios.")]
    [InlineData("12-345", "El NIT no debe contener simbolos ni caracteres especiales.")]
    [InlineData("12A-45", "El NIT no debe mezclar letras con simbolos o caracteres especiales.")]
    [InlineData("1234", "El NIT debe contener entre 5 y 12 digitos; faltan digitos.")]
    [InlineData("1234567890123", "El NIT debe contener entre 5 y 12 digitos; sobran digitos.")]
    [InlineData("00000", "El NIT no puede estar compuesto solo por ceros.")]
    public void Validar_NitInvalido_DevuelveError(string? nit, string error)
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.Nit = nit!;
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(error, resultado.Error);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("123456789012")]
    public void Validar_NitEnLimitesAcepta(string nit)
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.Nit = nit;
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_RazonSocialVacia_DevuelveError(string? razonSocial)
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.RazonSocial = razonSocial!;
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La razon social es obligatoria.", resultado.Error);
    }

    [Theory]
    [InlineData("Ab")]
    [InlineData("1234567890123456789012345678901234567890123456")]
    public void Validar_RazonSocialFueraDeLimites_DevuelveError(string razonSocial)
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.RazonSocial = razonSocial;
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La razon social debe tener entre 3 y 45 caracteres.", resultado.Error);
    }

    [Theory]
    [InlineData(" Empresa")]
    [InlineData("Empresa ")]
    public void Validar_RazonSocialConEspaciosExternos_DevuelveError(string razonSocial)
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.RazonSocial = razonSocial;
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La razon social no debe contener espacios al inicio o al final.", resultado.Error);
    }

    [Theory]
    [InlineData("Empresa\nNorte", "La razon social no debe contener tabs ni saltos de linea.")]
    [InlineData("Empresa @ Norte", "La razon social contiene caracteres no permitidos.")]
    [InlineData("---", "La razon social debe contener al menos letras o numeros.")]
    public void Validar_RazonSocialInvalida_DevuelveError(string razonSocial, string error)
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.RazonSocial = razonSocial;
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(error, resultado.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_CorreoVacioEsOpcional(string correo)
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.CorreoElectronico = correo;
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Theory]
    [InlineData("correo-invalido")]
    [InlineData("a@b")]
    public void Validar_CorreoConFormatoInvalido_DevuelveError(string correo)
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.CorreoElectronico = correo;
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El correo electronico no tiene un formato valido.", resultado.Error);
    }

    [Fact]
    public void Validar_CorreoMayorA45Caracteres_DevuelveError()
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.CorreoElectronico = new string('a', 36) + "@example.test";
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El correo electronico no puede superar los 45 caracteres.", resultado.Error);
    }

    [Fact]
    public void Validar_VariosErrores_DevuelveElPrimero()
    {
        // Arrange
        var cliente = ClienteValido();
        cliente.Nit = "";
        cliente.RazonSocial = "";
        cliente.CorreoElectronico = "invalido";
        var validador = new ClienteValidacion();

        // Act
        var resultado = validador.Validar(cliente);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El NIT es obligatorio.", resultado.Error);
    }

    private static Cliente ClienteValido() => new()
    {
        Nit = "12345",
        RazonSocial = "Cliente Uno",
        CorreoElectronico = "cliente@example.test"
    };
}
