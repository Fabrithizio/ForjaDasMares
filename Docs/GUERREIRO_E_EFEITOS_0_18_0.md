# Guerreiro, proporções e remoção dos efeitos de teste — 0.18.0

## Alterado

- Novo corpo Emberheart fornecido pelo usuário, auditado antes da integração: 12.618 triângulos, 72 ossos, uma skin e texturas PBR. Altura configurada 1,80 m; maga preservada em 1,70 m. Classe, atributos, equipamento e saves mantidos.
- Espada/machado limitados visualmente a 1,10 m, sem mudar alcance ou dano. Encaixe usa centro da palma e guarda inclinada. O deslocamento de equipamento herdado do modelo anterior não é aplicado às mãos Meshy. Dados de grip específicos dos itens continuam respeitados.
- Guerreiro recebe controlador com 25 clips no esqueleto correto; corrida real fornecida no ZIP, 19 frames, copiada sem root motion. Golpes de espada/machado separados. O guerreiro antigo permanece nos assets e nas cenas de referência, não sobreposto ao novo corpo da campanha.
- Gestos da maga ampliados: preparação mais alta, extensão de braço e mão livre mais ativa no grimório. Camadas do tronco e apoio dos pés preservados.
- Cones azuis do jogador desativados no uso normal. HeroAttackArea mantém opção explícita apenas para depuração/testes.
- Removidos os oito círculos de PrismaCastingView, os seis desenhos de HeroWeaponEffects e o círculo de Fúria. PrismaCastingView permanece como referência de identidade/encaixes da classe mágica, sem desenhar linhas.
- Espada/machado agora deixam uma malha de rastro construída a partir das posições reais da base e ponta da arma, com 24 amostras e desaparecimento rápido. Sem cone fixo ou arco artificial; limpa histórico ao trocar modelo, desativar e teleportar.
- Magia usa partículas de preparação, brilho junto às mãos, emissão direcional e faíscas no impacto; orçamento de 64 partículas por herói. Fúria usa brasas ascendentes, preservando aumento temporário de cadência.
- Discos sólidos das áreas de magia trocados por partículas suaves no chão, até 24 por área, mantendo a lógica de dano/armadilha. Réplicas usam a mesma apresentação. Avisos necessários de perigo de inimigos e armadilhas foram preservados.

## Arquitetura e reprodução

Gerar ambos os conjuntos com Blender 4.5 e ArtSource/Characters/SapphireMage/animate_sapphire.py; para guerreiro adicionar -- --warrior. Executar ForjaDasMares.Editor.HeroCombatPolishInstaller.Apply na Unity para materiais, prefabs, máscaras e cena CampaignCoop. Original do ZIP preservado fora do projeto; cópias de origem e fontes reprodutíveis incluídas. Bonecos antigos preservados para referência.

WeaponSweepRibbon, HeroWeaponEffects e MagicZoneMist são apenas apresentação. Não possuem colisores nem concedem dano/recompensas. Fury já existia no frame de rede; HeroPresentation agora recebe sua duração visual sem criar uma dependência do assembly Runtime para Networking. Sem novo protocolo ou serviço.

## Validação

34/34 testes PlayMode focados aprovados, zero falhas/ignorados: PrismaTests, ArsenalTests e ForjaDasMares.Tests.BrasaLocomotionTests. Incluem uma única skin nova, ossos, altura, dimensões das armas, comparação de poses, tipos de ataque, dano/colisões, ausência dos indicadores no combate normal e limpeza de partículas. Logs locais ember-tests.xml/ember-install.log. Nenhuma alegação de execução da suíte geral inteira.

Durante integração foram corrigidas referências restantes ao círculo de Fúria, uma validação de cajado aplicada indevidamente à espada e um controlador parcialmente gravado sem camada base. Instalador final confirmou corpos de 1,70/1,80 m e 25 clips por personagem.

Não houve APK novo, teste físico Android, benchmark móvel nem teste real de sincronização entre dispositivos. A qualidade artística e clipping durante todas as combinações ainda dependem da avaliação do usuário; não declarar animações finais por compilar. A ativação de Fúria continua sendo a mecânica de cadência existente com brasas, não uma nova habilidade de área.

## Arquivos principais

Assets/ForjaDasMares/Art/Characters/EmberHero e fontes; SapphireMage/SapphireAnimated e prefab/controller/mask; gerador animate_sapphire.py; SapphireMageInstaller.cs; HeroCombatPolishInstaller.cs; HeroModelView.cs; EquipmentVisualPlacement.cs; HeroPresentation.cs; HeroWeaponEffects.cs; WeaponSweepRibbon.cs; MagicZoneMist.cs; PrismaCastingView.cs; ActorImpactFeedback.cs; HeroAttackArea.cs; CombatZone.cs; CoopActor.cs; CoopDirector.cs; CoopReplicaView.cs; CampaignAbilityBar.cs; testes Prisma/Arsenal; CampaignCoop.unity; CampaignInstaller.cs; ProjectSettings.asset e documentação.

## Teste no PC

Abrir o executável estável em Builds/Atual/Windows/ForjaDasMaresCampanha.exe. No personagem, alternar para Guerreiro e comparar espada/machado. Em partida, segurar ataque, usar E e esquiva; comparar com cajado/grimório da maga. Observar mãos, tamanho das armas, articulação dos joelhos e rastro acompanhando a lâmina. O cone azul não deve aparecer. Armadilhas e ataques de inimigos continuam mostrando sinais de perigo.

Build final Windows: Succeeded/errors=0/exit=0, Logs/ember-windows.log. Inventário do guerreiro/espada conferido em janela PC 1280×720. A primeira verificação pontual do rastro perdeu sua janela curta; o teste passou a registrar a janela inteira do golpe com Time.captureDeltaTime=1/60, restaurando o valor ao final. Última rodada: 34/34 aprovada, incluindo emissão do rastro. Sem alterar a duração do efeito para satisfazer o teste.
