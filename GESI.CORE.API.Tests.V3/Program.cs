using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GESI.CORE.API.Tests.V3
{
    /// <summary>
    /// Ejecuta cada endpoint de pruebas.json contra la API V3 y, si está configurada la UrlV2,
    /// manda la misma solicitud a la V2 y compara las dos respuestas campo por campo.
    /// Las respuestas y las diferencias quedan en la carpeta Resultados\fecha_hora.
    /// </summary>
    internal class Program
    {
        static readonly JsonSerializerOptions opcionesLectura = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        static readonly JsonSerializerOptions opcionesEscritura = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string carpetaBase = AppContext.BaseDirectory;
            string filtro = args.Length > 0 ? args[0] : "";

            // 1. Configuración y lista de pruebas
            cConfiguracion oConfiguracion = JsonSerializer.Deserialize<cConfiguracion>(File.ReadAllText(Path.Combine(carpetaBase, "configuracion.json")), opcionesLectura)
                                            ?? throw new InvalidOperationException("No se pudo leer configuracion.json");
            List<cPrueba> lstPruebas = JsonSerializer.Deserialize<List<cPrueba>>(File.ReadAllText(Path.Combine(carpetaBase, "pruebas.json")), opcionesLectura)
                                       ?? new List<cPrueba>();

            if (filtro != "")
            {
                lstPruebas = lstPruebas.Where(p => p.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                                                   p.Ruta.Contains(filtro, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            bool compararConV2 = oConfiguracion.UrlV2 != "";

            string carpetaResultados = Path.Combine(carpetaBase, "Resultados", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(carpetaResultados);

            Console.WriteLine("API V3: " + oConfiguracion.UrlV3);
            Console.WriteLine("API V2: " + (compararConV2 ? oConfiguracion.UrlV2 : "(sin comparar)"));
            Console.WriteLine("Escrituras: " + (oConfiguracion.EjecutarEscrituras ? "SE EJECUTAN (solo en V3)" : "omitidas"));
            Console.WriteLine("Pruebas: " + lstPruebas.Count + (filtro != "" ? " (filtro: " + filtro + ")" : ""));
            Console.WriteLine("Resultados: " + carpetaResultados);
            Console.WriteLine();

            // 2. Cliente HTTP
            HttpClientHandler handler = new HttpClientHandler();
            if (oConfiguracion.AceptarCertificadosInvalidos)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            HttpClient cliente = new HttpClient(handler);
            cliente.Timeout = TimeSpan.FromSeconds(oConfiguracion.TimeoutSegundos);

            // 3. Login (si no se usa ApiKey)
            string tokenV3 = "";
            string tokenV2 = "";

            try
            {
                if (oConfiguracion.ApiKey == "")
                {
                    tokenV3 = await ObtenerToken(cliente, oConfiguracion, oConfiguracion.UrlV3);
                    Console.WriteLine("Login V3 correcto.");

                    if (compararConV2)
                    {
                        tokenV2 = await ObtenerToken(cliente, oConfiguracion, oConfiguracion.UrlV2);
                        Console.WriteLine("Login V2 correcto.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo hacer login: " + ex.Message);
                Console.ResetColor();
                return 1;
            }

            Console.WriteLine();

            // 4. Pruebas
            int cantidadOK = 0;
            int cantidadFallas = 0;
            int cantidadOmitidas = 0;
            int cantidadDiferentesV2 = 0;
            List<string> lstResumen = new List<string>();

            for (int i = 0; i < lstPruebas.Count; i++)
            {
                cPrueba oPrueba = lstPruebas[i];
                string numero = (i + 1).ToString("000");
                string nombreArchivo = numero + "_" + Regex.Replace(oPrueba.Nombre, @"[^\w]+", "_");
                string encabezado = "[" + numero + "/" + lstPruebas.Count.ToString("000") + "] ";
                string linea;

                // 4.1 Escrituras: solo si la configuración lo permite, y nunca contra la V2 (grabaría dos veces)
                if (oPrueba.Escritura && !oConfiguracion.EjecutarEscrituras)
                {
                    linea = encabezado + "OMITIDA  " + oPrueba.Metodo.PadRight(6) + oPrueba.Ruta + "  (escritura)";
                    Escribir(linea, ConsoleColor.DarkGray);
                    lstResumen.Add(linea);
                    cantidadOmitidas++;
                    continue;
                }

                // 4.2 Reemplazar los {{Valores}} en ruta, query y body
                string query = ReemplazarValores(oPrueba.Query, oConfiguracion.Valores, enUrl: true);
                string? body = null;

                if (oPrueba.BodyArchivo != "")
                {
                    // Bodies largos (comprobantes) en archivos aparte: carpeta Bodies
                    body = ReemplazarValores(File.ReadAllText(Path.Combine(carpetaBase, "Bodies", oPrueba.BodyArchivo)), oConfiguracion.Valores, enUrl: false);
                }
                else if (oPrueba.Body.HasValue && oPrueba.Body.Value.ValueKind != JsonValueKind.Null)
                {
                    body = ReemplazarValores(oPrueba.Body.Value.GetRawText(), oConfiguracion.Valores, enUrl: false);
                }

                List<string> lstFaltantes = BuscarValoresFaltantes(query + " " + body);

                if (oPrueba.Escritura && body == null && (oPrueba.Metodo == "POST" || oPrueba.Metodo == "PUT" || oPrueba.Metodo == "PATCH"))
                {
                    lstFaltantes.Add("Body");
                }

                if (lstFaltantes.Count > 0)
                {
                    linea = encabezado + "OMITIDA  " + oPrueba.Metodo.PadRight(6) + oPrueba.Ruta + "  (falta completar: " + string.Join(", ", lstFaltantes) + ")";
                    Escribir(linea, ConsoleColor.DarkGray);
                    lstResumen.Add(linea);
                    cantidadOmitidas++;
                    continue;
                }

                string rutaConQuery = oPrueba.Ruta + (query != "" ? "?" + query : "");

                // 4.3 Ejecutar en la V3
                cRespuesta oRespuestaV3 = await EjecutarSolicitud(cliente, oConfiguracion, oConfiguracion.UrlV3, tokenV3, oPrueba.Metodo, rutaConQuery, body);
                File.WriteAllText(Path.Combine(carpetaResultados, nombreArchivo + ".v3.json"), Formatear(oRespuestaV3.Texto));

                string resultado;
                ConsoleColor color;

                if (oRespuestaV3.Status != oPrueba.StatusEsperado)
                {
                    resultado = "FALLA";
                    color = ConsoleColor.Red;
                    cantidadFallas++;
                }
                else if (oPrueba.StatusEsperado == 200 && DevolvioSuccessFalse(oRespuestaV3.Texto))
                {
                    resultado = "FALLA";   // 200 pero con success = false
                    color = ConsoleColor.Red;
                    cantidadFallas++;
                }
                else
                {
                    resultado = "OK";
                    color = ConsoleColor.Green;
                    cantidadOK++;
                }

                linea = encabezado + resultado.PadRight(9) + oPrueba.Metodo.PadRight(6) + oRespuestaV3.Status + "  " +
                        (oRespuestaV3.Milisegundos + " ms").PadLeft(8) + "  " + oPrueba.Ruta;

                if (resultado == "FALLA")
                {
                    linea += "  -> " + ResumirError(oRespuestaV3.Texto);
                }

                // 4.3.1 Guardar valores de la respuesta para las pruebas siguientes (ej. el ID creado, o el registro leído para mandarlo en un Update)
                foreach (KeyValuePair<string, string> guardar in oPrueba.Guardar)
                {
                    JsonElement? valorLeido = LeerRuta(oRespuestaV3.Texto, guardar.Value);

                    if (valorLeido.HasValue)
                    {
                        oConfiguracion.Valores[guardar.Key] = valorLeido.Value;
                    }
                    else
                    {
                        linea += "  (no se encontró " + guardar.Value + " para guardar " + guardar.Key + ")";
                    }
                }

                // 4.4 Comparar con la V2 (solo lecturas)
                if (compararConV2 && !oPrueba.Escritura)
                {
                    cRespuesta oRespuestaV2 = await EjecutarSolicitud(cliente, oConfiguracion, oConfiguracion.UrlV2, tokenV2, oPrueba.Metodo, rutaConQuery, body);
                    File.WriteAllText(Path.Combine(carpetaResultados, nombreArchivo + ".v2.json"), Formatear(oRespuestaV2.Texto));

                    List<string> lstDiferencias = new List<string>();

                    if (oRespuestaV2.Status != oRespuestaV3.Status)
                    {
                        lstDiferencias.Add("Status HTTP: V2 " + oRespuestaV2.Status + ", V3 " + oRespuestaV3.Status);
                    }

                    CompararTextos(oRespuestaV2.Texto, oRespuestaV3.Texto, oConfiguracion.CamposIgnorados, lstDiferencias);

                    if (lstDiferencias.Count == 0)
                    {
                        linea += "  | V2: IGUAL";
                    }
                    else
                    {
                        linea += "  | V2: " + lstDiferencias.Count + " DIFERENCIA(S)";
                        if (color == ConsoleColor.Green) color = ConsoleColor.Yellow;
                        cantidadDiferentesV2++;
                        File.WriteAllLines(Path.Combine(carpetaResultados, nombreArchivo + ".diferencias.txt"), lstDiferencias);
                    }
                }

                Escribir(linea, color);
                lstResumen.Add(linea);
            }

            // 5. Resumen
            string totales = "OK: " + cantidadOK + "   Fallas: " + cantidadFallas + "   Omitidas: " + cantidadOmitidas +
                             (compararConV2 ? "   Distintas a la V2: " + cantidadDiferentesV2 : "");

            lstResumen.Add("");
            lstResumen.Add(totales);
            File.WriteAllLines(Path.Combine(carpetaResultados, "resumen.txt"), lstResumen);

            Console.WriteLine();
            Escribir(totales, cantidadFallas > 0 ? ConsoleColor.Red : ConsoleColor.Green);

            return cantidadFallas;   // 0 = todo bien (sirve para usarlo en un script)
        }

        /// <summary>
        /// POST api/Autenticacion/Login con usuario y contraseña. Devuelve el token.
        /// </summary>
        static async Task<string> ObtenerToken(HttpClient cliente, cConfiguracion oConfiguracion, string urlBase)
        {
            string body = JsonSerializer.Serialize(new { Username = oConfiguracion.Usuario, Password = oConfiguracion.Password });

            cRespuesta oRespuesta = await EjecutarSolicitud(cliente, oConfiguracion, urlBase, "", "POST", "api/Autenticacion/Login", body);

            if (oRespuesta.Status != 200)
            {
                throw new Exception(urlBase + " respondió " + oRespuesta.Status + ": " + ResumirError(oRespuesta.Texto));
            }

            using JsonDocument doc = JsonDocument.Parse(oRespuesta.Texto);

            if (!doc.RootElement.TryGetProperty("token", out JsonElement token) || token.GetString() == null)
            {
                throw new Exception(urlBase + " no devolvió el token.");
            }

            return token.GetString()!;
        }

        /// <summary>
        /// Ejecuta una solicitud con los encabezados de la API (GrupoESI, token o ApiKey, empresa y sucursal).
        /// Nunca lanza excepción: si no hay conexión devuelve Status 0 con el mensaje.
        /// </summary>
        static async Task<cRespuesta> EjecutarSolicitud(HttpClient cliente, cConfiguracion oConfiguracion, string urlBase,
                                                       string token, string metodo, string rutaConQuery, string? body)
        {
            cRespuesta oRespuesta = new cRespuesta();
            Stopwatch cronometro = Stopwatch.StartNew();

            try
            {
                HttpRequestMessage solicitud = new HttpRequestMessage(new HttpMethod(metodo), urlBase.TrimEnd('/') + "/" + rutaConQuery.TrimStart('/'));

                solicitud.Headers.Add("GrupoESI", oConfiguracion.GrupoESI);

                if (oConfiguracion.ApiKey != "")
                {
                    solicitud.Headers.Add("GrupoESI-ApiKey", oConfiguracion.ApiKey);
                }
                else if (token != "")
                {
                    solicitud.Headers.Add("Authorization", "Bearer " + token);
                }

                if (oConfiguracion.EmpresaID != "") solicitud.Headers.Add("empresaID", oConfiguracion.EmpresaID);
                if (oConfiguracion.SucursalID != "") solicitud.Headers.Add("sucursalID", oConfiguracion.SucursalID);

                if (body != null)
                {
                    solicitud.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage respuesta = await cliente.SendAsync(solicitud);

                oRespuesta.Status = (int)respuesta.StatusCode;
                oRespuesta.Texto = await respuesta.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                oRespuesta.Status = 0;
                oRespuesta.Texto = "Sin respuesta: " + ex.Message;
            }

            oRespuesta.Milisegundos = cronometro.ElapsedMilliseconds;
            return oRespuesta;
        }

        /// <summary>
        /// Reemplaza cada {{Nombre}} por el valor de configuracion.json → Valores.
        /// En el body, "{{Nombre}}" (con comillas) se reemplaza por el JSON tal cual: así un número queda número
        /// y un objeto (ej. la clave de un comprobante) queda objeto. Los valores vacíos o null no se reemplazan.
        /// </summary>
        static string ReemplazarValores(string texto, Dictionary<string, JsonElement> valores, bool enUrl)
        {
            foreach (KeyValuePair<string, JsonElement> valor in valores)
            {
                if (valor.Value.ValueKind == JsonValueKind.Null) continue;
                if (valor.Value.ValueKind == JsonValueKind.String && valor.Value.GetString() == "") continue;

                string marcador = "{{" + valor.Key + "}}";
                string valorTexto = valor.Value.ValueKind == JsonValueKind.String ? valor.Value.GetString()! : valor.Value.GetRawText();

                if (enUrl)
                {
                    texto = texto.Replace(marcador, Uri.EscapeDataString(valorTexto));
                }
                else
                {
                    texto = texto.Replace("\"" + marcador + "\"", valor.Value.GetRawText());
                    texto = texto.Replace(marcador, valorTexto);
                }
            }

            return texto;
        }

        static List<string> BuscarValoresFaltantes(string texto)
        {
            return Regex.Matches(texto, @"\{\{(\w+)\}\}").Select(m => m.Groups[1].Value).Distinct().ToList();
        }

        /// <summary>
        /// Compara las dos respuestas. Si alguna no es JSON, compara el texto.
        /// </summary>
        static void CompararTextos(string textoV2, string textoV3, List<string> camposIgnorados, List<string> lstDiferencias)
        {
            JsonDocument? docV2 = null;
            JsonDocument? docV3 = null;

            try { docV2 = JsonDocument.Parse(textoV2); } catch { }
            try { docV3 = JsonDocument.Parse(textoV3); } catch { }

            if (docV2 == null || docV3 == null)
            {
                if (textoV2 != textoV3)
                {
                    lstDiferencias.Add("Las respuestas no son JSON y el texto es distinto.");
                }
            }
            else
            {
                CompararJson(docV2.RootElement, docV3.RootElement, "$", camposIgnorados, lstDiferencias);
            }

            docV2?.Dispose();
            docV3?.Dispose();
        }

        /// <summary>
        /// Recorre los dos JSON y anota cada campo que falta, sobra o tiene distinto valor (máximo 200 diferencias).
        /// Los nombres se comparan respetando mayúsculas: el JSON de la V3 tiene que ser igual al de la V2.
        /// </summary>
        static void CompararJson(JsonElement v2, JsonElement v3, string ruta, List<string> camposIgnorados, List<string> lstDiferencias)
        {
            if (lstDiferencias.Count >= 200) return;
            if (EstaIgnorado(ruta, camposIgnorados)) return;

            if (v2.ValueKind != v3.ValueKind)
            {
                lstDiferencias.Add(ruta + ": tipo distinto. V2 " + Recortar(v2.GetRawText()) + " | V3 " + Recortar(v3.GetRawText()));
                return;
            }

            switch (v2.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (JsonProperty propiedadV2 in v2.EnumerateObject())
                    {
                        string rutaHija = ruta + "." + propiedadV2.Name;

                        if (v3.TryGetProperty(propiedadV2.Name, out JsonElement propiedadV3))
                        {
                            CompararJson(propiedadV2.Value, propiedadV3, rutaHija, camposIgnorados, lstDiferencias);
                        }
                        else if (!EstaIgnorado(rutaHija, camposIgnorados))
                        {
                            lstDiferencias.Add(rutaHija + ": falta en la V3");
                        }
                    }

                    foreach (JsonProperty propiedadV3 in v3.EnumerateObject())
                    {
                        string rutaHija = ruta + "." + propiedadV3.Name;

                        if (!v2.TryGetProperty(propiedadV3.Name, out _) && !EstaIgnorado(rutaHija, camposIgnorados))
                        {
                            lstDiferencias.Add(rutaHija + ": sobra en la V3 (no está en la V2)");
                        }
                    }
                    break;

                case JsonValueKind.Array:
                    int cantidadV2 = v2.GetArrayLength();
                    int cantidadV3 = v3.GetArrayLength();

                    if (cantidadV2 != cantidadV3)
                    {
                        lstDiferencias.Add(ruta + ": cantidad de elementos distinta. V2 " + cantidadV2 + " | V3 " + cantidadV3);
                    }

                    for (int i = 0; i < Math.Min(cantidadV2, cantidadV3); i++)
                    {
                        CompararJson(v2[i], v3[i], ruta + "[" + i + "]", camposIgnorados, lstDiferencias);
                    }
                    break;

                case JsonValueKind.Number:
                    // 1 y 1.0 son el mismo valor
                    bool iguales = v2.TryGetDecimal(out decimal numeroV2) && v3.TryGetDecimal(out decimal numeroV3)
                                   ? numeroV2 == numeroV3
                                   : v2.GetRawText() == v3.GetRawText();
                    if (!iguales)
                    {
                        lstDiferencias.Add(ruta + ": V2 " + v2.GetRawText() + " | V3 " + v3.GetRawText());
                    }
                    break;

                default:
                    if (v2.GetRawText() != v3.GetRawText())
                    {
                        lstDiferencias.Add(ruta + ": V2 " + Recortar(v2.GetRawText()) + " | V3 " + Recortar(v3.GetRawText()));
                    }
                    break;
            }
        }

        /// <summary>
        /// Un campo ignorado se escribe por nombre ("fechaAlta") o por ruta sin índices ("$.paginacion.tiempo").
        /// </summary>
        static bool EstaIgnorado(string ruta, List<string> camposIgnorados)
        {
            string rutaSinIndices = Regex.Replace(ruta, @"\[\d+\]", "");
            string nombre = rutaSinIndices.Substring(rutaSinIndices.LastIndexOf('.') + 1);

            return camposIgnorados.Contains(rutaSinIndices) || camposIgnorados.Contains(nombre);
        }

        /// <summary>
        /// true si la respuesta tiene "success": false. En las listas (Create/Update en lote) alcanza con que un elemento lo tenga.
        /// </summary>
        static bool DevolvioSuccessFalse(string texto)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(texto);
                JsonElement raiz = doc.RootElement;

                if (raiz.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement elemento in raiz.EnumerateArray())
                    {
                        if (elemento.ValueKind == JsonValueKind.Object &&
                            elemento.TryGetProperty("success", out JsonElement successElemento) &&
                            successElemento.ValueKind == JsonValueKind.False)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                return raiz.ValueKind == JsonValueKind.Object &&
                       raiz.TryGetProperty("success", out JsonElement success) &&
                       success.ValueKind == JsonValueKind.False;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lee un valor de la respuesta con una ruta simple: "$.tipoOperacion", "$[0].formaDePago.formaDePagoID", "$.ejercicios[0].mascara".
        /// Devuelve null si la respuesta no es JSON o la ruta no existe.
        /// </summary>
        static JsonElement? LeerRuta(string texto, string ruta)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(texto);
                JsonElement actual = doc.RootElement;

                foreach (Match parte in Regex.Matches(ruta.TrimStart('$'), @"\.(\w+)|\[(\d+)\]"))
                {
                    if (parte.Groups[1].Success)
                    {
                        if (actual.ValueKind != JsonValueKind.Object || !actual.TryGetProperty(parte.Groups[1].Value, out actual)) return null;
                    }
                    else
                    {
                        int indice = int.Parse(parte.Groups[2].Value);
                        if (actual.ValueKind != JsonValueKind.Array || indice >= actual.GetArrayLength()) return null;
                        actual = actual[indice];
                    }
                }

                return actual.Clone();   // Clone: sigue valiendo después de cerrar el documento
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saca el mensaje de error de la respuesta (cResponse.error.mensaje o el primer elemento de una lista).
        /// </summary>
        static string ResumirError(string texto)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(texto);
                JsonElement raiz = doc.RootElement;

                if (raiz.ValueKind == JsonValueKind.Array && raiz.GetArrayLength() > 0) raiz = raiz[0];

                if (raiz.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty propiedad in raiz.EnumerateObject())
                    {
                        if (propiedad.Name.Equals("error", StringComparison.OrdinalIgnoreCase) && propiedad.Value.ValueKind == JsonValueKind.Object)
                        {
                            foreach (JsonProperty campo in propiedad.Value.EnumerateObject())
                            {
                                if (campo.Name.StartsWith("mensaje", StringComparison.OrdinalIgnoreCase) ||
                                    campo.Name.Equals("message", StringComparison.OrdinalIgnoreCase))
                                {
                                    return Recortar(campo.Value.ToString());
                                }
                            }
                        }

                        if (propiedad.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
                        {
                            return Recortar(propiedad.Value.ToString());   // ProblemDetails de ASP.NET (ej. 404 de ruta)
                        }
                    }
                }
            }
            catch
            {
            }

            return Recortar(texto);
        }

        static string Formatear(string texto)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(texto);
                return JsonSerializer.Serialize(doc.RootElement, opcionesEscritura);
            }
            catch
            {
                return texto;
            }
        }

        static string Recortar(string texto)
        {
            texto = texto.Replace("\r", " ").Replace("\n", " ");
            return texto.Length > 150 ? texto.Substring(0, 150) + "..." : texto;
        }

        static void Escribir(string linea, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(linea);
            Console.ResetColor();
        }
    }

    /// <summary>configuracion.json</summary>
    internal class cConfiguracion
    {
        public string UrlV3 { get; set; } = "";
        public string UrlV2 { get; set; } = "";
        public string GrupoESI { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string Password { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string EmpresaID { get; set; } = "";
        public string SucursalID { get; set; } = "";
        public bool EjecutarEscrituras { get; set; }
        public bool AceptarCertificadosInvalidos { get; set; }
        public int TimeoutSegundos { get; set; } = 100;
        public List<string> CamposIgnorados { get; set; } = new();
        public Dictionary<string, JsonElement> Valores { get; set; } = new();
    }

    /// <summary>Cada elemento de pruebas.json</summary>
    internal class cPrueba
    {
        public string Nombre { get; set; } = "";
        public string Metodo { get; set; } = "GET";
        public string Ruta { get; set; } = "";
        public string Query { get; set; } = "";
        public JsonElement? Body { get; set; }
        public string BodyArchivo { get; set; } = "";   // archivo dentro de la carpeta Bodies (en lugar de Body)
        public bool Escritura { get; set; }
        public Dictionary<string, string> Guardar { get; set; } = new();   // nombre del valor → ruta en la respuesta
        public int StatusEsperado { get; set; } = 200;
    }

    internal class cRespuesta
    {
        public int Status { get; set; }
        public string Texto { get; set; } = "";
        public long Milisegundos { get; set; }
    }
}
