using System.Data;

namespace ProyectoArqSoft.Application.Ports.Output
{
    public interface IRepository<T> 
    {
        int Insert(T t);
        int Update(T t);
        int Delete(T t);
        DataTable GetAll();
        DataTable GetAll(string filtro);
        T? GetById(int id);

    }
}

