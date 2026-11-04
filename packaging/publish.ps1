[CmdletBinding()]
param(
    [string]$Configuration = 'Release'
)

$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root 'out/AgentBrowser.Api'

dotnet clean (Join-Path $root 'AgentBrowser.sln') -c Release | Write-Verbose
dotnet restore (Join-Path $root 'AgentBrowser.sln') -c Release
dotnet publish (Join-Path $root 'src/AgentBrowser.Api/AgentBrowser.Api.csproj') `
    --configuration $Configuration `
    --runtime linux-x64 `
    --self-contained false `
    --output $output

Write-Output "Published to $output"
