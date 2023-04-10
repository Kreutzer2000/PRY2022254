using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Usuario
    {
        public int idUsuario { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string cargoEmpleado { get; set; }
        public string razonSocial { get; set; }
        public string ruc { get; set; }
        public string email { get; set; }
        public string clave { get; set; }
        public string confirmarClave { get; set; }
        public bool restablecer { get; set; }
        public DateTime fechaRegistro { get; set; }
        public bool activo { get; set; }
        public Rol oRolc { get; set; }

    }
}
