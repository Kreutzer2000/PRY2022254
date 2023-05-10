using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
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
                    object resultado;
                    string mensaje = string.Empty;
                    string codigo = string.Empty;

                    resultado = new CN_Resultado().RegistrarResultado(usuario, out mensaje, out codigo);

                    if (Convert.ToInt32(resultado) == 0)
                    {
                        ViewBag.Error = "No se pudo registrar la sesión para la evaluación";
                        return View();
                    }
                    else
                    {
                        Session["emailCliente"] = usuario.email;
                        Session["idCliente"] = usuario.idUsuario;
                        return RedirectToAction("Evaluacion");
                    }
                }

            }


            //return Json(new { resultado = 0, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        public class PreguntaData
        {
            public int IdPregunta { get; set; }
            public int IdRptaPreguntas { get; set; }
            public int IdPuntaje_Actual { get; set; }
            public int IdPuntaje_Deseado { get; set; }
        }

        [HttpPost]
        public JsonResult GuardarResultado(int idResultado, List<PreguntaData> datos)
        {
            object resultado;
            string mensaje = string.Empty;

            List<Resultado> listaResutados = new List<Resultado>();
            listaResutados = new CN_Resultado().ListarResultados(idResultado);

            if (listaResutados.Count > 0)
            {
                Resultado ultimo_registro = listaResutados[listaResutados.Count - 1];
                string codigo = "";

                for (int i = 0; i < datos.Count(); i++)
                {
                    PreguntaData resumen = new PreguntaData();

                    resumen.IdPuntaje_Actual = datos[i].IdPuntaje_Actual;
                    resumen.IdPregunta = datos[i].IdPregunta;
                    resumen.IdRptaPreguntas = datos[i].IdRptaPreguntas;
                    resumen.IdPuntaje_Deseado = datos[i].IdPuntaje_Deseado;

                    resultado = new CN_Resumen().RegistrarResumen(ultimo_registro.codigo, ultimo_registro.idResultado, resumen.IdPuntaje_Actual,
                        resumen.IdPregunta, resumen.IdRptaPreguntas, resumen.IdPuntaje_Deseado,
                        out mensaje);

                    if (Convert.ToInt32(resultado) == 31)
                    {
                        return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json(new { resultado = 0, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
            }

            

            return Json(new { resultado = 0, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
            //return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

    }
}