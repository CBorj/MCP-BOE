# 🧪 Tests de Postman para MCP-BOE

Este directorio contiene todo lo necesario para testear el API MCP-BOE usando Postman.

## 📁 Archivos Incluidos

- `MCP-BOE-Collection.postman_collection.json` - Colección completa con 14+ endpoints
- `MCP-BOE-Production.postman_environment.json` - Variables para entorno de producción
- `MCP-BOE-Local.postman_environment.json` - Variables para entorno local

## 🚀 Inicio Rápido

### 1. Importar en Postman

1. **Abrir Postman**
2. **Import** → **Upload Files**
3. **Seleccionar los 3 archivos JSON**
4. **Import**

### 2. Configurar Entorno

1. **Seleccionar entorno**: `MCP-BOE Production` o `MCP-BOE Local`
2. **Verificar variables**: Asegúrate que `baseUrl` esté correcta

### 3. Ejecutar Tests

#### 🔥 **Secuencia Recomendada de Primera Ejecución:**

```
1. Sistema → Health Check          (verificar que esté funcionando)
2. Sistema → API Info             (ver endpoints disponibles)
3. Auxiliares → Get Sections      (entender estructura BOE)
4. Sumarios → Daily Summary       (probar con fecha de ayer)
5. Legislación → Search Basic     (búsqueda simple)
```

## 📊 **Runner Automático**

### Ejecutar toda la colección:

1. **Click derecho** en colección "MCP-BOE API Tests"
2. **Run collection**
3. **Seleccionar entorno**
4. **Run MCP-BOE API Tests**

### Configuración recomendada:
- **Iterations**: 1
- **Delay**: 500ms entre requests
- **Data file**: No necesario
- **Environment**: MCP-BOE Production

## 🎯 **Endpoints Principales**

### ✅ **Sistema** (Siempre disponibles)
- `GET /health` - Estado del servidor
- `GET /info` - Información del API

### 📜 **Legislación** (Requiere BOE online)
- `GET /legislation/summary` - Resumen general
- `GET /legislation/search` - Búsqueda de documentos
- `GET /legislation/document/{id}` - Documento específico

### 📋 **Sumarios** (Requiere BOE online)
- `GET /summary/daily` - Sumario diario
- `GET /summary/search` - Búsqueda en sumarios
- `GET /summary/section/{section}` - Sumario por sección

### 🏛️ **Auxiliares** (Generalmente disponibles)
- `GET /auxiliary/departments` - Departamentos/Ministerios
- `GET /auxiliary/sections` - Secciones del BOE
- `GET /auxiliary/document-types` - Tipos de documentos

## 🧪 **Tests Automáticos Incluidos**

Cada request incluye tests automáticos que verifican:

- ✅ **Status Code**: 200 para éxito, 400/404 para errores
- ✅ **Response Structure**: Campos requeridos presentes
- ✅ **Data Types**: Tipos de datos correctos
- ✅ **Performance**: Tiempo de respuesta < 10s
- ✅ **Content Type**: application/json

## 🐛 **Troubleshooting**

### **Error: Could not get response**
- Verificar que el entorno esté seleccionado
- Comprobar la URL base en variables
- Para local: asegúrate que Functions esté ejecutándose

### **Error 500 Internal Server Error**
- El servicio BOE puede estar temporalmente no disponible
- Prueba endpoints de Sistema primero
- Revisa logs en Azure si es producción

### **Error 400 Bad Request**
- Parámetros incorrectos (fechas futuras, IDs inválidos)
- Consulta la documentación del endpoint
- Revisa ejemplos en la colección

### **Tests Failing**
- Algunos tests pueden fallar si BOE está offline
- Tests de Sistema deberían pasar siempre
- Consulta el console de Postman para detalles

## 📈 **Tests de Performance**

### **Benchmark Esperados** (Production):
- Health Check: < 1000ms
- API Info: < 1500ms
- Búsquedas simples: < 3000ms
- Documentos específicos: < 5000ms
- Auxiliares: < 2000ms

### **Para pruebas de carga**:
1. Usar Runner con múltiples iteraciones
2. Activar delay entre requests (mínimo 500ms)
3. Monitor tiempos de respuesta
4. Revisar rate limits del BOE

## 🔍 **Variables Dinámicas**

La colección incluye scripts que generan:

- `{{yesterdayDate}}` - Fecha de ayer (para sumarios)
- `{{randomSearchTerm}}` - Término aleatorio para búsquedas
- `{{todayDate}}` - Fecha actual

## 📝 **Personalización**

### **Agregar nuevos tests**:

```javascript
pm.test("Mi test personalizado", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData.data).to.have.property('miCampo');
});
```

### **Modificar variables**:

1. **Collection Variables**: Para toda la colección
2. **Environment Variables**: Específicas del entorno
3. **Global Variables**: Para todas las colecciones

### **Scripts útiles**:

```javascript
// Pre-request: Generar datos aleatorios
const terms = ['ley', 'decreto', 'orden'];
const random = terms[Math.floor(Math.random() * terms.length)];
pm.collectionVariables.set("searchTerm", random);

// Test: Guardar datos para siguientes requests
const docId = pm.response.json().data.documents[0].id;
pm.collectionVariables.set("documentId", docId);
```

## 🎯 **Casos de Uso Específicos**

### **Testing de Integración MCP**:
1. Ejecutar Health Check para confirmar disponibilidad
2. Probar cada endpoint que usará el MCP
3. Verificar formato de respuestas compatible

### **Testing de Performance**:
1. Usar Collection Runner con 10+ iteraciones
2. Monitor tiempos de respuesta
3. Identificar endpoints lentos

### **Testing de Errores**:
1. Probar con datos inválidos
2. Verificar manejo de errores HTTP
3. Confirmar mensajes de error útiles

## 🔄 **Integración Continua**

### **Exportar resultados**:
1. Collection Runner → Export Results
2. Guardar como JSON para reportes
3. Integrar con pipelines CI/CD

### **Newman (CLI)**:
```bash
# Instalar Newman
npm install -g newman

# Ejecutar colección
newman run MCP-BOE-Collection.postman_collection.json \
  -e MCP-BOE-Production.postman_environment.json \
  --delay-request 500 \
  --reporters html,json
```

## 📞 **Soporte**

- **Documentación completa**: Ver `POSTMAN-TESTING.md`
- **Códigos de error**: Consultar documentación del API
- **Issues**: Reportar en el repositorio del proyecto

---

🚀 **¡Happy Testing!** El API MCP-BOE está listo para ser probado completamente.
