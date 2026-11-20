## Guía para el programador

Esta guía explica cómo autenticarse y las reglas generales de la API GESI.
El detalle de cada endpoint (parámetros, body y estructura de la respuesta) está más abajo, agrupado por módulo.

---

### 1. Antes de empezar

Para consumir la API se necesita:

- **Un usuario del ERP habilitado para la API.** Grupo ESI lo da de alta e indica con qué empresa y sucursal opera por defecto.
- **El valor del encabezado `GrupoESI`**, provisto por Grupo ESI.
- **La URL base** de la API del cliente, por ejemplo `https://{servidor}/`.

Todas las solicitudes y respuestas son **JSON en UTF-8** (`Content-Type: application/json`).

---

### 2. Encabezados

| Encabezado | Obligatorio | Descripción |
|---|---|---|
| `GrupoESI` | **Sí, en todas las solicitudes** (incluido el login) | Valor provisto por Grupo ESI. Si falta, la API responde 400. |
| `Authorization` | Sí, si se usa token | `Bearer {token}` obtenido en el login. |
| `GrupoESI-ApiKey` | Sí, si se usa API Key | Reemplaza al token. No se envían los dos. |
| `empresaID` y `sucursalID` | No | Para operar con una empresa o sucursal distinta de la predeterminada del usuario. Van **los dos juntos** y numéricos. |
| `Content-Type` | En POST / PUT / PATCH | `application/json` |

---

### 3. Autenticación

Hay dos formas de autenticarse. Se elige una:

| | Token (JWT) | API Key |
|---|---|---|
| Cómo se obtiene | `POST /api/Autenticacion/Login` con usuario y contraseña | La entrega Grupo ESI |
| Encabezado | `Authorization: Bearer {token}` | `GrupoESI-ApiKey: {clave}` |
| Vencimiento | Sí (ver `expiracion`); se renueva con el refresh token | La da de alta y de baja Grupo ESI |
| Uso recomendado | Aplicaciones con usuario que inicia sesión | Integraciones servidor a servidor |

**La API Key es una credencial permanente:** debe guardarse del lado del servidor y nunca incluirse en una aplicación web o móvil.

#### 3.1 Login (token)

```http
POST /api/Autenticacion/Login
GrupoESI: {valor}
Content-Type: application/json

{ "Username": "usuario", "Password": "contraseña" }
```

Respuesta `200`:

```json
{
  "success": true,
  "error": null,
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refresh_token": "8f3c...",
  "expiracion": "2026-11-18T14:30:00",
  "paginacion": null
}
```

- El token se envía en cada solicitud: `Authorization: Bearer {token}`.
- Vale **4 horas**; la fecha y hora exacta viene en `expiracion`.
- Si se vuelve a hacer login mientras el token sigue vigente, la API devuelve **el mismo token**. No hace falta pedir uno nuevo en cada solicitud: conviene guardarlo y reutilizarlo hasta que venza.
- Usuario o contraseña incorrectos → `401` con el código `4011`.

#### 3.2 Renovar el token (refresh token)

Antes o después de que venza el token, se puede pedir uno nuevo sin volver a enviar la contraseña:

```http
POST /api/Autenticacion/RefreshToken
GrupoESI: {valor}
Content-Type: application/json

{ "token": "{token anterior}", "refresh_token": "{refresh_token anterior}" }
```

La respuesta tiene el mismo formato que el login, con un **token y un refresh token nuevos**. Hay que guardar los dos y usar los nuevos de ahí en adelante.
Si el token o el refresh token no son válidos, la respuesta es `400` con el código `4014` (o `4041` si falta alguno de los dos). En ese caso hay que volver a hacer login.

#### 3.3 API Key

```http
GET /api/Maestros/Paises/GetList
GrupoESI: {valor}
GrupoESI-ApiKey: {clave}
```

Una API Key inválida responde `401`.

#### 3.4 Errores de autenticación

| Situación | Status | `code` |
|---|---|---|
| No se envió token ni API Key | 401 | 4013 |
| Token vencido, mal formado o con firma inválida | 401 | 4012 |
| Token dado de baja (ya no está habilitado) | 401 | 4013 |
| API Key inválida | 401 | 4012 |
| El usuario no está habilitado para la API | 400 | 0 |
| El usuario no está habilitado para la empresa o sucursal indicada | 403 | 0 |

---

### 4. Empresa y sucursal

Cada usuario tiene una empresa y una sucursal predeterminadas, y todas las operaciones se hacen con ellas.
Si el usuario puede operar con otras, se indican en cada solicitud con los encabezados `empresaID` y `sucursalID` (los dos juntos).

Los datos que se graban (comprobantes, asientos, etc.) siempre quedan en la empresa de la sesión, aunque el body indique otra `empresaID`.

---

### 5. Formato de las respuestas

Todas las respuestas tienen `success` y `error`. Además traen los datos del endpoint y, en las listas, la paginación:

```json
{
  "success": true,
  "error": null,
  "clientes": [ { "...": "..." } ],
  "paginacion": {
    "totalElementos": 245,
    "totalPaginas": 25,
    "paginaActual": 1,
    "tamañoPagina": 10,
    "paginaSolicitada": 1
  }
}
```

Cuando hay un error, `error` trae el detalle:

```json
{
  "success": false,
  "error": {
    "code": 4025,
    "message": "No se encontraron clientes",
    "error": "DATA_NOT_FOUND"
  }
}
```

- `code`: código numérico (ver la sección 8).
- `message`: descripción para el programador.
- `error`: código de texto, estable, para usar en el código del cliente.

Si el body no es un JSON válido, viene vacío o un campo tiene un tipo incorrecto (por ejemplo, texto donde va un número), la solicitud se rechaza con `400`, el código `4151` (`INVALID_JSON`) y los campos que fallaron en `message`:

```json
{
  "success": false,
  "error": {
    "code": 4151,
    "message": "El JSON del body o los parámetros no son válidos. Revisar: $[0].numero",
    "error": "INVALID_JSON"
  }
}
```

**Hay que revisar siempre `error`, aunque el status sea 200.** Algunas situaciones que no son errores del servidor vuelven con `200` y `error` cargado: una búsqueda sin resultados, o una página fuera de rango.

#### Altas y modificaciones en lote

Los `Create` / `Update` reciben una **lista** y devuelven **un resultado por cada elemento**, cada uno con su propio `success`.
Una lista puede venir con status 200 y tener algunos elementos con error: hay que revisar cada uno.

Algunos endpoints tienen un máximo de elementos por llamada (por ejemplo, 10 productos o 5 PDF). Está indicado en su descripción.

---

### 6. Paginación

Los endpoints que devuelven listas reciben:

| Parámetro | Predeterminado | Máximo |
|---|---|---|
| `pageNumber` | 1 | — |
| `pageSize` | 10 | 1000 |

- Para recorrer todos los resultados, se pide la página 1, se lee `paginacion.totalPaginas` y se piden las siguientes.
- Si se pide una página mayor que `totalPaginas`, la respuesta es `200` con el error `PAGE_OUT_OF_RANGE` (código `7000`) y sin datos.
- Sin resultados: `200` con el error `DATA_NOT_FOUND` (código `4025`).

---

### 7. Límite de solicitudes

Cada dirección IP puede hacer **hasta 1000 solicitudes por minuto**. Este valor lo configura cada instalación.

Si se supera, la API responde `429` con el encabezado `Retry-After: 60`:

```json
{
  "success": false,
  "error": {
    "code": 429,
    "message": "Se excedió el límite de 1000 solicitudes por minuto.",
    "error": "RATE_LIMIT_EXCEEDED"
  }
}
```

Hay que esperar los segundos que indica `Retry-After` antes de reintentar.

---

### 8. Status HTTP y códigos de error

| Status | Cuándo |
|---|---|
| 200 | Solicitud procesada. Revisar `error` igual (sección 5). |
| 400 | Parámetro, body o JSON inválido, o falta un dato obligatorio. |
| 401 | Sin autenticación o credenciales inválidas (sección 3.4). |
| 403 | El usuario no tiene permiso para la operación, la empresa o la sucursal. |
| 404 | No existe el registro pedido. |
| 429 | Se superó el límite de solicitudes. |
| 500 | Error interno. El detalle técnico no se devuelve; queda registrado y Grupo ESI recibe un aviso. |

| `code` | `error` | Significado |
|---|---|---|
| 4011 | USER_NOT_FOUND | Usuario o contraseña incorrectos |
| 4012 | INVALID_TOKEN | Token o API Key inválidos |
| 4013 | INVALID_TOKEN | No se envió el token o ya no está habilitado |
| 4014 | INVALID_TOKEN | Refresh token inválido |
| 4002 | INVALID_PARAMETER | Un dato no tiene el formato esperado |
| 4041 | INVALID_PARAMETER | Falta un parámetro obligatorio o tiene un valor no permitido |
| 4151 | INVALID_JSON | Body o parámetros inválidos |
| 4017 | UNAUTHORIZED | Permiso denegado |
| 4025 | DATA_NOT_FOUND | No se encontraron datos |
| 7000 | PAGE_OUT_OF_RANGE | La página pedida no existe |
| 429 | RATE_LIMIT_EXCEEDED | Se superó el límite de solicitudes |
| 5001 | INTERNAL_SERVER_ERROR | Error interno al generar el token |
| 5002 | INTERNAL_SERVER_ERROR | Error interno de la aplicación |

---

### 9. Convenciones

- **Opciones Sí/No:** los parámetros del tipo `incluirDetalle`, `costos` o `stock` reciben `"S"` o `"N"` (por defecto `"N"`).
- **Fechas:** formato ISO 8601, `AAAA-MM-DD` o `AAAA-MM-DDTHH:mm:ss`.
- **Listas de códigos:** separados por coma, por ejemplo `codigos=1,2,3`.
- **GET con body:** algunos endpoints de consulta (por ejemplo, los `Getpdf` de comprobantes) reciben las claves en el body de un `GET`. Hay que usar un cliente HTTP que permita enviar body en un GET.
- **Nombres de los campos:** se respetan mayúsculas y minúsculas tal como aparecen en esta documentación.

---

### 10. Ejemplo completo

```bash
# 1. Login
curl -X POST "https://{servidor}/api/Autenticacion/Login" \
     -H "GrupoESI: {valor}" -H "Content-Type: application/json" \
     -d '{ "Username": "usuario", "Password": "contraseña" }'

# 2. Consulta con el token
curl "https://{servidor}/api/Maestros/Clientes/GetList?pageNumber=1&pageSize=50" \
     -H "GrupoESI: {valor}" -H "Authorization: Bearer {token}"
```

---

### 11. Probar desde esta página

1. Hacer login con `POST /api/Autenticacion/Login` (botón **Try it out**). El encabezado `GrupoESI` se carga antes con **Authorize**.
2. Tocar **Authorize** y completar `GrupoESI` y `Bearer` (el token sin la palabra "Bearer"), o `GrupoESI` y `ApiKey`.
3. Desde ese momento, cada **Try it out** envía esos encabezados.
