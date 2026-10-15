# Forja das Marés — protótipo de ação e masmorras

Unity 6.3 LTS 6000.3.26f1, URP, C#, Input System e AI Navigation 2.0.16. Plataformas atuais: Windows e Android; iOS adiado. VS Code é o editor de código; Visual Studio IDE não é necessário.

## Executar

Versão local de progressão0.5.0 preservada. Cooperativo0.6.1 foi testado em Windows/Android na LAN;0.6.2 inclui vínculo Unity Cloud e salas privadas por MPS/Lobby/Relay, já conectadas realmente entre Windows e moto g15 em Wi-Fi. A versão atual é de desenvolvimento, não aprovada para Play Store. Builds e evidências constam em Docs/PROGRESSO.md e Docs/REDE_INTERNET_2026-10-15.md.

Para cooperativo, abrir Assets/ForjaDasMares/Scenes/CoopDungeon.unity. Executável vinculado: Builds/InternetWindows/ForjaDasMaresCoop.exe; APK: Builds/InternetAndroid/ForjaDasMaresCoop.apk. Host usa CRIAR SALA INTERNET; cliente informa o código retornado no campo abaixo desse botão e usa ENTRAR POR CÓDIGO. Host inicia a partida após os jogadores entrarem. Não usar IP/código de LAN para entrar no Relay. Configuração e limites: Docs/INTERNET_E_PUBLICACAO.md. Sair do host encerra a partida; sem migração.

Abra Assets/ForjaDasMares/Scenes/ProgressionDungeon.unity e pressione Play. Menu Forja das Mares → Open progression dungeon. VOLTAR A ARENA acessa ProgressionArena com equipamento/inventário; nessa arena, MASMORRA retorna ao percurso. ArenaTest e DungeonTest permanecem como referência e testes da base; F6 abre ArenaTest.

No modo local: WASD/setas, mouse para mirar/atacar, E/clique direito para habilidade, Espaço para esquiva, 1–4/Tab para herói, R reinicia. F/gamepad botão norte/INTERAGIR usa objeto próximo. No cooperativo, escolher herói no lobby; o host controla o início. Android mantém joystick e botões por toque. Mira automática, ajustes completos dos controles e menus finais pertencem à próxima etapa de interface.

Quatro heróis originais, cinco arquétipos de inimigos, invocações limitadas, três ondas de arena e chefe com três ataques/fases. Masmorras com três biomas, oito categorias de salas, seed configurável, mapa fixo, conexões físicas, portas, baús, chave opcional, descanso, objetivos, armadilhas e saída após chefe. Progressão local com9 categorias de armas,5 raridades,5 slots por conjunto de herói, inventário/comparação/descarte confirmado, entregas, XP/nível/moedas, saque temporário, resultado e save versionado. Mascote de suporte com cura/cargas finitas. Modelos geométricos e poses são provisórios: não há arte ou clips finais.

## Verificação

Regressão geral89/89 aprovada no checkpoint de integração cooperativa. Última suíte específica de rede/internet:21 casos,20 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. Quatro processos Windows/LAN e PC–Android físico testados; Relay real com combate, recompensa e retorno após encerrar processo também foi verificado. Esses testes não representam vitória humana completa com quatro jogadores, profiling sustentado ou redes/operadoras distintas. Históricos de testes locais/geração/auditoria continuam nos respectivos relatórios; consultar suas datas e limites.

Estado dos pedidos: base local implementada; multiplayer em validação privada, interface completa parcial, expansão de conteúdo e revisão de lançamento pendentes. Ver Docs/ESTADO_DOS_PEDIDOS.md. Arte e animações finais, autenticação comercial, servidor dedicado e economia online segura ainda não foram entregues.

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
- Docs/COOP_COMO_TESTAR.md
- Docs/INTERNET_E_PUBLICACAO.md
- Docs/REDE_INTERNET_2026-10-15.md
- Docs/ESTADO_DOS_PEDIDOS.md
- Docs/ARQUITETURA.md (plano mestre de longo prazo)

Backup contém Assets e .meta, Packages, ProjectSettings, código e documentação. Library, Temp, Logs e Builds ficam fora do Git. Para recuperar, clone/baixe o repositório e abra a pasta inteira na versão de Unity indicada, aguardando a resolução de pacotes. Não recupere somente os scripts, pois cenas/materials/configurações também são necessários.


