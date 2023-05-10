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
    public class CN_Resumen
    {
        CD_Resumen objResumen = new CD_Resumen();

        public int RegistrarResumen(string codigo, int idResultado, int idPuntaje_Actual, int idPregunta, int idRptaPreguntas, int idPuntaje_Deseado, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(Mensaje))
            {
                return objResumen.RegistrarResumen(codigo, idResultado, idPuntaje_Actual, idPregunta, idRptaPreguntas, idPuntaje_Deseado, out Mensaje);
            }

            return 0;
        }

        public List<Resumen> ListarResumen()
        {
            return objResumen.ListarResumen();
        }

        public List<Resumen> ListarResumenPorCodigo(string codigo)
        {
            return objResumen.ListarResumenPorCodigo(codigo);
        }
    }
}
