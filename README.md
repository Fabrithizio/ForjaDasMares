# Forja das Marés — protótipo de ação e masmorras

Unity 6.3 LTS 6000.3.26f1, URP, C#, Input System e AI Navigation 2.0.16. Plataformas atuais: Windows e Android; iOS adiado. VS Code é o editor de código; Visual Studio IDE não é necessário.

## Executar

Abra Assets/ForjaDasMares/Scenes/DungeonTest.unity e pressione Play. Menu Forja das Mares → Open dungeon test. ArenaTest permanece como referência: F6 abre a arena de heróis/PvE. No executável, VOLTAR A ARENA acessa essa cena.

WASD/setas, mouse para mirar/atacar, E/clique direito para habilidade, Espaço para esquiva, 1–4/Tab para herói, R reinicia. F/gamepad botão norte/INTERAGIR usa objeto próximo. Android mantém joystick fixo e botões; nova etapa sem teste físico porque o usuário está com o aparelho.

Quatro heróis originais, cinco arquétipos de inimigos, invocações limitadas, três ondas de arena e chefe com três ataques/fases. Masmorras com três biomas, oito categorias de salas, seed configurável, mapa fixo, conexões físicas, portas, baús, chave opcional, descanso, objetivos, armadilhas e saída após chefe. Modelos geométricos e poses são provisórios: não há arte ou clips finais.

## Verificação

41 testes PlayMode aprovados no PC: os 23 de base/heróis, 12 de PvE e seis de masmorra. Cada herói derrotou os cinco arquétipos e o chefe. Percurso completo da masmorra foi vencido por movimento/combate reais. Gerador testado em 2.000 seeds × seis limites (12.000 combinações), além de 12 gerações físicas nos três biomas, com caminhos normal/chefe verificados. Builds e limitações constam em Docs/PROGRESSO.md. Não alegamos crossplay, multiplayer, equipamentos, salvamento completo ou teste físico Android desta etapa.

## Guias

- Docs/MASMORRAS_COMO_JOGAR.md
- Docs/MASMORRAS_ARQUITETURA.md
- Docs/PVE_COMO_JOGAR.md
- Docs/HEROIS_COMO_TESTAR.md
- Docs/PROGRESSO.md
- Docs/ARQUITETURA.md (plano mestre de longo prazo)

Backup contém Assets e .meta, Packages, ProjectSettings, código e documentação. Library, Temp, Logs e Builds ficam fora do Git. Para recuperar, clone/baixe o repositório e abra a pasta inteira na versão de Unity indicada, aguardando a resolução de pacotes. Não recupere somente os scripts, pois cenas/materials/configurações também são necessários.
