[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot
$apiProject = Join-Path $projectRoot "src\ITHUB.SampleAPI.Api\ITHUB.SampleAPI.Api.csproj"
$productionSettingsPath = Join-Path $projectRoot "src\ITHUB.SampleAPI.Api\appsettings.Production.json"
$databaseInfoPath = Join-Path $projectRoot ".config\db-info.md"

function Get-UserSecretValue {
    param(
        [string[]] $SecretLines,
        [string] $Name
    )

    foreach ($line in $SecretLines) {
        $separator = $line.IndexOf('=')
        if ($separator -lt 0) { continue }

        if ($line.Substring(0, $separator).Trim() -ceq $Name) {
            return $line.Substring($separator + 1).Trim()
        }
    }

    return $null
}

$secretLines = @(& dotnet user-secrets list --project $apiProject 2>$null)
if ($LASTEXITCODE -ne 0) {
    throw "Could not read the API User Secrets."
}

$connectionString = Get-UserSecretValue $secretLines "ConnectionStrings:SampleApi"
$signingKey = Get-UserSecretValue $secretLines "Jwt:SigningKey"
if ([string]::IsNullOrWhiteSpace($connectionString)) {
    throw "Configure ConnectionStrings:SampleApi in User Secrets before preparing the Plesk publish settings."
}
if ([string]::IsNullOrWhiteSpace($signingKey) -or [System.Text.Encoding]::UTF8.GetByteCount($signingKey) -lt 32) {
    throw "Configure a Jwt:SigningKey of at least 32 UTF-8 bytes in User Secrets."
}

if (-not (Test-Path -LiteralPath $databaseInfoPath)) {
    throw "The target database Name is missing from .config\db-info.md."
}

$databaseInfo = [System.IO.File]::ReadAllText($databaseInfoPath)
$databaseNameMatch = [regex]::Match($databaseInfo, '(?im)^\s*Name:\s*`?([^`\r\n]+?)`?\s*$')
if (-not $databaseNameMatch.Success) {
    throw "Could not read the target database Name from .config\db-info.md."
}
$targetDatabaseName = $databaseNameMatch.Groups[1].Value.Trim()
$connectionBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($connectionString)
if ($connectionBuilder.InitialCatalog -cne $targetDatabaseName) {
    throw "The production User Secrets connection string does not target the database listed in .config\db-info.md."
}

$productionSettings = [ordered]@{
    ConnectionStrings = [ordered]@{
        SampleApi = $connectionString
    }
    Jwt = [ordered]@{
        Issuer = "ITHUB_SampleAPI"
        Audience = "ITHUB_SampleAPI.Client"
        SigningKey = $signingKey
        ExpirationMinutes = 60
    }
    SeedData = [ordered]@{
        Administrator = [ordered]@{
            Email = ""
            DisplayName = ""
            Password = ""
        }
        User = [ordered]@{
            Email = ""
            DisplayName = ""
            Password = ""
        }
    }
    Cors = [ordered]@{
        AllowedOrigins = @()
    }
    Logging = [ordered]@{
        LogLevel = [ordered]@{
            Default = "Information"
            "Microsoft.AspNetCore" = "Warning"
        }
    }
    AllowedHosts = "*"
}

$json = $productionSettings | ConvertTo-Json -Depth 8
[System.IO.File]::WriteAllText($productionSettingsPath, $json, (New-Object System.Text.UTF8Encoding($false)))
Write-Host "Created the ignored local Plesk settings file: $productionSettingsPath"
Write-Host "It contains production credentials. Do not commit or share this file."
