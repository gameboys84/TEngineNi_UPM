[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)][string]$ProjectPath,
    [string]$Revision = 'main',
    [string]$RepositoryUrl = 'https://github.com/gameboys84/TEngineNi_UPM.git'
)
$ErrorActionPreference = 'Stop'
if ($Revision -notmatch '^[A-Za-z0-9][A-Za-z0-9._/-]*$' -or $Revision.Contains('..')) { throw 'Invalid Git revision.' }
if ($RepositoryUrl -match '[?#]' -or [string]::IsNullOrWhiteSpace($RepositoryUrl)) { throw 'Provide the repository URL without query/revision suffix.' }
$root = (Resolve-Path -LiteralPath $ProjectPath).Path
$manifestPath = Join-Path $root 'Packages/manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
if (-not $manifest.dependencies) { throw 'Project manifest has no dependencies object.' }
$mapping = [ordered]@{
    'com.cysharp.unitask'='ThirdParty/UniTask'
    'com.tuyoogame.yooasset'='ThirdParty/YooAsset'
    'com.code-philosophy.hybridclr'='ThirdParty/HybridCLR'
    'com.ni.tengine'='com.ni.tengine'
    'com.ni.gamecore'='com.ni.gamecore'
}
foreach ($name in $mapping.Keys) {
    $embedded = @(Get-ChildItem -LiteralPath (Join-Path $root 'Packages') -Directory | Where-Object {
        $metadata = Join-Path $_.FullName 'package.json'
        (Test-Path -LiteralPath $metadata) -and ((Get-Content -LiteralPath $metadata -Raw -Encoding UTF8 | ConvertFrom-Json).name -eq $name)
    })
    if ($embedded.Count) { Write-Output "Keep embedded $name at $($embedded[0].Name)"; continue }
    $value = "${RepositoryUrl}?path=/$($mapping[$name])#${Revision}"
    $manifest.dependencies | Add-Member -MemberType NoteProperty -Name $name -Value $value -Force
    Write-Output "$name = $value"
}
if ($PSCmdlet.ShouldProcess($manifestPath, 'Configure Ni packages and explicit Git dependencies')) {
    [IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json -Depth 24) + "`n", [Text.UTF8Encoding]::new($false))
    Write-Output 'Open Unity to resolve packages; review manifest.json and packages-lock.json. Use a full commit SHA for stable consumption.'
}
