# Brasa — primeiro personagem 3D autoral

## Blender e arquivos editáveis

Blender4.5.14 LTS portátil baixado do servidor oficial download.blender.org. ZIP Windows x64 conferido contra SHA256 oficial B9533D2397AC1984DB4466FB23A7A4649391CCA93F6E84209F9BCC60D071C8B9. Extraído em Desktop/Area Dev/Ferramentas/blender-4.5.14-windows-x64. Atalho Area Dev/Blender.lnk; Jogos/Editar personagem Brasa.lnk abre o arquivo editável. Sem instalador administrativo, alteração de associação de arquivos ou assinatura paga.

Fonte editável: ArtSource/Characters/Brasa/Brasa.blend. Gerador reproduzível: create_brasa.py, executável com Blender -b --python CAMINHO. Preview real renderizado no Cycles CPU: Brasa_preview.png. Métricas exportadas: model_report.json. Backups automáticos .blend1/.blend2 preservados localmente, ignorados no Git; arquivo .blend atual e script fazem parte do projeto.

## Direção e conteúdo real

Usadas as imagens enviadas como direção geral de proporções e materiais, sem incorporar os pixels das referências. Personagem original com torso forte, cabeça estilizada, cabelo curto com mecha e trança lateral, barba angular, tecido verde-petróleo, couro, metal e cobre. Espada larga com detalhe de brasa e armadura assimétrica. Sem copiar brasão, nome ou arquivo de personagem externo.

Modelo efetivamente construído em geometria 3D: pele com anatomia de torso/braços unificada por voxel e redução de polígonos, cabeça/orelhas/nariz/olhos, cabelo/barba, mãos/dedos, roupas, ombreira, braceletes, botas, cinto e espada. Primeira renderização revelou olhos salientes e peito separado demais: revisados e unidos os volumes. Não é escultura ou animação final de produção; rosto, cabelo, mãos, proporções e acabamento devem ser avaliados pelo titular.

Métricas finais do Blender:7.898 vértices/15.262 triângulos,17 ossos,13 materiais. A Unity pode dividir vértices por normais/materiais na importação. Corpo e espada são skins distintas; anatomia usa pesos por proximidade de segmentos e armaduras/pormenores usam pesos rígidos. Sem atlas de texturas pintadas: cores de materiais URP. Treze materiais ainda merecem atlas/otimização antes de conteúdo em escala; não afirmar FPS ou qualidade de produção sem profiling.

## Rig e animação

Oito ações com keyframes de ossos exportadas realmente em FBX: Idle48frames, Walk24, Attack24, Skill32, Dodge18, Hit12, Dead30, Victory48, a24fps. Clipes básicos produzidos por script, sem mocap ou biblioteca comercial. Attack/Skill seguem o tempo normalizado do combate existente. Animações são apresentação: não concedem dano, deslocamento autoritativo, moeda ou invulnerabilidade. Movimento e esquiva continuam no PlayerMotor. Animator usa State(int), Speed(float), Alive(bool), root motion desligado e controller com oito estados.

Prefab em Assets/ForjaDasMares/Art/Characters/Brasa/Brasa.prefab, FBX correspondente, palette.json e13 materiais externos. Rig genérico; nomes Hand.R/Hand.L funcionam como encaixes para equipamento. Espada autoral aparece no equipamento padrão; itens equipados substituem a espada e ainda usam os visuais geométricos existentes das armas. Esses visuais específicos devem ser refinados separadamente, sem declarar toda arma remodelada.

## Integração preservada

BrasaModelInstaller instala de modo idempotente em GaleriasDungeon e GaleriasArena, preservando raízes do HeroPresentation e referências dos placeholders antigos (desativados). ArenaTest, DungeonTest, Progression e CoopDungeon não recebem o modelo automaticamente. Só o guerreiro foi modelado; demais classes mantêm os modelos atuais.

HeroModelView é opcional e contém Animator/encaixes/espada padrão. HeroPresentation usa esse componente quando presente e conserva as poses geométricas para os outros heróis. HeroEquipment monta visuais nos encaixes e restaura as poses locais dos heróis anteriores; dados de atributos, compatibilidade, dano e inventário não foram alterados. Nenhuma mudança de economia, mapas, inimigos ou autoridade de rede. Integração deste modelo nas cenas online ainda depende de validação específica posterior.

## Verificar e editar

Abrir Blender pelo atalho Editar personagem Brasa. O .blend contém câmera/luzes/chão somente para preview; não são exportados ao FBX do jogo. Editar geometria e pesos, manter nomes de bones/objetos ou adaptar installer. Para regenerar tudo do script, salvar modificações manuais antes: o gerador recria os assets autorais dele, não modifica outros personagens.

Na Unity: Forja das Mares/Characters/Install Brasa in Galerias reimporta materiais/clipes, salva prefab e atualiza as duas cenas. GaleriasDungeon +Play inicia o guerreiro; testar WASD/toque, mouse/ataque, E/habilidade, Espaço/esquiva, trocar heróis, equipar/desequipar e morte/vitória. O controller permite inspecionar os oito clipes na janela Animation. Não exige Blender instalado para abrir o FBX/rodar a build.

## Arquivos criados/alterados

Novos: ArtSource/Characters/Brasa (.py/.blend/preview/relatório); Assets/ForjaDasMares/Art/Characters/Brasa (FBX/prefab/controller/paleta/materiais/metas); Scripts/Presentation/HeroModelView.cs/meta; Editor/BrasaModelInstaller.cs/meta; Tests/PlayMode/BrasaModelTests.cs/meta e este relatório. Alterados HeroPresentation.cs, HeroEquipment.cs, GaleriasInstaller.cs, cenas GaleriasArena/GaleriasDungeon, .gitignore e documentação/versão de build. Instalador do Blender/ZIP fora do repositório. As referências visuais anexadas não foram copiadas para o repositório público.

## Validação e limites

Importador executado duas vezes com saída0, BRASA_IMPORTED_CLIPS Attack,Dead,Dodge,Hit,Idle,Skill,Victory,Walk e BRASA_MODEL_READY scenes2/clips8/referências antigas preservadas. Regeneração Blender/FBX/render também terminou com saída0. Viewport do Blender inspecionado e enquadramento salvo. Testes e builds serão registrados abaixo ao concluir; não alegar execução antes dos resultados.

## Checkpoint dos testes

Primeira suíte completa114 casos:112 aprovados,1 ignorado por exigir projeto sem vínculo Cloud,1 falha em CoopProgressionTests.AuthorityCompletesRealCombatsBossAndUniqueSavedVictory (encontro4 não concluído no prazo). Os4 testes novos de Brasa passaram: skins/clipes/encaixes, movimento real dos ossos, oito estados, equipar/desequipar/trocar herói. Não declarar a suíte inteira aprovada nem atribuir a falha a uma causa sem reprodução.

Inspeção adicional encontrou captura da pose original apenas no Awake: equipamento cooperativo pode ser adicionado e configurado depois do Awake. Configure passou a capturar também a pose original, preservando o posicionamento das armas desses atores. Repetição focada de modelo/mapa/HUD/câmera/progressão cooperativa em execução. Builds ainda não foram declaradas produzidas neste checkpoint.

## Fechamento0.7.0 — resultados executados

Repetição focada concluída:26/26 aprovados,0 falhas, incluindo os4 testes de Brasa, mapa, HUD, câmera e CoopProgressionTests com o percurso que falhara. Não foi repetida a suíte inteira de114; permanece registrado o resultado inicial. Evidências locais em work/blender-character/accepted.xml e regression.xml do workspace Codex. Avisos nativos JobTempAlloc em execução batch continuam como risco separado, sem atribuir correção a esta entrega.

Windows: Builds/GaleriasWindows-0.7.0/ForjaDasMaresGalerias.exe real, GALERIAS_BUILD StandaloneWindows64 Succeeded/errors0. Aberto e inspecionado na janela: modelo/material/escala corretos; ataque orientado por mouse e botão de habilidade acionados, cooldown observado. Smoke de salvar/reabrir em perfil isolado passou com13 itens,5XP,5 moedas, armadura equipada, vida máxima160 e opções restauradas. Atalho Jogar Galerias da Mare atualizado preservando argumentos/perfil. Blender aberto no .blend e viewport enquadrado/salvo.

Android: compilação nativa IL2CPP/ARM64 completada; empacotador interno falhou no Java com Unable to establish loopback connection (build interno Failed/errors4). Gradle direto no projeto gerado terminou BUILD SUCCESSFUL em1m5s. APK efetivamente produzido: Builds/GaleriasAndroid-0.7.0/ForjaDasMaresGalerias.apk,51.601.758 bytes, SHA256 F74D9882C9FC4140B361C156134FF4F3AA1C4C9732147D4D2DCF88827E026FDD, assinatura v2 verificada, pacote com.forjadasmares.galerias, versão0.7.0/code15, ARM64. Instalado com -r no moto g15 Android15; dados preservados e aplicativo online original não substituído.

QA físico Android: inspecionada renderização do novo personagem, deslocamento pelo joystick com acompanhamento da câmera, habilidade com efeito/cooldown5s e esquiva com deslocamento/cooldown1s. Botão de ataque acionado; captura posterior não comprova a pose intermediária nem dano contra inimigo nesse teste físico. Clipes/estados/efeitos no combate continuam cobertos pelos testes Unity mencionados. Log próprio DUNGEON_RUNTIME_READY seed73129; sem NullReferenceException/MissingReferenceException/FATAL ou erro de Animator nos registros examinados. Capturas/logs locais android-start/skill/attack/dodge.png e android-device.log no workspace.

Status: aprovado com limitações para primeiro personagem integrado ao modo local. Não houve partida completa nesta revisão visual, teste de rede com o modelo, medição sustentada de FPS/memória ou revisão artística humana. Animações são primeiras poses programadas; cadência, ciclos, alinhamento dos golpes e deformação ainda precisam de refinamento. Não corresponde ao acabamento das imagens de referência. Integração online e remodelagem dos outros heróis/armas permanecem trabalhos separados.
