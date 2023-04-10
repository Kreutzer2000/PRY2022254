using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PRY2022254.PresentacionAdmin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Usuarios()
        {
            return View();
        }

        [HttpGet]
        public JsonResult ListarUsuarios()
        {
            List<Usuario> usuarios = new List<Usuario>();
            usuarios = new CN_Usuario().Listar();



            return Json(new {data = usuarios},JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ListarRoles()
        {
            List<Rol> roles = new List<Rol>();
            roles = new CN_Rol().ListarRol();
            return Json(new {data = roles}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarUsuarios(Usuario objeto)
        {
            object resultado;
            string mensaje = string.Empty;
            if (objeto.idUsuario == 0)
            {
                resultado = new CN_Usuario().RegistrarUsuario(objeto, out mensaje);
            }
            else
            {
                resultado = new CN_Usuario().EditarUsuario(objeto, out mensaje);
            }
            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarUsuario(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;

            respuesta = new CN_Usuario().EliminarUsuario(id, out mensaje);

            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }


    }
}