namespace ProyectoArqSoft.Domain.Models

{
    public class Clasificacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public short Estado { get; set; } = 1;
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
        public string Origen { get; set; } = string.Empty;
        public int? IdUsuario { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public Clasificacion()
        {
        }

        public Clasificacion(string nombre, string origen, string descripcion)
        {   
            Nombre = nombre;
            Origen = origen;
            Descripcion = descripcion;
        }
    }
}
