# Brasa 0.7.2 — locomoção e apoio dos pés

## Problema confirmado

Na revisão 0.7.1 o ataque substituía o clipe de caminhada de corpo inteiro. As pernas paravam enquanto o CharacterController continuava andando. A passada de 0,36 unidade local também não correspondia à velocidade de 5,8 unidades/s. A apresentação usava velocidade desejada, inclusive quando o jogador estava bloqueado por uma parede. O usuário confirmou rigidez/deslizamento no jogo real; a aprovação anterior da compilação não comprovava qualidade de locomoção.

## Correções

- HeroPresentation mede deslocamento real no plano, ignora teletransportes maiores que 2 unidades/quadro e entrega parâmetros antes da avaliação do Animator. A apresentação geométrica dos demais heróis mantém LateUpdate.
- HeroModelView controla Idle/Walk/Run na base e ataques/receber dano/vitória em camada superior mascarada quando há movimento. O tronco ataca enquanto as pernas continuam. Em repouso permanecem os clipes de corpo inteiro. Esquiva e morte conservam corpo inteiro.
- GaitTime avança por distância percorrida; velocidade contra parede não produz passada. Comprimento de ciclo no Inspector: Walk 0,9173 unidade e Run 2,4571 unidades, calibrados para escala 0,86. Histerese 2,6/3,2 unidades/s evita alternância excessiva caminhada/corrida.
- Novo clipe Run; Walk e Idle refeitos na fonte Blender. Idle tem postura apoiada, joelhos flexionados, respiração e pequenos movimentos de braços/cabeça. Treze clipes autorais reais no FBX. Sem root motion e sem alterar velocidade/collider/dano do ator.
- BrasaFootPlant usa os seis ossos de pernas, contatos no mundo e solução de dois segmentos para manter os apoios também ao mover lateralmente mirando com mouse. Dois pés reutilizados; raios somente na camada Ground, sem objetos por quadro. Ajuste de quadril limitado a 0,10 unidade e suavizado. Esquiva/morte/teletransporte reinicializam apoio. Posição do modelo corrigida em -0,045 unidade vertical.
- ArenaSession solicita 60 FPS no Android em execução real, configurável no Inspector; o padrão anterior do aplicativo não solicitava esse alvo. Isso não garante 60 FPS em todos os aparelhos.
- Máscara de Avatar genérico BrasaUpperBody.mask referencia Spine e descendentes. Root/Hips/pernas não recebem as ações superiores. Installer reexecutável mantém um único corpo nas duas cenas Galerias.

## Arquivos

Novos: Scripts/Presentation/BrasaFootPlant.cs, Tests/PlayMode/BrasaLocomotionTests.cs, Art/Characters/Brasa/BrasaUpperBody.mask e seus metas; este documento.

Alterados: HeroModelView.cs, HeroPresentation.cs, ArenaSession.cs, BrasaModelInstaller.cs, GaleriasInstaller.cs, BrasaModelTests.cs; create_brasa.py, Brasa.blend, Brasa_preview.png, model_report.json, FBX/meta/prefab/controller, cenas GaleriasDungeon/GaleriasArena, versão e documentação. Pacotes, inventário, mapas e fórmulas de dano preservados.

## Configuração e teste manual

Abrir GaleriasDungeon na Unity 6000.3.26f1. O guerreiro deve conter apenas um Brasa_Model, HeroModelView, BrasaFootPlant com seis ossos e Animator com duas camadas. Jogar com WASD/setas/joystick; mudar de direção, andar de lado mirando, encostar em paredes, atacar andando, parar, esquivar e reiniciar. Observar apoio dos pés e transições. Ataque segurado, pesado e Q/SUPER continuam conforme BRASA_COMBATE_2026-10-16.md.

Para reproduzir arte: Blender 4.5.14 --background --python ArtSource/Characters/Brasa/create_brasa.py; depois executar Forja das Mares/Characters/Install Brasa in Galerias. A fonte aberta anteriormente no Blender não é atualizada automaticamente quando o arquivo é regenerado.

## Evidências nesta revisão

Primeira rodada: 31/32 aprovados; teste de apoio detectou desvio de 0,06554 unidade, superior ao limite de 0,06. Ajuste de alcance do quadril corrigido sem relaxar o limite; segunda rodada 32/32 aprovados. Rodada final inclui também corrida e movimento lateral rápido, suavização de quadril e preservação do LateUpdate geométrico; 32/32 aprovados em locomotion-final-tests.xml.

Windows0.7.2 compilado com GALERIAS_BUILD StandaloneWindows64 Succeeded errors=0. Atalho atualizado, jogo aberto no perfil original e observadas postura parada/entrada de movimento; log próprio sem exceções de referência/Animator. Isso não equivale a profiling sustentado. Android0.7.2 ainda em compilação neste checkpoint; aparelho conectado está bloqueado, aguardando desbloqueio pessoal para observação física. Não há declaração de animação final, FPS sustentado ou teste online novo. Outros heróis continuam com apresentação provisória.
