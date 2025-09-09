# MCPBoe Azure Functions

Azure Functions implementation of the MCP-BOE server for accessing Spanish Official State Gazette (BOE) data.

## 🎯 Proyecto

Migración completa del servidor MCP-BOE de Python a .NET 8 Azure Functions, preparado para despliegue automático desde GitHub.

### Características Principales

- ✅ **11 endpoints HTTP** para acceso a la API del BOE
- ✅ **Búsqueda y consulta de legislación** española
- ✅ **Generación de resúmenes** de documentos oficiales
- ✅ **Servicios auxiliares** (departamentos, secciones, tipos de documento)
- ✅ **Arquitectura limpia** con separación de responsabilidades
- ✅ **Infrastructure as Code** con Bicep
- ✅ **CI/CD automatizado** con GitHub Actions
- ✅ **Telemetría integrada** con Application Insights

## 🏗️ Arquitectura

```
MCPBoe.FunctionApp/
├── src/
│   ├── MCPBoe.Core/           # 📚 Lógica de negocio
│   │   ├── Clients/           # HTTP clients para BOE API
│   │   ├── Services/          # Servicios de dominio
│   │   ├── Models/            # DTOs y modelos
│   │   └── Configuration/     # Configuración y opciones
│   └── MCPBoe.FunctionApp/    # 🔌 Azure Functions HTTP API
│       ├── Functions/         # HTTP triggers
│       └── Program.cs         # Configuración DI
├── infra/                     # 🏗️ Infrastructure as Code
│   ├── main.bicep            # Plantilla principal
│   ├── prod.parameters.json  # Parámetros producción
│   └── dev.parameters.json   # Parámetros desarrollo
└── .github/workflows/         # 🚀 CI/CD Pipeline
    └── deploy-to-azure.yml   # GitHub Actions workflow
```

## 🚀 Despliegue Rápido

### Opción 1: Script Automatizado (Recomendado)

```powershell
# Ejecutar desde PowerShell como administrador
./setup-azure.ps1 -SubscriptionId "tu-subscription-id"
```

### Opción 2: Manual

Sigue la guía detallada en [DEPLOYMENT.md](./DEPLOYMENT.md)

## 📡 API Endpoints

Una vez desplegado, la API estará disponible en:
`https://mcpboe-func-prod.azurewebsites.net/api/`

### 📜 Legislación
- `GET /api/legislation/summary` - Resumen de documentos
- `GET /api/legislation/search?query={texto}&date={fecha}` - Búsqueda de documentos
- `GET /api/legislation/document/{id}` - Documento específico por ID

### 📋 Sumarios
- `GET /api/summary/daily?date={fecha}` - Sumario diario
- `GET /api/summary/search?query={texto}&section={seccion}` - Búsqueda en sumarios
- `GET /api/summary/section/{section}?date={fecha}` - Sumario por sección

### 🏛️ Auxiliares
- `GET /api/auxiliary/departments` - Lista de departamentos
- `GET /api/auxiliary/sections` - Secciones del BOE
- `GET /api/auxiliary/subsections` - Subsecciones disponibles
- `GET /api/auxiliary/document-types` - Tipos de documento
- `GET /api/auxiliary/territories` - Territorios y comunidades

## 🛠️ Desarrollo Local

### Requisitos
- .NET 8 SDK
- Azure Functions Core Tools v4
- Visual Studio Code o Visual Studio 2022

### Configuración
```bash
# Clonar repositorio
git clone <tu-repo>
cd Function

# Restaurar dependencias
dotnet restore MCPBoe.FunctionApp/MCPBoe.sln

# Compilar
dotnet build MCPBoe.FunctionApp/src/MCPBoe.FunctionApp/MCPBoe.FunctionApp.csproj

# Ejecutar localmente
cd MCPBoe.FunctionApp/src/MCPBoe.FunctionApp
func start
```

La API local estará disponible en: `http://localhost:7071/api/`

### Configuración Local
Editar `MCPBoe.FunctionApp/src/MCPBoe.FunctionApp/local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "BoeApi__BaseUrl": "https://www.boe.es/datosabiertos/api/",
    "BoeApi__Timeout": "00:00:30"
  }
}
```

## 🔧 Tecnologías Utilizadas

- **Azure Functions v4** - Serverless compute
- **.NET 8** - Framework con isolated worker model
- **System.Text.Json** - Serialización JSON de alto rendimiento
- **FluentValidation** - Validación de DTOs
- **Polly** - Políticas de resilencia (retry, circuit breaker)
- **Application Insights** - Telemetría y monitoreo
- **Bicep** - Infrastructure as Code
- **GitHub Actions** - CI/CD automatizado

## 🌍 Entornos

### Desarrollo
- **Resource Group**: `rg-mcpboe-dev`
- **App Service Plan**: Consumption Y1 (gratuito)
- **Function App**: `mcpboe-func-dev`

### Producción
- **Resource Group**: `rg-mcpboe-prod`
- **App Service Plan**: Premium EP1 (alta disponibilidad)
- **Function App**: `mcpboe-func-prod`

## 📊 Monitoreo

- **Application Insights**: Telemetría automática
- **Azure Monitor**: Métricas y alertas
- **Health Check**: `GET /api/health`
- **Logs**: Disponibles en Azure Portal

## 🔒 Seguridad

- Service Principal para GitHub Actions
- Managed Identity para recursos de Azure
- HTTPS obligatorio
- CORS configurado según necesidades

## 📈 Estado del Proyecto

| Componente | Estado | Descripción |
|------------|--------|-------------|
| ✅ Migración .NET | Completado | 100% funcional |
| ✅ Azure Functions | Completado | 11 endpoints operativos |
| ✅ Pruebas locales | Completado | Todas las funciones probadas |
| ✅ Infrastructure as Code | Completado | Bicep templates listos |
| ✅ CI/CD Pipeline | Completado | GitHub Actions configurado |
| 🔄 Despliegue Azure | Listo | Esperando configuración inicial |

## 🤝 Contribución

1. Fork el proyecto
2. Crear branch para feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit cambios (`git commit -am 'Agregar nueva funcionalidad'`)
4. Push al branch (`git push origin feature/nueva-funcionalidad`)
5. Crear Pull Request

## 📝 Licencia

Este proyecto está bajo la licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

## 🆘 Soporte

- **Documentación**: [DEPLOYMENT.md](./DEPLOYMENT.md)
- **Issues**: GitHub Issues
- **API BOE Oficial**: https://www.boe.es/datosabiertos/

---

**Estado**: ✅ Listo para despliegue en Azure Functions
**Última actualización**: Enero 2025

### Estructura del Proyecto

```
MCPBoe.FunctionApp/
├── src/
│   ├── MCPBoe.Core/                 # Biblioteca principal
│   │   ├── Clients/                 # Clientes HTTP
│   │   ├── Configuration/           # Opciones de configuración
│   │   ├── Extensions/              # Extensiones de DI
│   │   ├── Models/                  # Modelos de datos
│   │   └── Services/                # Servicios de negocio
│   └── MCPBoe.FunctionApp/          # Azure Functions
│       ├── Functions/               # HTTP triggers
│       ├── host.json               # Configuración de Functions
│       ├── local.settings.json     # Configuración local
│       └── Program.cs              # Punto de entrada
└── tests/                          # Tests unitarios (futuro)
```

### Componentes Principales

- **MCPBoe.Core**: Biblioteca compartida con lógica de negocio
- **Azure Functions**: Endpoints HTTP con triggers
- **BOE API Client**: Cliente HTTP con resilencia (Polly)
- **Dependency Injection**: Registro de servicios y configuración

## 🛠️ Tecnologías

- **.NET 8** - Framework principal
- **Azure Functions v4** - Serverless compute
- **Isolated Worker Model** - Modelo de ejecución
- **System.Text.Json** - Serialización JSON
- **Polly** - Políticas de resilencia
- **FluentValidation** - Validación de requests
- **Application Insights** - Telemetría y logging

## ⚙️ Configuración

### Variables de Entorno

```json
{
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "BoeApi:BaseUrl": "https://api.boe.es",
    "BoeApi:Timeout": "00:02:00",
    "BoeApi:MaxRetries": 3
  }
}
```

### Configuración de BOE API

```json
{
  "BoeApi": {
    "BaseUrl": "https://api.boe.es",
    "Timeout": "00:02:00",
    "MaxRetries": 3,
    "RetryDelaySeconds": 2
  },
  "App": {
    "Name": "MCPBoe Function App",
    "Version": "1.0.0",
    "Environment": "Development"
  }
}
```

## 🚀 Despliegue

### Requisitos
- .NET 8 SDK
- Azure Functions Core Tools v4
- Azure CLI (para despliegue)

### Desarrollo Local

1. **Clonar el repositorio**
   ```bash
   git clone <repository-url>
   cd MCPBoe.FunctionApp
   ```

2. **Restaurar dependencias**
   ```bash
   dotnet restore
   ```

3. **Compilar el proyecto**
   ```bash
   dotnet build
   ```

4. **Ejecutar localmente**
   ```bash
   cd src/MCPBoe.FunctionApp
   func start
   ```

### Despliegue a Azure

1. **Crear recursos en Azure**
   ```bash
   # Function App
   az functionapp create \
     --resource-group myResourceGroup \
     --consumption-plan-location westeurope \
     --runtime dotnet-isolated \
     --functions-version 4 \
     --name myFunctionApp \
     --storage-account mystorageaccount
   ```

2. **Desplegar aplicación**
   ```bash
   func azure functionapp publish myFunctionApp
   ```

## 📝 Uso de la API

### Ejemplo: Buscar Legislación

```bash
curl -X POST "https://myfunctionapp.azurewebsites.net/api/legislation/search" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "código civil",
    "limit": 10,
    "consolidatedOnly": true
  }'
```

### Ejemplo: Obtener Sumario BOE

```bash
curl -X POST "https://myfunctionapp.azurewebsites.net/api/summary/boe" \
  -H "Content-Type: application/json" \
  -d '{
    "date": "20241201",
    "maxItems": 50
  }'
```

### Ejemplo: Verificar Salud

```bash
curl "https://myfunctionapp.azurewebsites.net/api/health"
```

## 🔧 Desarrollo

### Estructura de Respuestas

Todas las respuestas siguen el formato:

```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "timestamp": "2024-12-01T10:00:00Z"
}
```

### Manejo de Errores

- **400 Bad Request**: Errores de validación
- **404 Not Found**: Recurso no encontrado
- **500 Internal Server Error**: Errores del servidor

### Logging

- **Application Insights**: Telemetría automática
- **Structured Logging**: JSON con contexto
- **Niveles de Log**: Information, Warning, Error

## 📊 Monitoreo

### Métricas Disponibles
- Número de requests por endpoint
- Tiempo de respuesta promedio
- Errores por tipo
- Uso de dependencias externas

### Health Checks
- Status de la aplicación
- Conectividad a BOE API
- Información de versión

## 🤝 Contribución

1. Fork el proyecto
2. Crear rama feature (`git checkout -b feature/amazing-feature`)
3. Commit cambios (`git commit -m 'Add amazing feature'`)
4. Push a la rama (`git push origin feature/amazing-feature`)
5. Abrir Pull Request

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver `LICENSE` para más detalles.

## 🔗 Enlaces

- [BOE API Documentación](https://www.boe.es/datosabiertos/documentos/Manual_API_BOE_1_1.pdf)
- [Azure Functions Documentación](https://docs.microsoft.com/azure/azure-functions/)
- [.NET 8 Documentación](https://docs.microsoft.com/dotnet/)

---

**Nota**: Esta es una migración del proyecto MCP-BOE original de Python a .NET Azure Functions, manteniendo la misma funcionalidad pero aprovechando la escalabilidad y el modelo serverless de Azure.
