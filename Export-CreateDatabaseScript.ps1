[CmdletBinding()]
param(
    [ValidateSet("Create", "Update", "MarkApplied")]
    [string] $Mode = "Create",
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot
$databaseProject = Join-Path $projectRoot "database\ITHUB.SampleAPI.Database.sqlproj"
$emptyProject = Join-Path $projectRoot "database\script-target\EmptySchema.sqlproj"
$databaseInfoPath = Join-Path $projectRoot ".config\db-info.md"
$baselineDacpac = Join-Path $projectRoot ".config\ITHUB.SampleAPI.Database.baseline.dacpac"
$sourceDacpac = Join-Path $projectRoot "database\bin\Release\ITHUB.SampleAPI.Database.dacpac"
$emptyTargetDacpac = Join-Path $projectRoot "database\script-target\bin\Release\EmptySchema.dacpac"

if (-not (Test-Path -LiteralPath $databaseInfoPath)) {
    throw "The target database Name is missing from .config\db-info.md."
}

$databaseInfo = [System.IO.File]::ReadAllText($databaseInfoPath)
$databaseNameMatch = [regex]::Match($databaseInfo, '(?im)^\s*Name:\s*`?([^`\r\n]+?)`?\s*$')
if (-not $databaseNameMatch.Success) {
    throw "Could not read the target database Name from .config\db-info.md."
}
$targetDatabaseName = $databaseNameMatch.Groups[1].Value.Trim()

if ([string]::IsNullOrWhiteSpace($OutputPath) -and $Mode -ne "MarkApplied") {
    $outputFileName = if ($Mode -eq "Create") { "create-sampleapi-schema.sql" } else { "update-sampleapi-schema.sql" }
    $OutputPath = Join-Path $projectRoot "database\deployment\$outputFileName"
}

if (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $projectRoot $OutputPath
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

Push-Location $projectRoot
try {
    Write-Host "Restoring the pinned SqlPackage tool..."
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "Could not restore the local SqlPackage tool." }

    Write-Host "Building the application database project..."
    dotnet build $databaseProject --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "The application database project build failed." }

    if ($Mode -eq "MarkApplied") {
        Copy-Item -LiteralPath $sourceDacpac -Destination $baselineDacpac -Force
        Write-Host "Baseline recorded at: $baselineDacpac"
        Write-Host "Use this mode only after the corresponding SQL script has been applied successfully."
        return
    }

    if ($Mode -eq "Create") {
        Write-Host "Building an empty target schema for an offline create script..."
        dotnet build $emptyProject --configuration Release
        if ($LASTEXITCODE -ne 0) { throw "The empty target schema project build failed." }
        $targetDacpac = $emptyTargetDacpac
    }
    else {
        if (-not (Test-Path -LiteralPath $baselineDacpac)) {
            throw "No published-schema baseline exists. Generate the initial script with -Mode Create and record it after applying it using -Mode MarkApplied."
        }
        $targetDacpac = $baselineDacpac
    }

    $scriptArgs = @(
        "tool", "run", "sqlpackage", "--",
        "/Action:Script",
        "/SourceFile:$sourceDacpac",
        "/TargetFile:$targetDacpac",
        "/TargetDatabaseName:$targetDatabaseName",
        "/DeployScriptPath:$OutputPath",
        "/p:BlockOnPossibleDataLoss=True",
        "/p:DropObjectsNotInSource=False"
    )

    Write-Host "Generating an offline $Mode schema script..."
    & dotnet @scriptArgs
    if ($LASTEXITCODE -ne 0) { throw "SqlPackage could not generate the $Mode schema script." }

    $deploymentScript = [System.IO.File]::ReadAllText($OutputPath)
    $deploymentScript = "SET NOEXEC OFF;`r`nGO`r`n" + $deploymentScript
    [System.IO.File]::WriteAllText($OutputPath, $deploymentScript, [System.Text.UTF8Encoding]::new($false))

    Write-Host "SQL script generated at: $OutputPath"
    Write-Host "Review and execute the SQLCMD script against the target database with a login allowed to create and alter schema objects."
}
finally {
    Pop-Location
}
