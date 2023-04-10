using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_Respuestas
    {
        private CD_Respuestas objRespuestas = new CD_Respuestas();

        public List<RptaPreguntas> Listar(int idpregunta)
        {
            return objRespuestas.ListarRespuesta(idpregunta);
        }

        public List<RptaPreguntas> ListarPreguntas()
        {
            return objRespuestas.ListarPreguntas();
        }

    }
}
