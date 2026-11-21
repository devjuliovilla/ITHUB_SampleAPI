[CmdletBinding()]
param(
    [ValidateSet("Local", "Production")]
    [string] $Target = "Local",
    [switch] $ConfirmProduction,
    [switch] $WhatIf
)

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot
$databaseProject = Join-Path $projectRoot "database\ITHUB.SampleAPI.Database.sqlproj"
$dacpacPath = Join-Path $projectRoot "database\bin\Release\ITHUB.SampleAPI.Database.dacpac"
$productionDatabaseName = $null

function Invoke-StatusCatalogMigration {
    param([string] $TargetConnectionString)

    $connection = $null
    try {
        Add-Type -AssemblyName System.Data
        $connection = New-Object System.Data.SqlClient.SqlConnection($TargetConnectionString)
        $connection.Open()

        $permissionCommand = $connection.CreateCommand()
        $permissionCommand.CommandText = "SELECT HAS_PERMS_BY_NAME(DB_NAME(), 'DATABASE', 'VIEW DEFINITION'), HAS_PERMS_BY_NAME(DB_NAME(), 'DATABASE', 'ALTER');"
        $permissionReader = $permissionCommand.ExecuteReader()
        $permissionReader.Read() | Out-Null
        $canViewDefinition = [Convert]::ToBoolean($permissionReader.GetValue(0))
        $canAlterSchema = [Convert]::ToBoolean($permissionReader.GetValue(1))
        $permissionReader.Close()
        if (-not $canViewDefinition -or -not $canAlterSchema) {
            throw "The SQL login needs VIEW DEFINITION and ALTER permissions on the target database before status migration or publishing. Assign db_owner on this database, or use a database deployment login with equivalent permissions."
        }

        $command = $connection.CreateCommand()
        $command.CommandText = [System.IO.File]::ReadAllText(
            (Join-Path $projectRoot "database\deployment\StatusCatalogMigration.sql"))
        $command.CommandTimeout = 120
        $reader = $command.ExecuteReader()
        do {
            if ($reader.FieldCount -gt 0) {
                while ($reader.Read()) { Write-Host $reader.GetValue(0) }
            }
        } while ($reader.NextResult())
        $reader.Close()
    }
    catch [System.Data.SqlClient.SqlException] {
        if ($_.Exception.Number -eq 4060) {
            Write-Host "Target database does not exist yet; SqlPackage will create it with the current schema."
            return
        }

        throw
    }
    finally {
        if ($null -ne $connection) { $connection.Dispose() }
    }
}

if ($Target -eq "Production" -and -not $ConfirmProduction) {
    throw "Production publishing requires the explicit -ConfirmProduction switch. No database changes were made."
}

if ($Target -eq "Production") {
    $connectionString = $env:SAMPLEAPI_PRODUCTION_CONNECTION_STRING
    if ([string]::IsNullOrWhiteSpace($connectionString)) {
        throw "Set SAMPLEAPI_PRODUCTION_CONNECTION_STRING in the current environment before publishing to Production."
    }

    $databaseInfoPath = Join-Path $projectRoot ".config\db-info.md"
    if (-not (Test-Path -LiteralPath $databaseInfoPath)) {
        throw "Production database name is missing. Add the Name field to .config\db-info.md."
    }

    $databaseInfo = [System.IO.File]::ReadAllText($databaseInfoPath)
    $databaseNameMatch = [regex]::Match($databaseInfo, '(?im)^\s*Name:\s*`?([^`\r\n]+?)`?\s*$')
    if (-not $databaseNameMatch.Success) {
        throw "Could not read the Production database Name from .config\db-info.md."
    }
    $productionDatabaseName = $databaseNameMatch.Groups[1].Value.Trim()

    $connectionBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($connectionString)
    if ($connectionBuilder.InitialCatalog -cne $productionDatabaseName) {
        throw "The production connection string Initial Catalog does not match the Name in .config\db-info.md. No database changes were made."
    }
}
else {
    $connectionString = $env:SAMPLEAPI_LOCAL_CONNECTION_STRING
    if ([string]::IsNullOrWhiteSpace($connectionString)) {
        $connectionString = "Server=localhost;Database=ITHUB_SampleAPI;Integrated Security=True;TrustServerCertificate=True;"
    }
}

if ($WhatIf) {
    Write-Host "Target: $Target"
    Write-Host "Database project: $databaseProject"
    Write-Host "A release dacpac would be built and published with BlockOnPossibleDataLoss=True and DropObjectsNotInSource=False."
    if ($Target -eq "Local") { Write-Host "Local target: localhost / ITHUB_SampleAPI (integrated security by default)." }
    else { Write-Host "Production database: $productionDatabaseName. The connection string is not displayed." }
    return
}

Push-Location $projectRoot
try {
    Write-Host "Restoring the pinned SqlPackage tool..."
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "Could not restore the local SqlPackage tool." }

    Write-Host "Building the SQL database project..."
    dotnet build $databaseProject --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "The database project build failed." }

    Write-Host "Migrating existing task and activity statuses..."
    Invoke-StatusCatalogMigration $connectionString

    $publishArgs = @(
        "tool", "run", "sqlpackage", "--",
        "/Action:Publish",
        "/SourceFile:$dacpacPath",
        "/TargetConnectionString:$connectionString",
        "/p:BlockOnPossibleDataLoss=True",
        "/p:DropObjectsNotInSource=False",
        "/DeployReportPath:$(Join-Path $projectRoot 'database\bin\Release\deployment-report.xml')"
    )

    Write-Host "Publishing schema to $Target. The connection string is not displayed."
    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { throw "SqlPackage could not publish the database schema to $Target." }

    Write-Host "Database schema successfully published to $Target."
}
finally {
    Pop-Location
}
