﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Resumen
    {
        public int idResumen { get; set; }
        public int codigo { get; set; }
        public string comentario { get; set; }
        public DateTime fechaResumen { get; set; }
        public Respuesta oRespuesta { get; set; }
        public Puntaje oPuntaje { get; set; }
        public Preguntas oPregunta { get; set; }
        public RptaPreguntas oRptaPreguntas { get; set; }
        public List<Respuesta> oRespuestas { get; set; }
    }
}