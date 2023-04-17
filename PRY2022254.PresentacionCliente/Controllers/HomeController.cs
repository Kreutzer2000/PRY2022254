using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PRY2022254.PresentacionCliente.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Filtro()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GuardarRespuesta(string correo, int ID, int PR, int DE, int RS, int RC)
        {
            object resultado;
            string mensaje = string.Empty;
            //List<Usuario> usuarios = new List<Usuario>();
            //usuarios = new CN_Usuario().Listar();

            Usuario usuario = new Usuario();
            usuario = new CN_Usuario().UsuarioLogeo(correo);

            if (usuario.email == correo)
            {
                resultado = new CN_Respuesta().RegistrarRespuesta(correo, usuario.idUsuario, ID, PR, DE, RS, RC, out mensaje);
                return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
            }

            //for (int i = 0; i < usuarios.Count; i++)
            //{
            //    if (usuarios[i].email == correo)
            //    {
            //        resultado = new CN_Respuesta().RegistrarRespuesta(correo, usuarios[i].idUsuario, ID, PR, DE, RS, RC, out mensaje);
            //        return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
            //    }
            //    //else
            //    //{
            //    //    ViewBag.Error = "Correo no existe";
            //    //    //return View();
            //    //}
            //}

            return Json(new { resultado = 0, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }
    }
}