using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

using CapaEntidad;
using CapaNegocio;

using System.Web.Security;

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

            if (usuario.email == correo && usuario.clave == contraseña)
            {
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
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Acceso");
        }
    }
}