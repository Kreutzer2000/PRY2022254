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
    public class FrameworkController : Controller
    {
        // GET: Framework
        public ActionResult Preguntas()
        {
            return View();
        }
        public ActionResult Respuestas()
        {
            return View();
        }
        public ActionResult Madurez()
        {
            return View();
        }

        #region Preguntas
        [HttpGet]
        public JsonResult ListarPreguntas_Respuestas(int idpregunta)
        {
            List<RptaPreguntas> rptaPreguntas = new List<RptaPreguntas>();
            rptaPreguntas = new CN_Respuestas().Listar(idpregunta);



            return Json(new { data = rptaPreguntas }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ListarPreguntas()
        {
            List<RptaPreguntas> rptaPreguntas = new List<RptaPreguntas>();
            rptaPreguntas = new CN_Respuestas().ListarPreguntas();



            return Json(new { data = rptaPreguntas }, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}