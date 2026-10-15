# Galerias da Maré — mapa original de referência

## Auditoria e recorte

Base: Unity6000.3.26f1/URP, gerador DungeonGenerator, grafo DungeonLayout, dados RoomDefinition/BiomeDefinition, encontros existentes, progressão e câmera estável com35% de opacidade. O gerador atual tem salas12m, corredores4m, portas, chave, baús, armadilhas e chefe. Não há destruição de cenário implementada; caixas são sólidos. Não reescrever movimento/combate/rede.

Referências de direção geral: [página oficial Dungeon Rampage](https://store.steampowered.com/app/3053950/Dungeon_Rampage), [portfólio de Joe Shochet](https://joeshochet.com/dungeon-rampage/). Consultadas como referência de ação cooperativa/cartunesca e composição, sem importar imagens, arquivos, personagens ou reproduzir uma planta específica. A porcentagem90% não é um critério técnico verificável nem meta de cópia. Criar um percurso e peças próprios.

## Arquitetura da entrega

- DungeonStyleDefinition opcional: meshes e materiais substituíveis, percurso fixo original. Campo nulo conserva cenas atuais.
- DungeonGenerator usa o estilo somente quando atribuído; mantém os colliders e a lógica de encontros/interações.
- DungeonLayout aceita rota principal opcional validada; geração anterior continua por padrão.
- GaleriasDungeon e GaleriasArena são variantes salvas das cenas de progressão existentes, com referências reais e retorno entre si. Cenas antigas preservadas.
- Meshas de piso/blocos/caixas/lanternas/grades criadas no Editor e salvas como assets. Não criar milhares de objetos por bloco: paredes/pisos são malhas compartilhadas com poucos renderers. Geometria provisória, sem arte/animações finais.
- Novo atalho/build separado de teste; perfil separado para não usar o progresso da partida atual. Primeiro mapa local; integração deste mapa ao cooperativo depende de validação posterior, sem alterar o mapa online existente nesta entrega.

## Percurso

Entrada → dois combates → evento com cristal/chave e baú → combate difícil → armadilha → chefe/saída, com tesouro e descanso opcionais. Trajeto principal próprio em ziguezague, até9 salas conectadas. Inimigos/recompensas reutilizam os dados existentes, sem inflar atributos. Câmera, movimento e controles atuais preservados.

## Verificação prevista

Compilar, testar compatibilidade da rota padrão, conectividade/navegação da nova cena, colisões/pontos de spawn, referências de meshes/materiais, encontros/portas/objetivos e retorno da arena. Gerar executável real PC e conferir apresentação. Android: manter compatibilidade de código, registrar build/teste real disponível. Nada será declarado testado em dispositivo desconectado.

Resultados, arquivos e limitações serão registrados ao finalizar.


## Resultados PC e configuração corrigida

Primeira regressão:105 casos,99 aprovados da base,5 falhas nos testes novos por referências de salas/bioma que não ficaram gravadas,1 ignorado por exigir projeto sem vínculo Unity Cloud. Instalador corrigido: recarrega os assets após abrir a cena, salva e reabre para validar8 referências de salas, estilo e bioma. Primeira repetição3/5: duas expectativas dos testes foram corrigidas (portas fechadas bloqueiam a navegação intencionalmente; nomes de meshes salvos não são o identificador do estilo). A verificação de conectividade agora abre temporariamente as portas e restaura os estados; a identificação usa os assets de mesh reais. Última rodada **5/5 aprovada,0 falhas**, XML work/galerias/map-accepted.xml. A suíte ampla não foi apresentada como repetida após essa correção exclusiva de instalador/cena/testes; os99 casos da base já passaram com o mesmo código runtime.

Teste completo de percurso utilizou dano de QA para derrotar os inimigos, ativou objetivos/interações reais e chegou ao chefe/saída/resultado, recusando repetição. Não é vitória humana nem validação final de balanceamento. Pontos de spawn, física, materiais e navegação das duas classes de agente foram verificados. Retorno GaleriasDungeon→GaleriasArena→GaleriasDungeon passou.

Windows0.6.6 gerado realmente: GALERIAS_BUILD StandaloneWindows64 Succeeded/errors0. Duas execuções do binário com -progressionSmoke write/read e diretório de QA isolado confirmaram baú, morte, equipagem de armadura,13 itens/5XP/5moedas e opções após reiniciar; maxHealth160 no reload. Sem exceções nos logs próprios examinados. Jogo normal aberto em paralelo com outro perfil isolado. Captura nativa mostrou piso e alvenaria renderizados, mas um aviso do Firewall cobre o centro e precisa de resposta pessoal; não afirmar revisão visual completa nem medir FPS a partir da captura.

## Abrir e substituir as peças

- PC: Desktop/Jogos/Jogar Galerias da Mare.lnk. Executável Builds/GaleriasWindows-0.6.6/ForjaDasMaresGalerias.exe. Começa no mapa; WASD/setas, mouse, habilidade/esquiva/interação e inventário atuais. Atalho usa -progressionSavePath em AppData/LocalLow/ForjaDasMaresPrototype/GaleriasTestProfile, separado do offline antigo e do Online. O atalho Online0.6.5 foi preservado.
- Unity: abrir GaleriasDungeon.unity e Play; GaleriasArena.unity retorna ao mapa. Menus Forja das Mares/Galerias/Create test map, Open test map e Build Windows/Android. Criar/reparar variante preserva as cenas de referência, mas reaplica os dados de estilo às cenas Galerias.
- Peças: Data/MapStyles/Galerias/Style.asset aponta para11 meshes salvos;8 materiais próprios e8 definições de salas, mais bioma. Malhas normalizadas, posicionadas/escaladas pelo gerador; manter pivô central e dimensões normalizadas ao trocar modelos ou ajustar a configuração para combinar com os colliders. Configuração calibrada para salas12m; uma escala arbitrária exige rever portas, decoração e spawn.
- Layout: mainRoute no Style.asset, exatamente7 posições principais adjacentes e únicas. Seed73129, máximo9; duas ramificações são determinadas pela seed. O validador rejeita rota desconectada/sobreposta. Dano, vida, ataques, recompensas e controles são os mesmos sistemas existentes.

## Limitações deste mapa

Geometria provisória original, sem texturas/modelos/animações finais. Caixas são sólidas e não destrutíveis. Chefe/inimigos são os anteriores, não conteúdo novo. HUD ainda é o protótipo funcional. Mapa novo local; não foi colocado silenciosamente no Relay ou declarado testado em quatro clientes. Não houve profiling sustentado nem teste Android físico desta variante. Não representa90% de semelhança nem fechamento do jogo inteiro.

## Arquivos

Novos: Scripts/Data/DungeonStyleDefinition.cs, Editor/GaleriasInstaller.cs e GaleriasMeshes.cs, Tests/PlayMode/GaleriasMapTests.cs e metadados; Scenes/GaleriasDungeon.unity e GaleriasArena.unity/metadados; Data/MapStyles/Galerias (29 assets:11 meshes,8 materiais,8 salas, estilo e bioma, com metadados de arquivos/pastas); este relatório. Alterados: DungeonGenerator.cs (estilo opcional), DungeonLayout.cs (rota opcional), ProgressionSceneBridge.cs (destino configurável), EditorBuildSettings.asset, versões de ProjectSettings após build, README e PROGRESSO. Cenas antigas sem modificação.

APK e fechamento de build serão acrescentados quando verificados.
