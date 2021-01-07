using System;
using System.Collections.Generic;

namespace RolesDeUsuarioActividad2.Models
{
    public partial class Maestro
    {
        public Maestro()
        {
            Alumno = new HashSet<Alumno>();
        }

        public int Id { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public ulong? Activo { get; set; }
        public int NumControl { get; set; }

        public virtual ICollection<Alumno> Alumno { get; set; }
    }
}
