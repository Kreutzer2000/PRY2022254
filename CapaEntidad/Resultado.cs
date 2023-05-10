using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Resultado
    {
        public int idResultado { get; set; }
        public Usuario oUsuario { get; set; }
        public float porcentajeTotal { get; set; }
        public DateTime fechaResultado { get; set; }
        public string codigo { get; set; }
    }
}
