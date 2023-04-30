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
        public List<RptaPreguntas> respuestas { get; set; }
        public List<List<RptaPreguntas>> nuevo { get; set; }
        public List<Preguntas> preguntas { get; set; }
        public List<string> Preguntas { get; set; }
        public string preg { get; set; }
    }
}
