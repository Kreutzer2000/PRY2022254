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

            List<Usuario> usuarios = new CN_Usuario().Listar();
            List<Usuario> usuariosAdmin = new CN_Usuario().ListarAdmins();
            //usuario = usuarios;
            for (int i = 0; i < usuarios.Count; i++)
            {
                if (usuarios[i].email == correo && usuarios[i].clave == contraseña)
                {

                    if (usuarios[i].restablecer)
                    {
                        Session["idusuario"] = usuarios[i].idUsuario;
                        return RedirectToAction("CambiarClave");
                    }

                    FormsAuthentication.SetAuthCookie(usuarios[i].email, false);
                    Session["rolUsuario"] = usuarios[i].oRolc.idRol;
                        ViewBag.Error = null;
                        return RedirectToAction("Index", "Home");

                }
                else
                {
                    for (int y = 0; y < usuariosAdmin.Count; y++)
                    {
                        if (usuariosAdmin[i].email == correo && usuariosAdmin[i].clave == contraseña)
                        {
                            Session["idusuario"] = usuariosAdmin[i].idUsuario;
                            FormsAuthentication.SetAuthCookie(usuariosAdmin[i].email, false);
                            Session["rolUsuario"] = usuariosAdmin[i].oRolc.idRol;
                            ViewBag.Error = null;
                            return RedirectToAction("Index", "Home");

                        }
                    }
                }
            }

            //usuario = new CN_Usuario().Listar().Where(u => u.email == correo && u.clave == aaaa).FirstOrDefault();

            //if (usuario == null)
            //{
            //    ViewBag.Error = "Correo o contraseña no correcta";
            //    return View();
            //}
            //else
            //{
            //    ViewBag.Error = null;
            //    return RedirectToAction("Index", "Home");
            //}
            ViewBag.Error = "Correo o contraseña no correcta";
            return View();
        }

        [HttpPost]
        public ActionResult CambiarClave(string idusuario, string claveAntigua, string claveNueva, string confirmarClave)
        {

            Usuario usuario = new Usuario();
            string contraseñaAntigua = CN_Recursos.ConvertirSha1(claveAntigua);
            string contraseñaNueva = CN_Recursos.ConvertirSha1(claveNueva);
            //string contraseñaConfirmada = CN_Recursos.ConvertirSha1(confirmarClave);
            int id = int.Parse(idusuario);

            List<Usuario> usuarios = new CN_Usuario().Listar();

            for (int i = 0; i < usuarios.Count; i++)
            {
                if (usuarios[i].idUsuario == id)
                {
                    if (usuarios[i].clave != contraseñaAntigua)
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

                    //return RedirectToAction("Index");

                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Reestablecer(string correo)
        {
            Usuario usuario = new Usuario();

            List<Usuario> usuarios = new CN_Usuario().Listar();

            for (int i = 0; i < usuarios.Count; i++)
            {
                if (usuarios == null)
                {
                    ViewBag.Error = "No se encontró un usuario relacionado a ese correo";
                    return View();
                }

                string mensaje = string.Empty;
                bool respuesta = new CN_Usuario().RestablecerClave(usuarios[i].idUsuario, correo, out mensaje);

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
            return View();
        }

        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Acceso");
        }
    }
}