using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GESI.CORE.API.BLL.V3.CORE
{
    /// <summary>
    /// Lee los bodies que la V2 leía con Newtonsoft (Pedidos y Cobros), con System.Text.Json.
    /// </summary>
    public static class LectorJson
    {
        private static readonly JsonSerializerOptions Opciones = CrearOpciones();

        public static T? Leer<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Opciones);
        }

        public static object? Leer(string json, Type tipo)
        {
            return JsonSerializer.Deserialize(json, tipo, Opciones);
        }

        private static JsonSerializerOptions CrearOpciones()
        {
            JsonSerializerOptions opciones = new JsonSerializerOptions();
            opciones.PropertyNameCaseInsensitive = true;
            opciones.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            opciones.Converters.Add(new ConversorTexto());
            opciones.Converters.Add(new ConversorBooleano());
            opciones.Converters.Add(new ConversorFecha());
            opciones.Converters.Add(new ConversorFechaAnulable());
            opciones.Converters.Add(new ConversorAnulable());

            DefaultJsonTypeInfoResolver resolver = new DefaultJsonTypeInfoResolver();
            resolver.Modifiers.Add(IncluirPropiedadesIgnoradas);
            opciones.TypeInfoResolver = resolver;

            return opciones;
        }

        /// <summary>
        /// Newtonsoft no respeta el [JsonIgnore] de System.Text.Json: las propiedades marcadas así se leían igual
        /// del body. Se vuelven a agregar para la lectura.
        /// </summary>
        private static void IncluirPropiedadesIgnoradas(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object)
                return;

            // System.Text.Json deja las propiedades con [JsonIgnore] en la lista, pero sin Get ni Set
            foreach (JsonPropertyInfo oPropiedadJson in typeInfo.Properties)
            {
                if (oPropiedadJson.Get != null || oPropiedadJson.Set != null)
                    continue;

                PropertyInfo? propiedad = oPropiedadJson.AttributeProvider as PropertyInfo;

                if (propiedad == null || propiedad.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition != JsonIgnoreCondition.Always)
                    continue;

                if (propiedad.SetMethod == null || !propiedad.SetMethod.IsPublic)
                    continue;

                oPropiedadJson.Get = propiedad.GetValue;
                oPropiedadJson.Set = propiedad.SetValue;
            }
        }

        /// <summary>
        /// Igual que Newtonsoft: un número o un booleano en un campo de texto se toma como texto.
        /// </summary>
        private sealed class ConversorTexto : JsonConverter<string>
        {
            public override bool HandleNull => false;

            public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                    return reader.GetString();

                if (reader.TokenType == JsonTokenType.Number)
                    return System.Text.Encoding.UTF8.GetString(reader.ValueSpan);

                if (reader.TokenType == JsonTokenType.True)
                    return "True";

                if (reader.TokenType == JsonTokenType.False)
                    return "False";

                throw new JsonException("Se esperaba un texto");
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }

        /// <summary>
        /// Igual que Newtonsoft: un booleano acepta también números (0 = false) y el texto "true" / "false".
        /// </summary>
        private sealed class ConversorBooleano : JsonConverter<bool>
        {
            public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.True)
                    return true;

                if (reader.TokenType == JsonTokenType.False)
                    return false;

                if (reader.TokenType == JsonTokenType.Number)
                    return reader.GetDecimal() != 0;

                if (reader.TokenType == JsonTokenType.String && bool.TryParse(reader.GetString(), out bool valor))
                    return valor;

                throw new JsonException("Se esperaba un booleano");
            }

            public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
            {
                writer.WriteBooleanValue(value);
            }
        }

        /// <summary>
        /// Igual que Newtonsoft: además de ISO 8601 acepta otras fechas en formato invariante (ej. "2026-11-18 10:20:30").
        /// </summary>
        private sealed class ConversorFecha : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                DateTime? fecha = LeerFecha(ref reader);

                if (fecha == null)
                    throw new JsonException("Se esperaba una fecha");

                return fecha.Value;
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }

        /// <summary>
        /// Igual que Newtonsoft: en una fecha anulable, el texto vacío es null.
        /// </summary>
        private sealed class ConversorFechaAnulable : JsonConverter<DateTime?>
        {
            public override bool HandleNull => false;

            public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String && reader.ValueSpan.Length == 0)
                    return null;

                DateTime? fecha = LeerFecha(ref reader);

                if (fecha == null)
                    throw new JsonException("Se esperaba una fecha");

                return fecha;
            }

            public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
            {
                if (value == null)
                    writer.WriteNullValue();
                else
                    writer.WriteStringValue(value.Value);
            }
        }

        /// <summary>
        /// Igual que Newtonsoft: en un número o booleano anulable (int?, decimal?, bool?...), el texto vacío es null.
        /// </summary>
        private sealed class ConversorAnulable : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return Nullable.GetUnderlyingType(typeToConvert) != null && typeToConvert != typeof(DateTime?);
            }

            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                Type tipoBase = Nullable.GetUnderlyingType(typeToConvert)!;
                return (JsonConverter?)Activator.CreateInstance(typeof(ConversorAnulableDe<>).MakeGenericType(tipoBase));
            }
        }

        private sealed class ConversorAnulableDe<T> : JsonConverter<T?> where T : struct
        {
            public override bool HandleNull => false;

            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String && reader.ValueSpan.Length == 0)
                    return null;

                return JsonSerializer.Deserialize<T>(ref reader, options);
            }

            public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
            {
                if (value == null)
                    writer.WriteNullValue();
                else
                    JsonSerializer.Serialize(writer, value.Value, options);
            }
        }

        private static DateTime? LeerFecha(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.String)
                return null;

            if (reader.TryGetDateTime(out DateTime fechaISO))
                return fechaISO;

            if (DateTime.TryParse(reader.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime fecha))
                return fecha;

            return null;
        }
    }
}
