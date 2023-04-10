using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class RptaPreguntas
    {
        public int idRptaPregunta { get; set; }
        public string respuesta { get; set; }
        public Preguntas oPregunta { get; set; }
        public FuncionNist oFuncionNist { get; set; }
    }
}
