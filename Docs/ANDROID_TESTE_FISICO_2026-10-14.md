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
