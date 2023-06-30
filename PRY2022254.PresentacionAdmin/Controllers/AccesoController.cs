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
using System.Web.Routing;
using Microsoft.AspNet.SignalR;

namespace PRY2022254.PresentacionAdmin.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
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

            Session["email"] = correo;
            usuario = new CN_Usuario().UsuarioLogeo(correo);
            //List<Usuario> usuarios = new CN_Usuario().Listar();
            //List<Usuario> usuariosAdmin = new CN_Usuario().ListarAdmins();
            //usuario = usuarios;
            string mensaje = string.Empty;
            int sesionGuardar = 0;

            List<ActiveSession> sesiones = new List<ActiveSession>();
            sesiones = new CN_Sesiones().ListarSesiones();

            if(usuario != null){
                if (usuario.email == correo && usuario.clave == contraseña)
                {

                    // Verificar si el usuario ya tiene una sesión activa en otro navegador
                    if (activeSessions.ContainsKey(usuario.email))
                    {
                        string sessionToken = activeSessions[usuario.email];
                        if (sessionToken != Session.SessionID)
                        {
                            // Almacenar el mensaje de error en la sesión del usuario activo
                            ViewBag.Error = "Su cuenta está actualmente en uso en otro navegador.";

                            // Enviar notificación al usuario que tiene la sesión abierta
                            NotificacionesHub.EnviarMensaje(usuario.email, "Hemos detectado un intento de inicio de sesión no autorizado " +
                                "en tu cuenta desde un navegador desconocido. Te recomendamos cambiar tu contraseña de inmediato " +
                                "para garantizar la seguridad de tu cuenta.");

                            // Buscar el ConnectionId del usuario
                            //ActiveSession session = new CN_Sesiones().ListarSesiones_Correo(correo);
                            //string connectionId = session.key;

                            // Enviar notificación al usuario específico
                            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificacionesHub>();
                            //context.Clients.Client(connectionId).recibirNotificacion("Un usuario ha intentado iniciar sesión con tu cuenta en otro navegador.");

                            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificacionesHub>();
                            //context.Clients.All.recibirNotificacion("Un usuario ha intentado iniciar sesión con tu cuenta en otro navegador.");

                            return View();
                        }
                    }
                    else
                    {
                        // El usuario no tiene sesiones activas, guardar el token de sesión actual
                        activeSessions.Add(usuario.email, Session.SessionID);

                        sesionGuardar = new CN_Sesiones().RegistrarSesion(usuario.email, Session.SessionID, out mensaje);

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
            }
            else
            {
                ViewBag.Error = "El usuario no existe, por favor ingresar un usuario válido";
                return View();
            }
            

            ViewBag.Error = "Por favor, ingresar las credenciales correctas";
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
        public ActionResult RestablecerClave(string correo)
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
            bool respuesta = false;
            string mensaje = string.Empty;

            if (User.Identity.IsAuthenticated)
            {
                string correo = User.Identity.Name;
                respuesta = new CN_Sesiones().EliminarSesion(correo, out mensaje);
                if (respuesta)
                {
                    activeSessions.Remove(correo);
                }
            }

            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Acceso");
        }
    }

    public class InactividadFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Verificar si el usuario ha iniciado sesión
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Verificar si existe una marca de tiempo de inicio de sesión en la sesión
                if (filterContext.HttpContext.Session["inicioSesion"] == null)
                {
                    // Establecer la marca de tiempo de inicio de sesión
                    filterContext.HttpContext.Session["inicioSesion"] = DateTime.Now;
                }
                else
                {
                    // Obtener la marca de tiempo de inicio de sesión de la sesión
                    DateTime inicioSesion = Convert.ToDateTime(filterContext.HttpContext.Session["inicioSesion"]);

                    // Calcular la diferencia de tiempo entre el inicio de sesión y el momento actual
                    TimeSpan tiempoInactividad = DateTime.Now - inicioSesion;

                    // Comprobar si el tiempo de inactividad supera los 15 minutos (900 segundos)
                    if (tiempoInactividad.TotalSeconds > 900)
                    {
                        // Cerrar la sesión y redirigir al controlador "Acceso" para cerrar sesión
                        filterContext.HttpContext.Session.Clear();
                        filterContext.HttpContext.Session.Abandon();
                        filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Acceso", action = "CerrarSesion" }));
                        return;
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }



}