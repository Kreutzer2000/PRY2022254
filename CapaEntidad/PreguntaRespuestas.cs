using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class PreguntaRespuestas
    {
        public string Pregunta { get; set; }
        public List<string> Respuestas { get; set; }
        public List<RptaPreguntas> ListaRespuestas { get; set; }
        public RptaPreguntas oRespuesta { get; set; }
        public List<int> idRespuesta { get; set; }
    }
}
