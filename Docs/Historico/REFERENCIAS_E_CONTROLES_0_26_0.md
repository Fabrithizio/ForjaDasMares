# Referências e ajustes 1–3 — 0.26.0

31/10/2026. Bloco de PC. Android permanece com o APK 0.25.0 enviado anteriormente; nenhum APK novo solicitado/gerado neste bloco.

## Referências
12 PNGs individuais originais em Materiais do jogo/03 Kits para Meshy/Mundo magico - kit 01, junto de LEIA_PRIMEIRO.md, PROMPTS.md, CATALOGO_GERAL.csv e ZIP ao lado da pasta. Cópia de produção para backup Git em ArtSource/WorldDesign/MemoryWorldKit01. Ferramenta integrada image_gen, não CLI. O catálogo tem 122 entradas planejadas; NÃO são 122 imagens prontas. As outras 110 referências ainda não foram produzidas. Os PNGs não são modelos e não foram importados como cenário final.

Primeiro produzir: 01 console, 02 pilar inteiro, 07 arbusto, 08 árvore. Demais do kit: pilar quebrado, arco, placa reparada, raiz com estrutura, luminária, bancada, marco e cobertura. Geometria modular repetitiva e efeitos não devem ser pedidos como centenas de modelos independentes. Os modelos já existentes continuam aproveitáveis.

## 1. Cliques, colisões e navegação
DesktopRpgControls resolve chão pela camada 8 independentemente do sólido da camada 9. NPC/objeto interativo ainda usa seu collider para clique e aproximação; inimigos continuam selecionados antes da consulta ao chão e ataques não ganham perseguição automática. Clique dentro de sólido procura NavMesh em até 4m, rejeitando grande diferença vertical e caminho incompleto. Fora desse limite, rejeita o destino.

ActOneInstaller: limites do renderer transformados para o espaço do collider, corrigindo 112 props da CostaAto1. Migração pontual RepairPropCollisions mantém terreno/posições/missão. TownInstaller restringe colisão da árvore ao tronco em vez da copa. NavMeshes das duas cenas atualizados. Passagens muito estreitas e modelos futuros ainda exigem revisão própria; não é um solucionador universal de navegação.

## 2. Desempenho
F8 mostra/esconde FPS, tempo médio de quadro, p95, memória alocada pela Unity e inimigos ativos na região/cidade. Reutiliza CampaignPerformanceHud, antes limitado ao cooperativo. Janela de até 240 amostras, texto atualizado a cada 0,5s. Não são medidas separadas de CPU/GPU, não representa toda a RAM do processo, nem ping. Valores de carregamento acima de 1s não entram nessa janela. Na campanha, testes existentes devem preservar exclusão de inventário/lobby.

## 3. Seleção
TargetOutline acompanha o alvo sob o mouse (ou mira automática móvel já existente). Contorno dourado ao apontar, laranja durante ataque válido. Limpa com UI/morte/troca de alvo, não modifica vida/material original e não cria colliders. Silhuetas são renderers adicionais reutilizados por alvo; um alvo visível por controlador local. Shader URP com teste de profundidade, sem renderização por cima de paredes. Inverted hull pode ter emendas em malhas com normais separadas e precisa de avaliação com cada novo asset. Compatibilidade móvel no código, sem teste físico nesta revisão.

## História e primeiro vestígio
BASE_DO_MUNDO_2026_10_31.md documenta a nova direção aprovada. Causa da ruptura continua desconhecida. A missão marítima anterior é legado e não foi reescrita. AncientMemorySite é um bloco original provisório na cidade: três meshes combinadas, três materiais, console examinável por clique/F, sem recompensa, novo save ou desbloqueio global.

## Arquivos principais
Novos: Scripts/Presentation/TargetOutline.cs; Resources/SelectionOutline.shader; dados/malhas/materiais Ancient* em Data/World/Town; documentos BASE_DO_MUNDO_2026_10_31.md e REFERENCIAS_E_CONTROLES_0_26_0.md; kit em ArtSource/WorldDesign/MemoryWorldKit01.
Alterados sob Assets/ForjaDasMares: Scripts/Input/DesktopRpgControls.cs; Networking/CampaignPerformanceHud.cs; Networking/World/RegionalWorldHud.cs e RegionalProbe.cs; Networking/Editor/ActOneInstaller.cs, TownInstaller.cs e CampaignInstaller.cs; Networking/Tests/ArpgControlsTests.cs e TownTests.cs; cenas CostaAto1 e RefugioUltimoSino e respectivos Navigation.asset. ProjectSettings/ProjectSettings.asset: versão Windows 0.26.0. Nenhuma dependência nova.

## Teste manual
Abra a versão estável Windows, entre na cidade, clique no chão atrás da árvore/tenda e dentro de uma área sólida. Observe o contorno ao apontar para o alvo de treino; segure ataque para mudar a cor. F8 abre medições. Console da nova estrutura: à direita e acima da chegada, próximo de (10,0,12), clique ou aproxime e use F. Vá à costa e confira a passagem junto das tendas e braseiro.

## Evidências desta entrega
- 26/26 testes do primeiro vestígio/cidade/controles em Logs/memory026-tests.xml, antes dos ajustes 1–3.
- Validação final ampliada: 50/50 PlayMode em Logs/controls026-final-tests.xml (ArpgControlsTests, TownTests, ActOneTests e InventoryClarityTests).
- Windows compilado de verdade: Logs/controls026-windows.log, CAMPAIGN_BUILD StandaloneWindows64 Succeeded errors=0.
- Build aberta com perfil isolado: Logs/Controls026QA/screen.png e report.json; FPS visível e contorno do alvo de treino conferidos na captura. Nenhuma exceção/shader error no log observado. Amostra curta em cidade sem combate na MX130, sem conclusão sobre hordas, estabilidade prolongada ou Android.
- A cor de ataque, bloqueio por UI e limpeza por morte foram verificadas pelos testes. Não foi feita sessão online ou teste físico móvel neste bloco.

Status: aprovado com limitações para PC. Restam 110 referências do catálogo futuro, produção/integração dos modelos finais, validação visual de cada novo inimigo e teste físico Android. Não se avançou para combate/animações nem mundo completo.
