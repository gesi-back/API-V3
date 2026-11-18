# API GESI V3.Pruebas

Ejecutable que llama a cada endpoint de la API V3 por HTTP, igual que un cliente real.
Si se configura `UrlV2`, manda cada lectura también a la V2 y compara los JSON campo por campo.

## Uso

1. Levantar la API V3 (y la V2 si se quiere comparar).
2. Completar `configuracion.json`: `GrupoESI`, `Usuario`/`Password` (o `ApiKey`) y los `Valores` (IDs reales de la base).
3. Ejecutar:

```
dotnet run --project "API GESI V3.Pruebas"                 # todas las pruebas
dotnet run --project "API GESI V3.Pruebas" -- Productos    # solo las que contienen "Productos" en el nombre o la ruta
```

También se puede ejecutar desde Visual Studio como proyecto de inicio.

## Resultado

- En consola, una línea por prueba: `OK`, `FALLA`, `OMITIDA` (falta un valor o es una escritura), status, tiempo y, si compara, `V2: IGUAL` o la cantidad de diferencias.
- En `bin\...\Resultados\fecha_hora\`: la respuesta de cada API (`.v3.json`, `.v2.json`), las diferencias (`.diferencias.txt`) y `resumen.txt`.
- Código de salida = cantidad de fallas (0 = todo bien).

Una prueba falla si el status no es el esperado (por defecto 200) o si devuelve 200 con `"success": false`.

## Agregar o cambiar pruebas

Cada línea de `pruebas.json` es una prueba:

```json
{ "Nombre": "Maestros Clientes GetItem", "Metodo": "GET", "Ruta": "api/Maestros/Clientes/GetItem", "Query": "clienteID={{ClienteID}}" }
```

- `{{Nombre}}` se reemplaza con `configuracion.json → Valores`. En el body, `"{{Nombre}}"` se reemplaza por el JSON del valor (número u objeto).
- `Escritura: true`: solo corre con `EjecutarEscrituras: true`, nunca contra la V2, y necesita `Body`.
- `StatusEsperado`: para probar errores (ej. 401, 404).
