# MCP-BOE Integration Guide

## 📋 Esquema de Herramientas para GPT/LLM

Este documento describe cómo integrar el servidor MCP-BOE en herramientas GPT o sistemas LLM para acceder a la API del Boletín Oficial del Estado español.

## 🎯 Información General

- **Servidor**: MCP-BOE Azure Functions
- **URL Base**: `https://mcpboe-func-prod.azurewebsites.net/api`
- **Protocolo**: Model Context Protocol (MCP) 2024-11-05
- **Autenticación**: No requerida
- **Formato**: JSON REST API

## 🛠️ Herramientas Disponibles

### 📜 Legislación (3 herramientas)

#### 1. `boe_legislation_summary`
**Propósito**: Obtener resumen de legislación disponible
```json
{
  "name": "boe_legislation_summary",
  "description": "Obtiene estadísticas y documentos recientes de legislación",
  "parameters": {
    "date": "2024-01-15",        // Opcional: YYYY-MM-DD
    "limit": 20                  // Opcional: 1-100, default 20
  }
}
```

#### 2. `boe_legislation_search`
**Propósito**: Buscar documentos legislativos con filtros
```json
{
  "name": "boe_legislation_search",
  "description": "Busca documentos con criterios específicos",
  "parameters": {
    "query": "inteligencia artificial",           // Opcional: texto a buscar
    "date": "2024-01-15",                       // Opcional: fecha específica
    "dateRange": {"from": "2024-01-01", "to": "2024-01-31"}, // Opcional
    "department": "Ministerio de Justicia",     // Opcional: departamento
    "section": "I",                             // Opcional: I,II,III,IV,V
    "documentType": "real decreto",             // Opcional: tipo documento
    "limit": 20                                 // Opcional: 1-100
  }
}
```

#### 3. `boe_legislation_document`
**Propósito**: Obtener contenido completo de un documento
```json
{
  "name": "boe_legislation_document",
  "description": "Obtiene documento específico por ID",
  "parameters": {
    "id": "BOE-A-2024-00001",      // Requerido: ID del documento
    "format": "xml",               // Opcional: xml,pdf,html (default: xml)
    "includeMetadata": true        // Opcional: incluir metadatos
  }
}
```

### 📋 Sumarios (3 herramientas)

#### 4. `boe_summary_daily`
**Propósito**: Sumario completo de un día específico
```json
{
  "name": "boe_summary_daily",
  "description": "Sumario diario del BOE",
  "parameters": {
    "date": "2024-01-15",          // Requerido: YYYY-MM-DD
    "section": "all",              // Opcional: I,II,III,IV,V,all (default: all)
    "includeContent": false        // Opcional: incluir extractos
  }
}
```

#### 5. `boe_summary_search`
**Propósito**: Buscar en sumarios históricos
```json
{
  "name": "boe_summary_search",
  "description": "Busca en sumarios por texto",
  "parameters": {
    "query": "subvenciones",                    // Requerido: texto a buscar
    "section": "III",                          // Opcional: sección específica
    "dateRange": {"from": "2024-01-01", "to": "2024-01-31"}, // Opcional
    "limit": 20                                // Opcional: 1-100
  }
}
```

#### 6. `boe_summary_section`
**Propósito**: Sumario de una sección específica
```json
{
  "name": "boe_summary_section",
  "description": "Sumario de sección específica por fecha",
  "parameters": {
    "section": "I",                // Requerido: I,II,III,IV,V
    "date": "2024-01-15",          // Requerido: YYYY-MM-DD
    "detailed": false              // Opcional: información detallada
  }
}
```

### 🏛️ Auxiliares (5 herramientas)

#### 7. `boe_auxiliary_departments`
**Propósito**: Lista de departamentos y organismos
```json
{
  "name": "boe_auxiliary_departments",
  "description": "Obtiene departamentos que publican en BOE",
  "parameters": {
    "filter": "Ministerio",        // Opcional: filtro de texto
    "includeStats": false          // Opcional: estadísticas
  }
}
```

#### 8. `boe_auxiliary_sections`
**Propósito**: Información sobre secciones del BOE
```json
{
  "name": "boe_auxiliary_sections",
  "description": "Información de secciones BOE",
  "parameters": {
    "includeDescription": true     // Opcional: incluir descripciones
  }
}
```

#### 9. `boe_auxiliary_subsections`
**Propósito**: Subsecciones dentro de cada sección
```json
{
  "name": "boe_auxiliary_subsections",
  "description": "Subsecciones disponibles",
  "parameters": {
    "section": "I"                 // Opcional: sección específica
  }
}
```

#### 10. `boe_auxiliary_document_types`
**Propósito**: Tipos de documentos publicados
```json
{
  "name": "boe_auxiliary_document_types",
  "description": "Tipos de documentos del BOE",
  "parameters": {
    "section": "I",                // Opcional: filtrar por sección
    "includeExamples": false       // Opcional: incluir ejemplos
  }
}
```

#### 11. `boe_auxiliary_territories`
**Propósito**: Territorios y comunidades autónomas
```json
{
  "name": "boe_auxiliary_territories",
  "description": "Información territorial",
  "parameters": {
    "includeStats": false          // Opcional: estadísticas por territorio
  }
}
```

## 🔍 Secciones del BOE

| Código | Nombre | Descripción |
|--------|--------|-------------|
| I | Disposiciones generales | Leyes, decretos, órdenes |
| II | Autoridades y personal | Nombramientos, ceses |
| III | Otras disposiciones | Resoluciones, instrucciones |
| IV | Administración de Justicia | Edictos, requisitorias |
| V | Anuncios | Licitaciones, concursos |

## 📝 Ejemplos de Uso Común

### 1. Monitoreo de Nueva Legislación
```javascript
// Obtener sumario del día
const summary = await callTool("boe_summary_daily", {
  date: "2024-01-15",
  section: "I"
});

// Buscar documentos específicos
const aiLaws = await callTool("boe_legislation_search", {
  query: "inteligencia artificial",
  section: "I",
  limit: 5
});
```

### 2. Análisis Departamental
```javascript
// Obtener departamentos
const departments = await callTool("boe_auxiliary_departments", {
  filter: "Ministerio de Justicia"
});

// Buscar publicaciones del departamento
const deptDocs = await callTool("boe_legislation_search", {
  department: "Ministerio de Justicia",
  dateRange: {from: "2024-01-01", to: "2024-01-31"}
});
```

### 3. Investigación Temática
```javascript
// Buscar por tema en sumarios
const summaryResults = await callTool("boe_summary_search", {
  query: "subvenciones digitales",
  section: "III",
  dateRange: {from: "2024-01-01", to: "2024-12-31"}
});

// Obtener documentos completos
for (const result of summaryResults.summaries) {
  const fullDoc = await callTool("boe_legislation_document", {
    id: result.id,
    format: "xml",
    includeMetadata: true
  });
}
```

## ⚡ Patrones de Integración

### Para ChatGPT/GPT-4
```json
{
  "tools": [
    {
      "type": "function",
      "function": {
        "name": "boe_legislation_search",
        "description": "Busca documentos en el BOE español",
        "parameters": {
          "type": "object",
          "properties": {
            "query": {"type": "string", "description": "Texto a buscar"},
            "section": {"type": "string", "enum": ["I","II","III","IV","V"]},
            "limit": {"type": "integer", "minimum": 1, "maximum": 100}
          }
        }
      }
    }
  ]
}
```

### Para Claude/Anthropic
```json
{
  "name": "boe_legislation_search",
  "description": "Busca documentos legislativos en el BOE español. Útil para encontrar leyes, decretos y normativas.",
  "input_schema": {
    "type": "object",
    "properties": {
      "query": {"type": "string", "description": "Términos de búsqueda"},
      "section": {"type": "string", "enum": ["I","II","III","IV","V"], "description": "Sección del BOE"}
    }
  }
}
```

## 🚦 Limitaciones y Consideraciones

### Rate Limits
- **60 requests/minuto**
- **1000 requests/hora**
- Implementar retry logic con backoff exponencial

### Manejo de Errores
```json
{
  "INVALID_DATE": "Usar formato YYYY-MM-DD",
  "DOCUMENT_NOT_FOUND": "Verificar ID del documento",
  "RATE_LIMIT_EXCEEDED": "Esperar antes de nuevas requests"
}
```

### Optimizaciones
1. **Cache resultados** frecuentes (departamentos, secciones)
2. **Batch requests** cuando sea posible
3. **Usar límites apropiados** para evitar timeouts
4. **Filtrar por sección** para resultados más relevantes

## 🔧 Configuración Técnica

### Headers Requeridos
```
Content-Type: application/json
Accept: application/json
```

### URLs de Endpoints
```
Base: https://mcpboe-func-prod.azurewebsites.net/api
Health: /health
Docs: /swagger (si está habilitado)
```

### Timeout Recomendado
- **Request timeout**: 30 segundos
- **Read timeout**: 60 segundos para documentos grandes

## 📚 Recursos Adicionales

- **Schema JSON**: `mcp-boe-schema.json`
- **TypeScript Types**: `mcp-boe-types.ts`
- **Documentación API**: [DEPLOYMENT.md](./DEPLOYMENT.md)
- **Código Fuente**: https://github.com/CBorj/MCP-BOE

## 🆘 Soporte

Para problemas de integración:
1. Verificar el endpoint `/health` 
2. Revisar logs en Azure Portal
3. Consultar documentación oficial del BOE
4. Crear issue en el repositorio GitHub
