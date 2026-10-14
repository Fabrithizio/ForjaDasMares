# Forja das Marés — protótipo de ação e masmorras

Unity 6.3 LTS 6000.3.26f1, URP, C#, Input System e AI Navigation 2.0.16. Plataformas atuais: Windows e Android; iOS adiado. VS Code é o editor de código; Visual Studio IDE não é necessário.

## Executar

Windows: versão de progressão0.5.0 em validação final. O ZIP de PC0.4.0 apresentou falta de recursos gráficos da URP;0.4.1 corrigiu a seleção da plataforma na compilação. Builds e testes atuais constam em Docs/PROGRESSO.md.

Abra Assets/ForjaDasMares/Scenes/ProgressionDungeon.unity e pressione Play. Menu Forja das Mares → Open progression dungeon. VOLTAR A ARENA acessa ProgressionArena com equipamento/inventário; nessa arena, MASMORRA retorna ao percurso. ArenaTest e DungeonTest permanecem como referência e testes da base; F6 abre ArenaTest.

WASD/setas, mouse para mirar/atacar, E/clique direito para habilidade, Espaço para esquiva, 1–4/Tab para herói, R reinicia. F/gamepad botão norte/INTERAGIR usa objeto próximo. Android mantém joystick fixo e botões; nova etapa sem teste físico porque o usuário está com o aparelho.

Quatro heróis originais, cinco arquétipos de inimigos, invocações limitadas, três ondas de arena e chefe com três ataques/fases. Masmorras com três biomas, oito categorias de salas, seed configurável, mapa fixo, conexões físicas, portas, baús, chave opcional, descanso, objetivos, armadilhas e saída após chefe. Progressão local com9 categorias de armas,5 raridades,5 slots por conjunto de herói, inventário/comparação/descarte confirmado, entregas, XP/nível/moedas, saque temporário, resultado e save versionado. Mascote de suporte com cura/cargas finitas. Modelos geométricos e poses são provisórios: não há arte ou clips finais.

## Verificação

68 testes PlayMode aprovados no PC, conservando os45 anteriores e acrescentando13 de domínio de progressão,9 de integração e1 percurso completo com equipamento/loot. Após revisão visual,9 testes de integração foram repetidos com verificação dos rótulos de botões. Cada herói derrotou os cinco arquétipos e o chefe nos testes da base; a masmorra também foi vencida com equipamento, coleta e persistência de resultado. Gerador:12.000 combinações lógicas e12 gerações físicas. Auditoria:4 cenas,7 prefabs,92 definições,0 scripts/referências ausentes. Sem crossplay ou multiplayer implementado neste checkpoint. APK e teste físico são verificações separadas.

## Compilar por plataforma

No Editor, os menus de build ativam a plataforma correspondente antes de compilar. Em modo batch, inicie a Unity com `-buildTarget Win64` para `ForjaDasMares.Editor.ArenaBuilds.Windows` ou `-buildTarget Android` para `ForjaDasMares.Editor.ArenaBuilds.Android`. O método interrompe a compilação se o destino ativo estiver errado: a URP usa esse destino para selecionar e remover recursos gráficos. Não basta definir somente BuildPlayerOptions.target após alternar entre Android e Windows.

## Guias de conteúdo

- Docs/MASMORRAS_COMO_JOGAR.md
- Docs/PROGRESSAO_COMO_JOGAR.md
- Docs/PROGRESSAO_ARQUITETURA.md
- Docs/REVISAO_TECNICA_2026-10-14.md
- Docs/MASMORRAS_ARQUITETURA.md
- Docs/PVE_COMO_JOGAR.md
- Docs/HEROIS_COMO_TESTAR.md
- Docs/PROGRESSO.md
- Docs/ARQUITETURA.md (plano mestre de longo prazo)

Backup contém Assets e .meta, Packages, ProjectSettings, código e documentação. Library, Temp, Logs e Builds ficam fora do Git. Para recuperar, clone/baixe o repositório e abra a pasta inteira na versão de Unity indicada, aguardando a resolução de pacotes. Não recupere somente os scripts, pois cenas/materials/configurações também são necessários.


