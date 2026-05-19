# Documentació tècnica — API d'extracció de dades

> **Document 2 de 4** · API OCR  
> Projecte: `API_Extraccio_de_dades/Backend`  
> URL base: `http://localhost:5000`  
> Swagger: `http://localhost:5000/swagger`

---

## Índex

1. [Visió general](#1-visió-general)
2. [Configuració](#2-configuració)
3. [Endpoints](#3-endpoints)
4. [JSON del servei OCR (IA)](#4-json-del-servei-ocr-ia)
5. [Gestió de fitxers](#5-gestió-de-fitxers)
6. [Errors](#6-errors)
7. [Arquitectura interna](#7-arquitectura-interna)

---

## 1. Visió general

Aquesta API és responsable de:

- Llistar i processar PDFs de la carpeta `Storage/ToProcess`.
- Cridar el **servei OCR (IA)** per obtenir dades estructurades en JSON.
- Validar i persistir ordres a SQL Server (`POST /OCRservice/ordres`).
- Gestionar el cicle de vida dels fitxers (processats, erronis, historial).
- Endpoint auxiliar de proveïdors (`POST /OCRservice/proveidor`) — no utilitzat pel frontend WPF.

**Tecnologies:** .NET 9, Minimal APIs, SQL Server (ADO), integració amb servei OCR (IA).

---

## 2. Configuració

Fitxer: `API_Extraccio_de_dades/Backend/appsettings.json`

| Clau | Descripció |
|------|------------|
| `ConnectionStrings:DefaultConnection` | Cadena de connexió SQL Server |
| `OpenAI:ApiKey` | Clau del proveïdor del servei OCR (IA) — *només configuració, no es documenta el proveïdor a la memòria* |
| `Storage:BasePath` | Carpeta arrel de Storage (per defecte `Storage`) |

**CORS:** política `AllowAll` (desenvolupament).

**JSON:** `PropertyNameCaseInsensitive = true` per a peticions entrants.

---

## 3. Endpoints

### Resum

| Mètode | Ruta | Descripció |
|--------|------|------------|
| GET | `/OCRservice/files` | Llista PDFs pendents |
| POST | `/OCRservice/process` | Processa un o més PDFs amb OCR (IA) |
| GET | `/OCRservice/preview/{fileName}` | Previsualitza JSON ja generat |
| POST | `/OCRservice/finalize` | Mou PDF processat a carpeta final |
| DELETE | `/OCRservice/history` | Esborra historial i JSON processats |
| POST | `/OCRservice/ordres` | Guarda ordre revisada a SQL Server |
| POST | `/OCRservice/proveidor` | Crea proveïdor (endpoint secundari) |

---

### GET `/OCRservice/files`

Llista els noms de fitxer PDF a `Storage/ToProcess`.

**Petició:** sense cos.

**Resposta 200 OK:**

```json
{
  "files": ["ordre_2024_001.pdf", "ordre_2024_002.pdf"],
  "path": "C:\\...\\Storage\\ToProcess"
}
```

| Camp | Tipus | Descripció |
|------|-------|------------|
| `files` | `string[]` | Noms de fitxer (no rutes completes) |
| `path` | `string` | Ruta absoluta de la carpeta escanejada |

---

### POST `/OCRservice/process`

Envia un o més PDFs al servei OCR (IA) i retorna el resultat per fitxer.

**Cos (application/json):**

```json
{
  "fileNames": ["ordre_2024_001.pdf"]
}
```

| Camp | Tipus | Obligatori | Descripció |
|------|-------|------------|------------|
| `fileNames` | `string[]` | Sí | Llista de noms de fitxer dins `ToProcess` |

**Notes:**
- El frontend envia `fileNames` en camelCase.
- Si hi ha **un** fitxer, la resposta és un **array d'un element**.
- Si hi ha **varis**, es processen en paral·lel i es retorna un array amb un resultat per fitxer.

**Resposta 200 OK (un fitxer):**

```json
[
  {
    "fileName": "ordre_2024_001.pdf",
    "success": true,
    "errorMessage": null,
    "jsonPath": "C:\\...\\Storage\\JSON\\ordre_2024_001.json",
    "extractedData": { }
  }
]
```

**Resposta 400 Bad Request:**

```text
No se proporcionaron nombres de archivo.
```

**Objecte `OcrProcessResult`:**

| Camp | Tipus | Descripció |
|------|-------|------------|
| `fileName` | `string` | Nom del PDF |
| `success` | `boolean` | `true` si l'extracció ha funcionat |
| `errorMessage` | `string?` | Missatge d'error si `success` és false |
| `jsonPath` | `string?` | Ruta del JSON guardat a `Storage/JSON` |
| `extractedData` | `object?` | Dades extretes (estructura a la secció 4) |

**Exemple d'error en processament:**

```json
[
  {
    "fileName": "inexistent.pdf",
    "success": false,
    "errorMessage": "El archivo no existe.",
    "jsonPath": null,
    "extractedData": null
  }
]
```

---

### GET `/OCRservice/preview/{fileName}`

Retorna el JSON ja generat per a un PDF (sense tornar a cridar el servei OCR).

**Paràmetre de ruta:** `fileName` — nom del PDF (sense extensió es busca `{fileName}.json` a `Storage/JSON`).

**Resposta 200 OK:** mateix format que un element de `OcrProcessResult`.

**Resposta 400 Bad Request:**

```json
{
  "error": "No se encontró el JSON para el archivo."
}
```

---

### POST `/OCRservice/finalize`

Marca un document com a finalitzat: mou el PDF de `ToProcess` a `Processed` i el JSON a `JSON_Processed`.

**Cos:**

```json
{
  "fileName": "ordre_2024_001.pdf"
}
```

| Camp | Tipus | Obligatori |
|------|-------|------------|
| `fileName` | `string` | Sí |

**Resposta 200 OK:**

```json
{
  "message": "Proceso finalizado correctamente."
}
```

**Resposta 400:** `Nombre de archivo no proporcionado.`

**Resposta 500:** problema en Problem Details (`title`, `detail`).

---

### DELETE `/OCRservice/history`

Esborra el contingut de les carpetes d'historial (`Processed`, `JSON_Processed`, etc.).

**Resposta 200 OK:**

```json
{
  "message": "Historial borrado correctamente."
}
```

**Resposta 500:** error en esborrat.

---

### POST `/OCRservice/ordres`

Guarda una ordre revisada a SQL Server (client + ordre + línies).

**Cos (application/json):**

```json
{
  "source_file_name": "ordre_2024_001.pdf",
  "cliente": {
    "nombre": "Empresa Client SL",
    "direccion": "Carrer Exemple 1",
    "ciudad": "Barcelona",
    "codigo_postal": "08001",
    "pais": "ES",
    "telefono": "+34 600 000 000",
    "nif_iva": "ESB12345678",
    "codigo_cliente": "CLI001"
  },
  "orden": {
    "numero": "PO-2024-100",
    "fecha": "15/03/2024",
    "fecha_recepcion": null,
    "modo_pago": "30 dies",
    "gestionado_por": null,
    "direccion_entrega": "Magatzem central"
  },
  "lineas": [
    {
      "descripcion": "Servei de manteniment",
      "cantidad": 2,
      "precio_unitario": 150.00,
      "descuento": null,
      "importe_ht": 300.00,
      "tva": 21,
      "codigo": null,
      "codigo_producto": "SRV-01",
      "codigo_cliente": null,
      "codigo_proveedor": null
    }
  ],
  "totales": {
    "total_ht": 300.00,
    "total_iva": 63.00,
    "total_ttc": 363.00,
    "moneda": "EUR"
  }
}
```

#### Esquema `OrdresRequest`

| Camp | Tipus | Obligatori | Descripció |
|------|-------|------------|------------|
| `source_file_name` | `string?` | No | PDF d'origen; si falla validació, es pot moure a `Erronies` |
| `cliente` | `ClienteRequest` | Sí | Dades del client |
| `orden` | `OrdenInfoRequest` | Sí | Capçalera de l'ordre |
| `lineas` | `LineaOrdreRequest[]` | Sí | Línies (mínim una vàlida segons validadors) |
| `totales` | `TotalesRequest` | Sí | Imports totals |

#### `cliente`

| Camp JSON | Tipus | Obligatori |
|-----------|-------|------------|
| `nombre` | string | Sí |
| `direccion` | string | Sí |
| `ciudad` | string | Sí |
| `codigo_postal` | string? | No |
| `pais` | string? | No |
| `telefono` | string? | No |
| `nif_iva` | string? | No |
| `codigo_cliente` | string? | No |

#### `orden`

| Camp JSON | Tipus | Obligatori |
|-----------|-------|------------|
| `numero` | string | Sí |
| `fecha` | string | Sí |
| `fecha_recepcion` | string? | No |
| `modo_pago` | string? | No |
| `gestionado_por` | string? | No |
| `direccion_entrega` | string? | No |

#### `lineas[]`

| Camp JSON | Tipus | Obligatori |
|-----------|-------|------------|
| `descripcion` | string | Sí |
| `cantidad` | decimal | Sí |
| `precio_unitario` | decimal | Sí |
| `descuento` | decimal? | No |
| `importe_ht` | decimal? | No |
| `tva` | decimal? | No |
| `codigo` | string? | No |
| `codigo_producto` | string? | No |
| `codigo_cliente` | string? | No |
| `codigo_proveedor` | string? | No |

#### `totales`

| Camp JSON | Tipus | Obligatori | Per defecte |
|-----------|-------|------------|-------------|
| `total_ht` | decimal | Sí | — |
| `total_iva` | decimal | Sí | — |
| `total_ttc` | decimal | Sí | — |
| `moneda` | string | Sí | `"EUR"` |

**Resposta 200 OK:**

```json
{
  "message": "Orden creada correctamente."
}
```

**Resposta 400 Bad Request:**

```json
{
  "error": "VAL_ERROR",
  "message": "Descripció del error de validació",
  "movedToErronies": true
}
```

| Camp | Descripció |
|------|------------|
| `error` | Codi fix `VAL_ERROR` |
| `message` | Detall per a l'usuari |
| `movedToErronies` | `true` si el PDF s'ha mogut a `Storage/Erronies` |

---

### POST `/OCRservice/proveidor`

Crea un registre de proveïdor a la base de dades. **No el crida el frontend WPF**; documentat per completitud de l'API.

**Cos (JSON — propietats en camelCase per serialització per defecte de .NET):**

```json
{
  "nombre": "Proveïdor SL",
  "direccion": "Carrer Proveïdor 2",
  "ciudad": "Madrid",
  "codigoPostal": "28001",
  "pais": "ES",
  "telefono": "910000000",
  "fax": "",
  "email": "contacte@proveidor.com",
  "nif_iva": "ESB87654321"
}
```

**Resposta 200 OK:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nombre": "Proveïdor SL",
  "direccion": "Carrer Proveïdor 2",
  "ciudad": "Madrid",
  "codigoPostal": "28001",
  "pais": "ES",
  "telefono": "910000000",
  "fax": "",
  "email": "contacte@proveidor.com",
  "nif_iva": "ESB87654321"
}
```

**Resposta 400:**

```json
{
  "error": "VAL_ERROR",
  "message": "Missatge de validació"
}
```

---

## 4. JSON del servei OCR (IA)

El servei OCR (IA) ha de retornar **només JSON vàlid** (sense markdown), amb l'estructura definida al prompt del sistema (`ServiceOCR/SystemPrompt.txt`).

### Estructura esperada (`extractedData`)

```json
{
  "proveedor": {
    "nombre": null,
    "direccion": null,
    "ciudad": null,
    "pais": null,
    "telefono": null,
    "fax": null,
    "email": null,
    "nif_iva": null,
    "siret": null,
    "iban": null,
    "bic": null
  },
  "cliente": {
    "nombre": "Nom client",
    "direccion": "Adreça",
    "ciudad": "Ciutat",
    "codigo_postal": "08001",
    "pais": "ES",
    "telefono": null,
    "nif_iva": "ESB12345678",
    "codigo_cliente": "COD123"
  },
  "orden": {
    "numero": "PO-100",
    "fecha": "15/03/2024",
    "fecha_recepcion": null,
    "modo_pago": null,
    "gestionado_por": null,
    "direccion_entrega": null
  },
  "lineas": [
    {
      "descripcion": "Producte o servei",
      "cantidad": 1,
      "precio_unitario": 100.0,
      "importe_ht": 100.0,
      "tva": 21,
      "codigo_producto": "REF-01",
      "codigo_cliente": null,
      "codigo_proveedor": null
    }
  ],
  "totales": {
    "total_ht": 100.0,
    "total_iva": 21.0,
    "total_ttc": 121.0,
    "moneda": "EUR"
  }
}
```

El frontend mapa aquest JSON cap a `OrdresRequest` mitjançant `OcrOrderMapper` abans de `POST /OCRservice/ordres`.

---

## 5. Gestió de fitxers

| Carpeta | Funció |
|---------|--------|
| `Storage/ToProcess` | PDFs pendents |
| `Storage/JSON` | JSON generats pel OCR (IA) |
| `Storage/Processed` | PDFs finalitzats |
| `Storage/JSON_Processed` | JSON associats a finalitzats |
| `Storage/Erronies` | PDFs amb error de validació en guardar |

---

## 6. Errors

| HTTP | Situació |
|------|----------|
| 200 | Operació correcta |
| 400 | Validació, cos mal format, fitxer no trobat |
| 500 | Error intern (finalize, history, excepcions no controlades) |

Codi d'error de negoci habitual: `VAL_ERROR`.

---

## 7. Arquitectura interna

```
Application/Endpoints/     → MapOcrEndpoints, MapOrdresEndpoints, MapProveidorEndpoints
Business/                  → OrdreBusiness, ProveidorBusiness
Domain/                    → Entitats i validadors
Infrastructure/DTO/        → OrdresRequest, ProveidorRequest
Infrastructure/Persistence → ADO SQL Server
ServiceOCR/                → OcrService, integració OCR (IA), carpetes Storage
```

**Enllaços:** [Plantejament](../General/03_Plantejament.md) · [Memòria general](../General/00_Memoria_General.md) · [API ERP](../API_ERP/Documentacio_Tecnica.md)
