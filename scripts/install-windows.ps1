param(
    [switch]$SkipPathUpdate
)

$ErrorActionPreference = "Stop"

function Fail($Message) {
    throw "[install-windows] $Message"
}

$runningOnWindows = $false
if ($PSVersionTable.PSVersion.Major -ge 6) {
    $runningOnWindows = $IsWindows
}
else {
    $runningOnWindows = ($env:OS -eq "Windows_NT")
}

if (-not $runningOnWindows) {
    Fail "Este script deve ser executado no Windows."
}

function Require-Command($Name, $Hint) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Fail "$Name nao encontrado. $Hint"
    }
}

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$SrcDir = Join-Path $RepoRoot "src"
$CsProj = Join-Path $SrcDir "ACC-CLI.csproj"

if (-not (Test-Path $CsProj)) {
    Fail "Projeto nao encontrado em $CsProj"
}

Require-Command "git" "Instale o Git for Windows: https://git-scm.com/download/win"
Require-Command "dotnet" "Instale o .NET SDK 8.0: https://dotnet.microsoft.com/download/dotnet/8.0"

$sdkList = & dotnet --list-sdks
if (-not ($sdkList | Select-String -Pattern '^8\.')) {
    Fail ".NET SDK 8.0 nao encontrado. Instale o SDK 8.0 e tente novamente."
}

[xml]$projectXml = Get-Content $CsProj
$version = $projectXml.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) {
    Fail "Nao foi possivel identificar a versao no ACC-CLI.csproj."
}

Write-Host "[install-windows] Compilando e empacotando autocli $version..."
Push-Location $SrcDir
try {
    dotnet restore
    dotnet build ACC-CLI.csproj -c Release
    dotnet pack ACC-CLI.csproj -c Release

    try {
        dotnet tool uninstall --global autocli | Out-Null
    } catch {
        # ignore
    }

    dotnet tool install --global --add-source "$SrcDir\bin\Release" autocli --version $version
}
finally {
    Pop-Location
}

$toolsPath = Join-Path $HOME ".dotnet\tools"
if (-not $SkipPathUpdate) {
    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $userParts = @()
    if (-not [string]::IsNullOrWhiteSpace($userPath)) {
        $userParts = $userPath -split ';'
    }

    if ($userParts -notcontains $toolsPath) {
        $newUserPath = if ([string]::IsNullOrWhiteSpace($userPath)) {
            $toolsPath
        } else {
            "$userPath;$toolsPath"
        }
        [Environment]::SetEnvironmentVariable("Path", $newUserPath, "User")
        if (($env:Path -split ';') -notcontains $toolsPath) {
            $env:Path = "$env:Path;$toolsPath"
        }
        Write-Host "[install-windows] PATH do usuario atualizado com $toolsPath."
        Write-Host "[install-windows] Se necessario, abra um novo terminal."
    }
}

Write-Host "[install-windows] Instalacao concluida."
autocli --version
