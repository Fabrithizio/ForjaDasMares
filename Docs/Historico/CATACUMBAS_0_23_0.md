# Catacumbas das Marés — prévia local 0.23.0

## Escopo e auditoria

Unity 6000.3.26f1, URP 17.3, Input System 1.14.2 e AI Navigation 2.0.16 existentes preservados; sem pacotes novos. Inspecionados RegionalWorld/Installer/HUD, EnemyActor/Pool/Brain/Motor/Combat/Presentation e os scripts de preparação Blender anteriores antes das alterações. CostaDasRuinas, campanha, controles e câmera existentes reutilizados. A nova cena é separada: CatacumbasDasMares, acessível no início por CATACUMBAS · PRÉVIA JOGÁVEL. Não substitui a Costa nem as masmorras da campanha.

Fornecidos nesta pasta: Ancient_Tidegate (arco), Ancient_Wave_Urn (urna) e Meshy_AI_Character_output.glb. A inspeção do GLB confirmou uma criatura quadrúpede com 18 ossos e pesos, zero animações; não era o assaltante humanoide. Coluna, nicho e baú novos não estavam presentes. Os arquivos zip originais não foram alterados. O GLB original está preservado em ArtSource/Characters/TideCrawler/Source.glb; SHA256 43DEC9FA6C7820387BB4963BB6087EA1F7B95426D113149F62AB53193C4C7569. Os originais dos cenários permanecem na pasta externa do usuário; FBXs normalizados e texturas de trabalho ficam em Art/World/Catacombs.

## Conteúdo

Galeria funerária, cripta central rebaixada com água estática e duas passagens laterais mais uma ponte central, santuário ao norte, refúgio inicial e portal de retorno aos desafios. Três patrulhas reutilizam detecção, navegação, validação de dano, morte, pooling, respawn e objetivo já existentes. Sem ganho permanente/economia nesta prévia local.

Arcos e urnas são os modelos fornecidos. Paredes, pilares, sarcófagos e luminárias complementares são peças geométricas provisórias, não arte final esculpida. O piso usa a imagem de cor gerada no kit de referência anterior, não um material PBR completo. A água não simula fluidos nem tem reflexão planar. Urnas são decoração neste estágio, não destrutíveis com drops. Não existe chefe novo nesta cena; a patrulha final não deve ser apresentada como chefe.

## Animação e integração

Seis clipes reais com chaves de ossos criados no Blender sobre o rig fornecido: Idle, Walk, Run, Attack, Hit, Dead. FBX Generic e Animator Controller próprios, sem root motion e sem conceder dano por eventos da animação. RiggedEnemyAnimation seleciona clipes conforme movimento/estado/vida e ajusta ritmo de locomoção. A detecção e dano continuam em EnemyCombat. EnemyPresentation preserva o material texturizado e não acrescenta oscilação/inclinação de boneco segmentado sobre o rig.

É uma primeira animação autoral do quadrúpede, não captura de movimento, animação final aprovada ou pacote de movimentos dos oito inimigos. Pés, rig e transições ainda precisam da avaliação visual do usuário. Só a criatura fornecida foi integrada nesta prévia, não os personagens das imagens restantes nem as réplicas online da campanha.

## Orçamento de desempenho

- Criatura: original 12.870 triângulos; LODs 6.435 / 3.217 / 1.608, mesmo esqueleto de 18 ossos. Texturas de trabalho 1K com mipmaps, uma superfície por renderer. Skinning limitado a duas influências na configuração do renderer; avaliar deformação visual antes de aumentar quantidade.
- Arco: 3.191 / 1.594 / 702 triângulos. Urna: 2.314 / 1.156 / 508. Três LODs cada, sem MeshColliders complexos.
- Pisos e paredes agrupados por material em setores de 32 m. Colisores estáticos simples permanecem para navegação/câmera; agrupamento não deve romper a associação do colisor com o renderer que pode ficar translúcido.
- Pool de 12 criaturas, limite de 10 vivas; três patrulhas de três. Atualização de nascimento existente a cada 0,25 s, IA com percepção intervalada existente.
- Seis luzes pontuais sem sombras, uma direcional; sem partículas novas, transparência de água animada, pós-processamento caro ou geometrias de terreno adicionais. Animator com culling de transformações fora de visão.

Isso é orçamento de conteúdo, não garantia de FPS em todos os aparelhos. Medição do player e evidências finais são registradas abaixo após a execução. Não foi gerado APK nem executado teste físico móvel.

## Validação e como testar

Primeira rodada: 21/21 PlayMode. Após ajustes de associação de paredes, colisores dos arcos, ponte e morte, rodada final: 42/42 passaram, zero ignorados: 4 CatacombPreviewTests, 7 RegionalWorldTests, 10 ArpgControlsTests e 21 CampaignTests. Testados conectividade do caminho e spawns fora das paredes; clipes, deformação de osso e ausência de deslocamento do root pela animação; dano ao jogador; morte, retorno ao pool, conclusão das patrulhas, retorno seguro ao refúgio; controles e costa preservados. A criação inicial do LODGroup no instalador falhou por null de componente Unity tratado com ??; corrigido com teste explícito de null antes da execução aprovada.

Abrir Builds/Atual/Windows/ForjaDasMaresCampanha.exe; no menu escolher CATACUMBAS · PRÉVIA JOGÁVEL. Andar/atacar nos modos F8 existentes, F para descansar no refúgio/usar portal liberado, R para retornar ao refúgio. Derrotar a patrulha de cada um dos três locais para liberar o portal. Inimigos reaparecem em 45 s, conforme a regra regional existente. Testar caminhar nos dois lados da cripta e junto às paredes, observar criatura correndo, atacando, recebendo golpe e morrendo.

O probe de desenvolvimento -catacombProbe CAMINHO é opt-in e escreve captura/relatório local, com média e pior tempo de quadro após aquecimento de 5 s; a medição inclui apresentação/cap de quadros e não separa CPU/GPU. Seus comandos de QA não são funções de gameplay nem mensagens de rede.

## Arquivos e reprodução

Novos: Networking/Editor/CatacombPreviewInstaller.cs, Networking/Tests/CatacombPreviewTests.cs, Scripts/Gameplay/RiggedEnemyAnimation.cs, Scenes/CatacumbasDasMares.unity; Art/Enemies/TideCrawler, Art/World/Catacombs, Data/World/Catacombs; scripts prepare_catacombs.py, animate_crawler.py e relatórios em ArtSource/RegionalWorld; GLB fonte em ArtSource/Characters/TideCrawler.

Alterados: CampaignHome.cs (entrada), CampaignInstaller.cs (cena/versionamento), RegionalWorldHud.cs (nome/dados da região), RegionalProbe.cs (QA opt-in), EnemyPresentation.cs (ramo de apresentação rigada), prepare_models.py (aceita lista de trabalhos alternativa), EditorBuildSettings.asset, ProjectSettings.asset e PROGRESSO.md.

Preparação: Python prepare_catacombs.py extrai apenas os dois zips novos e mapas do GLB; Blender animate_crawler.py gera o rig animado com LODs; Blender prepare_models.py -- ArtSource/RegionalWorld/Sources/catacomb_jobs.json prepara os props. Na Unity, executar ForjaDasMares.Networking.Editor.CatacombPreviewInstaller.Apply recria SOMENTE a prévia a partir da costa preservada; Windows pela rotina CampaignInstaller.Windows, sempre -force-d3d11 nesta máquina. Arquivos em Sources são staging ignorado no Git.

## Limitações e próximas peças

Falta arte modular final de parede reta/canto, nichos, colunas, sarcófagos e bordas de canal. Essas formas rígidas podem ser modeladas diretamente com medidas e pivôs consistentes, evitando gastar gerações em encaixes. Se usadas imagens novas para Meshy, cada objeto deve vir separado, realmente frontal/ortográfico, sem VFX nem equipamento fundido. Não são necessárias novas imagens para executar esta prévia.

Não inclui regiões públicas de 20 jogadores, servidor dedicado, progresso regional permanente, história, chefe novo, restante dos oito inimigos, animações finais, mobile/online físico ou certificação de desempenho. A prévia é local e reutiliza os limites da primeira região; não representa conclusão do mundo solicitado.

## Evidência final de Windows

Build Windows 0.23.0 concluída com Succeeded/errors=0 em Logs/catacomb-final-windows.log. Player real aberto a 1280x720: criatura, arco, piso, HUD e ataque inimigo conferidos por capturas; sem NullReference/MissingReference/Exception no log dessa execução. Capturas em Logs/CatacombFinalQA. Não houve avaliação humana completa do ritmo de animação ou sessão online/móvel.

Amostra preliminar na NVIDIA GeForce MX130: 19.513 quadros após aquecimento, média 8,88 ms/quadro e pico 99,54 ms, aproximadamente 120,3 MiB de memória total alocada reportada pela Unity. A amostra mistura refúgio, patrulhas, morte e teletransportes de QA, com nove inimigos ativos; não é benchmark exclusivamente de combate nem medida separada de CPU/GPU/VRAM. Os picos continuam sendo um risco a investigar com profiler; não significa estabilidade universal de 60 FPS. O profiler usado não mede toda a memória do processo. Fonte: report.json do probe, sem APK ou dispositivo físico.

Pendências anteriores de seleção mais precisa de alvo e zoom mais forte permanecem fora desta entrega de assets/catacumbas.
