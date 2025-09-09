# Guía de Testing con Postman - MCP-BOE

## 🎯 Configuración Inicial

### 1. **Importar Colección Base**
1. Abrir Postman
2. Crear nueva colección: "MCP-BOE Tests"
3. Configurar variables de colección:
   - `baseUrl`: `https://mcpboe-func-prod.azurewebsites.net/api`
   - `localUrl`: `http://localhost:7071/api`

### 2. **Headers Globales**
Agregar a nivel de colección:
```
Content-Type: application/json
Accept: application/json
```

## 🏥 Tests de Sistema

### **1. Health Check**
```
Method: GET
URL: {{baseUrl}}/health
Description: Verificar estado del servidor

Expected Response (200):
{
  "success": true,
  "data": {
    "Status": "Healthy",
    "Timestamp": "2024-01-15T10:30:00Z",
    "Version": "1.0.0",
    "Environment": "Production"
  }
}
```

### **2. API Info**
```
Method: GET
URL: {{baseUrl}}/info
Description: Información del API y endpoints

Expected Response (200):
{
  "success": true,
  "data": {
    "Name": "MCPBoe Function App",
    "Description": "Azure Functions API for Spanish BOE operations",
    "Version": "1.0.0",
    "Endpoints": {
      "Legislation": [...],
      "Summary": [...],
      "Auxiliary": [...]
    }
  }
}
```

## 📜 Tests de Legislación

### **3. Legislation Summary**
```
Method: GET
URL: {{baseUrl}}/legislation/summary
Query Params (opcionales):
  - date: 2024-01-15
  - limit: 10

Expected Response (200):
{
  "success": true,
  "data": {
    "totalDocuments": 45,
    "documentsBySection": {...},
    "recentDocuments": [...]
  }
}
```

### **4. Legislation Search (Básica)**
```
Method: GET
URL: {{baseUrl}}/legislation/search
Query Params:
  - query: inteligencia artificial
  - limit: 5

Expected Response (200):
{
  "success": true,
  "data": {
    "totalResults": 12,
    "documents": [
      {
        "id": "BOE-A-2024-00001",
        "title": "...",
        "date": "2024-01-15",
        "section": "I",
        "department": "...",
        "summary": "..."
      }
    ]
  }
}
```

### **5. Legislation Search (Avanzada)**
```
Method: GET
URL: {{baseUrl}}/legislation/search
Query Params:
  - query: subvenciones
  - section: III
  - department: Ministerio de Justicia
  - limit: 10

Tests to Add:
- Status code is 200
- Response has success: true
- Response.data.documents is array
- Response.data.totalResults is number
```

### **6. Get Specific Document**
```
Method: GET
URL: {{baseUrl}}/legislation/document/BOE-A-2024-00001
Query Params (opcionales):
  - format: xml
  - includeMetadata: true

Expected Response (200):
{
  "success": true,
  "data": {
    "document": {...},
    "content": "...",
    "format": "xml",
    "metadata": {...}
  }
}
```

## 📋 Tests de Sumarios

### **7. Daily Summary**
```
Method: GET
URL: {{baseUrl}}/summary/daily
Query Params:
  - date: 2024-01-15 (REQUIRED)
  - section: all (opcional)
  - includeContent: false (opcional)

Expected Response (200):
{
  "success": true,
  "data": {
    "date": "2024-01-15",
    "sections": {
      "I": [...],
      "II": [...],
      "III": [...],
      "IV": [...],
      "V": [...]
    },
    "totalDocuments": 47
  }
}
```

### **8. Summary Search**
```
Method: GET
URL: {{baseUrl}}/summary/search
Query Params:
  - query: subvenciones (REQUIRED)
  - section: III (opcional)
  - limit: 20 (opcional)

Expected Response (200):
{
  "success": true,
  "data": {
    "query": "subvenciones",
    "totalResults": 23,
    "summaries": [...]
  }
}
```

### **9. Section Summary**
```
Method: GET
URL: {{baseUrl}}/summary/section/I
Query Params:
  - date: 2024-01-15 (REQUIRED)
  - detailed: true (opcional)

Expected Response (200):
{
  "success": true,
  "data": {
    "section": "I",
    "date": "2024-01-15",
    "sectionName": "Disposiciones generales",
    "entries": [...],
    "totalDocuments": 8
  }
}
```

## 🏛️ Tests de Auxiliares

### **10. Get Departments**
```
Method: GET
URL: {{baseUrl}}/auxiliary/departments
Query Params (opcionales):
  - filter: Ministerio
  - includeStats: false

Expected Response (200):
{
  "success": true,
  "data": {
    "departments": [
      {
        "code": "12345",
        "name": "Ministerio de Justicia",
        "fullName": "Ministerio de Justicia"
      }
    ]
  }
}
```

### **11. Get Sections**
```
Method: GET
URL: {{baseUrl}}/auxiliary/sections
Query Params (opcionales):
  - includeDescription: true

Expected Response (200):
{
  "success": true,
  "data": {
    "sections": [
      {
        "code": "I",
        "name": "Disposiciones generales",
        "description": "Leyes, Reales Decretos..."
      }
    ]
  }
}
```

### **12. Get Subsections**
```
Method: GET
URL: {{baseUrl}}/auxiliary/subsections
Query Params (opcionales):
  - section: I

Expected Response (200):
{
  "success": true,
  "data": {
    "subsections": [...]
  }
}
```

### **13. Get Document Types**
```
Method: GET
URL: {{baseUrl}}/auxiliary/document-types
Query Params (opcionales):
  - section: I
  - includeExamples: false

Expected Response (200):
{
  "success": true,
  "data": {
    "documentTypes": [
      {
        "code": "ley",
        "name": "Ley",
        "description": "Norma jurídica..."
      }
    ]
  }
}
```

### **14. Get Territories**
```
Method: GET
URL: {{baseUrl}}/auxiliary/territories
Query Params (opcionales):
  - includeStats: false

Expected Response (200):
{
  "success": true,
  "data": {
    "territories": [
      {
        "code": "01",
        "name": "Andalucía",
        "type": "comunidad_autonoma"
      }
    ]
  }
}
```

## 🧪 Tests Automáticos con Scripts

### **Test Script Básico** (agregar en pestaña Tests):

```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response has success field", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('success');
    pm.expect(jsonData.success).to.be.true;
});

pm.test("Response has data field", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('data');
});

pm.test("Response time is less than 5000ms", function () {
    pm.expect(pm.response.responseTime).to.be.below(5000);
});
```

### **Test Script para Búsquedas**:

```javascript
pm.test("Search results structure", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData.data).to.have.property('totalResults');
    pm.expect(jsonData.data.totalResults).to.be.a('number');
    
    if (jsonData.data.totalResults > 0) {
        pm.expect(jsonData.data).to.have.property('documents');
        pm.expect(jsonData.data.documents).to.be.an('array');
        pm.expect(jsonData.data.documents.length).to.be.above(0);
    }
});
```

### **Test Script para Documentos**:

```javascript
pm.test("Document structure", function () {
    const jsonData = pm.response.json();
    const doc = jsonData.data.document;
    
    pm.expect(doc).to.have.property('id');
    pm.expect(doc).to.have.property('title');
    pm.expect(doc).to.have.property('date');
    pm.expect(doc).to.have.property('section');
    
    // Validar formato de ID
    pm.expect(doc.id).to.match(/^BOE-[A-Z]-\d{4}-\d{5}$/);
    
    // Validar sección
    pm.expect(doc.section).to.be.oneOf(['I', 'II', 'III', 'IV', 'V']);
});
```

## 🔄 Colección de Pruebas Completas

### **Variables de Entorno**:
```json
{
  "prod": {
    "baseUrl": "https://mcpboe-func-prod.azurewebsites.net/api"
  },
  "local": {
    "baseUrl": "http://localhost:7071/api"
  }
}
```

### **Runner de Tests**:
1. **Secuencia Recomendada**:
   ```
   1. Health Check
   2. API Info
   3. Auxiliary Sections (para entender estructura)
   4. Daily Summary (fecha reciente)
   5. Legislation Search (términos básicos)
   6. Specific Document (usar ID de búsqueda anterior)
   7. Summary Search
   8. Departments
   9. Document Types
   10. Territories
   ```

### **Tests de Carga**:
```javascript
// Pre-request Script para generar datos aleatorios
const searchTerms = ['ley', 'decreto', 'orden', 'resolución', 'subvención'];
const randomTerm = searchTerms[Math.floor(Math.random() * searchTerms.length)];
pm.collectionVariables.set("randomSearchTerm", randomTerm);

const yesterday = new Date();
yesterday.setDate(yesterday.getDate() - 1);
const dateString = yesterday.toISOString().split('T')[0];
pm.collectionVariables.set("yesterdayDate", dateString);
```

## ❌ Tests de Casos de Error

### **Error 400 - Bad Request**:
```
Method: GET
URL: {{baseUrl}}/legislation/document/INVALID-ID

Expected Response (400):
{
  "success": false,
  "error": "Invalid document ID format"
}
```

### **Error 404 - Not Found**:
```
Method: GET
URL: {{baseUrl}}/legislation/document/BOE-A-9999-99999

Expected Response (404):
{
  "success": false,
  "error": "Document not found"
}
```

### **Error de Fecha Inválida**:
```
Method: GET
URL: {{baseUrl}}/summary/daily
Query Params:
  - date: 2025-01-01

Expected Response (400):
{
  "success": false,
  "error": "Invalid date or future date"
}
```

## 📊 Validaciones Avanzadas

### **Script de Validación de Performance**:
```javascript
pm.test("Performance benchmark", function () {
    const responseTime = pm.response.responseTime;
    
    if (responseTime > 2000) {
        console.log(`Warning: Slow response time: ${responseTime}ms`);
    }
    
    pm.expect(responseTime).to.be.below(10000); // Max 10 segundos
});
```

### **Script de Validación de Datos**:
```javascript
pm.test("Data quality validation", function () {
    const jsonData = pm.response.json();
    
    if (jsonData.data.documents) {
        jsonData.data.documents.forEach((doc, index) => {
            pm.expect(doc.date, `Document ${index} date`).to.match(/^\d{4}-\d{2}-\d{2}$/);
            pm.expect(doc.title, `Document ${index} title`).to.not.be.empty;
        });
    }
});
```

## 🚀 Exportar e Importar

### **Exportar Colección**:
1. Click derecho en colección
2. Export
3. Collection v2.1 (recomendado)
4. Guardar como `MCP-BOE-Tests.postman_collection.json`

### **Compartir con Equipo**:
```bash
# Estructura de archivos recomendada
/postman-tests/
  ├── MCP-BOE-Tests.postman_collection.json
  ├── MCP-BOE-Environment-Prod.postman_environment.json
  ├── MCP-BOE-Environment-Local.postman_environment.json
  └── README.md
```

## 💡 Tips y Mejores Prácticas

1. **Usar variables** para URLs y datos repetitivos
2. **Crear tests automáticos** para validación continua
3. **Organizar por carpetas** (Sistema, Legislación, Sumarios, Auxiliares)
4. **Documentar cada request** con descripción clara
5. **Usar Pre-request Scripts** para generar datos dinámicos
6. **Monitor la collection** para alerts automáticas
7. **Versionar la colección** en Git junto al código

## 🔍 Debugging

### **Console Debugging**:
```javascript
console.log("Request URL:", pm.request.url);
console.log("Response Body:", pm.response.text());
console.log("Headers:", pm.response.headers);
```

### **Variables Debugging**:
```javascript
console.log("Environment:", pm.environment.name);
console.log("Base URL:", pm.collectionVariables.get("baseUrl"));
```

¡Con esta guía tendrás una suite completa de tests para validar toda la funcionalidad del MCP-BOE! 🚀
