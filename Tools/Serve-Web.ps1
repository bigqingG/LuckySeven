param([int]$Port = 8078)
$projectRoot = Split-Path $PSScriptRoot -Parent
$buildRoot = Join-Path $projectRoot 'Builds/WebGLV2'
if (!(Test-Path (Join-Path $buildRoot 'index.html'))) { throw 'Build Web first: Unity > Lucky Seven > Build Web Prototype.' }
Write-Host "Open http://localhost:$Port/ in a browser. Ctrl+C stops the server."
python -m http.server $Port --bind 127.0.0.1 --directory $buildRoot
