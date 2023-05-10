using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Table.PivotTable;


namespace PRY2022254.PresentacionAdmin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string correo = Convert.ToString(Session["email"]);
            int rol = Convert.ToInt32(Session["rolUsuario"]);
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

        [HttpGet]
        public JsonResult ListarResumen()
        {
            List<Resumen> resumen = new List<Resumen>();
            resumen = new CN_Resumen().ListarResumen();

            return Json(new { data = resumen }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public FileContentResult ReporteExcel_Cliente(string codigo)
        {
            object resultado = codigo;
            string mensaje = string.Empty;

            List<Resumen> resumen = new List<Resumen>();
            resumen = new CN_Resumen().ListarResumenPorCodigo(codigo);

            byte[] excelFile = GenerarExcel(resumen);

            //return Json(new { resultado = resumen, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Resumen_" + codigo + ".xlsx");
        }

        private byte[] GenerarExcel(List<Resumen> resumenes)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                // Crea la hoja de trabajo y agrega los encabezados y datos
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Resumen");

                // Agregar encabezados
                worksheet.Cells["B1"].Value = "Estado actual";
                worksheet.Cells["E1"].Value = "Objetivo deseado";
                worksheet.Cells["A2"].Value = "Subcategoría";
                worksheet.Cells["B2"].Value = "Respuesta/Comentario";
                worksheet.Cells["C2"].Value = "Nivel";
                worksheet.Cells["D2"].Value = "Logro";
                worksheet.Cells["E2"].Value = "Nivel";
                worksheet.Cells["F2"].Value = "Logro";
                worksheet.Cells["G1"].Value = "Prioridad";
                worksheet.Cells["G2"].Value = "Brecha";
                worksheet.Cells["H2"].Value = "Prioridad";

                // Rellenar los datos en la hoja de trabajo
                for (int i = 0; i < resumenes.Count; i++)
                {
                    Resumen resumen = resumenes[i];
                    //worksheet.Column(1).Width = 70;
                    worksheet.Cells[i + 3, 1].Value = resumen.oPregunta.oSubCategoria.subCategoria;
                    worksheet.Cells[i + 3, 2].Value = ""; // Coloca aquí la respuesta o comentario según tus necesidades
                    worksheet.Cells[i + 3, 4].Value = resumen.oPuntajeActual.idPuntaje;
                    worksheet.Cells[i + 3, 3].Value = resumen.oPuntajeActual.idPuntaje == 1 ? "Inicial" 
                        : (resumen.oPuntajeActual.idPuntaje == 2 ? "Gestionado" 
                        : (resumen.oPuntajeActual.idPuntaje == 3 ? "Definido" 
                        : (resumen.oPuntajeActual.idPuntaje == 4 ? "Predecible" 
                        : (resumen.oPuntajeActual.idPuntaje == 5 ? "Optimizado" : "" ))));
                    worksheet.Cells[i + 3, 6].Value = resumen.oPuntajeDeseado.idPuntaje;
                    worksheet.Cells[i + 3, 5].Value = resumen.oPuntajeDeseado.idPuntaje == 1 ? "Inicial"
                        : (resumen.oPuntajeDeseado.idPuntaje == 2 ? "Gestionado"
                        : (resumen.oPuntajeDeseado.idPuntaje == 3 ? "Definido"
                        : (resumen.oPuntajeDeseado.idPuntaje == 4 ? "Predecible"
                        : (resumen.oPuntajeDeseado.idPuntaje == 5 ? "Optimizado" : ""))));
                    worksheet.Cells[i + 3, 7].Value = resumen.oPuntajeDeseado.idPuntaje - resumen.oPuntajeActual.idPuntaje;
                    worksheet.Cells[i + 3, 8].Value = "";
                }

                // Ajustar el ancho de las columnas automáticamente
                worksheet.Cells.AutoFitColumns();

                // Establecer el ancho de la columna "A" a 70 unidades (aproximadamente 637 píxeles)
                worksheet.Column(1).Width = 70;

                // Aplicar el formato "Ajustar texto" a las celdas de la columna "A" desde A3 hacia abajo
                for (int i = 3; i <= worksheet.Dimension.End.Row; i++)
                {
                    worksheet.Cells[i, 1].Style.WrapText = true;
                }

                // Guardar el paquete de Excel como un arreglo de bytes
                byte[] fileContents = excelPackage.GetAsByteArray();
                return fileContents;
            }
        }

    }
}