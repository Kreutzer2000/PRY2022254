using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Respuesta
    {
        public int idRespuesta { get; set; }
        public Usuario oUsuario { get; set; }
        public FuncionNist oFuncion_ID { get; set; }
        public FuncionNist oFuncion_PR { get; set; }
        public FuncionNist oFuncion_DE { get; set; }
        public FuncionNist oFuncion_RS { get; set; }
        public FuncionNist oFuncion_RC { get; set; }
    }                      
}
