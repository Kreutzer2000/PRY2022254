using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

using CapaEntidad;
using CapaNegocio;

using System.Web.Security;
using PRY2022254.PresentacionAdmin.Utils;

namespace PRY2022254.PresentacionAdmin.Controllers
{
    public class AccesoController : Controller
    {
        // GET: Acceso
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CambiarClave()
        {
            return View();
        }

        public ActionResult RestablecerClave()
        {
            return View();
        }

        private static Dictionary<string, string> activeSessions = new Dictionary<string, string>();

        [HttpPost]
        public ActionResult Index(string correo, string clave)
        {
            Usuario usuario = new Usuario();
            string contraseña = CN_Recursos.ConvertirSha1(clave);

            // Verificar si ya existe una sesión activa para este usuario
            //if (Session["idusuario"] != null)
            //{
            //    // Ya existe una sesión activa para este usuario
            //    NotificacionesHub.EnviarMensaje("Intento de acceso a la cuenta desde otro lugar.");
            //    return RedirectToAction("Index", "Acceso");
            //}

            Session["email"] = correo;
            usuario = new CN_Usuario().UsuarioLogeo(correo);
            //List<Usuario> usuarios = new CN_Usuario().Listar();
            //List<Usuario> usuariosAdmin = new CN_Usuario().ListarAdmins();
            //usuario = usuarios;

            if (usuario.email == correo && usuario.clave == contraseña)
            {

                // Verificar si el usuario ya tiene una sesión activa en otro navegador
                if (activeSessions.ContainsKey(usuario.email))
                {
                    string sessionToken = activeSessions[usuario.email];
                    if (sessionToken != Session.SessionID)
                    {
                        // Se detecta un intento de acceso externo a la cuenta
                        //string errorMessage = "Usted ya inició sesión en un navegador.";

                        // Almacenar el mensaje de error en la sesión del usuario activo
                        ViewBag.Error = "Su cuenta está actualmente en uso en otro navegador.";
                        return View();
                    }
                }
                else
                {
                    // El usuario no tiene sesiones activas, guardar el token de sesión actual
                    activeSessions.Add(usuario.email, Session.SessionID);
                }

                if (usuario.oRolc.idRol == 1)
                {
                    Session["idusuario"] = usuario.idUsuario;
                    FormsAuthentication.SetAuthCookie(usuario.email, false);
                    Session["rolUsuario"] = usuario.oRolc.idRol;
                    ViewBag.Error = null;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (usuario.restablecer)
                    {
                        Session["idusuario"] = usuario.idUsuario;
                        return RedirectToAction("CambiarClave");
                    }
                    FormsAuthentication.SetAuthCookie(usuario.email, false);
                    Session["rolUsuario"] = usuario.oRolc.idRol;
                    ViewBag.Error = null;
                    return RedirectToAction("Index", "Home");
                }

            }

            ViewBag.Error = "Correo o contraseña no correcta";
            return View();
        }

        // Método para cerrar sesión
        public ActionResult CerrarSesionDuplicidadDeSesion()
        {
            if (User.Identity.IsAuthenticated)
            {
                string correo = User.Identity.Name;
                activeSessions.Remove(correo);
            }

            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Acceso");
        }

        [HttpPost]
        public ActionResult CambiarClave(string idusuario, string claveAntigua, string claveNueva, string confirmarClave)
        {

            Usuario usuario = new Usuario();
            string contraseñaAntigua = CN_Recursos.ConvertirSha1(claveAntigua);
            string contraseñaNueva = CN_Recursos.ConvertirSha1(claveNueva);
            string correo = Convert.ToString(Session["email"]);

            //string contraseñaConfirmada = CN_Recursos.ConvertirSha1(confirmarClave);
            int id = int.Parse(idusuario);

            usuario = new CN_Usuario().UsuarioLogeo(correo);
            //List<Usuario> usuarios = new CN_Usuario().Listar();

            if (usuario.idUsuario == id)
            {
                if (usuario.clave != contraseñaAntigua)
                {
                    TempData["idusuario"] = idusuario;

                    ViewBag.Error = "La contraseña actual no es la correcta";
                    ViewData["vclave"] = "";
                    return View();
                }
                else if (claveNueva != confirmarClave)
                {
                    TempData["idusuario"] = idusuario;
                    ViewData["vclave"] = claveAntigua;
                    ViewBag.Error = "Las contraseñas no coinciden";
                    return View();
                }
                ViewData["vclave"] = "";

                string mensaje = string.Empty;
                bool resultado = new CN_Usuario().CambiarClave(id, contraseñaNueva, out mensaje);

                if (resultado)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["idusuario"] = idusuario;
                    ViewBag.Error = mensaje;
                    return View();
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Reestablecer(string correo)
        {
            Usuario usuario = new Usuario();

            usuario = new CN_Usuario().UsuarioLogeo(correo);
            //List<Usuario> usuarios = new CN_Usuario().Listar();

            if (usuario == null)
            {
                ViewBag.Error = "No se encontró un usuario relacionado a ese correo";
                return View();
            }

            string mensaje = string.Empty;
            bool respuesta = new CN_Usuario().RestablecerClave(usuario.idUsuario, correo, out mensaje);

            if (respuesta)
            {
                ViewBag.Error = null;
                return RedirectToAction("Index", "Acceso");
            }
            else
            {
                ViewBag.Error = mensaje;
                return View();
            }
        }

        public ActionResult CerrarSesion()
        {
            //FormsAuthentication.SignOut();
            //return RedirectToAction("Index", "Acceso");
            if (User.Identity.IsAuthenticated)
            {
                string correo = User.Identity.Name;
                activeSessions.Remove(correo);
            }

            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Acceso");
        }
    }
}