# Plan de Migración MCP-BOE a .NET Azure Functions

## 📋 Resumen Ejecutivo

**Objetivo**: Adaptar el servidor MCP-BOE de Python a .NET y prepararlo para despliegue en Azure Functions con integración GitHub CI/CD.

**Proyecto Original**: Servidor MCP (Model Context Protocol) en Python que proporciona acceso a la API oficial del BOE (Boletín Oficial del Estado) español para consultas de legislación, sumarios y tablas auxiliares.

**Arquitectura Objetivo**: Azure Functions .NET 8 Isolated con despliegue automático desde GitHub.

## 🏗️ Análisis de Arquitectura Actual

### Componentes Python Existentes:
- **MCP Server**: Servidor principal que coordina herramientas
- **HTTP Client**: Cliente para API del BOE (httpx async)
- **Tools**: 3 categorías de herramientas:
  - `LegislationTools`: Búsqueda de legislación consolidada
  - `SummaryTools`: Sumarios del BOE/BORME
  - `AuxiliaryTools`: Tablas de departamentos, rangos, códigos
- **Models**: Modelos Pydantic para validación de datos
- **REST API Wrapper**: API REST opcional con FastAPI

### Endpoints BOE Identificados:
- Legislación consolidada: búsqueda y obtención de normas
- Sumarios diarios BOE/BORME
- Tablas auxiliares (departamentos, rangos normativos)

## 🎯 Arquitectura .NET Propuesta

### Stack Tecnológico:
- **.NET 8** (LTS) con modelo de proceso aislado
- **Azure Functions v4** 
- **System.Text.Json** para serialización
- **HttpClient** con IHttpClientFactory
- **FluentValidation** para validación de datos
- **Serilog** para logging estructurado
- **GitHub Actions** para CI/CD

### Estructura del Proyecto:
```
MCPBoe.FunctionApp/
├── src/
│   ├── MCPBoe.FunctionApp/
│   │   ├── Functions/           # Azure Functions endpoints
│   │   ├── Services/           # Lógica de negocio
│   │   ├── Models/             # DTOs y modelos
│   │   ├── Clients/            # HTTP clients para BOE API
│   │   ├── Configuration/      # Configuración y opciones
│   │   └── Extensions/         # Métodos de extensión
│   └── MCPBoe.Core/           # Librería compartida
├── tests/
│   ├── MCPBoe.FunctionApp.Tests/
│   └── MCPBoe.Core.Tests/
├── infra/                     # Bicep templates
├── .github/workflows/         # CI/CD pipelines
├── host.json
├── local.settings.json
└── MCPBoe.FunctionApp.csproj
```

## 🔧 Plan de Implementación

### Fase 1: Setup de Proyecto .NET (2-3 horas)
1. **Crear estructura de solución**
   - Proyecto Azure Functions .NET 8 Isolated
   - Proyecto Core para lógica compartida
   - Proyectos de testing

2. **Configurar dependencias**
   - Microsoft.Azure.Functions.Worker
   - Microsoft.Extensions.Http
   - System.Text.Json
   - FluentValidation
   - Serilog.Extensions.Logging

3. **Setup de configuración**
   - `host.json` con extension bundles [4.*, 5.0.0)
   - `local.settings.json` para desarrollo
   - Configuración de logging y HTTP client

### Fase 2: Migración de Modelos (1-2 horas)
1. **Convertir modelos Pydantic a C#**
   - `BOELegislationModel`
   - `BOESummaryModel`  
   - `BOEAuxiliaryModel`
   - DTOs para requests/responses

2. **Implementar validación**
   - FluentValidation rules
   - Atributos de validación
   - Manejo de errores estructurado

### Fase 3: Cliente HTTP para BOE API (2-3 horas)
1. **IBOEApiClient interface**
   - Métodos async para todos los endpoints
   - Manejo de rate limiting
   - Retry policies con Polly

2. **BOEApiClient implementation**
   - HttpClient configurado con IHttpClientFactory
   - Serialización/deserialización JSON
   - Logging y métricas

3. **Configuration**
   - BOE API base URL y configuración
   - Timeouts y retry policies
   - Authentication si es necesaria

### Fase 4: Servicios de Lógica de Negocio (3-4 horas)
1. **ILegislationService**
   - SearchConsolidatedLegislation
   - GetConsolidatedLaw
   - GetLawStructure

2. **ISummaryService**
   - GetBOESummary
   - GetBORMESummary
   - SearchRecentBOE

3. **IAuxiliaryService**
   - GetDepartmentsTable
   - GetLegalRangesTable
   - GetCodeDescription
   - SearchAuxiliaryData

### Fase 5: Azure Functions HTTP Triggers (2-3 horas)
1. **LegislationFunction**
   - POST /api/legislation/search
   - GET /api/legislation/{lawId}
   - GET /api/legislation/{lawId}/structure

2. **SummaryFunction**  
   - POST /api/summary/boe
   - POST /api/summary/borme
   - POST /api/summary/search

3. **AuxiliaryFunction**
   - GET /api/auxiliary/departments
   - GET /api/auxiliary/ranges
   - GET /api/auxiliary/code/{code}
   - POST /api/auxiliary/search

### Fase 6: Infraestructura como Código (2-3 horas)
1. **Bicep templates en `/infra`**
   - Function App con plan de consumo
   - Application Insights
   - Storage Account
   - Configuración CORS si es necesaria

2. **Azure DevOps/GitHub Actions**
   - Build pipeline
   - Deploy pipeline con environments
   - Automated testing

### Fase 7: Testing y Validación (2-3 horas)
1. **Unit Tests**
   - Servicios con mocks del HttpClient
   - Validadores
   - Mappers y extensiones

2. **Integration Tests**
   - Tests contra la API real del BOE
   - Tests de Azure Functions

3. **Load Testing**
   - Configuración básica de load testing

### Fase 8: CI/CD y Deployment (1-2 horas)
1. **GitHub Actions Workflows**
   - Build y test automation
   - Deploy to staging/production
   - Automated infrastructure provisioning

2. **Configuración de monitoring**
   - Application Insights
   - Health checks
   - Alertas básicas

## 📊 Mapeo de Funcionalidades

### Python → .NET Equivalencias:

| Python | .NET Equivalent | Notas |
|--------|----------------|-------|
| `httpx.AsyncClient` | `HttpClient` con `IHttpClientFactory` | Mejor pooling de conexiones |
| `pydantic.BaseModel` | Record types + FluentValidation | Inmutabilidad y validación |
| `fastapi` routes | Azure Functions HTTP triggers | Serverless scaling |
| `asyncio` | `async/await` con `Task<T>` | Nativo en .NET |
| `orjson` | `System.Text.Json` | Performance similar |
| `lxml` | `System.Xml` o `HtmlAgilityPack` | Para parsing XML/HTML |

## 🔐 Consideraciones de Seguridad

1. **Authentication & Authorization**
   - Azure AD integration si es necesaria
   - Function keys para acceso a endpoints
   - CORS configuration

2. **Data Protection**
   - HTTPS enforcement
   - Input validation estricta
   - Rate limiting

3. **Secrets Management**
   - Azure Key Vault para secrets
   - No hardcoded credentials
   - Managed Identity para auth

## 🚀 Configuración de Despliegue

### Variables de Entorno:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "BOE_API_BASE_URL": "https://www.boe.es/datosabiertos/api",
    "BOE_API_TIMEOUT": "30",
    "BOE_API_RETRY_COUNT": "3"
  }
}
```

### host.json:
```json
{
  "version": "2.0",
  "functionTimeout": "00:05:00",
  "extensionBundle": {
    "id": "Microsoft.Azure.Functions.ExtensionBundle",
    "version": "[4.*, 5.0.0)"
  },
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true
      }
    }
  }
}
```

## 📈 Estimación de Tiempo

| Fase | Estimación | Prioridad |
|------|------------|-----------|
| Setup Proyecto | 2-3h | Alta |
| Migración Modelos | 1-2h | Alta |
| Cliente HTTP | 2-3h | Alta |
| Servicios | 3-4h | Alta |
| Azure Functions | 2-3h | Alta |
| Infrastructure | 2-3h | Media |
| Testing | 2-3h | Media |
| CI/CD | 1-2h | Media |

**Total Estimado: 15-23 horas**

## ✅ Criterios de Éxito

1. **Funcionalidad**: Todos los endpoints del MCP Python funcionando en .NET
2. **Performance**: Tiempo de respuesta < 2s para el 95% de requests
3. **Reliability**: Disponibilidad > 99.9% 
4. **Deployment**: CI/CD completamente automatizado
5. **Monitoring**: Observabilidad completa con Application Insights
6. **Testing**: Cobertura de código > 80%

## 🔄 Plan de Migración Gradual

1. **Fase Alpha**: Core functionality (legislación)
2. **Fase Beta**: Sumarios y auxiliares
3. **Fase GA**: Feature parity completa + optimizaciones

¿Proceder con la implementación según este plan?
