param([string]$Blender='C:\Users\Fabricio_S\Desktop\Area Dev\Ferramentas\blender-4.5.14-windows-x64\blender.exe')
$ErrorActionPreference='Stop'
if(-not (Test-Path -LiteralPath $Blender)){throw 'Informe o caminho do blender.exe com -Blender.'}
# Substitui apenas os arquivos de producao deste kit. Salve edicoes manuais com outro nome antes.
$env:SKIP_RENDER='1'
try {
 & $Blender -b --factory-startup --python-exit-code 1 --python (Join-Path $PSScriptRoot 'gerar_kit.py')
 if($LASTEXITCODE -ne 0){throw 'Falha ao gerar modelos.'}
} finally {Remove-Item Env:\SKIP_RENDER -ErrorAction SilentlyContinue}
foreach($script in @('finalizar_kit.py','validar_kit.py','conferir_render_final.py')) {
 & $Blender -b --factory-startup --python-exit-code 1 --python (Join-Path $PSScriptRoot $script)
 if($LASTEXITCODE -ne 0){throw ('Falha: '+$script)}
}
& (Join-Path $PSScriptRoot 'criar_pranchas.ps1')
