using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using CapaEntidad;
using CapaNegocio;

namespace PRY2022254.PresentacionCliente.Controllers
{
    public class CuestionarioController : Controller
    {
        // GET: Cuestionario
        
        public ActionResult Cuestionario()
        {
            return View();
        }

        public ActionResult Evaluacion()
        {

            string correo = Convert.ToString(Session["emailCliente"]);

            if (correo == "")
            {
                return RedirectToAction("Cuestionario");
            }

            return View();
        }

        [HttpGet]
        public JsonResult ListarPreguntas()
        {
            List<Preguntas> preguntas = new List<Preguntas>();
            preguntas = new CN_Preguntas().ListarPreguntas();
            return Json(new { data = preguntas }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Cuestionario(string correo)
        {
            object resultado;
            string mensaje = string.Empty;
            //List<Usuario> usuarios = new List<Usuario>();
            //usuarios = new CN_Usuario().Listar();
            //List<Preguntas> preguntas = new List<Preguntas>();
            //preguntas = new CN_Preguntas().ListarPreguntas();
            //Session["listaPreguntas"] = preguntas;

            Usuario usuario = new Usuario();
            usuario = new CN_Usuario().UsuarioLogeo(correo);

            if (usuario == null)
            {
                //resultado = new CN_Respuesta().RegistrarRespuesta(correo, out mensaje);
                ViewBag.Error = "El correo no es válido, por favor ingresar uno correcto";
                return View();
            }
            else
            {
                if (usuario.oRolc.idRol == 1)
                {
                    ViewBag.Error = "El correo no es válido, por favor ingresar uno correcto";
                    return View();
                }
                else
                {
                    Session["emailCliente"] = usuario.email;
                    return RedirectToAction("Evaluacion");
                }

            }


            //return Json(new { resultado = 0, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

    }
}