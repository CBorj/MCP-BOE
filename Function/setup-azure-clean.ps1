# Script de configuración inicial para despliegue en Azure
param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "West Europe",
    
    [Parameter(Mandatory=$false)]
    [string]$ServicePrincipalName = "mcpboe-github-actions"
)

Write-Host "Configurando despliegue de MCPBoe Azure Functions" -ForegroundColor Blue
Write-Host "=================================================" -ForegroundColor Blue

# Verificar si Azure CLI está instalado
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Host "Azure CLI encontrado: $($azVersion.'azure-cli')" -ForegroundColor Green
} catch {
    Write-Host "Azure CLI no encontrado. Por favor instalalo desde: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# Login a Azure
Write-Host "Iniciando sesión en Azure..." -ForegroundColor Yellow
try {
    az login --output none
    Write-Host "Sesión iniciada correctamente" -ForegroundColor Green
} catch {
    Write-Host "Error al iniciar sesión en Azure" -ForegroundColor Red
    exit 1
}

# Configurar suscripción
Write-Host "Configurando suscripción: $SubscriptionId" -ForegroundColor Yellow
try {
    az account set --subscription $SubscriptionId
    $currentAccount = az account show --output json | ConvertFrom-Json
    Write-Host "Suscripción configurada: $($currentAccount.name)" -ForegroundColor Green
} catch {
    Write-Host "Error al configurar la suscripción" -ForegroundColor Red
    exit 1
}

# Crear Resource Groups
Write-Host "Creando Resource Groups..." -ForegroundColor Yellow

$resourceGroups = @(
    @{Name="rg-mcpboe-prod"; Description="Producción"},
    @{Name="rg-mcpboe-dev"; Description="Desarrollo"}
)

foreach ($rg in $resourceGroups) {
    try {
        $existingRg = az group show --name $rg.Name --output json 2>$null | ConvertFrom-Json
        if ($existingRg) {
            Write-Host "Resource Group $($rg.Name) ya existe" -ForegroundColor Yellow
        } else {
            az group create --name $rg.Name --location $Location --output none
            Write-Host "Resource Group creado: $($rg.Name) ($($rg.Description))" -ForegroundColor Green
        }
    } catch {
        Write-Host "Error al crear Resource Group: $($rg.Name)" -ForegroundColor Red
    }
}

# Crear Service Principal
Write-Host "Creando Service Principal para GitHub Actions..." -ForegroundColor Yellow
try {
    $sp = az ad sp create-for-rbac --name $ServicePrincipalName --role contributor --scopes "/subscriptions/$SubscriptionId" --sdk-auth --output json | ConvertFrom-Json
    
    Write-Host "Service Principal creado correctamente" -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Blue
    Write-Host "CONFIGURACIÓN DE GITHUB SECRETS" -ForegroundColor Blue
    Write-Host "=================================================" -ForegroundColor Blue
    
    Write-Host "Copia los siguientes valores en GitHub:" -ForegroundColor Yellow
    Write-Host "Repositorio -> Settings -> Secrets and variables -> Actions" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "AZURE_CREDENTIALS:" -ForegroundColor Green
    Write-Output ($sp | ConvertTo-Json -Depth 10)
    Write-Host ""
    
    Write-Host "AZURE_SUBSCRIPTION_ID:" -ForegroundColor Green
    Write-Output $SubscriptionId
    Write-Host ""
    
    Write-Host "AZURE_RG:" -ForegroundColor Green
    Write-Output "rg-mcpboe-prod"
    Write-Host ""
    
    Write-Host "AZURE_RG_DEV:" -ForegroundColor Green
    Write-Output "rg-mcpboe-dev"
    Write-Host ""
    
} catch {
    Write-Host "Error al crear Service Principal" -ForegroundColor Red
    Write-Host "Verifica que tengas permisos de Global Administrator o Application Administrator en Azure AD" -ForegroundColor Yellow
}

# Verificar archivos necesarios
Write-Host "Verificando archivos de infraestructura..." -ForegroundColor Yellow

$requiredFiles = @(
    "infra/main.bicep",
    "infra/prod.parameters.json",
    "infra/dev.parameters.json",
    ".github/workflows/deploy-to-azure.yml"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "Encontrado: $file" -ForegroundColor Green
    } else {
        Write-Host "Faltante: $file" -ForegroundColor Red
    }
}

# Mostrar próximos pasos
Write-Host ""
Write-Host "=================================================" -ForegroundColor Blue
Write-Host "PRÓXIMOS PASOS" -ForegroundColor Blue
Write-Host "=================================================" -ForegroundColor Blue
Write-Host "1. Configura los secrets en GitHub (mostrados arriba)" -ForegroundColor Yellow
Write-Host "2. Haz push de tu código al repositorio main" -ForegroundColor Yellow
Write-Host "3. Ve a GitHub Actions para ver el despliegue" -ForegroundColor Yellow
Write-Host "4. Una vez desplegado, tu API estará en:" -ForegroundColor Yellow
Write-Host "   https://mcpboe-func-prod.azurewebsites.net/api/" -ForegroundColor Green
Write-Host ""

# Guardar información en archivo
$setupInfo = @{
    SubscriptionId = $SubscriptionId
    ResourceGroups = $resourceGroups.Name
    ServicePrincipal = $ServicePrincipalName
    Location = $Location
    Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
}

$setupInfo | ConvertTo-Json | Out-File -FilePath "azure-setup.json" -Encoding UTF8
Write-Host "Información guardada en: azure-setup.json" -ForegroundColor Green

Write-Host ""
Write-Host "Configuración completada!" -ForegroundColor Green
