using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Preguntas
    {
        public int idPregunta { get; set; }
        public string pregunta { get; set; }
        public string comentario { get; set; }
        public SubCategoriaNist oSubCategoria { get; set; }
        public List<Preguntas> preguntas { get; set; }
        public RptaPreguntas respuesta { get; set; }
        public List<RptaPreguntas> respuestas { get; set; }
    }
}
