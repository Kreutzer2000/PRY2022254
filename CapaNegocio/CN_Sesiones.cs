using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class CN_Sesiones
    {
        private CD_Sesiones obSesion = new CD_Sesiones();
        public List<ActiveSession> ListarSesiones()
        {
            return obSesion.ListarSesiones();
        }
        public ActiveSession ListarSesiones_Correo(string email)
        {
            return obSesion.ListarSesiones_Correo(email);
        }
        public int RegistrarSesion(string email, string llave, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(Mensaje))
            {
                return obSesion.RegistrarSesion(email, llave, out Mensaje);
            }
            else
            {
                return 0;
            }
        }

        /* ELIMINAR SESIONES DE CLIENTES ACTIVOS */
        public bool EliminarSesion(string email, out string Mensaje)
        {
            return obSesion.EliminarSesion(email, out Mensaje);
        }
    }
}
