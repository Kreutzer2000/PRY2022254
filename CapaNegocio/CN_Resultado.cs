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
    public class CN_Resultado
    {
        public CD_Resultado objResultado = new CD_Resultado();

        public int RegistrarResultado(Usuario usuario, out string Mensaje, out string codigo)
        {
            Mensaje = string.Empty;
            codigo = string.Empty;

            if (usuario == null)
            {
                Mensaje = "Error: El Usuario no existe";
            }

            if (string.IsNullOrEmpty(Mensaje))
            {
                return objResultado.RegistrarResultado(usuario, out Mensaje, out codigo);
            }
            else
            {
                return 0;
            }
        }

        public List<Resultado> ListarResultados(int idUsuario)
        {
            return objResultado.ListarResultados(idUsuario);
        }
    }
}
