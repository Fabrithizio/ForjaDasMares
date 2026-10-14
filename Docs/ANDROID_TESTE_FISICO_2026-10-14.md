# Teste físico Android — versão local0.5.0

Aparelho real: moto g15, Android15/API35, ARM64, tela1080x2400 (jogo em paisagem2400x1080). Em14/10/2026, versão instalada0.2.0 foi atualizada com adb install -r para o APK0.5.0 já produzido e assinado, preservando os dados.

Observado em capturas do aparelho: masmorra e arena renderizam; joystick move e aponta após arrasto horizontal/diagonal; ataque básico Guerreiro reduz alvo100→74; habilidade reduz74→29 e mostra cooldown5s; troca para Patrulheiro e reinício restauram alvo100; projétil à distância reduz100→80. Inventário abre por toque, comparação real mostra crítico0→3%, EQUIPAR aplica3%. Após force-stop e reabrir o app, selecionar Patrulheiro mostrou inventário12/24 e crítico3%, confirmando persistência em reinício físico. Não houve exceção/FATAL/ANR nos trechos de log do processo examinados.

PSS pontual observado315–329MiB; isso é uma amostra de memória, não uma medição de FPS, CPU, GPU ou vazamentos. Capturas em work/android-qa no workspace Codex. Ainda não foram percorridos todos os heróis/chefes/salas em teste humano prolongado nem medidos todos os critérios de desempenho. Textos de dois HUDs se sobrepõem na arena; limitação visual registrada para a etapa de interface. Não declarar que todos os recursos estão aprovados por esse teste parcial.

Próximo recorte: compilar o cliente cooperativo Android e conectar ao host Windows na mesma rede. Até aqui nenhum teste PC–Android multiplayer foi concluído. A implementação da rede continua na etapa7.

## Continuação real: cliente Android cooperativo0.6.0

APK IL2CPP/ARM64 gerado e instalado preservando dados. A montagem interna encontrou o problema Java NIO conhecido; o projeto Gradle gerado terminou BUILD SUCCESSFUL por montagem direta com ajuste restrito ao processo. Assinatura verificada; SHA2567FB66E4A388364B4DCD17FE7EB0A410B365064885441887AAAA7ABCE8CD8F2FB. Esta primeira build cooperativa ainda antecede a ligação das recompensas à rede.

PC Windows como host e moto g15 como cliente na mesma rede local: sala2/4, Patrulheiro no celular e Guerreiro no PC, seed12345/grafo9salas idêntico. Arrasto do joystick físico alterou a posição autoritativa vista no PC; passagem para sala2 reuniu os jogadores e iniciou inimigos/portas bloqueadas. Ambos receberam estado Defeat e vidas0 após permanecerem sem lutar. Encerrar a sala no PC produziu Closed no telefone. Capturas e logs próprios em work/android-qa/physical-lan-*.log, coop-playing/move/fight/current/host-ended.png. Não registrar esse trecho como vitória ou ataque remoto confirmado: essa rodada não demonstrou um golpe do celular causando dano.

Também observados na build local: Arcanista reduziu alvo100→84 com projétil/controle; Cavaleiro Rúnico reduziu100→76 com investida e proteção temporária. Ataque básico do Cavaleiro no primeiro posicionamento ficou fora do alcance; não contado como acerto.

Pendências comprovadas: filtro de mouse cooperativo emitia aviso por consultar EventSystem durante callback de InputAction; botões de sessão sobrepunham o joystick. Correções preparadas no código posterior; precisam de nova build e conferência física. Teste acima é LAN real entre duas plataformas, sem comprovação de Relay/internet, quatro aparelhos ou sessão completa.

## Validação cooperativa0.6.1/code7

APK produzido, assinatura verificada e instalação-r real. Primeira revisão tinha SHA256CCF65BB6CE738A8D8C7916B58970B3EB95B4AEA4AEA97A1283A355814A85B7B2. Após correções finais, o hash do APK de entrega será registrado separadamente. Montagem direta Gradle49s após falha interna de Java NIO; nenhum erro C# no build. Dados offline foram preservados.

- Toques no ATAQUE do Patrulheiro no moto g15 derrotaram o goblin36HP controlado pelo PC; host registrou COOP_PROBE_REMOTE_KILL e12XP nos dois perfis. O host de QA só se movia, não atacava. Captura final-attack e log coop-final-host/player.log.
- Joystick físico deslocou o slot1 de(0.9,-3) para aproximadamente(2.48,-1.62) no planoXZ; posição vista também na janela Windows. final-before/after-move e final-move.png.
- Inventário recebido do host12/24; selecionar Presas de Vidro e EQUIPAR pelo celular retornou EQUIPADO/MainHand e mudou a arma visual nas replicas. Na sessão seguinte o mesmo perfil ainda tinha as adagas; ataque do celular voltou a matar o alvo com o padrão corpo a corpo. Comparação/equipagem em final-compare/equipped/gear.png.
- Primeira reconexão foi tentada após a reserva de90s e recusada corretamente. Nova rodada: saída pelo botão, force-stop/reabrir o app e retorno em aproximadamente40s recuperaram slot1, Patrulheiro105HP e adagas; host permaneceu com60HP. second-before/after-return.json e second-return.png.
- Baú da autoridade e derrota acionados por instrumentação produziram resultado real no Android:11XP,6 moedas e um Pingente da Estrada épico. Não foi vitória humana da expedição. phone-result.png.
- Windows recompilado com runInBackground=true. Outra janela do jogo foi ativada; host continuou de tick3505 para3799 em cerca31s e processou movimento/derrota recebidos. background-start/moving/defeat.json. Não garante execução Android com tela apagada: suspensão do sistema e timeout ainda precisam de teste.
- Reaparecimento: host morto0HP, aliado Android como Cavaleiro Rúnico210HP. Ao completar a sala real via mortes controladas de QA, callback de vitória restaurou host para70HP; telefone mostrou70/140 e permaneceu210/210. coop-revive-safe/before/after-win.json e safe-room-win.png. Esse teste valida integração e replicação do reaparecimento; o dano administrativo aos inimigos não comprova dificuldade justa.

Uma tentativa anterior do harness pediu encontro no Lobby antes de gerar mapa e causou NullReferenceException em CoopSmokeProbe. Corrigido com guarda de fase; repetição do comando antes do carregamento e teste correto de vitória não produziram exceção. Não ocultar esse registro. Também corrigido rótulo do herói selecionado ao fechar a sessão. Suíte de rede17/17 repetida após essas alterações.

Pendências: vínculo e teste real Relay/internet; sessão completa de quatro jogadores humanos; latência/perda de pacotes; FPS/CPU/GPU e memória sustentados; interface final/alto contraste/gamepad físico. HUD ainda aparece escurecido atrás do inventário/resultado e alguns textos longos disputam espaço com botões. Arte continua geométrica. Estes resultados não aprovam lançamento nem crossplay universal.

APK final de entrega: SHA2569069C045C6B9AF3FFAED818EFABC0D293171C90D9A3CC321594C1DC8EC5A5F78,0.6.1/code7, min25/target35/ARM64; Gradle direto BUILD SUCCESSFUL29s. Assinatura verificada, instalado-r e iniciado novamente. Host Windows normal e Android final conectaram2/4 em Lobby; logs do novo processo do aparelho e host sem exceções no trecho examinado. Mecânicas de combate/reconexão acima foram exercitadas na revisão anterior de0.6.1 com a mesma lógica de gameplay; a revisão final modifica guardas da instrumentação e rótulo de seleção ao fechar a sessão, seguida de17 testes de rede aprovados. Não declarar nova repetição de toda a sessão completa na última troca de APK.
