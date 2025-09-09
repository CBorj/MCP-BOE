# Guía de Despliegue en Azure Functions

## Requisitos Previos

1. **Cuenta de Azure activa**
2. **Azure CLI instalado**
3. **Repositorio en GitHub**
4. **Permisos de Contributor en la suscripción de Azure**

## Paso 1: Configurar Azure CLI

```bash
# Instalar Azure CLI (si no está instalado)
# Windows: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows

# Iniciar sesión en Azure
az login

# Verificar la suscripción activa
az account show

# Si necesitas cambiar de suscripción
az account set --subscription "Tu-Subscription-ID"
```

## Paso 2: Crear Service Principal para GitHub Actions

```bash
# Crear Service Principal para GitHub Actions
az ad sp create-for-rbac --name "mcpboe-github-actions" --role contributor --scopes /subscriptions/{subscription-id} --sdk-auth

# Esto devuelve un JSON como este:
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

## Paso 3: Crear Resource Groups

```bash
# Resource Group para Producción
az group create --name "rg-mcpboe-prod" --location "West Europe"

# Resource Group para Desarrollo (opcional)
az group create --name "rg-mcpboe-dev" --location "West Europe"
```

## Paso 4: Configurar Secrets en GitHub

Ve a tu repositorio en GitHub → Settings → Secrets and variables → Actions

Crea los siguientes secrets:

### Para Producción:
- `AZURE_CREDENTIALS`: Todo el JSON del Service Principal
- `AZURE_SUBSCRIPTION_ID`: Tu subscription ID
- `AZURE_RG`: `rg-mcpboe-prod`

### Para Desarrollo (opcional):
- `AZURE_CREDENTIALS_DEV`: JSON del Service Principal para dev
- `AZURE_RG_DEV`: `rg-mcpboe-dev`

## Paso 5: Despliegue Manual Inicial (Opcional)

Si quieres desplegar manualmente primero para probar:

```bash
# Navegar al directorio del proyecto
cd MCPBoe.FunctionApp

# Desplegar infraestructura
az deployment group create \
  --resource-group rg-mcpboe-prod \
  --template-file ../infra/main.bicep \
  --parameters ../infra/prod.parameters.json

# Compilar y publicar la aplicación
dotnet publish src/MCPBoe.FunctionApp/MCPBoe.FunctionApp.csproj --configuration Release --output ./publish

# Crear un ZIP para despliegue
cd publish
zip -r ../mcpboe-function.zip .
cd ..

# Desplegar la función
az functionapp deployment source config-zip \
  --resource-group rg-mcpboe-prod \
  --name mcpboe-func-prod \
  --src mcpboe-function.zip
```

## Paso 6: Activar GitHub Actions

1. Haz push de tu código al repositorio
2. Ve a GitHub → Actions
3. El workflow se ejecutará automáticamente

## Estructura del Workflow

- **Push a main**: Despliega a producción
- **Pull Request**: Despliega a desarrollo (si está configurado)
- **Workflow_dispatch**: Permite ejecución manual

## Endpoints de la API

Una vez desplegado, tu API estará disponible en:

```
https://mcpboe-func-prod.azurewebsites.net/api/
```

### Endpoints disponibles:

#### Legislación:
- `GET /api/legislation/summary` - Resumen de documentos
- `GET /api/legislation/search` - Búsqueda de documentos
- `GET /api/legislation/document/{id}` - Documento específico

#### Sumarios:
- `GET /api/summary/daily` - Sumario diario
- `GET /api/summary/search` - Búsqueda en sumarios
- `GET /api/summary/section/{section}` - Sumario por sección

#### Auxiliares:
- `GET /api/auxiliary/departments` - Departamentos
- `GET /api/auxiliary/sections` - Secciones
- `GET /api/auxiliary/subsections` - Subsecciones
- `GET /api/auxiliary/document-types` - Tipos de documento
- `GET /api/auxiliary/territories` - Territorios

## Monitoreo

- **Application Insights**: Configurado automáticamente para telemetría
- **Azure Monitor**: Métricas y logs disponibles en el portal de Azure
- **Health Check**: Endpoint `/api/health` para verificar el estado

## Solución de Problemas

### Error de permisos
```bash
# Verificar permisos del Service Principal
az role assignment list --assignee {client-id}
```

### Error de despliegue
- Revisar los logs en GitHub Actions
- Verificar que todos los secrets estén configurados
- Comprobar que el Resource Group existe

### Function App no responde
- Verificar en Azure Portal → Function App → Functions
- Revisar logs en Application Insights
- Comprobar configuración de CORS si es necesario

## Configuración Adicional

### Variables de Entorno
Puedes agregar variables de entorno en:
- Azure Portal → Function App → Configuration → Application Settings

### CORS (si necesitas acceso desde browser)
```bash
az functionapp cors add --resource-group rg-mcpboe-prod --name mcpboe-func-prod --allowed-origins "*"
```

### Custom Domain (opcional)
- Configurar en Azure Portal → Function App → Custom domains
- Requiere certificado SSL (puede usar App Service Managed Certificate gratuito)
