# Script de configuración inicial para despliegue en Azure
# Ejecutar desde PowerShell con permisos de administrador

param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "West Europe",
    
    [Parameter(Mandatory=$false)]
    [string]$ServicePrincipalName = "mcpboe-github-actions"
)

# Colores para output
$Red = "`e[31m"
$Green = "`e[32m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-ColorText {
    param($Text, $Color)
    Write-Host "$Color$Text$Reset"
}

Write-ColorText "Configurando despliegue de MCPBoe Azure Functions" $Blue
Write-ColorText "=================================================" $Blue

# Verificar si Azure CLI está instalado
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-ColorText "✅ Azure CLI encontrado: $($azVersion.'azure-cli')" $Green
} catch {
    Write-ColorText "❌ Azure CLI no encontrado. Por favor instálalo desde: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" $Red
    exit 1
}

# Login a Azure
Write-ColorText "Iniciando sesión en Azure..." $Yellow
try {
    az login --output none
    Write-ColorText "✅ Sesión iniciada correctamente" $Green
} catch {
    Write-ColorText "❌ Error al iniciar sesión en Azure" $Red
    exit 1
}

# Configurar suscripción
Write-ColorText "Configurando suscripción: $SubscriptionId" $Yellow
try {
    az account set --subscription $SubscriptionId
    $currentAccount = az account show --output json | ConvertFrom-Json
    Write-ColorText "✅ Suscripción configurada: $($currentAccount.name)" $Green
} catch {
    Write-ColorText "❌ Error al configurar la suscripción" $Red
    exit 1
}

# Crear Resource Groups
Write-ColorText "Creando Resource Groups..." $Yellow

$resourceGroups = @(
    @{Name="rg-mcpboe-prod"; Description="Producción"},
    @{Name="rg-mcpboe-dev"; Description="Desarrollo"}
)

foreach ($rg in $resourceGroups) {
    try {
        $existingRg = az group show --name $rg.Name --output json 2>$null | ConvertFrom-Json
        if ($existingRg) {
            Write-ColorText "⚠️  Resource Group $($rg.Name) ya existe" $Yellow
        } else {
            az group create --name $rg.Name --location $Location --output none
            Write-ColorText "✅ Resource Group creado: $($rg.Name) ($($rg.Description))" $Green
        }
    } catch {
        Write-ColorText "❌ Error al crear Resource Group: $($rg.Name)" $Red
    }
}

# Crear Service Principal
Write-ColorText "Creando Service Principal para GitHub Actions..." $Yellow
try {
    $sp = az ad sp create-for-rbac --name $ServicePrincipalName --role contributor --scopes "/subscriptions/$SubscriptionId" --sdk-auth --output json | ConvertFrom-Json
    
    Write-ColorText "✅ Service Principal creado correctamente" $Green
    Write-ColorText "=================================================" $Blue
    Write-ColorText "CONFIGURACIÓN DE GITHUB SECRETS" $Blue
    Write-ColorText "=================================================" $Blue
    
    Write-ColorText "Copia los siguientes valores en GitHub:" $Yellow
    Write-ColorText "Repositorio → Settings → Secrets and variables → Actions" $Yellow
    Write-ColorText "" $Reset
    
    Write-ColorText "AZURE_CREDENTIALS:" $Green
    Write-Output ($sp | ConvertTo-Json -Depth 10)
    Write-ColorText "" $Reset
    
    Write-ColorText "AZURE_SUBSCRIPTION_ID:" $Green
    Write-Output $SubscriptionId
    Write-ColorText "" $Reset
    
    Write-ColorText "AZURE_RG:" $Green
    Write-Output "rg-mcpboe-prod"
    Write-ColorText "" $Reset
    
    Write-ColorText "AZURE_RG_DEV:" $Green
    Write-Output "rg-mcpboe-dev"
    Write-ColorText "" $Reset
    
} catch {
    Write-ColorText "❌ Error al crear Service Principal" $Red
    Write-ColorText "Verifica que tengas permisos de Global Administrator o Application Administrator en Azure AD" $Yellow
}

# Verificar archivos necesarios
Write-ColorText "Verificando archivos de infraestructura..." $Yellow

$requiredFiles = @(
    "infra/main.bicep",
    "infra/prod.parameters.json",
    "infra/dev.parameters.json",
    ".github/workflows/deploy-to-azure.yml"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-ColorText "✅ Encontrado: $file" $Green
    } else {
        Write-ColorText "❌ Faltante: $file" $Red
    }
}

# Mostrar próximos pasos
Write-ColorText "" $Reset
Write-ColorText "=================================================" $Blue
Write-ColorText "PRÓXIMOS PASOS" $Blue
Write-ColorText "=================================================" $Blue
Write-ColorText "1. Configura los secrets en GitHub (mostrados arriba)" $Yellow
Write-ColorText "2. Haz push de tu código al repositorio main" $Yellow
Write-ColorText "3. Ve a GitHub Actions para ver el despliegue" $Yellow
Write-ColorText "4. Una vez desplegado, tu API estará en:" $Yellow
Write-ColorText "   https://mcpboe-func-prod.azurewebsites.net/api/" $Green
Write-ColorText "" $Reset

# Guardar información en archivo
$setupInfo = @{
    SubscriptionId = $SubscriptionId
    ResourceGroups = $resourceGroups.Name
    ServicePrincipal = $ServicePrincipalName
    Location = $Location
    Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
}

$setupInfo | ConvertTo-Json | Out-File -FilePath "azure-setup.json" -Encoding UTF8
Write-ColorText "Información guardada en: azure-setup.json" $Green

Write-ColorText "" $Reset
Write-ColorText "Configuración completada!" $Green
