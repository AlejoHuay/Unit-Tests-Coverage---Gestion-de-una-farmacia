using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Moq;
using ProyectoArqSoft.Infrastructure.Creadores;
using ProyectoArqSoft.Infrastructure.Persistence.Connection;
using ProyectoArqSoft.Infrastructure.Persistence.Repositories;
using Xunit;

namespace ProyectoArqSoft.Tests.Creadores;

[Trait("Category", "Unit")]
public class UsuarioTokenRepositoryCreatorTests
{
    [Fact]
    public void CreateRepo_CreaInstanciasDelTipoEsperadoSinAccederALaBase()
    {
        var conexiones = new Mock<Func<DbConnection>>(MockBehavior.Strict);
        var contexto = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        var database = new PostgresDatabase(conexiones.Object, contexto.Object);
        var creator = new UsuarioTokenRepositoryCreator(database);

        var primero = creator.CreateRepo();
        var segundo = creator.CreateRepo();

        Assert.IsType<UsuarioTokenRepository>(primero);
        Assert.IsType<UsuarioTokenRepository>(segundo);
        Assert.NotSame(primero, segundo);
        conexiones.VerifyNoOtherCalls();
        contexto.VerifyNoOtherCalls();
    }
}

