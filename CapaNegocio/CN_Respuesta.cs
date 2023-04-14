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
    public class CN_Respuesta
    {
        CD_Respuesta objRespuesta = new CD_Respuesta();
        public int RegistrarRespuesta(string correo, int idusuario, int ID, int PR, int DE, int RS, int RC, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(correo) || string.IsNullOrWhiteSpace(correo))
            {
                Mensaje = "El correo del usuario no puede ser vacío";
            }
            if (string.IsNullOrEmpty(Mensaje))
            {
                //string clave = CN_Recursos.GenerarClave();
                string enlaceEvaluación = "nuevo enlace";
                string asunto = "Evaluación de Protocolos";

                string mensajeCorreo = "<h1 style=\"color: #5e9ca0;\">Proceso de evaluación de protocolos</h1></br>" +
                    "<p>Acceda al siguiente link para la evaluación: <span style=\"background-color: #B9B8B1; color: #fff; display: inline-block; " +
                    "padding: 3px 10px; font-weight: bold; border-radius: 5px;\">!enlace!</span></p><p>Recuerde que para cualquier duda, puede escribir a este mismo correo" +
                    "</p><p>Atte.<br />" +
                    "PRY20220254.TP2.<br /><a href=\"mailto:pry20220254.tp2@gmail.com\">pry20220254.tp2@gmail.com</a></p>";

                mensajeCorreo = mensajeCorreo.Replace("!enlace!", enlaceEvaluación);
                //mensajeCorreo = mensajeCorreo.Replace("!email!", "");

                bool respuesta = CN_Recursos.EnviarCorreo(correo, asunto, mensajeCorreo);

                if (respuesta)
                {
                    //obj.clave = CN_Recursos.ConvertirSha1(clave);
                    //obj.confirmarClave = CN_Recursos.ConvertirSha1(clave);
                    return objRespuesta.RegistrarRespuesta(correo, idusuario, ID, PR, DE, RS, RC, out Mensaje);
                }
                else
                {
                    Mensaje = "No se pudo enviar el correo";
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
