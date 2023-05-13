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
using OfficeOpenXml.Style;
using System.Drawing;
using OfficeOpenXml.Drawing.Chart;

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

            return Json(new { data = usuarios }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ListarRoles()
        {
            List<Rol> roles = new List<Rol>();
            roles = new CN_Rol().ListarRol();
            return Json(new { data = roles }, JsonRequestBehavior.AllowGet);
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

            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Resumen_" + codigo + ".xlsx");
        }

        public class DataRow
        {
            public string Categoria { get; set; }
            public double Logro { get; set; }
            public double Objetivo { get; set; }
            public double Brecha { get; set; }
        }

        private byte[] GenerarExcel(List<Resumen> resumenes)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                #region HOJA - RESUMEN
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

                // Unir celdas B1, C1, D1
                worksheet.Cells["B1:D1"].Merge = true;

                // Unir celdas E1, F1
                worksheet.Cells["E1:F1"].Merge = true;

                // Unir celdas G1, H1
                worksheet.Cells["G1:H1"].Merge = true;

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
                        : (resumen.oPuntajeActual.idPuntaje == 5 ? "Optimizado" : ""))));
                    worksheet.Cells[i + 3, 6].Value = resumen.oPuntajeDeseado.idPuntaje;
                    worksheet.Cells[i + 3, 5].Value = resumen.oPuntajeDeseado.idPuntaje == 1 ? "Inicial"
                        : (resumen.oPuntajeDeseado.idPuntaje == 2 ? "Gestionado"
                        : (resumen.oPuntajeDeseado.idPuntaje == 3 ? "Definido"
                        : (resumen.oPuntajeDeseado.idPuntaje == 4 ? "Predecible"
                        : (resumen.oPuntajeDeseado.idPuntaje == 5 ? "Optimizado" : ""))));

                    int brecha = resumen.oPuntajeDeseado.idPuntaje - resumen.oPuntajeActual.idPuntaje;
                    worksheet.Cells[i + 3, 7].Value = brecha;

                    ExcelRange cell = worksheet.Cells[i + 3, 8];

                    if (brecha == 0)
                    {
                        cell.Value = "Objetivo Alcanzado";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(6, 208, 40)); // VERDE CLARO
                    }
                    else if (brecha == 1)
                    {
                        cell.Value = "Bajo";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 204, 102)); // CREMA
                    }
                    else if (brecha == 2)
                    {
                        cell.Value = "Medio";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 255)); // BLANCO
                    }
                    else if (brecha == 3)
                    {
                        cell.Value = "Alto";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 153, 0)); // NARANJA
                    }
                    else if (brecha == 4)
                    {
                        cell.Value = "Muy Alto";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 0, 0)); // ROJO
                    }

                }
                // Establecer la alineación horizontal y vertical de las celdas B3 hasta H33
                var range = worksheet.Cells[3, 2, 33, 8]; // Ajusta los números de las filas según tus necesidades
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Establecer un rango para aplicar bordes (A1:H33)
                ExcelRange rangeWithBorders = worksheet.Cells["A1:H33"];

                // Aplicar bordes a todas las celdas en el rango
                rangeWithBorders.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangeWithBorders.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangeWithBorders.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangeWithBorders.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Ajustar el ancho de las columnas automáticamente
                worksheet.Cells.AutoFitColumns();

                // Establecer el ancho de la columna "A" a 70 unidades (aproximadamente 637 píxeles)
                worksheet.Column(1).Width = 70;

                // Aplicar el formato "Ajustar texto" a las celdas de la columna "A" desde A3 hacia abajo
                for (int i = 3; i <= worksheet.Dimension.End.Row; i++)
                {
                    worksheet.Cells[i, 1].Style.WrapText = true;
                }

                #endregion

                #region HOJA - RESULTADOS

                // Agregar una nueva hoja de trabajo llamada "Resultados"
                ExcelWorksheet worksheetResultados = excelPackage.Workbook.Worksheets.Add("Resultados");

                // Agregar encabezados, datos y aplicar formato a las celdas de la nueva hoja de trabajo
                worksheetResultados.Cells["A1"].Value = "Función";
                worksheetResultados.Cells["B1"].Value = "Categoría";
                worksheetResultados.Cells["C1"].Value = "%Logro";
                worksheetResultados.Cells["D1"].Value = "%Objetivo";
                worksheetResultados.Cells["E1"].Value = "%Brecha";

                // Crear un diccionario para almacenar la cantidad de celdas que se deben unir para cada Función
                Dictionary<string, int> funcionRowCount = new Dictionary<string, int>();

                // Rellenar los datos en la hoja de trabajo "Resultados"
                Dictionary<string, int> categoriaRows = new Dictionary<string, int>();

                Dictionary<string, double> categoriaLogroSuma = new Dictionary<string, double>();
                Dictionary<string, int> categoriaContador = new Dictionary<string, int>();
                ExcelWorksheet worksheetResumen = excelPackage.Workbook.Worksheets["Resumen"];

                Dictionary<string, double> categoriaObjetivoSuma = new Dictionary<string, double>();
                Dictionary<string, int> categoriaObjetivoContador = new Dictionary<string, int>();

                for (int i = worksheetResumen.Dimension.Start.Row + 1; i <= worksheetResumen.Dimension.End.Row; i++)
                {
                    // Obtener el valor de la subcategoria (columna A) y logro (columna D)
                    string subcategoria = worksheetResumen.Cells[i, 1].Value.ToString();
                    object logroObj = worksheetResumen.Cells[i, 4].Value;

                    double logro;

                    // Verificar si el valor de logro puede ser convertido a double
                    if (logroObj != null && Double.TryParse(logroObj.ToString(), out logro))
                    {
                        // Obtener las primeras letras de la subcategoria (por ejemplo, "ID.GV" de "ID.GV: Se establece y se comunica la política de seguridad cibernética organizacional.")
                        string[] splitSubcategoria = subcategoria.Split('-');
                        string subcategoriaCorta = splitSubcategoria[1].Split(':')[0];


                        // Si la subcategoriaCorta no está en categoriaLogroSuma, agregarla
                        if (!categoriaLogroSuma.ContainsKey(subcategoriaCorta))
                        {
                            categoriaLogroSuma[subcategoriaCorta] = 0;
                        }

                        // Si la subcategoriaCorta no está en categoriaContador, agregarla
                        if (!categoriaContador.ContainsKey(subcategoriaCorta))
                        {
                            categoriaContador[subcategoriaCorta] = 0;
                        }

                        // Sumar el logro al total de logro para esa subcategoriaCorta
                        categoriaLogroSuma[subcategoriaCorta] += logro;

                        // Incrementar el contador para esa subcategoriaCorta
                        categoriaContador[subcategoriaCorta]++;
                    }
                }

                for (int i = worksheetResumen.Dimension.Start.Row + 1; i <= worksheetResumen.Dimension.End.Row; i++)
                {
                    // Obtener el valor de la subcategoria (columna A) y logro (columna F)
                    string subcategoria = worksheetResumen.Cells[i, 1].Value.ToString();
                    object objetivoObj = worksheetResumen.Cells[i, 6].Value;

                    double objetivo;

                    // Verificar si el valor de logro puede ser convertido a double
                    if (objetivoObj != null && Double.TryParse(objetivoObj.ToString(), out objetivo))
                    {
                        // Obtener las primeras letras de la subcategoria (por ejemplo, "ID.GV" de "ID.GV: Se establece y se comunica la política de seguridad cibernética organizacional.")
                        string[] splitSubcategoria = subcategoria.Split('-');
                        string subcategoriaCorta = splitSubcategoria[1].Split(':')[0];


                        // Si la subcategoriaCorta no está en categoriaObjetivoSuma, agregarla
                        if (!categoriaObjetivoSuma.ContainsKey(subcategoriaCorta))
                        {
                            categoriaObjetivoSuma[subcategoriaCorta] = 0;
                        }

                        // Si la subcategoriaCorta no está en categoriaObjetivoContador, agregarla
                        if (!categoriaObjetivoContador.ContainsKey(subcategoriaCorta))
                        {
                            categoriaObjetivoContador[subcategoriaCorta] = 0;
                        }

                        // Sumar el logro al total de logro para esa subcategoriaCorta
                        categoriaObjetivoSuma[subcategoriaCorta] += objetivo;

                        // Incrementar el contador para esa subcategoriaCorta
                        categoriaObjetivoContador[subcategoriaCorta]++;
                    }
                }

                for (int i = 0; i < resumenes.Count; i++)
                {
                    Resumen resumen = resumenes[i];

                    string funcion = resumen.oPregunta.oSubCategoria.oCategoria.oFuncion.funcion;
                    string categoria = resumen.oPregunta.oSubCategoria.oCategoria.categoria;
                    string categoriaCorta = categoria.Length >= 5 ? categoria.Substring(0, 5) : categoria;

                    // Si categoriaCorta está en categoriaLogroSuma y categoriaContador, calcular el porcentaje de logro
                    if (categoriaLogroSuma.ContainsKey(categoriaCorta) && categoriaContador.ContainsKey(categoriaCorta))
                    {
                        double logroSuma = categoriaLogroSuma[categoriaCorta];
                        int contador = categoriaContador[categoriaCorta];
                        double logroPorcentaje = (logroSuma / (contador * 5)) * 100;

                        // Redondear el porcentaje de brecha a 2 decimales y convertir a decimal
                        //decimal logroPorcentajeDec = Convert.ToDecimal(Math.Round(logroPorcentaje, 2) / 100.0);

                        // Agregar el porcentaje de logro a la celda correspondiente en la columna %Logro
                        worksheetResultados.Cells[i + 2, 3].Value = logroPorcentaje;

                        // Aplicar formato de porcentaje a la celda
                        //worksheetResultados.Cells[i + 2, 3].Style.Numberformat.Format = "00.00%";
                    }

                    // Si categoriaCorta está en categoriaObjetivoSuma y categoriaContador, calcular el porcentaje de objetivo
                    if (categoriaObjetivoSuma.ContainsKey(categoriaCorta) && categoriaContador.ContainsKey(categoriaCorta))
                    {
                        double objetivoSuma = categoriaObjetivoSuma[categoriaCorta];
                        int contador = categoriaContador[categoriaCorta];
                        double objetivoPorcentaje = (objetivoSuma / (contador * 5)) * 100;

                        // Redondear el porcentaje de brecha a 2 decimales y convertir a decimal
                        //decimal objetivoPorcentajeDec = Convert.ToDecimal(Math.Round(objetivoPorcentaje, 2) / 100.0);

                        // Agregar el porcentaje de objetivo a la celda correspondiente en la columna %Objetivo
                        worksheetResultados.Cells[i + 2, 4].Value = objetivoPorcentaje;

                        // Aplicar formato de porcentaje a la celda
                        //worksheetResultados.Cells[i + 2, 4].Style.Numberformat.Format = "0.00%";
                    }

                    // Calcular el porcentaje de brecha solo si ambos porcentajes de logro y objetivo están disponibles
                    if (worksheetResultados.Cells[i + 2, 3].Value != null && worksheetResultados.Cells[i + 2, 4].Value != null)
                    {
                        double logroPorcentaje = Convert.ToDouble(worksheetResultados.Cells[i + 2, 3].Value);
                        double objetivoPorcentaje = Convert.ToDouble(worksheetResultados.Cells[i + 2, 4].Value);

                        double brechaPorcentaje = objetivoPorcentaje - logroPorcentaje;

                        // Redondear el porcentaje de brecha a 2 decimales y convertir a decimal
                        decimal brechaPorcentajeDec = Convert.ToDecimal(Math.Round(brechaPorcentaje, 2) / 100.0);

                        // Agregar el porcentaje de brecha a la celda correspondiente en la columna %Brecha
                        worksheetResultados.Cells[i + 2, 5].Value = brechaPorcentajeDec;

                        // Aplicar formato de porcentaje a la celda
                        worksheetResultados.Cells[i + 2, 5].Style.Numberformat.Format = "0.00%";
                    }

                    // Incrementar el contador de filas para la Función actual
                    if (!funcionRowCount.ContainsKey(funcion))
                    {
                        funcionRowCount[funcion] = 0;
                    }
                    funcionRowCount[funcion]++;

                    // Agregar datos a las celdas
                    worksheetResultados.Cells[i + 2, 1].Value = funcion;
                    worksheetResultados.Cells[i + 2, 2].Value = categoriaCorta;

                    double porcentajeLogro = 0; // Inicializar a 0 por defecto

                    // Verificar si la clave existe en los diccionarios antes de acceder a los valores
                    if (categoriaLogroSuma.ContainsKey(categoriaCorta))
                    {
                        if (categoriaContador.ContainsKey(categoriaCorta))
                        {
                            porcentajeLogro = (categoriaLogroSuma[categoriaCorta] / (5.0 * categoriaContador[categoriaCorta])) * 100.0;
                        }
                        else
                        {
                            Console.WriteLine("La clave " + categoriaCorta + " no se encuentra en el diccionario categoriaContador.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("La clave " + categoriaCorta + " no se encuentra en el diccionario categoriaLogroSuma.");
                    }


                    worksheetResultados.Cells[i + 2, 3].Value = porcentajeLogro;

                    // Agregar otros datos aquí según tus necesidades

                    if (!categoriaRows.ContainsKey(categoriaCorta))
                    {
                        categoriaRows.Add(categoriaCorta, i + 2);
                    }

                    bool isMerged = categoriaRows[categoriaCorta] != i + 2;
                    if (isMerged)
                    {
                        using (var categoriaRange = worksheetResultados.Cells[categoriaRows[categoriaCorta], 2, i + 2, 2])
                        {
                            categoriaRange.Merge = true;
                            categoriaRange.Value = categoriaCorta;
                            categoriaRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        }
                    }

                    // Unir las celdas de las columnas %Logro, %Objetivo, %Brecha si se unieron las celdas de la columna Categoría
                    if (isMerged)
                    {
                        for (int col = 3; col <= 5; col++)
                        {
                            using (var logroObjetivoBrechaRange = worksheetResultados.Cells[categoriaRows[categoriaCorta], col, i + 2, col])
                            {
                                logroObjetivoBrechaRange.Merge = true;
                                logroObjetivoBrechaRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }
                        }
                    }


                }

                // Unir las celdas de la columna Función
                int currentRow = 2;
                foreach (var funcion in funcionRowCount)
                {
                    // Unir las celdas verticalmente para la Función actual
                    if (funcion.Value > 1)
                    {
                        worksheetResultados.Cells[currentRow, 1, currentRow + funcion.Value - 1, 1].Merge = true;
                    }

                    // Centrar el texto verticalmente en las celdas unidas
                    using (var functionRange = worksheetResultados.Cells[currentRow, 1, currentRow + funcion.Value - 1, 1])
                    {
                        functionRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    currentRow += funcion.Value;
                }

                // Agregar encabezados, datos y aplicar formato a las celdas de la nueva hoja de trabajo
                worksheetResultados.Cells["H1"].Value = "Función";
                worksheetResultados.Cells["I1"].Value = "%Logro";
                worksheetResultados.Cells["J1"].Value = "%Objetivo";
                worksheetResultados.Cells["K1"].Value = "%Brecha";

                worksheetResultados.Cells["H2"].Value = "Función ID";
                worksheetResultados.Cells["H3"].Value = "Función PR";
                worksheetResultados.Cells["H4"].Value = "Función DE";
                worksheetResultados.Cells["H5"].Value = "Función RS";
                worksheetResultados.Cells["H6"].Value = "Función RC";

                //List<string> categoriaID = new List<string>();
                //List<double> logrosID = new List<double>();
                //List<double> objetivosID = new List<double>();
                //List<double> brechasID = new List<double>();

                List<DataRow> dataRows = new List<DataRow>();

                for (int i = 2; i <= worksheetResultados.Dimension.End.Row; i++)
                {
                    if (worksheetResultados.Cells[i, 2].Value != null &&
                        worksheetResultados.Cells[i, 3].Value != null &&
                        worksheetResultados.Cells[i, 4].Value != null &&
                        worksheetResultados.Cells[i, 5].Value != null)
                    {
                        dataRows.Add(new DataRow
                        {
                            Categoria = Convert.ToString(worksheetResultados.Cells[i, 2].Value),
                            Logro = Convert.ToDouble(worksheetResultados.Cells[i, 3].Value),
                            Objetivo = Convert.ToDouble(worksheetResultados.Cells[i, 4].Value),
                            Brecha = Convert.ToDouble(worksheetResultados.Cells[i, 5].Value)
                        });
                    }
                }

                var promediosPorCategoria = dataRows
                    .GroupBy(row => row.Categoria.Substring(0, 2))  // Agrupar por los primeros 2 caracteres de la categoría
                    .Select(group => new
                        {
                            Categoria = group.Key,
                            PromedioLogro = group.Average(row => row.Logro),
                            PromedioObjetivo = group.Average(row => row.Objetivo),
                            PromedioBrecha = group.Average(row => row.Brecha)
                });

                int fila = 2;
                foreach (var promedio in promediosPorCategoria)
                {
                    worksheetResultados.Cells["I" + fila].Value = promedio.PromedioLogro;
                    worksheetResultados.Cells["J" + fila].Value = promedio.PromedioObjetivo;
                    worksheetResultados.Cells["K" + fila].Value = promedio.PromedioBrecha;
                    fila++;
                }

                //// Calcular los promedios
                //double promedioLogroID = logrosID.Average();
                //double promedioObjetivoID = objetivosID.Average();
                //double promedioBrechaID = brechasID.Average();

                //// Escribir los promedios en las celdas correspondientes
                //worksheetResultados.Cells["I2"].Value = promedioLogroID;
                //worksheetResultados.Cells["J2"].Value = promedioObjetivoID;
                //worksheetResultados.Cells["K2"].Value = promedioBrechaID;

                // Aplicar formato de porcentaje a las celdas
                worksheetResultados.Cells["K2"].Style.Numberformat.Format = "0.00%";
                worksheetResultados.Cells["K3"].Style.Numberformat.Format = "0.00%";
                worksheetResultados.Cells["K4"].Style.Numberformat.Format = "0.00%";
                worksheetResultados.Cells["K5"].Style.Numberformat.Format = "0.00%";
                worksheetResultados.Cells["K6"].Style.Numberformat.Format = "0.00%";

                // Establecer un rango para aplicar bordes (A1:H33)
                ExcelRange rangeWithBordersResultados = worksheetResultados.Cells["A1:E32"];
                ExcelRange rangeWithBordersResultadosPromedio = worksheetResultados.Cells["H1:K6"];

                // Aplicar bordes a todas las celdas en el rango
                rangeWithBordersResultados.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangeWithBordersResultados.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangeWithBordersResultados.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangeWithBordersResultados.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                rangeWithBordersResultadosPromedio.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangeWithBordersResultadosPromedio.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangeWithBordersResultadosPromedio.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangeWithBordersResultadosPromedio.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Aplicar formato a las celdas, como ajustar el ancho de las columnas, centrar el texto, etc.
                worksheetResultados.Cells.AutoFitColumns();

                #endregion

                #region HOJA ESTADISTICA

                // Crear la hoja de trabajo
                //var worksheetEstadistica = excelPackage.Workbook.Worksheets.Add("Estadística");

                //// Obteniendo la hoja de Resumen
                //var worksheetResumen_Estadistica = excelPackage.Workbook.Worksheets["Resumen"];

                //// Copia los datos de subcategorías y prioridades de la hoja "Resumen" a la hoja "Estadística"
                //for (int i = 3; i <= worksheetResumen_Estadistica.Dimension.End.Row; i++)
                //{
                //    worksheetEstadistica.Cells[i, 1].Value = worksheetResumen_Estadistica.Cells[i, 1].Value; // Subcategoría
                //    worksheetEstadistica.Cells[i, 2].Value = worksheetResumen_Estadistica.Cells[i, 8].Value; // Prioridad
                //}

                //// Crear el gráfico
                //var chart = worksheetEstadistica.Drawings.AddChart("chart", OfficeOpenXml.Drawing.Chart.eChartType.ColumnClustered);
                //chart.Series.Add(worksheetEstadistica.Cells[3, 2, worksheetEstadistica.Dimension.End.Row, 2], worksheetEstadistica.Cells[3, 1, worksheetEstadistica.Dimension.End.Row, 1]);

                //// Configurar el título del gráfico
                //chart.Title.Text = "Prioridad por Subcategoría";

                //// Configurar la posición y el tamaño del gráfico
                //chart.SetPosition(5, 0, 0, 0);
                //chart.SetSize(800, 600);


                #endregion

                // Guardar el paquete de Excel como un arreglo de bytes
                byte[] fileContents = excelPackage.GetAsByteArray();
                return fileContents;
            }
        }

    }
}