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

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

using OpenAI_API;
using OpenAI_API.Completions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System.Net.Http.Headers;

using Aspose.Cells;
using Aspose.Cells.Rendering;

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
            int rol = Convert.ToInt32(Session["rolUsuario"]);
            string correo = Convert.ToString(Session["email"]);

            if (rol == 1)
            {
                List<Resumen> resumen = new List<Resumen>();
                resumen = new CN_Resumen().ListarResumen();

                return Json(new { data = resumen }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<Resumen> resumen = new List<Resumen>();
                resumen = new CN_Resumen().ListarResumen_Cliente(correo);

                return Json(new { data = resumen }, JsonRequestBehavior.AllowGet);
            }

            //return Json(new { data = resumen }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public async Task<ActionResult> GenerarPDF(string email, string codigo)
        {
            List<Resumen> resumen = new List<Resumen>();
            resumen = new CN_Resumen().ListarResumenPorCodigo(codigo);

            // Generar el Excel en memoria.
            byte[] excelFile = GenerarExcel(resumen);

            // Crear un nuevo documento PDF.
            Document document = new Document();

            // Crear un MemoryStream para almacenar el PDF en memoria mientras se genera.
            using (MemoryStream stream = new MemoryStream())
            {
                // Crear un nuevo escritor PDF que escribe en el MemoryStream.
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                // Abrir el documento.
                document.Open();

                // Aquí procesamos el archivo de Excel en memoria
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    // Cargar el archivo Excel en memoria.
                    excelPackage.Load(new MemoryStream(excelFile));

                    // Obtener la hoja de trabajo "Resumen".
                    var workbook = new Aspose.Cells.Workbook(new MemoryStream(excelFile));
                    var worksheet = workbook.Worksheets["Resumen"];

                    // Ocultar la columna A.
                    worksheet.Cells.HideColumn(0);
                    worksheet.Cells.HideColumn(1);

                    // Renderizar la hoja de cálculo como imagen
                    ImageOrPrintOptions options = new ImageOrPrintOptions
                    {
                        OnePagePerSheet = true,
                        SaveFormat = SaveFormat.Png
                    };

                    SheetRender sr = new SheetRender(worksheet, options);
                    MemoryStream imageStream = new MemoryStream();
                    sr.ToImage(0, imageStream);

                    // Convertir la imagen en un iTextSharp Image
                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageStream.GetBuffer());

                    // Ajustar la imagen al tamaño de la página.
                    image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);

                    // Alinear la imagen al centro de la página.
                    image.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                    document.Add(image);
                }

                // Generar texto utilizando OpenAI.
                string preguntaParaOpenAI = "Como te llamas?";
                string respuestaOpenAI = await new OpenAIService().GenerarTextoConOpenAI(preguntaParaOpenAI);

                // Agregar la respuesta de OpenAI al documento.
                document.Add(new Paragraph(respuestaOpenAI));

                // Cerrar el documento.
                document.Close();

                // Cerrar el escritor.
                writer.Close();

                // Convertir el MemoryStream a un array de bytes.
                byte[] bytes = stream.ToArray();

                // Devolver el array de bytes como un archivo para descarga.
                return File(bytes, "application/pdf", "Reporte" + email + ".pdf");
            }
        }

        public class OpenAIService
        {
            private readonly string apiKey = "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";

            public async Task<string> GenerarTextoConOpenAI(string email)
            {
                // Crear el mensaje para enviar a GPT-3.
                string mensaje = " Quiero saber como implementar un api. " /*+ email*/;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);

                    var content = new StringContent(
                        JsonConvert.SerializeObject(new { prompt = email, max_tokens = 60 }),
                        Encoding.UTF8,
                        "application/json");

                    var response = await httpClient.PostAsync("https://api.openai.com/v1/engines/text-davinci-002/completions", content);

                    var result = await response.Content.ReadAsStringAsync();
                    var parsedResult = JsonConvert.DeserializeObject<OpenAIResponse>(result);

                    string text = parsedResult.Choices[0].Text;

                    // Quitar los caracteres "\n" del inicio del texto.
                    text = text.TrimStart('\n');

                    return text;
                }
            }
        }

        public class OpenAIResponse
        {
            public List<OpenAIChoice> Choices { get; set; }
        }

        public class OpenAIChoice
        {
            public string Text { get; set; }
        }

        //private string GenerarTextoConOpenAI(string email)
        //{
        //    // Conexion de la api de Open AI
        //    string apiKey = "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";
        //    string answer = string.Empty;

        //    // Configurar el modelo y los parámetros de generación de OpenAI.
        //    string modelo = "text-davinci-003";
        //    string prompt = "Hola, soy " + email + ". ";
        //    int maxTokens = 1000;

        //    // Generar texto con OpenAI.
        //    //CompletionResponse respuesta = OpenAIApi.Completions.CreateCompletion(modelo, prompt, maxTokens: maxTokens);
        //    var openAI = new OpenAIAPI(apiKey);
        //    CompletionRequest completion = new CompletionRequest();
        //    completion.Prompt = prompt;
        //    completion.Model = modelo;
        //    completion.MaxTokens = maxTokens;
        //    var result = openAI.Completions.CreateCompletionAsync(completion);
        //    // Obtener la respuesta generada por OpenAI.
        //    string respuestaGenerada = result.IsCompleted.ToString();

        //    return respuestaGenerada;
        //}

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
                worksheet.Cells["G2"].Value = "Criticidad";
                worksheet.Cells["H2"].Value = "Brecha";
                worksheet.Cells["I2"].Value = "Prioridad";

                // Unir celdas B1, C1, D1
                worksheet.Cells["B1:D1"].Merge = true;

                // Unir celdas E1, F1
                worksheet.Cells["E1:F1"].Merge = true;

                // Unir celdas G1, H1
                worksheet.Cells["G1:I1"].Merge = true;

                // Rellenar los datos en la hoja de trabajo
                for (int i = 0; i < resumenes.Count; i++)
                {
                    Resumen resumen = resumenes[i];
                    //worksheet.Column(1).Width = 70;
                    worksheet.Cells[i + 3, 1].Value = resumen.oPregunta.oSubCategoria.subCategoria;
                    worksheet.Cells[i + 3, 2].Value = resumen.oRptaPreguntas.respuesta; // Respuesta de Evaluación
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

                    decimal urgencia = Convert.ToDecimal(Math.Round(resumen.oNivelUrgencia.nivel, 2));

                    worksheet.Cells[i + 3, 7].Value = urgencia; // Nivel de Urgencia

                    decimal brecha = Convert.ToDecimal(Math.Round((resumen.oPuntajeDeseado.idPuntaje - resumen.oPuntajeActual.idPuntaje) * urgencia, 2));
                    worksheet.Cells[i + 3, 8].Value = brecha;

                    ExcelRange cell = worksheet.Cells[i + 3, 9];

                    if (brecha == 0m)
                    {
                        cell.Value = "Objetivo Alcanzado";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(6, 208, 40)); // VERDE CLARO
                    }
                    else if (brecha == 0.2m || brecha == 0.4m || brecha == 0.6m || brecha == 0.8m || brecha == 1m)
                    {
                        cell.Value = "Bajo";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 204, 102)); // CREMA
                    }
                    else if (brecha == 1.2m || brecha == 1.4m || brecha == 1.6m || brecha == 1.8m || brecha == 2m)
                    {
                        cell.Value = "Medio";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 255)); // BLANCO
                    }
                    else if (brecha == 2.4m || brecha == 3m)
                    {
                        cell.Value = "Alto";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 153, 0)); // NARANJA
                    }
                    else if (brecha == 3.2m || brecha == 4m)
                    {
                        cell.Value = "Crítico";
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 0, 0)); // ROJO
                    }

                }
                // Establecer la alineación horizontal y vertical de las celdas B3 hasta H33
                var range = worksheet.Cells[3, 1, 33, 9]; // Ajusta los números de las filas según tus necesidades
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Establecer un rango para aplicar bordes (A1:H33)
                ExcelRange rangeWithBorders = worksheet.Cells["A1:I33"];

                // Aplicar bordes a todas las celdas en el rango
                rangeWithBorders.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangeWithBorders.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangeWithBorders.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangeWithBorders.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Ajustar el ancho de las columnas automáticamente
                worksheet.Cells.AutoFitColumns();

                // Establecer el ancho de la columna "A" a 70 unidades (aproximadamente 637 píxeles)
                worksheet.Column(1).Width = 30;

                // Establecer el ancho de la columna "N" a 50 unidades
                worksheet.Column(2).Width = 70;

                // Aplicar el formato "Ajustar texto" a las celdas de la columna "A" desde A3 hacia abajo
                for (int i = 3; i <= worksheet.Dimension.End.Row; i++)
                {
                    worksheet.Cells[i, 1].Style.WrapText = true;
                }

                // Aplicar el formato "Ajustar texto" a las celdas de la columna "B" desde B3 hacia abajo
                for (int i = 3; i <= worksheet.Dimension.End.Row; i++)
                {
                    worksheet.Cells[i, 2].Style.WrapText = true;
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

                Dictionary<string, double> categoriaBrechaSuma = new Dictionary<string, double>();
                Dictionary<string, int> categoriaBrechaContador = new Dictionary<string, int>();

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

                for (int i = worksheetResumen.Dimension.Start.Row + 1; i <= worksheetResumen.Dimension.End.Row; i++)
                {
                    // Obtener el valor de la subcategoria (columna A) y logro (columna H)
                    string subcategoria = worksheetResumen.Cells[i, 1].Value.ToString();
                    object brechaObj = worksheetResumen.Cells[i, 8].Value;

                    double brecha;

                    // Verificar si el valor de logro puede ser convertido a double
                    if (brechaObj != null && Double.TryParse(brechaObj.ToString(), out brecha))
                    {
                        // Obtener las primeras letras de la subcategoria (por ejemplo, "ID.GV" de "ID.GV: Se establece y se comunica la política de seguridad cibernética organizacional.")
                        string[] splitSubcategoria = subcategoria.Split('-');
                        string subcategoriaCorta = splitSubcategoria[1].Split(':')[0];


                        // Si la subcategoriaCorta no está en categoriaBrechaSuma, agregarla
                        if (!categoriaBrechaSuma.ContainsKey(subcategoriaCorta))
                        {
                            categoriaBrechaSuma[subcategoriaCorta] = 0;
                        }

                        // Si la subcategoriaCorta no está en categoriaBrechaContador, agregarla
                        if (!categoriaBrechaContador.ContainsKey(subcategoriaCorta))
                        {
                            categoriaBrechaContador[subcategoriaCorta] = 0;
                        }

                        // Sumar el logro al total de logro para esa subcategoriaCorta
                        categoriaBrechaSuma[subcategoriaCorta] += brecha;

                        // Incrementar el contador para esa subcategoriaCorta
                        categoriaBrechaContador[subcategoriaCorta]++;
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
                        decimal logroPorcentajeDec = Convert.ToDecimal(Math.Round(logroPorcentaje, 2));

                        // Agregar el porcentaje de logro a la celda correspondiente en la columna %Logro
                        worksheetResultados.Cells[i + 2, 3].Value = logroPorcentajeDec;

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
                    //if (worksheetResultados.Cells[i + 2, 3].Value != null && worksheetResultados.Cells[i + 2, 4].Value != null)
                    //{
                    //    double logroPorcentaje = Convert.ToDouble(worksheetResultados.Cells[i + 2, 3].Value);
                    //    double objetivoPorcentaje = Convert.ToDouble(worksheetResultados.Cells[i + 2, 4].Value);

                    //    double brechaPorcentaje = objetivoPorcentaje - logroPorcentaje;

                    //    // Redondear el porcentaje de brecha a 2 decimales y convertir a decimal
                    //    decimal brechaPorcentajeDec = Convert.ToDecimal(Math.Round(brechaPorcentaje, 2) / 100.0);

                    //    // Agregar el porcentaje de brecha a la celda correspondiente en la columna %Brecha
                    //    worksheetResultados.Cells[i + 2, 5].Value = brechaPorcentajeDec;

                    //    // Aplicar formato de porcentaje a la celda
                    //    worksheetResultados.Cells[i + 2, 5].Style.Numberformat.Format = "0.00%";
                    //}
                    if (categoriaBrechaSuma.ContainsKey(categoriaCorta) && categoriaContador.ContainsKey(categoriaCorta))
                    {
                        double objetivoSuma = categoriaBrechaSuma[categoriaCorta];
                        int contador = categoriaContador[categoriaCorta];
                        double brechaPorcentaje = (objetivoSuma / (contador * 4)) * 100;

                        // Redondear el porcentaje de brecha a 2 decimales y convertir a decimal
                        //decimal objetivoPorcentajeDec = Convert.ToDecimal(Math.Round(brechaPorcentaje, 2) );

                        // Agregar el porcentaje de objetivo a la celda correspondiente en la columna %Objetivo
                        worksheetResultados.Cells[i + 2, 5].Value = brechaPorcentaje;

                        // Aplicar formato de porcentaje a la celda
                        //worksheetResultados.Cells[i + 2, 5].Style.Numberformat.Format = "0.00%";
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
                    double promedioLogroFinal = 0;
                    double promedioObjetivoFinal = 0;
                    double promedioBrechaFinal = 0;

                    switch (promedio.Categoria)
                    {
                        case "ID":
                            promedioLogroFinal = (promedio.PromedioLogro * 4) / 31;
                            promedioObjetivoFinal = (promedio.PromedioObjetivo * 4) / 31;
                            promedioBrechaFinal = (promedio.PromedioBrecha * 4) / 31;
                            break;
                        case "PR":
                            promedioLogroFinal = (promedio.PromedioLogro * 11) / 31;
                            promedioObjetivoFinal = (promedio.PromedioObjetivo * 11) / 31;
                            promedioBrechaFinal = (promedio.PromedioBrecha * 11) / 31;
                            break;
                        case "DE":
                            promedioLogroFinal = (promedio.PromedioLogro * 6) / 31;
                            promedioObjetivoFinal = (promedio.PromedioObjetivo * 6) / 31;
                            promedioBrechaFinal = (promedio.PromedioBrecha * 6) / 31;
                            break;
                        case "RS":
                            promedioLogroFinal = (promedio.PromedioLogro * 8) / 31;
                            promedioObjetivoFinal = (promedio.PromedioObjetivo * 8) / 31;
                            promedioBrechaFinal = (promedio.PromedioBrecha * 8) / 31;
                            break;
                        case "RC":
                            promedioLogroFinal = (promedio.PromedioLogro * 2) / 31;
                            promedioObjetivoFinal = (promedio.PromedioObjetivo * 2) / 31;
                            promedioBrechaFinal = (promedio.PromedioBrecha * 2) / 31;
                            break;
                        default:
                            
                            break;
                    }

                    worksheetResultados.Cells["I" + fila].Value = promedioLogroFinal;
                    worksheetResultados.Cells["J" + fila].Value = promedioObjetivoFinal;
                    worksheetResultados.Cells["K" + fila].Value = promedioBrechaFinal;
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
                worksheetResultados.Cells["K2"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["K3"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["K4"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["K5"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["K6"].Style.Numberformat.Format = "0.00";

                worksheetResultados.Cells["J2"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["J3"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["J4"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["J5"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["J6"].Style.Numberformat.Format = "0.00";

                worksheetResultados.Cells["I2"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["I3"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["I4"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["I5"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["I6"].Style.Numberformat.Format = "0.00";

                // Aplicar formato de dos decimales a las celdas de las columnas C, D y E desde la fila 2 hasta la 32
                worksheetResultados.Cells["C2:C32"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["D2:D32"].Style.Numberformat.Format = "0.00";
                worksheetResultados.Cells["E2:E32"].Style.Numberformat.Format = "0.00";

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
                var worksheetEstadistica = excelPackage.Workbook.Worksheets.Add("Estadística");

                // Obteniendo la hoja de Resumen
                var worksheetResumen_Estadistica = excelPackage.Workbook.Worksheets["Resultados"];

                var chartRangeBrecha = worksheetResumen_Estadistica.Cells["K2:K6"];
                var chartRangeObjetivo = worksheetResumen_Estadistica.Cells["J2:J6"];

                // Añade un gráfico en la posición dada (fila 7, columna 5 en este ejemplo)
                var pieChartBrecha = worksheetEstadistica.Drawings.AddChart("pieChartBrecha", eChartType.Pie) as ExcelPieChart;
                var pieChartObjetivo = worksheetEstadistica.Drawings.AddChart("pieChartObjetivo", eChartType.Pie) as ExcelPieChart;

                // Define los datos del gráfico
                var serieBrecha = pieChartBrecha.Series.Add(worksheetResumen_Estadistica.Cells["K2:K6"], worksheetResumen_Estadistica.Cells["H2:H6"]);
                var serieObjetivo = pieChartObjetivo.Series.Add(worksheetResumen_Estadistica.Cells["J2:J6"], worksheetResumen_Estadistica.Cells["H2:H6"]);

                // Establece las propiedades del gráfico
                pieChartBrecha.Title.Text = "Brecha";
                //pieChartBrecha.DataLabel.ShowCategory = true;
                pieChartBrecha.DataLabel.ShowPercent = true;

                pieChartObjetivo.Title.Text = "Objetivo";
                //pieChartObjetivo.DataLabel.ShowCategory = true;
                pieChartObjetivo.DataLabel.ShowPercent = true;

                // Define la posición del primer gráfico
                pieChartBrecha.SetPosition(0, 0, 0, 0); // El gráfico de brecha comienza en la fila 5, columna 5
                pieChartBrecha.SetSize(400, 400);

                // Define la posición del segundo gráfico
                pieChartObjetivo.SetPosition(0, 0, 7, 0); // El gráfico de objetivo comienza en la fila 5, columna 15
                pieChartObjetivo.SetSize(400, 400);

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