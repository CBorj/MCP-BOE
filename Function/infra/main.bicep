@description('Nombre de la aplicación que se utilizará para generar nombres de recursos únicos')
param appName string = 'mcpboe'

@description('Región de Azure donde se desplegarán los recursos')
param location string = resourceGroup().location

@description('Entorno de despliegue (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string = 'dev'

@description('Nombre del entorno AZD')
param environmentName string

@description('SKU del plan de App Service')
@allowed(['FC1', 'EP1', 'EP2', 'EP3'])
param appServicePlanSku string = 'EP1'

// Variables
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location, environmentName)
var resourceSuffix = '${appName}-${environment}-${resourceToken}'
var functionAppName = 'func-${resourceSuffix}'
var appServicePlanName = 'asp-${resourceSuffix}'
var applicationInsightsName = 'ai-${resourceSuffix}'
var logAnalyticsWorkspaceName = 'law-${resourceSuffix}'
var managedIdentityName = 'mi-${resourceSuffix}'

// Ensure storage account name is valid (3-24 chars, alphanumeric only)
var cleanStorageName = toLower(replace(replace('st${appName}${environment}${take(resourceToken, 8)}', '-', ''), '_', ''))
var storageAccountNameFinal = length(cleanStorageName) > 24 ? take(cleanStorageName, 24) : cleanStorageName

// Storage Account para Azure Functions
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountNameFinal
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// User-assigned Managed Identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
  tags: {
    'azd-env-name': environmentName
  }
}

// Role assignments for the managed identity
resource storageBlobDataOwnerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, managedIdentity.id, 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageBlobDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, managedIdentity.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageQueueDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, managedIdentity.id, '974c5e8b-45b9-4653-ba55-5f855dd0fb88')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88')
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageTableDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, managedIdentity.id, '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3')
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource monitoringMetricsPublisherRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resourceGroup()
  name: guid(resourceGroup().id, managedIdentity.id, '3913510d-42f4-4e42-8a64-420c390055eb')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '3913510d-42f4-4e42-8a64-420c390055eb')
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
    tier: appServicePlanSku == 'FC1' ? 'FlexConsumption' : 'ElasticPremium'
  }
  properties: {
    reserved: true // Para Linux
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// Function App
resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage__accountName'
          value: storageAccount.name
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedidentity'
        }
        {
          name: 'AzureWebJobsStorage__clientId'
          value: managedIdentity.properties.clientId
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: appServicePlanSku != 'FC1' ? 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}' : ''
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: appServicePlanSku != 'FC1' ? toLower(functionAppName) : ''
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'AzureWebJobsFeatureFlags'
          value: appServicePlanSku == 'FC1' ? 'EnableWorkerIndexing' : ''
        }
        {
          name: 'BoeApi__BaseUrl'
          value: 'https://www.boe.es/datosabiertos'
        }
        {
          name: 'BoeApi__Timeout'
          value: '00:02:00'
        }
        {
          name: 'BoeApi__MaxRetries'
          value: '3'
        }
        {
          name: 'BoeApi__RetryDelaySeconds'
          value: '2'
        }
        {
          name: 'App__Name'
          value: 'MCPBoe Function App'
        }
        {
          name: 'App__Version'
          value: '1.0.0'
        }
        {
          name: 'App__Environment'
          value: environment
        }
      ]
      cors: {
        allowedOrigins: [
          'https://portal.azure.com'
        ]
        supportCredentials: false
      }
      use32BitWorkerProcess: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    'azd-env-name': environmentName
    'azd-service-name': 'api'
  }
  dependsOn: [
    storageBlobDataOwnerRoleAssignment
    storageBlobDataContributorRoleAssignment
    storageQueueDataContributorRoleAssignment
    storageTableDataContributorRoleAssignment
  ]
}

// Diagnostic Settings for Function App
resource functionAppDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'default'
  scope: functionApp
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    logs: [
      {
        category: 'FunctionAppLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// Outputs
output functionAppName string = functionApp.name
output functionAppUrl string = 'https://${functionApp.properties.defaultHostName}'
output storageAccountName string = storageAccount.name
output applicationInsightsName string = applicationInsights.name
output resourceGroupName string = resourceGroup().name
output RESOURCE_GROUP_ID string = resourceGroup().id
