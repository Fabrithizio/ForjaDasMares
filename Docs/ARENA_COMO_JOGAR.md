# Arena de teste — primeira versão jogável

A cena foi gerada pelo Editor e os 11 testes de PlayMode foram aprovados. Consulte PROGRESSO.md para os limites da validação.

## Local e dependências

Projeto: `C:/Users/Fabricio_S/Desktop/Jogos/ForjaDasMares`.

Editor escolhido: Unity 6.3 LTS, `6000.3.26f1`. Base: template oficial Universal 3D. O manifest e o lock do projeto registram as versões de pacotes efetivamente resolvidas após importação. Os alvos atuais são Windows e Android; iOS está adiado por instrução do usuário.

## Executar na Unity

1. No Unity Hub, adicionar esta pasta como projeto e abrir com a versão indicada em `ProjectSettings/ProjectVersion.txt`.
2. Aguardar a importação/resolução de pacotes e conferir o Console.
3. Abrir `Assets/ForjaDasMares/Scenes/ArenaTest.unity`, ou usar Forja das Mares → Open playable arena (F6).
4. Clicar em Play e dar foco à janela Game.

A cena já está salva e configurada. O menu `Forja das Mares/Create test arena (only if absent)` existe como ferramenta de recuperação: gera a cena somente se ela não existir e preserva uma arena já editada. Não é preciso executar um gerador a cada Play.

## Controles

| Ação | PC | Protótipo Android |
| --- | --- | --- |
| Mover | WASD ou setas, relativo à câmera | Arrastar joystick MOVER |
| Mirar | Mouse no plano do chão | Direção do movimento; orientação mantida quando para |
| Atacar | Clique esquerdo | ATAQUE |
| Esquiva | Espaço | ESQUIVA |
| Reiniciar | R ou botão REINICIAR | REINICIAR |

Gamepad tem bindings de movimento (analógico esquerdo), ataque (botão sul), esquiva (leste) e reset (Start). A integração é inicial; a interface final/remapeamento pertence a outra etapa. O protótipo Android usa paisagem. Toque funciona pelo mesmo Input System, por controles virtuais de gamepad, sem leitura de teclado no motor.

## O que observar

O personagem ciano é um placeholder composto de cápsula, esfera e cubos. O alvo laranja fica próximo do centro da arena. Aproximar-se, apontar e atacar reduz a vida em 25 por golpe válido, com flash branco e barra de vida. Após quatro golpes o alvo fica escuro; R restaura sua vida e devolve o personagem ao marcador de spawn.

A espada geométrica gira e um indicador geométrico de golpe aparece durante o ataque; são feedbacks procedurais, não uma animação final criada por artista. O alvo derrotado permanece como obstáculo de treinamento. Obstáculos e limites devem bloquear movimento e esquiva. Paredes bloqueiam o golpe.

## Ajustes no Inspector

- `Player_Placeholder/PlayerMotor`: velocidade, aceleração, desaceleração, gravidade, duração/velocidade/cooldown de esquiva.
- `Player_Placeholder/PlayerAim`: velocidade de giro e plano da mira.
- `03_MainCamera/ArenaCamera`: offset diagonal, ponto de observação, suavidade, máscara e raio de colisão, FOV e proporção de referência.
- `Data/TrainingSword.asset`: dano, alcance, arco, preparação e recuperação.
- `TrainingTarget_Placeholder/Health`: vida máxima.
- `TrainingTarget_Placeholder/TargetFeedback`: cor e duração do flash.
- `ArenaSession`: referências, spawn seguro, máscara de obstáculos e limite de queda.
- `05_PrototypeHUD/ArenaHud`: ativar `Show Touch In Editor` para inspecionar os botões de toque; isso não substitui um teste em aparelho Android.
- `Input/ArenaControls.inputactions`: ações Gameplay Move, Point, Attack, Dodge e Reset.

## Testes e limites

Os testes de PlayMode estão em `Assets/ForjaDasMares/Tests/PlayMode`. Usar Window → General → Test Runner → PlayMode. `PROGRESSO.md` deve informar quais foram realmente executados.

Esta entrega não inclui inimigos, multiplayer, progressão ou inventário. Também não demonstra desempenho em celular, crossplay ou prontidão comercial. Ao encontrar as paredes baixas desta arena, a câmera tenta uma inclinação mais alta mantendo o mesmo eixo horizontal, antes de reduzir sua distância. Em uma futura sala com teto/ambiente fechado, a estratégia precisa ser revalidada; espaços sem linha de visão livre podem reduzir o enquadramento. Arte, animações, áudio, safe area/remapeamento completos e interface final serão tratados em etapas posteriores.

## Android

Para um teste em aparelho, usar Android Build Support com o SDK/NDK/OpenJDK da mesma versão de Editor. No Build Profiles, criar/selecionar Android, incluir ArenaTest e conferir ferramentas externas em Preferences. A geração/instalação de APK e testes físicos só podem ser registrados como concluídos após execução real. Os módulos não substituem um telefone conectado/configurado.



## Executáveis desta rodada

Windows: abrir Builds/Windows/ForjaDasMares.exe mantendo a pasta de dados ao lado. Android: APK ARM64 em Builds/Android/ForjaDasMares.apk; já instalado e testado no moto g15 conectado. Movimento, ataque, esquiva e reset foram conferidos no aparelho. Consultar PROGRESSO.md para a limitação do Gradle na invocação interna da Unity nesta sessão.


## Montagem Android alternativa observada nesta sessão

O menu Forja das Mares → Build Android prototype configura ARM64/IL2CPP/API35 e tenta o build normal da Unity. Se falhar especificamente com Unable to establish loopback connection depois de gerar o projeto Gradle, a montagem abaixo foi usada com sucesso. Não usar esse procedimento para esconder erros C# ou outros erros de build: esses precisam ser corrigidos primeiro.

Na pasta raiz do projeto, em um PowerShell aberto normalmente, executar:

```powershell
$projectRoot = (Get-Location).Path
$androidToolsRoot = 'C:\Program Files\Unity\Hub\Editor\6000.3.26f1\Editor\Data\PlaybackEngines\AndroidPlayer'
$javaLauncher = Join-Path $androidToolsRoot 'OpenJDK\bin\java.exe'
$gradleLauncher = Join-Path $androidToolsRoot 'Tools\gradle\lib\gradle-launcher-9.3.1.jar'
$generatedProject = Join-Path $projectRoot 'Library\Bee\Android\Prj\IL2CPP\Gradle'
$previousJavaOptions = $env:JAVA_TOOL_OPTIONS
# README.md é um arquivo, portanto não permite criar sockets Unix sob esse caminho.
# O JDK instalado trata a falha do listener e utiliza seu fallback TCP.
$ipcMarker = (Join-Path $projectRoot 'README.md').Replace('\', '/')
$env:JAVA_TOOL_OPTIONS = "$previousJavaOptions -Djava.net.preferIPv4Stack=true `"-Djdk.net.unixdomain.tmpdir=$ipcMarker`""
Push-Location $generatedProject
try {
    & $javaLauncher -classpath $gradleLauncher org.gradle.launcher.GradleMain '-Dorg.gradle.jvmargs=-Xmx4096m' assembleDebug --no-daemon
    if ($LASTEXITCODE -ne 0) { throw 'Gradle falhou; conferir o erro antes de instalar.' }
    $apkOutput = Join-Path $projectRoot 'Builds\Android\ForjaDasMares.apk'
    New-Item -ItemType Directory -Force -Path (Split-Path $apkOutput) | Out-Null
    Copy-Item -LiteralPath 'launcher\build\outputs\apk\debug\launcher-debug.apk' -Destination $apkOutput -Force
} finally {
    Pop-Location
    $env:JAVA_TOOL_OPTIONS = $previousJavaOptions
}
```

Esse ajuste é por processo e foi validado com o JDK 17.0.18 fornecido neste Editor; revalidar se mudar o JDK. Não altera firewall, permissões de Windows ou configuração global Java. A pasta Gradle e o APK são resultados de build e não ficam no Git. Builds obtidos pela Unity aberta fora do contexto desta sessão ainda precisam de confirmação.
