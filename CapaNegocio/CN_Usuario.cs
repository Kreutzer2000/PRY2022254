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
    public class CN_Usuario
    {
        private CD_Usuario objUsuario = new CD_Usuario();

        public List<Usuario> Listar()
        {
            return objUsuario.Listar();
        }

        public List<Usuario> ListarAdmins()
        {
            return objUsuario.ListarAdmins();
        }
        public Usuario UsuarioLogeo(string correo)
        {
            return objUsuario.UsuarioLogeo(correo);
        }

        public int RegistrarUsuario(Usuario obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.nombres) || string.IsNullOrWhiteSpace(obj.nombres))
            {
                Mensaje = "El nombre del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.apellidos) || string.IsNullOrWhiteSpace(obj.apellidos))
            {
                Mensaje = "El apellido del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.cargoEmpleado) || string.IsNullOrWhiteSpace(obj.cargoEmpleado))
            {
                Mensaje = "El cargo del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.razonSocial) || string.IsNullOrWhiteSpace(obj.razonSocial))
            {
                Mensaje = "La razon social del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.ruc) || string.IsNullOrWhiteSpace(obj.ruc))
            {
                Mensaje = "El ruc del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.email) || string.IsNullOrWhiteSpace(obj.email))
            {
                Mensaje = "El correo del usuario no puede ser vacío";
            }
            else if (obj.oRolc.idRol == 0 || obj.oRolc.idRol == 1)
            {
                Mensaje = "Debe seleccionar el Rol Cliente";
            }

            if (string.IsNullOrEmpty(Mensaje))
            {
                string clave = CN_Recursos.GenerarClave();

                string asunto = "Creación de Cuenta";

                string mensajeCorreo = "<h1 style=\"color: #5e9ca0;\">Su cuenta fue creada correctamente</h1></br>" +
                    "<p>Su correo para acceder es: <span style=\"background-color: #B9B8B1; color: #fff; display: inline-block; " +
                    "padding: 3px 10px; font-weight: bold; border-radius: 5px;\">!email!</span></p><p>Su contraseña para acceder es: " +
                    "<span style=\"background-color: #2b2301; color: #fff; display: inline-block; " +
                    "padding: 3px 10px; font-weight: bold; border-radius: 5px;\">!clave!</span></p><p>Atte.<br />" +
                    "PRY20220254.TP2.<br /><a href=\"mailto:pry20220254.tp2@gmail.com\">pry20220254.tp2@gmail.com</a></p>";

                mensajeCorreo = mensajeCorreo.Replace("!clave!", clave);
                mensajeCorreo = mensajeCorreo.Replace("!email!", obj.email);

                bool respuesta = CN_Recursos.EnviarCorreo(obj.email, asunto, mensajeCorreo);

                if (respuesta)
                {
                    obj.clave = CN_Recursos.ConvertirSha1(clave);
                    obj.confirmarClave = CN_Recursos.ConvertirSha1(clave);
                }
                else
                {
                    Mensaje = "No se pudo enviar el correo";
                    return 0;
                }

                return objUsuario.RegistrarUsuario(obj, out Mensaje);
            }
            else
            {
                return 0;
            }
        }


        public bool EditarUsuario(Usuario obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.nombres) || string.IsNullOrWhiteSpace(obj.nombres))
            {
                Mensaje = "El nombre del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.apellidos) || string.IsNullOrWhiteSpace(obj.apellidos))
            {
                Mensaje = "El apellido del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.cargoEmpleado) || string.IsNullOrWhiteSpace(obj.cargoEmpleado))
            {
                Mensaje = "El cargo del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.razonSocial) || string.IsNullOrWhiteSpace(obj.razonSocial))
            {
                Mensaje = "La razon social del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.ruc) || string.IsNullOrWhiteSpace(obj.ruc))
            {
                Mensaje = "El ruc del usuario no puede ser vacío";
            }
            else if (string.IsNullOrEmpty(obj.email) || string.IsNullOrWhiteSpace(obj.email))
            {
                Mensaje = "El correo del usuario no puede ser vacío";
            }
            else if (obj.oRolc.idRol == 0)
            {
                Mensaje = "Debe seleccionar un Rol";
            }

            if (string.IsNullOrEmpty(Mensaje))
            {

                return objUsuario.EditarProducto(obj, out Mensaje);
            }
            else
            {
                return false;
            }
        }


        /* ELIMINAR USUARIO */
        public bool EliminarUsuario(int id, out string Mensaje)
        {
            return objUsuario.EliminarUsuario(id, out Mensaje);
        }

        /* CAMBIAR CLAVE USUARIO */
        public bool CambiarClave(int idusuario, string nuevaClave, out string Mensaje)
        {
            return objUsuario.CambiarClave(idusuario, nuevaClave, out Mensaje);
        }

        /* RESTABLECER CLAVE USUARIO */
        public bool RestablecerClave(int idusuario, string correo, out string Mensaje)
        {
            Mensaje = string.Empty;

            string nuevaClave = CN_Recursos.GenerarClave();
            bool resultado = objUsuario.RestablecerClave(idusuario, CN_Recursos.ConvertirSha1(nuevaClave), out Mensaje);

            if (resultado)
            {
                string asunto = "Contraseña Restablecida";

                string mensajeCorreo = "<h1 style=\"color: #5e9ca0;\">Su contraseña fue restablecida correctamente</h1></br>" +
                    "<p>Su correo para acceder es: <span style=\"background-color: #B9B8B1; color: #fff; display: inline-block; " +
                    "padding: 3px 10px; font-weight: bold; border-radius: 5px;\">!email!</span></p><p>Su contraseña para acceder es: " +
                    "<span style=\"background-color: #2b2301; color: #fff; display: inline-block; " +
                    "padding: 3px 10px; font-weight: bold; border-radius: 5px;\">!clave!</span></p><p>Atte.<br />" +
                    "PRY20220254.TP2.<br /><a href=\"mailto:pry20220254.tp2@gmail.com\">pry20220254.tp2@gmail.com</a></p>";

                mensajeCorreo = mensajeCorreo.Replace("!clave!", nuevaClave);
                mensajeCorreo = mensajeCorreo.Replace("!email!", correo);

                bool respuesta = CN_Recursos.EnviarCorreo(correo, asunto, mensajeCorreo);

                if (respuesta)
                {

                    return true;
                }
                else
                {
                    Mensaje = "No se pudo enviar el correo";
                    return false;
                }

            }
            else
            {
                Mensaje = "No se pudo restablecer la contraseña";
                return false;
            }


        }

        /* FUNCION PARA ENVIAR CORREO A CORREO DEL PROYECTO */
        public bool EnviarContacto_CorreoProyecto(string name, string correo, string asunto, string mensaje, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            {
                Mensaje = "El nombre y apellido no pueden ser vacío";
            }
            else if (string.IsNullOrEmpty(correo) || string.IsNullOrWhiteSpace(correo))
            {
                Mensaje = "El correo no puede estar vacío";
            }
            else if (string.IsNullOrEmpty(asunto) || string.IsNullOrWhiteSpace(asunto))
            {
                Mensaje = "El asunto no puede estar vacío";
            }
            else if (string.IsNullOrEmpty(mensaje) || string.IsNullOrWhiteSpace(mensaje))
            {
                Mensaje = "El mensaje no puede estar vacío";
            }

            if (string.IsNullOrEmpty(Mensaje))
            {
                //string clave = CN_Recursos.GenerarClave();

                //asunto = "Creación de Cuenta";

                string mensajeCorreo = @"
                <html>
                <head>
                    <style>
                        body {
                            font-family: Arial, sans-serif;
                            color: #333333;
                        }
                        .container {
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                            border: 1px solid #e1e1e1;
                            border-radius: 5px;
                            background-color: #f8f8f8;
                        }
                        .header {
                            text-align: center;
                            margin-bottom: 20px;
                        }
                        .title {
                            color: #333333;
                            font-size: 28px;
                            font-weight: bold;
                            text-transform: uppercase;
                            margin-bottom: 10px;
                        }
                        .message {
                            font-size: 16px;
                            line-height: 1.5;
                            margin-bottom: 20px;
                        }
                        .signature {
                            font-weight: bold;
                            margin-top: 20px;
                        }
                        .email {
                            color: #5e9ca0;
                            text-decoration: none;
                        }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1 class='title'>MENSAJE DE !name!</h1>
                        </div>
                        <div class='message'>
                            <p>!mensaje!</p>
                        </div>
                        <div class='signature'>
                            Atentamente,<br />
                            <span class='sender'>!name!</span><br />
                            <a href='mailto:!correo!' class='email'>!correo!</a>
                        </div>
                    </div>
                </body>
                </html>";

                //mensajeCorreo = mensajeCorreo.Replace("!clave!", clave);
                mensajeCorreo = mensajeCorreo.Replace("!correo!", correo);
                mensajeCorreo = mensajeCorreo.Replace("!name!", name);
                mensajeCorreo = mensajeCorreo.Replace("!mensaje!", mensaje);

                bool respuesta = CN_Recursos.EnviarCorreo_Contacto(correo, asunto, mensajeCorreo);

                if (respuesta)
                {
                    return true;
                }
                else
                {
                    Mensaje = "No se pudo enviar el correo";
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
