namespace ProyectoArqSoft.Domain.Models; 

public class Medicamento
{
    public  int Id {get; set; }
    public string Nombre {get; set; } = string.Empty; 
    public string Presentacion {get; set; } =string.Empty; 
    public int IdClasificacion { get; set; }
    public string Concentracion {get; set; }  = string.Empty; 
    public decimal Precio {get; set; }  
    public int Stock { get; set; }

    public short Estado { get; set; } = 1; 
    public DateTime FechaRegistro { get; set; }
    public DateTime UltimaActualizacion { get; set; }
    public int? IdUsuario { get; set; }
    
    public Medicamento()
    {
        
    }

    public Medicamento(string nombre, string presentacion, int idClasificacion, string concentracion, decimal precio, int stock)
    {
        Nombre = nombre;
        Presentacion = presentacion;
        IdClasificacion = idClasificacion;
        Concentracion = concentracion;
        Precio = precio;
        Stock = stock;
    }
}
