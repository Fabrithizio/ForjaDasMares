# Cidade e controles — 0.25.0 (30/10/2026)

## Executar
Abra Builds/Atual/Windows/ForjaDasMaresCampanha.exe e escolha ENTRAR NA CIDADE · REFÚGIO DO SINO. Na Unity, abra Assets/ForjaDasMares/Scenes/RefugioUltimoSino.unity e pressione Play. Não é necessário configurar serviços para esta cena local.

## O que testar
- Clique no chão para andar; segure para atualizar o destino. WASD e alternância F8 foram removidos do PC. Gamepad/toque continuam separados da entrada de teclado.
- Clique/segure em inimigo ou alvo quebrável para atacar parado, sem perseguir. Shift + ataque permite disparar livremente parado. Ao sair do alvo mantendo o gesto, não deve andar involuntariamente.
- Um clique no chão durante a recuperação do golpe fica pendente até liberar a janela de movimento, em vez de ser perdido. E: especial; Espaço: esquiva; Q: super quando carregado; F: interação próxima. Roda: zoom.
- Clique em moradores: aproximação por NavMesh e uma interação por gesto. Nara/Daro reutilizam a missão; pistas permanecem na costa. Iria/fonte recuperam vida, Luma troca entre heróis existentes. Téo só conversa, não vende itens.
- Cidade de 76 × 76 m sem encontros hostis, oito moradores geométricos, três circulando, três casas fechadas, tendas, praça, banca, bancos e caixas. NPCs/casas são placeholders, não arte final.
- Placa circular de chegada com quatro posições livres; não há cidade online implementada nesta cena. Portão sul leva à CostaAto1 existente. Alvo de treino não concede recompensas.
- HUD usa retrato do modelo, vida e super reais, ações circulares e diálogo separado. Não foi criada mana ou barra de seis habilidades fictícias.

## Arquitetura e arquivos
Novos: Networking/Editor/TownInstaller.cs, Networking/World/TownHub.cs, TownResident.cs, Networking/Tests/TownTests.cs, Scripts/Input/WorldClickTarget.cs, Scenes/RefugioUltimoSino.unity e dados/materiais/NavMesh em Data/World/Town (todos sob Assets/ForjaDasMares).
Alterados: Networking/CampaignHome.cs, CampaignSessionMenu.cs, Editor/CampaignInstaller.cs, Tests/ArpgControlsTests.cs, World/RegionalProbe.cs, RegionalWorld.cs, RegionalWorldHud.cs; Scripts/Gameplay/HeroCombat.cs; Scripts/Input/ArenaInput.cs, DesktopRpgControls.cs, TouchMoveStick.cs; Scripts/UI/EquipmentPaperDoll.cs; ProjectSettings/EditorBuildSettings.asset e ProjectSettings.asset.

WorldClickTarget separa interação de dano. TownHub integra a missão e serviços já existentes. TownInstaller preserva CostaAto1 e cria a cidade separadamente; pode ser reproduzido pelo executeMethod ForjaDasMares.Networking.Editor.TownInstaller.Apply. Colliders dos modelos importados são calculados pelos limites reais no espaço local. Cilindros estáticos da placa/fonte usam MeshCollider da geometria simples, evitando a cápsula invisível alta do primitivo Unity.
Dependências existentes: Unity 6000.3.26f1, URP, Input System e AI Navigation; nenhum serviço ou pacote novo exigido.

## Evidências
- 59/59 testes PlayMode: Logs/town025-final-tests.xml.
- Após corrigir a colisão da placa, repetidos 22/22 testes de controles/cidade: Logs/town025-collider-tests.xml; após corrigir a aproximação de objetos escalados, 23/23 passaram em Logs/town025-release-tests.xml. O teste agora limita também a altura máxima do personagem.
- Windows real compilado: Logs/town025-release-windows.log, CAMPAIGN_BUILD StandaloneWindows64 Succeeded errors=0.
- Conferência da build por captura e relatório em Logs/Town025FinalQA; perfil isolado de QA, sem substituir o progresso do usuário.

## Limitações e próximos passos
Aprovado com limitações para prévia local de PC. Arte de NPCs/prédios e movimentos de moradores ainda provisórios. Ajustar apresentação/áudio/escala com feedback jogando. As colisões corrigidas são da cidade; a costa antiga ainda precisa de auditoria de tendas/altares e demais props. Não foi medido combate prolongado nem desempenho móvel nesta revisão. O joystick ganhou início de arraste sem limiar, mas a responsividade precisa de teste físico posterior. A pedido posterior, APK 0.25.0/code49 ARM64 gerado em Builds/Atual/Android/ForjaDasMaresCampanha.apk (Logs/town025-android-release.log, Succeeded errors=0). Instalação USB tentada, mas aparelho ficou offline; atualização física ainda não confirmada. Cidade compartilhada, servidores de região, novos testes PC–Android e restante do mundo não estão concluídos. As sessões cooperativas anteriores permanecem separadas.
