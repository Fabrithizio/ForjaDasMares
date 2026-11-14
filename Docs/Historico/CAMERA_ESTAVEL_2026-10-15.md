# Câmera estável — revisão 0.6.4

## Causa e mudança

Na partida observada, caixas próximas ao herói alteravam simultaneamente a inclinação e o comprimento do braço da câmera. A revisão0.6.3 limitou a velocidade angular, mas manteve recolhimento imediato por qualquer bloqueio da linha de visão; isso ainda mudava muito o tamanho do mapa na tela.

ArenaCamera agora preserva a direção diagonal e o braço configurados quando uma caixa/parede fica entre câmera e herói. CameraOcclusion trata esse bloqueio somente na apresentação: dois SphereCastsNonAlloc para tronco/pernas ocultam temporariamente o Renderer da geometria no caminho. Não desativa GameObject, Collider, navegação, dano ou portas. O objeto volta após0.2s sem obstruir; ao desativar/destruir câmera ou usar Snap, restaura o estado original forceRenderingOff. Não altera materiais nem cria cópias por quadro.

Apenas sobreposição real na posição da lente mantém recolhimento de emergência, sem mudar o ângulo; a câmera fica acima das paredes/caixas atuais e esse caso não é a obstrução comum observada. Não garante ausência de recolhimento em futuros prédios/tetos muito altos. A ocultação é binária, não transparência gradual. O renderer inteiro da peça é ocultado; o chão deve continuar visível. Limite de128 resultados por cast, adequado ao mapa atual mas não promessa para geometria futura arbitrariamente densa. Uma câmera de jogo ativa por cliente; divisão de tela não suportada por esse mecanismo.

## Configuração e testes

Sem regravar cenas. Novos controles no Inspector: Visibility Radius0.3m, Obstruction Release Delay0.2s; offset/FOV/followTime e máscara existentes preservados. Removidos os parâmetros de reação angular introduzidos em0.6.3, pois o ângulo não reage mais às caixas. Os testes antigos de subida/descida foram substituídos pela expectativa correta: pitch e distância constantes com caixa se movendo na linha de visão. Há testes de restauração, colisão preservada, chão, estado previamente oculto, proporções e pausa.

Fontes de API consultadas: [Unity Renderer6000.3](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/renderer), [SphereCastNonAlloc](https://docs.unity.com/en-us/engine/6000.5/script-reference/unityengine/physics/spherecastnonalloc). forceRenderingOff exclui o componente do desenho sem mudar Renderer.enabled; o código não altera Collider.enabled.

## Arquivos

Criados Scripts/Presentation/CameraOcclusion.cs e.meta. Alterados ArenaCamera.cs, Tests/PlayMode/CameraObstructionTests.cs, Networking/Editor/CoopInstaller.cs (saídas separadas0.6.4/code10), ProjectSettings/ProjectSettings.asset (versão após build) e documentação. Nenhuma migração de save ou alteração de combate/rede.

## Como conferir

Reabrir Jogar ForjaDasMares Online após atualizar o atalho; andar dos dois lados da caixa da sala2 e pelos cantos. O tamanho dos objetos e a inclinação devem permanecer estáveis. A peça que encobre o corpo some temporariamente, reaparece após afastar e continua bloqueando o personagem. Conferir PC e Android. Builds anteriores preservadas; jogos abertos continuam usando código anterior até reabrir.

Resultados da execução serão acrescentados após finalizar os processos.


## Regressão de código

Suíte PlayMode completa:99 casos,98 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. XML work/camera-stable/tests.xml. Inclui os seis testes atualizados de câmera e os sistemas existentes. Sem erros C#; aviso preexistente CS0108 em Telegraph.audio permanece fora deste ajuste. A Unity em modo batch também registrou avisos internos de alocação temporária na execução de cenas; não houve falha de teste e não há atribuição comprovada ao código de câmera. Verificação visual e builds serão registrados separadamente.


## Checkpoint Windows

Windows0.6.4 compilou com COOP_BUILD Succeeded/errors=0. A limpeza do Editor registrou erro ao encerrar ADB, posterior ao resultado de build; isso não foi tratado como sucesso Android. Executável novo aberto realmente, janela de menu renderizada e sem Exception/NullReference/MissingReference no log próprio inicial examinado. Usuário recebeu a versão nova para testar. Atalho Online aponta para InternetWindows-0.6.4 e conserva o perfil InternetCoopProfiles. APK0.6.4 ainda em preparação neste checkpoint; o APK instalado anteriormente continua0.6.3 até atualização verificada.
