using Moq;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

public class UsuarioValidacionGeneralTests
{
    [Fact]
    public void ValidarEliminacion_IdInvalido_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        int idUsuario = 0;

        // Act
        Result resultado = validador.ValidarEliminacion(idUsuario);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El identificador del usuario no es válido.",
            resultado.Error);

        repositoryMock.Verify(
            repo => repo.GetById(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public void ValidarEliminacion_UsuarioInexistente_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();

        repositoryMock
            .Setup(repo => repo.GetById(10))
            .Returns((Usuario?)null);

        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        int idUsuario = 10;

        // Act
        Result resultado = validador.ValidarEliminacion(idUsuario);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El usuario no existe.", resultado.Error);

        repositoryMock.Verify(
            repo => repo.GetById(10),
            Times.Once);
    }

    [Fact]
    public void ValidarEliminacion_UsuarioExistente_RetornaExito()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();

        var usuario = new Usuario
        {
            IdUsuario = 10,
            Nombres = "Ana"
        };

        repositoryMock
            .Setup(repo => repo.GetById(10))
            .Returns(usuario);

        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        int idUsuario = 10;

        // Act
        Result resultado = validador.ValidarEliminacion(idUsuario);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);

        repositoryMock.Verify(
            repo => repo.GetById(10),
            Times.Once);
    }

    private static ProyectoArqSoft.Domain.DTOs.UsuarioActualizarDto CrearDtoValido()
    {
        return new ProyectoArqSoft.Domain.DTOs.UsuarioActualizarDto
        {
            IdUsuario = 10,
            Nombres = "Ana",
            ApellidoPaterno = "Perez",
            Ci = "12345678",
            Telefono = "71234567",
            Email = "ana@correo.com"
        };
    }

    [Fact]
    public void ValidarActualizacion_DtoNulo_RetornaError()
    {
        // Arrange
        ProyectoArqSoft.Domain.DTOs.UsuarioActualizarDto? dto = null;

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto!);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("Los datos del usuario no pueden ser nulos.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_IdInvalido_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.IdUsuario = 0;

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El identificador del usuario no es válido.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_NombresVacios_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Nombres = "";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El campo Nombres es obligatorio.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_CiInvalido_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Ci = "ABC123";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El CI debe tener 8 dígitos y un complemento opcional de hasta dos caracteres (Ej. 10000000-1B).",
            resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_TelefonoInvalido_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Telefono = "123";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El teléfono debe tener exactamente 8 dígitos.",
            resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_EmailInvalido_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Email = "correo-invalido";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El formato del email no es válido.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_DatosValidos_RetornaExito()
    {
        // Arrange
        var dto = CrearDtoValido();

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    private static ProyectoArqSoft.Domain.DTOs.UsuarioRegistroDto CrearRegistroValido()
    {
        return new ProyectoArqSoft.Domain.DTOs.UsuarioRegistroDto
        {
            Nombres = "Ana",
            ApellidoPaterno = "Perez",
            Ci = "12345678",
            Telefono = "71234567",
            Email = "ana@correo.com",
            UserName = "ana123",
            Password = "Password123"
        };
    }

    [Fact]
    public void ValidarRegistro_DtoNulo_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        ProyectoArqSoft.Domain.DTOs.UsuarioRegistroDto? dto = null;

        // Act
        Result resultado = validador.ValidarRegistro(dto!);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("Los datos del usuario no pueden ser nulos.", resultado.Error);
        repositoryMock.Verify(r => r.ExisteEmail(It.IsAny<string>()), Times.Never);
        repositoryMock.Verify(r => r.ExisteUserName(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ValidarRegistro_NombresVacios_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();
        dto.Nombres = "";

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El campo Nombres es obligatorio.", resultado.Error);
        repositoryMock.Verify(r => r.ExisteEmail(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ValidarRegistro_CiInvalido_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();
        dto.Ci = "ABC123";

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Contains("El CI debe tener 8 dígitos", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_TelefonoInvalido_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();
        dto.Telefono = "123";

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El teléfono debe tener exactamente 8 dígitos.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_EmailInvalido_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();
        dto.Email = "correo-invalido";

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El formato del email no es válido.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_PasswordCorta_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();
        dto.Password = "123";

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La contraseña debe tener al menos 8 caracteres.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_EmailDuplicado_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        repositoryMock.Setup(r => r.ExisteEmail("ana@correo.com")).Returns(true);

        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El email ya está registrado en el sistema.", resultado.Error);
        repositoryMock.Verify(r => r.ExisteEmail("ana@correo.com"), Times.Once);
        repositoryMock.Verify(r => r.ExisteUserName(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ValidarRegistro_UserNameDuplicado_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        repositoryMock.Setup(r => r.ExisteEmail("ana@correo.com")).Returns(false);
        repositoryMock.Setup(r => r.ExisteUserName("ana123")).Returns(true);

        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre de usuario ya está registrado en el sistema.", resultado.Error);
        repositoryMock.Verify(r => r.ExisteEmail("ana@correo.com"), Times.Once);
        repositoryMock.Verify(r => r.ExisteUserName("ana123"), Times.Once);
    }

    [Fact]
    public void ValidarRegistro_DatosValidos_RetornaExito()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        repositoryMock.Setup(r => r.ExisteEmail("ana@correo.com")).Returns(false);
        repositoryMock.Setup(r => r.ExisteUserName("ana123")).Returns(false);

        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
        repositoryMock.Verify(r => r.ExisteEmail("ana@correo.com"), Times.Once);
        repositoryMock.Verify(r => r.ExisteUserName("ana123"), Times.Once);
    }

    [Fact]
    public void ValidarActualizacion_ApellidoPaternoVacio_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.ApellidoPaterno = "";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El campo Apellido Paterno es obligatorio.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_EmailVacio_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Email = "";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El campo Email es obligatorio.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_NombresExceden100_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Nombres = new string('A', 101);

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El campo Nombres no puede exceder 100 caracteres.",
            resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_ApellidoPaternoExcede100_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.ApellidoPaterno = new string('P', 101);

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El campo Apellido Paterno no puede exceder 100 caracteres.",
            resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_CiVacio_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Ci = "";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El número de carnet es obligatorio.",
            resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_CiConEspacios_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Ci = "1234 678";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El número de carnet no debe contener espacios.",
            resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_TelefonoVacio_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Telefono = "";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El teléfono es obligatorio.",
            resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_TelefonoNoNumerico_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Telefono = "1234567A";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El teléfono debe contener solo dígitos numéricos.",
            resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_EmailExcede255_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Email = new string('a', 245) + "@correo.com";

        Assert.Equal(256, dto.Email.Length);

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "El email no puede exceder 255 caracteres.",
            resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_PasswordVacia_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();
        dto.Password = "";

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "La contraseña es obligatoria.",
            resultado.Error);

        repositoryMock.Verify(
            r => r.ExisteEmail(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void ValidarRegistro_PasswordExcede128_RetornaError()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var validador = new UsuarioValidacionGeneral(repositoryMock.Object);
        var dto = CrearRegistroValido();
        dto.Password = new string('A', 129);

        // Act
        Result resultado = validador.ValidarRegistro(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "La contraseña no puede exceder 128 caracteres.",
            resultado.Error);

        repositoryMock.Verify(
            r => r.ExisteEmail(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_RepositorioNulo_LanzaArgumentNullException()
    {
        // Arrange
        IUsuarioRepository? repository = null;

        // Act
        ArgumentNullException excepcion = Assert.Throws<ArgumentNullException>(
            () => new UsuarioValidacionGeneral(repository!));

        // Assert
        Assert.Equal("repository", excepcion.ParamName);
    }

    [Fact]
    public void ValidarActualizacion_NombresNulos_RetornaError()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Nombres = null!;

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El campo Nombres es obligatorio.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_NombresConEspacios_RecortaTexto()
    {
        // Arrange
        var dto = CrearDtoValido();
        dto.Nombres = "  Ana  ";

        // Act
        Result resultado = UsuarioValidacionGeneral.ValidarActualizacion(dto);

        // Assert
        Assert.True(resultado.IsSuccess);
    }
}
