using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Resumen
    {
        public int idResumen { get; set; }
        public string codigo { get; set; }
        public string comentario { get; set; }
        public string fechaResumen { get; set; }
        public Resultado oResultado { get; set; }
        //public Respuesta oRespuesta { get; set; }
        public Puntaje oPuntajeActual { get; set; }
        public Preguntas oPregunta { get; set; }
        public RptaPreguntas oRptaPreguntas { get; set; }
        //public List<Respuesta> oRespuestas { get; set; }
        public Puntaje oPuntajeDeseado { get; set; }
        public NivelUrgencia oNivelUrgencia { get; set; }
    }
}
