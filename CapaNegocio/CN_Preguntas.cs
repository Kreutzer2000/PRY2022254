using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_Preguntas
    {
        private CD_Preguntas objPreguntas = new CD_Preguntas();
        public List<Preguntas> ListarPreguntas()
        {
            return objPreguntas.ListarPreguntas();
        }
    }
}
