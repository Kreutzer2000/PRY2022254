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
using Google.Cloud.Translation.V2;
using Microsoft.AspNet.SignalR;
using PRY2022254.PresentacionAdmin.Utils;
using System.Threading;
using System.Web.WebSockets;

namespace PRY2022254.PresentacionAdmin.Controllers
{
    
    [System.Web.Mvc.Authorize]
    [InactividadFilter]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class HomeController : Controller
    {
        private IHubContext _hubContext;

        public HomeController(IHubContext hubContext)
        {
            _hubContext = hubContext;
        }
        // Constructor sin parámetros
        public HomeController()
        {
            // Lógica adicional si es necesaria
        }
        public ActionResult WebSocket()
        {
            if (HttpContext.IsWebSocketRequest)
            {
                HttpContext.AcceptWebSocketRequest(HandleWebSocket);
            }

            return new HttpStatusCodeResult(400, "Bad Request");
        }
        private async Task HandleWebSocket(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket;

            // Aquí puedes realizar acciones adicionales cuando se establece la conexión WebSocket

            var buffer = new byte[1024];
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                // Procesar el mensaje recibido desde el cliente WebSocket
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Aquí puedes manejar el mensaje recibido

                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            // Aquí puedes realizar acciones adicionales cuando se cierra la conexión WebSocket

            socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public ActionResult Index()
        {
            string correo = Convert.ToString(Session["email"]);
            int rol = Convert.ToInt32(Session["rolUsuario"]);

            // Obtener el correo electrónico del usuario autenticado
            //string email = User.Identity.Name;

            //// Obtener la instancia de IHubContext
            //var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificacionesHub>();

            //// Verificar que hubContext no sea null
            //if (hubContext != null)
            //{
            //    // Notificar a los clientes conectados que se ha iniciado sesión
            //    hubContext.Clients.All.recibirNotificacion(email);
            //}

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
                    #region Cargar el archivo Excel en el documento como imagen
                    //// Cargar el archivo Excel en memoria.
                    //excelPackage.Load(new MemoryStream(excelFile));

                    //// Obtener la hoja de trabajo "Resumen".
                    //var workbook = new Aspose.Cells.Workbook(new MemoryStream(excelFile));
                    //var worksheetResumen = workbook.Worksheets["Resumen"];
                    //var worksheetEstadistica = workbook.Worksheets["Estadística"];  // Nueva línea

                    //worksheetResumen.Cells.HideColumn(0);
                    //worksheetResumen.Cells.HideColumn(2);
                    //worksheetResumen.Cells.HideColumn(3);
                    //worksheetResumen.Cells.HideColumn(4);
                    //worksheetResumen.Cells.HideColumn(5);
                    //worksheetResumen.Cells.HideColumn(6);
                    //worksheetResumen.Cells.HideColumn(7);

                    //// Renderizar la hoja de cálculo como imagen
                    //ImageOrPrintOptions options = new ImageOrPrintOptions
                    //{
                    //    OnePagePerSheet = true,
                    //    SaveFormat = SaveFormat.Png
                    //};

                    //// Renderizar la hoja "Resumen" como imagen
                    //SheetRender srResumen = new SheetRender(worksheetResumen, options);
                    //MemoryStream imageStreamResumen = new MemoryStream();
                    //srResumen.ToImage(0, imageStreamResumen);

                    //// Convertir la imagen en un iTextSharp Image
                    //iTextSharp.text.Image imageResumen = iTextSharp.text.Image.GetInstance(imageStreamResumen.GetBuffer());

                    //// Ajustar la imagen al tamaño de la página.
                    //imageResumen.ScaleToFit(document.PageSize.Width, document.PageSize.Height);

                    //// Alinear la imagen al centro de la página.
                    //imageResumen.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                    //// Antes de agregar la imagen de Resumen
                    //document.Add(new Paragraph("Reporte de Prioridades"));

                    //document.Add(imageResumen);
                    //document.Add(new Paragraph("Se muestra todas las prioridades de la evaluación para un mayor entendimiento del problema " +
                    //    "de la empresa y tomar las desiciones correspondientes del caso dado."));
                    //// Renderizar la hoja "Estadística" como imagen  // Nueva sección
                    //SheetRender srEstadistica = new SheetRender(worksheetEstadistica, options);
                    //MemoryStream imageStreamEstadistica = new MemoryStream();
                    //srEstadistica.ToImage(0, imageStreamEstadistica);

                    //// Convertir la imagen en un iTextSharp Image
                    //iTextSharp.text.Image imageEstadistica = iTextSharp.text.Image.GetInstance(imageStreamEstadistica.GetBuffer());

                    //// Ajustar la imagen al tamaño de la página.
                    //imageEstadistica.ScaleToFit(document.PageSize.Width, document.PageSize.Height);

                    //// Alinear la imagen al centro de la página.
                    //imageEstadistica.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                    //// Antes de agregar la imagen de Estadística
                    //document.NewPage();
                    //document.Add(new Paragraph("Reporte de Estadísticas"));

                    //document.Add(imageEstadistica);  // Nueva línea
                    #endregion

                    // Cargar el archivo Excel en memoria.
                    excelPackage.Load(new MemoryStream(excelFile));

                    // Obtener la hoja de trabajo "Resumen".
                    var worksheetResumen = excelPackage.Workbook.Worksheets["Resumen"];

                    

                    // Cargar el archivo Excel en memoria.
                    excelPackage.Load(new MemoryStream(excelFile));

                    // Obtener la hoja de trabajo "Resumen".
                    var workbook = new Aspose.Cells.Workbook(new MemoryStream(excelFile));
                    var worksheetEstadistica = workbook.Worksheets["Estadística"];  // Nueva línea

                    // Renderizar la hoja de cálculo como imagen
                    ImageOrPrintOptions options = new ImageOrPrintOptions
                    {
                        OnePagePerSheet = true,
                        SaveFormat = SaveFormat.Png
                    };

                    // Renderizar la hoja "Estadística" como imagen  // Nueva sección
                    SheetRender srEstadistica = new SheetRender(worksheetEstadistica, options);
                    MemoryStream imageStreamEstadistica = new MemoryStream();
                    srEstadistica.ToImage(0, imageStreamEstadistica);

                    // Convertir la imagen en un iTextSharp Image
                    iTextSharp.text.Image imageEstadistica = iTextSharp.text.Image.GetInstance(imageStreamEstadistica.GetBuffer());

                    // Ajustar la imagen al tamaño de la página.
                    imageEstadistica.ScaleToFit(document.PageSize.Width, document.PageSize.Height);

                    // Alinear la imagen al centro de la página.
                    imageEstadistica.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                    // Para darle formato al titulo
                    Paragraph tituloEstadistica = new Paragraph("Reporte de Estadísticas", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 18, iTextSharp.text.Font.UNDERLINE));
                    tituloEstadistica.Alignment = Element.ALIGN_CENTER;
                    document.Add(tituloEstadistica);

                    document.Add(imageEstadistica);  // Nueva línea

                    document.NewPage();
                    // Crear una tabla PDF.
                    var table = new PdfPTable(2);  // Ajustar el número de columnas.

                    // Establecer los anchos de las columnas
                    float[] columnWidths = { 5f, 1f };
                    table.SetWidths(columnWidths);

                    // Para darle formato al titulo
                    Paragraph tituloPrioridad = new Paragraph("Reporte de Prioridades", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 18, iTextSharp.text.Font.UNDERLINE));
                    tituloPrioridad.Alignment = Element.ALIGN_CENTER;
                    document.Add(tituloPrioridad);
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph(" "));

                    // Iterar a través de todas las filas y columnas en la hoja de trabajo de Excel.
                    for (int row = worksheetResumen.Dimension.Start.Row; row <= worksheetResumen.Dimension.End.Row; row++)
                    {
                        // Comprobar si el valor de la segunda columna es "Objetivo Alcanzado".
                        if (worksheetResumen.Cells[row, 9].Text == "Objetivo Alcanzado")
                        {
                            continue;  // Saltar al siguiente ciclo del bucle.
                        }

                        for (int col = worksheetResumen.Dimension.Start.Column; col <= worksheetResumen.Dimension.End.Column; col++)
                        {
                            // Excluir las columnas que no se quieren incluir.
                            if (col == 1 || col == 3 || col == 4 || col == 5 || col == 6 || col == 7 || col == 8)
                            {
                                continue;
                            }
                            //string[] splitSubcategoria = worksheetResumen.Cells[row, 1].Text.Split('-');
                            //string subcategoriaCorta = splitSubcategoria[1].Split(':')[0];

                            // Obtener el valor de la celda.
                            var cellValue = worksheetResumen.Cells[row, col].Text;

                            // Crear una celda PDF con el valor y añadirla a la tabla.
                            var pdfCell = new PdfPCell(new Phrase(cellValue));

                            if (col == 2)  // La primera columna en la tabla PDF.
                            {
                                pdfCell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                            }
                            else if (col == 9)  // La segunda columna en la tabla PDF.
                            {
                                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            }

                            table.AddCell(pdfCell);
                        }
                    }


                    

                    // Añadir la tabla al documento.
                    document.Add(table);

                    // Generar texto utilizando OpenAI.
                    document.NewPage();

                    // Para darle formato al titulo
                    Paragraph tituloMejora = new Paragraph("Reporte de Mejoras", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 18, iTextSharp.text.Font.UNDERLINE));
                    tituloMejora.Alignment = Element.ALIGN_CENTER;
                    document.Add(tituloMejora);
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph(" "));
                    //string preguntaParaOpenAI = "Como te llamas?";
                    //string respuestaOpenAI = await new OpenAIService().GenerarTextoConOpenAI(preguntaParaOpenAI);

                    //// Inicializar el diccionario para guardar las respuestas agrupadas por prioridad.
                    //Dictionary<string, List<string>> respuestasPorPrioridad = new Dictionary<string, List<string>>();

                    //int lastRow = worksheetResumen.Dimension.End.Row;

                    //// Recorrer las filas de la hoja de trabajo.
                    //for (int i = 3; i <= lastRow; i++)
                    //{
                    //    // Accede a los valores de las celdas en las columnas que necesitas.
                    //    string celda1 = worksheetResumen.Cells[i, 1].Text; // Columna B.
                    //    string celdaI = worksheetResumen.Cells[i, 9].Text; // Columna I.

                    //    // Solo llama a la API de OpenAI si la celda en la columna I no contiene "Objetivo Alcanzado".
                    //    if (celdaI != "Objetivo Alcanzado")
                    //    {
                    //        // Crear el prompt para OpenAI.
                    //        string prompt = $"Como un experto en seguridad de la información y prevención de fugas de datos, analiza la siguiente situación de prioridad {celdaI}: '{celda1}'. ¿Cuál sería tu recomendación de mejora?";

                    //        // Generar la respuesta de OpenAI.
                    //        string respuestaOpenAI = await new OpenAIService().GenerarTextoConOpenAI(prompt);

                    //        // Agregar la respuesta al diccionario en la lista correspondiente a su prioridad.
                    //        if (!respuestasPorPrioridad.ContainsKey(celdaI))
                    //        {
                    //            respuestasPorPrioridad[celdaI] = new List<string>();
                    //        }
                    //        respuestasPorPrioridad[celdaI].Add(respuestaOpenAI);
                    //    }
                    //}

                    //// Recorrer el diccionario y agregar las respuestas al documento PDF.
                    //foreach (string prioridad in respuestasPorPrioridad.Keys)
                    //{
                    //    // Agregar la prioridad al documento.
                    //    document.Add(new Paragraph($"Prioridad: {prioridad}"));

                    //    // Recorrer las respuestas de esta prioridad y agregarlas al documento.
                    //    foreach (string respuesta in respuestasPorPrioridad[prioridad])
                    //    {
                    //        document.Add(new Paragraph($"Mejora: {respuesta}"));
                    //    }
                    //}
                    int lastRow = worksheetResumen.Dimension.End.Row;
                    List<(string RespuestaCliente, string Prioridad)> datos = new List<(string, string)>();

                    for (int i = 3; i <= lastRow; i++)
                    {
                        // Accede a los valores de las celdas en las columnas que necesitas.
                        string celda1 = worksheetResumen.Cells[i, 2].Text; // Columna B.
                        string celdaI = worksheetResumen.Cells[i, 9].Text; // Columna I.

                        // Solo llama a la API de OpenAI si la celda en la columna I no contiene "Objetivo Alcanzado".
                        if (celdaI != "Objetivo Alcanzado")
                        {
                            datos.Add((celda1, celdaI));
                        }
                    }

                    // Después de generar la tabla en el PDF...
                    List<Task<string>> respuestaTareas = new List<Task<string>>();
                    foreach (var dato in datos)
                    {
                        string prompt = $"Soy un especialista en seguridad de la información y prevención de fuga de datos. Me he percatado de un problema clasificado con una prioridad " +
                            $"{dato.Prioridad} en nuestra infraestructura de seguridad. El problema específico es: {dato.RespuestaCliente}. " +
                            $"Como experto, ¿qué acciones inmediatas recomendarías para mitigar este problema y prevenir futuras ocurrencias similares? " +
                            $"Por favor, ofrece una respuesta detallada y en español."; respuestaTareas.Add(new OpenAIService().GenerarTextoConOpenAI(prompt));
                    }

                    // Espera a que todas las tareas de generación de texto se completen.
                    string[] respuestas = await Task.WhenAll(respuestaTareas);

                    // Crea una fuente en negrita
                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    // Crea una fuente normal
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                    // Imprime las respuestas de OpenAI en el PDF.
                    for (int i = 0; i < respuestas.Length; i++)
                    {
                        // Usa la fuente en negrita para las etiquetas y la normal para el texto
                        Paragraph prioridad = new Paragraph();
                        prioridad.Add(new Chunk("Prioridad: ", boldFont));
                        prioridad.Add(new Chunk($"{datos[i].Prioridad}", normalFont));
                        prioridad.Alignment = Element.ALIGN_JUSTIFIED;
                        //prioridad.SpacingAfter = 10f;  // Agrega un espacio después de este párrafo.
                        document.Add(prioridad);

                        Paragraph respuestaCliente = new Paragraph();
                        respuestaCliente.Add(new Chunk("Respuesta del Cliente: ", boldFont));
                        respuestaCliente.Add(new Chunk($"{datos[i].RespuestaCliente}", normalFont));
                        respuestaCliente.Alignment = Element.ALIGN_JUSTIFIED;
                        //respuestaCliente.SpacingAfter = 10f;  // Agrega un espacio después de este párrafo.
                        document.Add(respuestaCliente);

                        // Traduce la respuesta de OpenAI al español antes de añadirla al PDF.
                        string respuestaTraducida = TranslateText(respuestas[i]);
                        Paragraph mejora = new Paragraph();
                        mejora.Add(new Chunk("Mejora: ", boldFont));
                        mejora.Add(new Chunk($"{respuestaTraducida}", normalFont));
                        mejora.Alignment = Element.ALIGN_JUSTIFIED;
                        //mejora.SpacingAfter = 20f;  // Agrega un espacio más grande después de este párrafo.
                        document.Add(mejora);

                        // Agrega un espacio después de cada conjunto de 'Prioridad', 'Respuesta del Cliente' y 'Mejora'.
                        document.Add(new Chunk("\n"));
                    }

                    // Agregar la respuesta de OpenAI al documento.
                    //document.Add(new Paragraph(respuestaOpenAI));

                }

                // Cerrar el documento.
                document.Close();

                // Cerrar el escritor.
                writer.Close();

                // Convertir el MemoryStream a un array de bytes.
                byte[] bytes = stream.ToArray();

                // Devolver el array de bytes como un archivo para descarga.
                return File(bytes, "application/pdf", "Reporte_" + codigo + ".pdf");
            }
        }

        public class OpenAIService
        {
            //private readonly string apiKey = "clave_api";
            private readonly string apiKeyOpenAI = Environment.GetEnvironmentVariable("OpenAI");

            public async Task<string> GenerarTextoConOpenAI(string mensaje)
            {
                // Crear el mensaje para enviar a GPT-3.
                //string mensaje = " Quiero saber como implementar un api. " /*+ email*/;

                using (var httpClient = new HttpClient())
                {
                    // Aumentar el tiempo de espera a 2 minutos (el tiempo de espera predeterminado es 100 segundos).
                    httpClient.Timeout = TimeSpan.FromMinutes(2);

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKeyOpenAI);

                    var content = new StringContent(
                        JsonConvert.SerializeObject(new { prompt = mensaje, max_tokens = 2048 }),
                        Encoding.UTF8,
                        "application/json");

                    var response = await httpClient.PostAsync("https://api.openai.com/v1/engines/text-davinci-002/completions", content);

                    var result = await response.Content.ReadAsStringAsync();
                    var parsedResult = JsonConvert.DeserializeObject<OpenAIResponse>(result);

                    string text = parsedResult.Choices[0].Text;

                    // Quitar los caracteres "\n" del inicio del texto.
                    text = text.TrimStart('\n');
                    text = text.TrimStart('+');

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
 
        public static string TranslateText(string input, string targetLanguage = "es")
        {
            //TranslationClient client = TranslationClient.CreateFromApiKey("clave_api");
            //var response = client.TranslateText(input, targetLanguage);
            //return response.TranslatedText;
            string apiKeyGoogleTranslate = Environment.GetEnvironmentVariable("ApiGoogleTranslate");
            try
            {
                TranslationClient client = TranslationClient.CreateFromApiKey(apiKeyGoogleTranslate);
                var response = client.TranslateText(input, targetLanguage);
                return response.TranslatedText;
            }
            catch (Exception)
            {
                // Puedes registrar el error aquí si lo deseas.
                // Retorna el texto original si la traducción falla.
                return input;
            }
        }

        //private string GenerarTextoConOpenAI(string email)
        //{
        //    // Conexion de la api de Open AI
        //    string apiKey = "clave_api";
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

                // Agregar encabezados
                worksheetResultados.Cells["H9"].Value = "Prioridad";
                worksheetResultados.Cells["I9"].Value = "Cantidad";

                // Agregar las categorías de prioridad
                worksheetResultados.Cells["H10"].Value = "Objetivo Alcanzado";
                worksheetResultados.Cells["H11"].Value = "Bajo";
                worksheetResultados.Cells["H12"].Value = "Medio";
                worksheetResultados.Cells["H13"].Value = "Alto";
                worksheetResultados.Cells["H14"].Value = "Crítico";

                // Calcular el número de cada tipo de prioridad en la hoja de "Resumen"
                var prioridadCounts = new Dictionary<string, int>()
                {
                    { "Objetivo Alcanzado", 0 },
                    { "Bajo", 0 },
                    { "Medio", 0 },
                    { "Alto", 0 },
                    { "Crítico", 0 }
                };

                for (int i = 3; i <= worksheet.Dimension.End.Row; i++)
                {
                    string prioridad = worksheet.Cells[i, 9].Value.ToString();
                    if (prioridadCounts.ContainsKey(prioridad))
                    {
                        prioridadCounts[prioridad]++;
                    }
                }

                // Agregar los resultados a la hoja de "Resultados"
                worksheetResultados.Cells["I10"].Value = prioridadCounts["Objetivo Alcanzado"];
                worksheetResultados.Cells["I11"].Value = prioridadCounts["Bajo"];
                worksheetResultados.Cells["I12"].Value = prioridadCounts["Medio"];
                worksheetResultados.Cells["I13"].Value = prioridadCounts["Alto"];
                worksheetResultados.Cells["I14"].Value = prioridadCounts["Crítico"];

                // Establecer un rango para aplicar bordes (H9:I14)
                ExcelRange rangeWithBordersPrioridades = worksheetResultados.Cells["H9:I14"];

                // Aplicar bordes a todas las celdas en el rango
                rangeWithBordersPrioridades.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangeWithBordersPrioridades.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangeWithBordersPrioridades.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangeWithBordersPrioridades.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Agregar encabezados en las columnas N, O, P
                worksheetResultados.Cells["N1"].Value = "Nivel de Madurez";
                worksheetResultados.Cells["O1"].Value = "Estado Actual";
                worksheetResultados.Cells["P1"].Value = "Estado Deseado";

                // Definir los niveles de madurez
                string[] nivelesMadurez = { "Inicial", "Gestionado", "Definido", "Predecible", "Optimizado" };

                for (int i = 0; i < nivelesMadurez.Length; i++)
                {
                    // Escribir el nivel de madurez en la columna N
                    worksheetResultados.Cells[i + 2, 14].Value = nivelesMadurez[i];

                    // Contar y escribir el número de ocurrencias en el estado actual (columna C de "Resumen")
                    int countActual = resumenes.Count(r => r.oPuntajeActual.idPuntaje == i + 1);
                    worksheetResultados.Cells[i + 2, 15].Value = countActual;

                    // Contar y escribir el número de ocurrencias en el estado deseado (columna E de "Resumen")
                    int countDeseado = resumenes.Count(r => r.oPuntajeDeseado.idPuntaje == i + 1);
                    worksheetResultados.Cells[i + 2, 16].Value = countDeseado;
                }

                // Definir el rango desde N1 hasta P6
                ExcelRange rangeN1P6 = worksheetResultados.Cells["N1:P6"];

                // Centrar horizontal y verticalmente el texto en las celdas
                rangeN1P6.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rangeN1P6.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Aplicar bordes a todas las celdas en el rango
                rangeN1P6.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangeN1P6.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangeN1P6.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangeN1P6.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Ajustar el texto en las celdas de la columna "N" desde N1 hasta N6
                for (int i = 1; i <= 6; i++)
                {
                    worksheetResultados.Cells[i, 14].Style.WrapText = true;
                }

                // Ajustar el texto en las celdas de la columna "O" desde O1 hasta O6
                for (int i = 1; i <= 6; i++)
                {
                    worksheetResultados.Cells[i, 15].Style.WrapText = true;
                }

                // Ajustar el texto en las celdas de la columna "P" desde P1 hasta P6
                for (int i = 1; i <= 6; i++)
                {
                    worksheetResultados.Cells[i, 16].Style.WrapText = true;
                }

                // Establecer el ancho de la columna "N" a 16 unidades (aproximadamente 150 píxeles)
                worksheetResultados.Column(14).Width = 16;

                // Establecer el ancho de la columna "O" a 16 unidades
                worksheetResultados.Column(15).Width = 16;

                // Establecer el ancho de la columna "P" a 16 unidades
                worksheetResultados.Column(16).Width = 16;


                #endregion

                #region HOJA ESTADISTICA

                // Crear la hoja de trabajo
                var worksheetEstadistica = excelPackage.Workbook.Worksheets.Add("Estadística");

                // Obteniendo la hoja de Resumen
                var worksheetResumen_Estadistica = excelPackage.Workbook.Worksheets["Resultados"];

                var chartRangeBrecha = worksheetResumen_Estadistica.Cells["K2:K6"];
                var chartRangeObjetivo = worksheetResumen_Estadistica.Cells["J2:J6"];

                // Añade un gráfico de barras en la posición dada (fila 22, columna 0 en este ejemplo)
                var barChart = worksheetEstadistica.Drawings.AddChart("barChart", eChartType.ColumnClustered) as ExcelBarChart;

                // Añade un gráfico de barras en la posición dada (fila 22, columna 8 en este ejemplo)
                var barChartTendencia = worksheetEstadistica.Drawings.AddChart("barChartTendencia", eChartType.ColumnClustered) as ExcelBarChart;

                // Añade un gráfico en la posición dada (fila 7, columna 5 en este ejemplo)
                var pieChartBrecha = worksheetEstadistica.Drawings.AddChart("pieChartBrecha", eChartType.Pie) as ExcelPieChart;
                var pieChartObjetivo = worksheetEstadistica.Drawings.AddChart("pieChartObjetivo", eChartType.Pie) as ExcelPieChart;

                // Define los datos del gráfico
                var serieBrecha = pieChartBrecha.Series.Add(worksheetResumen_Estadistica.Cells["K2:K6"], worksheetResumen_Estadistica.Cells["H2:H6"]);
                var serieObjetivo = pieChartObjetivo.Series.Add(worksheetResumen_Estadistica.Cells["J2:J6"], worksheetResumen_Estadistica.Cells["H2:H6"]);

                // Define los datos del gráfico
                var seriePrioridad = barChart.Series.Add(worksheetResumen_Estadistica.Cells["I10:I14"], worksheetResumen_Estadistica.Cells["H10:H14"]);

                // Define las dos series de datos del gráfico
                var seriesActual = barChartTendencia.Series.Add(worksheetResumen_Estadistica.Cells["O2:O6"], worksheetResumen_Estadistica.Cells["N2:N6"]);
                var seriesDeseado = barChartTendencia.Series.Add(worksheetResumen_Estadistica.Cells["P2:P6"], worksheetResumen_Estadistica.Cells["N2:N6"]);

                // Establece las propiedades del gráfico
                pieChartBrecha.Title.Text = "Prioridad según Función";
                //pieChartBrecha.DataLabel.ShowCategory = true;
                pieChartBrecha.DataLabel.ShowPercent = true;

                pieChartObjetivo.Title.Text = "Estado Deseado según Función";
                //pieChartObjetivo.DataLabel.ShowCategory = true;
                pieChartObjetivo.DataLabel.ShowPercent = true;

                // Define la posición del primer gráfico
                pieChartBrecha.SetPosition(0, 0, 0, 0); // El gráfico de brecha comienza en la fila 5, columna 5
                pieChartBrecha.SetSize(400, 400);

                // Define la posición del segundo gráfico
                pieChartObjetivo.SetPosition(0, 0, 7, 0); // El gráfico de objetivo comienza en la fila 5, columna 15
                pieChartObjetivo.SetSize(400, 400);

                // Establece las propiedades del gráfico
                barChart.Title.Text = "Cantidad de Prioridades"; 
                barChart.DataLabel.ShowCategory = false;
                barChart.DataLabel.ShowValue = true;

                // Define la posición del gráfico
                barChart.SetPosition(22, 0, 0, 0); // El gráfico comienza en la fila 22, columna 0
                barChart.SetSize(400, 400);

                // Establece las propiedades de las series
                seriesActual.Header = worksheetResumen_Estadistica.Cells["O1"].Value.ToString(); // Pone el encabezado de la columna O como nombre de la serie
                seriesDeseado.Header = worksheetResumen_Estadistica.Cells["P1"].Value.ToString(); // Pone el encabezado de la columna P como nombre de la serie

                // Establece las propiedades del gráfico
                barChartTendencia.Title.Text = "Estado Actual vs Estado Deseado"; 
                barChartTendencia.DataLabel.ShowCategory = false;
                barChartTendencia.DataLabel.ShowValue = true;
                barChartTendencia.Legend.Position = eLegendPosition.Bottom;

                // Añadir una línea de tendencia al gráfico para la serie "Estado Actual"
                var trendline = seriesActual.TrendLines.Add(eTrendLine.Linear);
                trendline.DisplayEquation = false;
                trendline.DisplayRSquaredValue = false;

                // Define la posición del gráfico
                barChartTendencia.SetPosition(22, 0, 7, 0); // El gráfico comienza en la fila 22, columna 7
                barChartTendencia.SetSize(400, 400);


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

                #region HOJA DEL MAPA DE CALOR

                // Agregar una nueva hoja de trabajo llamada "Mapa de Calor"
                ExcelWorksheet worksheetMapaCalor = excelPackage.Workbook.Worksheets.Add("Mapa de Calor");

                // Obteniendo la hoja de Resumen
                var worksheetMapaCalor_Resumen = excelPackage.Workbook.Worksheets["Resumen"];

                // Agregar encabezados, datos y aplicar formato a las celdas de la nueva hoja de trabajo
                worksheetMapaCalor.Cells["B2"].Value = "Mapa de Calor por Función";
                worksheetMapaCalor.Cells["B3"].Value = "Función/Prioridad";
                worksheetMapaCalor.Cells["C3"].Value = "Objetivo Alcanzado";
                worksheetMapaCalor.Cells["D3"].Value = "Bajo";
                worksheetMapaCalor.Cells["E3"].Value = "Medio";
                worksheetMapaCalor.Cells["F3"].Value = "Alto";
                worksheetMapaCalor.Cells["G3"].Value = "Crítico";
                worksheetMapaCalor.Cells["B4"].Value = "Identificar";
                worksheetMapaCalor.Cells["B5"].Value = "Proteger";
                worksheetMapaCalor.Cells["B6"].Value = "Detectar";
                worksheetMapaCalor.Cells["B7"].Value = "Responder";
                worksheetMapaCalor.Cells["B8"].Value = "Recuperar";

                // Unir las celdas B2:G2
                ExcelRange rangeMergeCells = worksheetMapaCalor.Cells["B2:G2"];
                rangeMergeCells.Merge = true;

                // Centrar el contenido de las celdas unidas
                rangeMergeCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rangeMergeCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Aplicar estilos de borde a las celdas unidas
                rangeMergeCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangeMergeCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangeMergeCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangeMergeCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Obtener los datos relevantes de la hoja "Resumen"
                var columnAData = worksheetMapaCalor_Resumen.Cells["A3:A33"].Select(c => c.Value.ToString()).ToList();
                var columnIData = worksheetMapaCalor_Resumen.Cells["I3:I33"].Select(c => c.Value.ToString()).ToList();

                // Procesar los datos obtenidos
                Dictionary<string, Dictionary<string, int>> groupCount = new Dictionary<string, Dictionary<string, int>>();
                for (int i = 0; i < columnAData.Count; i++)
                {
                    var group = columnAData[i].Substring(columnAData[i].IndexOf("-") + 1, columnAData[i].IndexOf(".") - columnAData[i].IndexOf("-") - 1);
                    var category = columnIData[i];

                    if (!groupCount.ContainsKey(group))
                        groupCount[group] = new Dictionary<string, int>();

                    if (groupCount[group].ContainsKey(category))
                        groupCount[group][category]++;
                    else
                        groupCount[group][category] = 1;
                }

                // Actualizar el mapa de calor en la hoja "Mapa de Calor"
                int startCol = 0;
                foreach (var group in groupCount)
                {
                    int startRow = GetRowIndexByCategory(group.Key);
                    foreach (var category in group.Value)
                    {
                        int count = category.Value;
                        int col = startCol + GetColumnIndexByCategory(category.Key);
                        int row = startRow;


                        worksheetMapaCalor.Cells[row, col].Value = count;
                    }
                }

                // Actualizar el mapa de calor en la hoja "Mapa de Calor"
                int startColCero = 2; // Índice de columna inicial para el rango C4:G8
                int startRowCero = 3; // Índice de fila inicial para el rango C4:G8

                int rowCountCero = 5; // Número total de filas en el rango C4:G8
                int colCountCero = 5; // Número total de columnas en el rango C4:G8

                for (int row = startRowCero; row <= startRowCero + rowCountCero; row++)
                {
                    for (int col = startColCero; col <= startColCero + colCountCero; col++)
                    {
                        var cell = worksheetMapaCalor.Cells[row, col];

                        // Verificar si la celda está vacía y asignar 0 en caso afirmativo
                        if (cell.Value == null)
                        {
                            cell.Value = 0;
                        }
                    }
                }

                /* Metodo para hacer el mapa de colores */
                int minValue = 0;
                int maxValue = int.MinValue;

                foreach (var cell in range)
                {
                    if (cell.Value != null && int.TryParse(cell.Value.ToString(), out int value))
                    {
                        if (value > maxValue)
                        {
                            maxValue = value;
                        }
                    }
                }

                // Obtener el rango C4:G8
                var rangoC4G8 = worksheetMapaCalor.Cells["C4:G8"];

                // Recorrer cada celda del rango y establecer el color de fondo
                foreach (var cell in rangoC4G8)
                {
                    if (cell.Value == null)
                    {
                        cell.Value = 0;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                    }
                    else
                    {
                        int value = int.Parse(cell.Value.ToString());
                        double percentage = (double)value / maxValue;

                        Color color = InterpolateColor(Color.LightGreen, Color.Red, percentage);

                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(color);
                        //int value = int.Parse(cell.Value.ToString());
                        //double percentage = (double)value / maxValue;

                        //Color color = GetColorByPercentage(percentage);

                        //cell.Value = value;
                        //cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //cell.Style.Fill.BackgroundColor.SetColor(color);
                    }
                }

                // Aplicar formato condicional de escalas de colores al rango C4:G8
                var rango = worksheetMapaCalor.Cells["C4:G8"];
                var conditionalFormatting = rango.ConditionalFormatting.AddThreeColorScale();

                // Establecer los colores mínimo, medio y máximo
                Color minColor = Color.FromArgb(0, 196, 89);   // Verde claro
                Color midColor = Color.FromArgb(239, 255, 121);   // Amarillo claro
                Color maxColor = Color.FromArgb(255, 79, 79);   // Rojo claro

                conditionalFormatting.LowValue.Color = minColor;
                conditionalFormatting.MiddleValue.Color = midColor;
                conditionalFormatting.HighValue.Color = maxColor;

                // Aplicar el formato condicional al rango
                rango.Style.Numberformat.Format = "General"; // Establecer el formato numérico

                // Establecer un rango para aplicar bordes (B2:G8)
                ExcelRange rangeWithBordersMapaCalor = worksheetMapaCalor.Cells["B3:G8"];

                // Centrar el contenido de las celdas
                rangeWithBordersMapaCalor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rangeWithBordersMapaCalor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Aplicar bordes a todas las celdas en el rango
                rangeWithBordersMapaCalor.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangeWithBordersMapaCalor.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangeWithBordersMapaCalor.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangeWithBordersMapaCalor.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                rangeWithBordersMapaCalor.AutoFitColumns();



                #endregion

                // Guardar el paquete de Excel como un arreglo de bytes
                byte[] fileContents = excelPackage.GetAsByteArray();
                return fileContents;
            }
        }

        private Color GetColorByPercentage(double percentage)
        {
            int red, green, blue;

            if (percentage <= 0.5)
            {
                red = (int)(510 * percentage); // Desde 0 a 0.5, incrementa el rojo de 0 a 255
                green = 255; // Mantén el verde en su valor máximo (255)
            }
            else
            {
                red = 255; // Mantén el rojo en su valor máximo (255)
                green = (int)(510 * (1 - percentage)); // Desde 0.5 a 1, decrementa el verde de 255 a 0
            }

            blue = 0; // Mantén el azul en su valor mínimo (0)

            return Color.FromArgb(red, green, blue);
        }

        // Método para interpolar un color entre dos colores dados en función de un porcentaje
        private static Color InterpolateColor(Color minColor, Color maxColor, double percentage)
        {
            // Obtener los componentes RGB de los colores mínimo y máximo
            int minR = minColor.R;
            int minG = minColor.G;
            int minB = minColor.B;

            int maxR = maxColor.R;
            int maxG = maxColor.G;
            int maxB = maxColor.B;

            // Calcular los valores interpolados de los componentes RGB
            int r = (int)Math.Round(minR + (maxR - minR) * percentage);
            int g = (int)Math.Round(minG + (maxG - minG) * percentage);
            int b = (int)Math.Round(minB + (maxB - minB) * percentage);

            // Asegurarse de que los valores de color estén dentro del rango válido (0-255)
            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));

            return Color.FromArgb(r, g, b);
        }

        // Método para obtener el índice de columna en el mapa de calor según la categoría
        private int GetRowIndexByCategory(string category)
        {
            switch (category)
            {
                case "ID":
                    return 4;
                case "PR":
                    return 5;
                case "DE":
                    return 6;
                case "RS":
                    return 7;
                case "RC":
                    return 8;
                default:
                    return -1;
            }
        }

        // Método para obtener el índice de fila en el mapa de calor según la categoría
        private int GetColumnIndexByCategory(string category)
        {
            switch (category)
            {
                case "Bajo":
                    return 4;
                case "Medio":
                    return 5;
                case "Alto":
                    return 6;
                case "Crítico":
                    return 7;
                case "Objetivo Alcanzado":
                    return 3;
                default:
                    return -1;
            }
        }

    }

    //public class InactividadFilter : ActionFilterAttribute
    //{
    //    public override void OnActionExecuting(ActionExecutingContext filterContext)
    //    {
    //        // Verificar si el usuario ha iniciado sesión
    //        if (filterContext.HttpContext.User.Identity.IsAuthenticated)
    //        {
    //            // Obtener la última actividad registrada en la sesión
    //            DateTime ultimaActividad = Convert.ToDateTime(filterContext.HttpContext.Session["ultimaActividad"]);

    //            // Calcular la diferencia de tiempo entre la última actividad y el momento actual
    //            TimeSpan tiempoInactividad = DateTime.Now - ultimaActividad;

    //            // Comprobar si el tiempo de inactividad supera los 5 minutos (300 segundos)
    //            if (tiempoInactividad.TotalSeconds > 300000000000000000)
    //            {
    //                // Cerrar la sesión y redirigir al controlador "Acceso" para cerrar sesión
    //                filterContext.HttpContext.Session.Clear();
    //                filterContext.HttpContext.Session.Abandon();
    //                filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));
    //                filterContext.HttpContext.Response.Redirect("~/Acceso/CerrarSesion");
    //                return;
    //            }
    //        }

    //        // Actualizar la última actividad registrada en la sesión
    //        filterContext.HttpContext.Session["ultimaActividad"] = DateTime.Now;

    //        base.OnActionExecuting(filterContext);
    //    }
    //}
}