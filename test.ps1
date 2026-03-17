# Script: RunExecutableFromSln.ps1
# Ubícate en la carpeta donde está tu .sln antes de ejecutar

param (
    [string]$SolutionFile = "Uam3.sln"
)

# Leer la lista de proyectos en la solución
$projects = dotnet sln $SolutionFile list | Select-Object -Skip 2 | Where-Object {$_ -ne ""}

$exeFound = $false

foreach ($proj in $projects) {
    $projPath = Join-Path -Path (Get-Location) -ChildPath $proj
    if (Test-Path $projPath) {
        $content = Get-Content $projPath
        $outputType = ($content | Select-String "<OutputType>(.*)</OutputType>").Matches.Value
        if ($outputType -match "Exe") {
            Write-Host "Proyecto ejecutable encontrado: $proj"
            $projDir = Split-Path $projPath
            Set-Location $projDir
            dotnet run
            $exeFound = $true
            break
        }
    }
}

if (-not $exeFound) {
    Write-Host "No se encontró proyecto ejecutable en la solución."
}
